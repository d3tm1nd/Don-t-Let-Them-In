using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NPCSpawner (2-day rules) - PAIRED + OFFSET PER DAY
///
/// Purpose:
/// - Keep OUTSIDE door prefab and INSIDE prefab matched as a pair (fixes identity mismatch).
/// - Allow Day 2 to use a NEW set of NPCs (humans/ghosts) by starting from an offset index.
///
/// ✅ Patch (ตามที่ขอ):
/// - ป้องกันการ Spawn "ซ้อนทับตำแหน่ง" สำหรับ INSIDE spawn points เท่านั้น
///   โดยจะตรวจว่าจุด spawn ว่างไหมก่อน Instantiate
///   ถ้าจุดนั้นไม่ว่าง จะลองจุดถัดไปจนกว่าจะเจอจุดว่าง (ภายในจำนวน spawn points)
///   ถ้าทุกจุดไม่ว่าง จะ "ไม่ spawn" เพื่อกันทับกัน
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

    [Header("Per-Day Pair Offsets")]
    [Tooltip("Start index into HUMAN pairs for Day 1")]
    public int humanStartIndexDay1 = 0;
    [Tooltip("Start index into HUMAN pairs for Day 2")]
    public int humanStartIndexDay2 = 0;
    [Tooltip("Start index into GHOST pairs for Day 1")]
    public int ghostStartIndexDay1 = 0;
    [Tooltip("Start index into GHOST pairs for Day 2")]
    public int ghostStartIndexDay2 = 0;

    [Header("Plan Options")]
    [Tooltip("Shuffle the final visitor order for the day (pairs remain matched).")]
    public bool shuffleOrder = true;

    [Header("Delays")]
    public float initialOutsideDelay = 1f;
    public float insideSpawnDelay = 0.5f;
    public float newOutsideDelay = 2.5f;

    [Header("Audio")]
    public AudioClip knockSound;
    private AudioSource audioSource;

    [Header("Anti-Overlap (INSIDE only)")]
    [Tooltip("ถ้าเปิด: จะพยายามหาจุด spawn ด้านในที่ว่าง เพื่อกันการ spawn ซ้อนทับ")]
    public bool preventInsideOverlap = true;

    [Tooltip("รัศมีตรวจว่ามี NPC/ตัวละครอยู่ใกล้จุด spawn หรือไม่ (เมตร)")]
    public float insideOverlapRadius = 0.6f;

    [Tooltip("ถ้าเปิด: จะข้ามการ spawn ถ้าหาจุดว่างไม่เจอ (กันซ้อนทับ 100%)")]
    public bool skipSpawnIfNoFreePoint = true;

    private readonly Queue<VisitorPlan> todayQueue = new Queue<VisitorPlan>();
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

        // หมายเหตุ: รีเซ็ต index ต่อวันยังคงอยู่ (ตามของเดิม)
        // แต่เรามีกันซ้อนทับแล้ว: ถ้าจุด 0-2 มีคนอยู่ จะไปลองจุดถัดไปให้เอง
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

        int humanStart = (day <= 1) ? humanStartIndexDay1 : humanStartIndexDay2;
        int ghostStart = (day <= 1) ? ghostStartIndexDay1 : ghostStartIndexDay2;

        AddPairedPlansWithOffset(plans, NPCKind.Human, humans, humanOutsideDoorPrefabs, humanInsidePrefabs, humanStart);
        AddPairedPlansWithOffset(plans, NPCKind.Ghost, ghosts, ghostOutsideDoorPrefabs, ghostInsidePrefabs, ghostStart);

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

        Debug.Log($"📅 NPCSpawner plan (PAIRED+OFFSET) Day {dayStamp} total={todayTotal} (H={humans}, G={ghosts}) humanStart={humanStart} ghostStart={ghostStart}");
    }

    void AddPairedPlansWithOffset(List<VisitorPlan> plans, NPCKind kind, int count, GameObject[] outsideArr, GameObject[] insideArr, int startIndex)
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

        if (outsideLen != insideLen)
        {
            Debug.LogWarning($"⚠️ NPCSpawner: {kind} outside/inside arrays length mismatch. Using min={pairLen}. outside={outsideLen}, inside={insideLen}");
        }

        // Normalize start index
        int normalizedStart = startIndex % pairLen;
        if (normalizedStart < 0) normalizedStart += pairLen;

        // Create plans in offset order and wrap if needed
        for (int i = 0; i < count; i++)
        {
            int idx = (normalizedStart + i) % pairLen;
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

        Debug.Log($"🚪 Spawned OUTSIDE ({plan.kind}) {spawnedOutsideCount}/{todayTotal} Door='{plan.outsideDoorPrefab.name}' Inside='{plan.insidePrefab.name}'");
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

        Transform point = GetNextInsidePointRespectingOverlap();
        if (point == null)
        {
            if (skipSpawnIfNoFreePoint)
            {
                Debug.LogWarning("⚠️ SpawnInsideNPC: No free inside spawn point (skip to prevent overlap).");
                yield break;
            }
            // fallback: spawn at spawner position
            point = transform;
        }

        Vector3 pos = point.position;
        Quaternion rot = point.rotation;

        GameObject insideNPC = Instantiate(prefab, pos, rot);
        RecordNPCData(prefab, insideNPC.transform.position, insideNPC.transform.rotation);

        Debug.Log($"🏠 Spawned INSIDE '{prefab.name}' at '{point.name}'");
    }

    private Transform GetNextInsidePointRespectingOverlap()
    {
        if (insideSpawnPoints == null || insideSpawnPoints.Length == 0)
            return null;

        // ถ้าไม่กัน overlap ก็ใช้แบบเดิม
        if (!preventInsideOverlap)
        {
            int idx = insidePointIndex % insideSpawnPoints.Length;
            insidePointIndex++;
            return insideSpawnPoints[idx];
        }

        int attempts = insideSpawnPoints.Length;

        for (int i = 0; i < attempts; i++)
        {
            int idx = insidePointIndex % insideSpawnPoints.Length;
            insidePointIndex++;

            Transform p = insideSpawnPoints[idx];
            if (p == null) continue;

            if (!IsInsidePointBlockedByNPC(p.position, insideOverlapRadius))
                return p;
        }

        // ไม่มีจุดว่าง
        return null;
    }

    private bool IsInsidePointBlockedByNPC(Vector3 position, float radius)
    {
        // ใช้ OverlapSphere แล้ว "กรอง" เฉพาะ collider ที่เป็น NPC จริง ๆ
        // (กันปัญหาไปชนพื้น/ผนังแล้วเข้าใจว่าจุดไม่ว่าง)
        Collider[] hits = Physics.OverlapSphere(position, Mathf.Max(0.01f, radius), ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;

            // กรองเฉพาะสิ่งที่ดูเป็น NPC ในโปรเจกต์นี้
            // - NPCHealthProfile (NPC ในบ้าน)
            // - DoorNPC (เผื่อบาง prefab มี)
            // - Tag Human/Ghost (สำรอง)
            if (c.GetComponentInParent<NPCHealthProfile>() != null) return true;
            if (c.GetComponentInParent<DoorNPC>() != null) return true;

            if (c.CompareTag("Human") || c.CompareTag("Ghost")) return true;
            Transform root = c.transform.root;
            if (root != null && (root.CompareTag("Human") || root.CompareTag("Ghost"))) return true;
        }

        return false;
    }

    public void OnNPCAccepted()
    {
        acceptedCount++;
        Debug.Log($"✅ AcceptedCount={acceptedCount} SpawnedOutside={spawnedOutsideCount}/{todayTotal}");
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
