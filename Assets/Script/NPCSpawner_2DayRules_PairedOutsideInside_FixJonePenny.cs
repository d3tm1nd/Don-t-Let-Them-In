using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NPCSpawner (2-day rules) - PAIR outside+inside per visitor (Fixes Jone/Penny mismatch)
///
/// Problem fixed:
/// - Previously: OUTSIDE door prefab was picked randomly from humanOutsideDoorPrefabs
///   while INSIDE prefab was chosen sequentially by kind. This can mismatch (e.g., Penny outside -> Jone inside).
///
/// New behavior:
/// - We build a queue of VisitorPlan where each entry contains BOTH:
///     - outsideDoorPrefab
///     - insidePrefab
///     - kind
///   So the OUTSIDE identity always matches the INSIDE identity.
///
/// Setup requirement:
/// - humanOutsideDoorPrefabs and humanInsidePrefabs MUST be the same length and aligned by index.
///   Example:
///     [0] JoneDoor, [1] PennyDoor
///     [0] JoneInside, [1] PennyInside
/// - Same idea for ghost arrays (optional).
///
/// Notes:
/// - Door prefabs must have DoorNPC component.
/// - Uses TwoDayRules counts to decide how many Human/Ghost visitors to enqueue per day.
/// </summary>
public class NPCSpawner : MonoBehaviour
{
    [Header("Rules (2-day)")]
    public TwoDayRules rules;

    [Header("Spawn Points")]
    public Transform outsideSpawnPoint;
    public Transform[] insideSpawnPoints;

    [Header("Outside Door Prefabs (paired by index)")]
    public GameObject[] humanOutsideDoorPrefabs;
    public GameObject[] ghostOutsideDoorPrefabs;

    [Header("Inside Prefabs (paired by index)")]
    public GameObject[] humanInsidePrefabs;
    public GameObject[] ghostInsidePrefabs;

    [Header("Plan Options")]
    [Tooltip("If true, shuffle the visitor order for the day (still keeps pairs matched)")]
    public bool shuffleOrder = true;

    [Header("Delays")]
    public float initialOutsideDelay = 1f;
    public float insideSpawnDelay = 0.5f;
    public float newOutsideDelay = 2.5f;

    [Header("Audio")]
    public AudioClip knockSound;

    private AudioSource audioSource;

    private Queue<VisitorPlan> todayQueue = new Queue<VisitorPlan>();
    private int todayTotal = 0;
    private int dayStamp = -1;

    public int spawnedOutsideCount = 0;
    public int acceptedCount = 0;

    private int insidePointIndex = 0;

