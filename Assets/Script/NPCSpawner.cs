using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("Rules (2-day)")]
    [Tooltip("TwoDayRules asset (Day1: 2H 2G, Day2: 2H 1G)")]
    public TwoDayRules rules;

    [Header("Outside Spawn")]
    public Transform outsideSpawnPoint;

    [Header("Outside Door NPC Prefabs")]
    [Tooltip("Door NPC prefabs for HUMAN visitors (at least 1)")]
    public GameObject[] humanOutsideDoorPrefabs;
    [Tooltip("Door NPC prefabs for GHOST visitors (at least 1)")]
    public GameObject[] ghostOutsideDoorPrefabs;

    [Header("Inside Prefabs")]
    [Tooltip("Inside prefabs for HUMAN visitors (must have NPCTypeTag=Human)")]
    public GameObject[] humanInsidePrefabs;
    [Tooltip("Inside prefabs for GHOST visitors (must have NPCTypeTag=Ghost)")]
    public GameObject[] ghostInsidePrefabs;

    [Header("Inside Spawn Points")]
    [Tooltip("Multiple inside spawn points (rotating)")]
    public Transform[] insideSpawnPoints;

    [Header("Delays")]
    public float insideSpawnDelay = 0.5f;
    public float newOutsideDelay = 2.5f;

    [Header("Audio")]
    public AudioClip knockSound;

    private AudioSource audioSource;

    private int insideIndex = 0;

    // Today plan
    private Queue<VisitorPlan> _todayQueue = new Queue<VisitorPlan>();
    private int _todayTotal = 0;

    // Stats (optional)
    public int spawnedCount = 0;
    public int acceptedCount = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        BuildTodayQueue();
        StartCoroutine(SpawnOutsideInitial());
    }

    private void BuildTodayQueue()
    {
        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;

        int humans = 2;
        int ghosts = 2;

        if (rules != null)
        {
            if (day <= 1) { humans = rules.day1Humans; ghosts = rules.day1Ghosts; }
            else if (day == 2) { humans = rules.day2Humans; ghosts = rules.day2Ghosts; }
            else { humans = 0; ghosts = 0; }
        }

        _todayTotal = Mathf.Max(0, humans + ghosts);
        _todayQueue.Clear();

        // build a random sequence of kinds (human/ghost) matching the counts
        List<NPCKind> kinds = new List<NPCKind>(_todayTotal);
        for (int i = 0; i < humans; i++) kinds.Add(NPCKind.Human);
        for (int i = 0; i < ghosts; i++) kinds.Add(NPCKind.Ghost);

        // shuffle
        for (int i = 0; i < kinds.Count; i++)
        {
            int j = Random.Range(i, kinds.Count);
            (kinds[i], kinds[j]) = (kinds[j], kinds[i]);
        }

        // map kinds -> prefabs
        for (int i = 0; i < kinds.Count; i++)
        {
            var k = kinds[i];
            var outside = PickOutsidePrefab(k);
            var inside = PickInsidePrefab(k);
            _todayQueue.Enqueue(new VisitorPlan { kind = k, outsideDoorPrefab = outside, insidePrefab = inside });
        }

        Debug.Log($"📅 Day {day} visitor plan: total={_todayTotal} (H={humans}, G={ghosts})");
    }

    private GameObject PickOutsidePrefab(NPCKind kind)
    {
        var arr = (kind == NPCKind.Ghost) ? ghostOutsideDoorPrefabs : humanOutsideDoorPrefabs;
        if (arr == null || arr.Length == 0)
        {
            Debug.LogError($"❌ NPCSpawner: Missing outside door prefabs for {kind}");
            return null;
        }
        return arr[Random.Range(0, arr.Length)];
    }

    private GameObject PickInsidePrefab(NPCKind kind)
    {
        var arr = (kind == NPCKind.Ghost) ? ghostInsidePrefabs : humanInsidePrefabs;
        if (arr == null || arr.Length == 0)
        {
            Debug.LogError($"❌ NPCSpawner: Missing inside prefabs for {kind}");
            return null;
        }
        return arr[Random.Range(0, arr.Length)];
    }

    IEnumerator SpawnOutsideInitial()
    {
        yield return new WaitForSeconds(1f);
        SpawnNextOutsideNow();

        if (knockSound != null)
            audioSource.PlayOneShot(knockSound);
    }

    private void SpawnNextOutsideNow()
    {
        if (spawnedCount >= _todayTotal)
        {
            Debug.Log($"🚫 No more visitors today. Spawned={spawnedCount}/{_todayTotal}");
            return;
        }

        if (_todayQueue.Count == 0)
        {
            Debug.LogWarning("⚠️ Visitor queue empty unexpectedly.");
            return;
        }

        var plan = _todayQueue.Dequeue();
        if (plan.outsideDoorPrefab == null)
        {
            Debug.LogError("❌ plan.outsideDoorPrefab is null");
            return;
        }

        GameObject npcDoor = Instantiate(plan.outsideDoorPrefab, outsideSpawnPoint.position, outsideSpawnPoint.rotation);
        var doorNPC = npcDoor.GetComponent<DoorNPC>();
        if (doorNPC == null)
        {
            Debug.LogError($"❌ Prefab {plan.outsideDoorPrefab.name} has no DoorNPC component!");
            Destroy(npcDoor);
            return;
        }

        doorNPC.spawner = this;
        doorNPC.insidePrefab = plan.insidePrefab; // inside prefab for accept

        spawnedCount++;
        Debug.Log($"✅ Spawn Outside Visitor ({plan.kind}) | Spawned {spawnedCount}/{_todayTotal} | Accepted {acceptedCount}");
    }

    // ================= Spawn Inside (records data) =================
    public IEnumerator SpawnInsideNPC(GameObject prefabOverride = null)
    {
        Debug.Log($"⏳ [Inside] Wait {insideSpawnDelay}s...");
        yield return new WaitForSeconds(insideSpawnDelay);

        GameObject prefab = prefabOverride != null ? prefabOverride : null;
        if (prefab == null)
        {
            Debug.LogWarning("⚠️ SpawnInsideNPC: prefab is null");
            yield break;
        }

        if (insideSpawnPoints == null || insideSpawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ No insideSpawnPoints! Using spawner position fallback.");
            GameObject fallbackNPC = Instantiate(prefab, transform.position, Quaternion.identity);
            RecordNPCData(prefab, fallbackNPC.transform.position, fallbackNPC.transform.rotation);
            yield break;
        }

        int idx = insideIndex % insideSpawnPoints.Length;
        Transform spawnPoint = insideSpawnPoints[idx];

        GameObject insideNPC = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        insideIndex++;

        RecordNPCData(prefab, insideNPC.transform.position, insideNPC.transform.rotation);
        Debug.Log($"✅ [Inside] Spawn at point {idx + 1} (Prefab: {prefab.name})");
    }

    // ================= Spawn Next Outside Visitor =================
    public IEnumerator SpawnNewOutsideNPC()
    {
        if (spawnedCount >= _todayTotal)
        {
            Debug.Log($"🚫 Visitor limit reached today ({_todayTotal}). Accepted={acceptedCount}");
            yield break;
        }

        Debug.Log($"⏳ [New Outside] Wait {newOutsideDelay}s...");
        yield return new WaitForSeconds(newOutsideDelay);

        SpawnNextOutsideNow();

        if (knockSound != null)
            audioSource.PlayOneShot(knockSound);
    }

    public void OnNPCAccepted()
    {
        acceptedCount++;
        Debug.Log($"✅ Accepted visitor. Accepted={acceptedCount} (today spawned {spawnedCount}/{_todayTotal})");
    }

    // Records data in NPCDataManager
    private void RecordNPCData(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (NPCDataManager.Instance != null)
        {
            NPCDataManager.Instance.AddAcceptedNPC(prefab, position, rotation);
        }
        else
        {
            Debug.LogWarning("⚠️ NPCDataManager not found! Data not recorded.");
        }
    }

    private struct VisitorPlan
    {
        public NPCKind kind;
        public GameObject outsideDoorPrefab;
        public GameObject insidePrefab;
    }
}