    private bool spawningOutside = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        EnsureTodayPlan();
        StartCoroutine(SpawnOutsideAfterDelay(initialOutsideDelay));
    }

    void EnsureTodayPlan()
    {
        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        if (dayStamp == day && todayTotal > 0) return;

        dayStamp = day;
        BuildPlanForDay(day);

        spawnedOutsideCount = 0;
        acceptedCount = 0;
        spawningOutside = false;
        insidePointIndex = 0;
    }

    void BuildPlanForDay(int day)
    {
        int humans = 2;
        int ghosts = 2;

        if (rules != null)
        {
            if (day <= 1) { humans = rules.day1Humans; ghosts = rules.day1Ghosts; }
            else if (day == 2) { humans = rules.day2Humans; ghosts = rules.day2Ghosts; }
            else { humans = 0; ghosts = 0; }
        }

        var plans = new List<VisitorPlan>();

        // Build human paired plans
        AddPairedPlans(plans, NPCKind.Human, humans, humanOutsideDoorPrefabs, humanInsidePrefabs);

        // Build ghost paired plans
        AddPairedPlans(plans, NPCKind.Ghost, ghosts, ghostOutsideDoorPrefabs, ghostInsidePrefabs);

        // Optional shuffle (still matched)
        if (shuffleOrder)
        {
            for (int i = 0; i < plans.Count; i++)
            {
                int j = Random.Range(i, plans.Count);
                var tmp = plans[i];
                plans[i] = plans[j];
                plans[j] = tmp;
            }
        }

        todayQueue.Clear();
        for (int i = 0; i < plans.Count; i++) todayQueue.Enqueue(plans[i]);

        todayTotal = plans.Count;

        Debug.Log($"📅 NPCSpawner plan (PAIRED) | Day {dayStamp} | total={todayTotal} (H={humans}, G={ghosts})");
    }

    void AddPairedPlans(List<VisitorPlan> plans, NPCKind kind, int count, GameObject[] outsideArr, GameObject[] insideArr)
    {
        if (count <= 0) return;

        int outsideLen = (outsideArr != null) ? outsideArr.Length : 0;
        int insideLen = (insideArr != null) ? insideArr.Length : 0;

        if (outsideLen == 0 || insideLen == 0)
        {
            Debug.LogError($"❌ NPCSpawner: Missing prefabs for {kind}. outsideLen={outsideLen}, insideLen={insideLen}");
            return;
        }

        int pairLen = Mathf.Min(outsideLen, insideLen);
        if (pairLen <= 0)
        {
            Debug.LogError($"❌ NPCSpawner: Pair length is 0 for {kind}");
            return;
        }

        // If arrays are not same length, warn but still proceed with min length.
        if (outsideLen != insideLen)
        {
            Debug.LogWarning($"⚠️ NPCSpawner: {kind} outside/inside arrays length mismatch. Using min={pairLen}. outside={outsideLen}, inside={insideLen}");
        }

        // Create plans in index order and repeat (wrap) if count > pairLen
        for (int i = 0; i < count; i++)
        {
            int idx = i % pairLen;
            GameObject outPrefab = outsideArr[idx];
            GameObject inPrefab = insideArr[idx];

            if (outPrefab == null || inPrefab == null)
            {
                Debug.LogWarning($"⚠️ NPCSpawner: Null prefab in pair {kind}[{idx}] (outside={outPrefab}, inside={inPrefab}) - skipping");
                continue;
            }

            plans.Add(new VisitorPlan { kind = kind, outsideDoorPrefab = outPrefab, insidePrefab = inPrefab });
        }
    }

    IEnumerator SpawnOutsideAfterDelay(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        SpawnNextOutsideNow();
    }

    public IEnumerator SpawnNewOutsideNPC()
    {
        EnsureTodayPlan();

        if (spawnedOutsideCount >= todayTotal)
        {
            Debug.Log($"🚫 No more visitors today. Spawned={spawnedOutsideCount}/{todayTotal}");
            yield break;
        }

        if (spawningOutside)
        {
            Debug.Log("ℹ️ SpawnNewOutsideNPC ignored: already spawning outside.");
            yield break;
        }

        spawningOutside = true;
        yield return new WaitForSeconds(newOutsideDelay);
        spawningOutside = false;

        SpawnNextOutsideNow();
    }

    void SpawnNextOutsideNow()
    {
        EnsureTodayPlan();

        if (outsideSpawnPoint == null)
        {
            Debug.LogError("❌ NPCSpawner: outsideSpawnPoint is NULL");
            return;
        }

        if (spawnedOutsideCount >= todayTotal)
        {
            Debug.Log($"🚫 No more visitors today. Spawned={spawnedOutsideCount}/{todayTotal}");
            return;
        }

        if (todayQueue.Count == 0)
        {
            Debug.LogWarning("⚠️ NPCSpawner: todayQueue empty. Rebuilding plan.");
            BuildPlanForDay(dayStamp);
            if (todayQueue.Count == 0) return;
        }

        var plan = todayQueue.Dequeue();

        GameObject npcDoor = Instantiate(plan.outsideDoorPrefab, outsideSpawnPoint.position, outsideSpawnPoint.rotation);
        var doorNPC = npcDoor.GetComponent<DoorNPC>();
        if (doorNPC == null)
        {
            Debug.LogError($"❌ NPCSpawner: Outside prefab '{plan.outsideDoorPrefab.name}' has NO DoorNPC component. Destroying spawned object.");
            Destroy(npcDoor);
            return;
        }

        doorNPC.spawner = this;
        doorNPC.insidePrefab = plan.insidePrefab;

        spawnedOutsideCount++;
        if (knockSound != null) audioSource.PlayOneShot(knockSound);

        Debug.Log($"🚪 Spawned OUTSIDE ({plan.kind}) {spawnedOutsideCount}/{todayTotal} | Door='{plan.outsideDoorPrefab.name}' | Inside='{plan.insidePrefab.name}'");
    }

    // ================= Spawn Inside (records data) =================
    public IEnumerator SpawnInsideNPC(GameObject prefabOverride = null)
    {
        if (insideSpawnDelay > 0f)
            yield return new WaitForSeconds(insideSpawnDelay);

        GameObject prefab = prefabOverride;
        if (prefab == null)
        {
            Debug.LogWarning("⚠️ SpawnInsideNPC: prefab is null");
            yield break;
        }

        Transform point = null;
        if (insideSpawnPoints != null && insideSpawnPoints.Length > 0)
        {
            int idx = insidePointIndex % insideSpawnPoints.Length;
            point = insideSpawnPoints[idx];
            insidePointIndex++;
        }

        Vector3 pos = point != null ? point.position : transform.position;
        Quaternion rot = point != null ? point.rotation : Quaternion.identity;

        GameObject insideNPC = Instantiate(prefab, pos, rot);
        RecordNPCData(prefab, insideNPC.transform.position, insideNPC.transform.rotation);

        Debug.Log($"🏠 Spawned INSIDE '{prefab.name}'");
    }

    public void OnNPCAccepted()
    {
        acceptedCount++;
        Debug.Log($"✅ AcceptedCount={acceptedCount} | SpawnedOutside={spawnedOutsideCount}/{todayTotal}");
    }

    private void RecordNPCData(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (NPCDataManager.Instance != null)
            NPCDataManager.Instance.AddAcceptedNPC(prefab, position, rotation);
        else
            Debug.LogWarning("⚠️ NPCDataManager not found! Data not recorded.");
    }

    private struct VisitorPlan
    {
        public NPCKind kind;
        public GameObject outsideDoorPrefab;
        public GameObject insidePrefab;
    }
}
