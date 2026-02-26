using UnityEngine;
using System.Collections;

public class NPCSpawner : MonoBehaviour
{
    [Header("Outside NPC (ตัวแรก)")]
    public GameObject outsideNPCPrefab;
    public Transform outsideSpawnPoint;

    [Header("Next Outside NPC (ตัวใหม่/ตัวอื่น)")]
    [Tooltip("Prefab NPCdoor ตัวใหม่ที่ spawn ข้างนอก")]
    public GameObject nextOutsideNPCPrefab;

    [Header("Inside NPC")]
    public GameObject insideNPCPrefab;
    [Tooltip("Prefab Inside สำหรับตัวใหม่ (เหมือนกับ prefab ตัวใหม่)")]
    public GameObject newInsideNPCPrefab;
    [Tooltip("จุด spawn ด้านใน (หลายจุด → คนละที่ทุกครั้ง)")]
    public Transform[] insideSpawnPoints;

    [Header("Spawn Limits & Delays")]
    [Tooltip("จำนวนสูงสุดของ NPC ที่ spawn (รวมตัวแรก)")]
    public int maxNPCs = 5;
    [Tooltip("Delay ก่อน spawn NPC ด้านใน")]
    public float insideSpawnDelay = 0.5f;
    [Tooltip("Delay ก่อน spawn NPCdoor ตัวใหม่")]
    public float newOutsideDelay = 2.5f;

    [Header("Audio")]
    public AudioClip knockSound;

    AudioSource audioSource;

    private int insideIndex = 0;
    private int spawnedCount = 0;
    public int acceptedCount = 0;

    public bool AllNPCsAccepted => acceptedCount >= maxNPCs;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(SpawnOutside());
    }

    IEnumerator SpawnOutside()
    {
        yield return new WaitForSeconds(1f);

        GameObject npc = Instantiate(
            outsideNPCPrefab,
            outsideSpawnPoint.position,
            outsideSpawnPoint.rotation
        );

        DoorNPC doorNPC = npc.GetComponent<DoorNPC>();
        if (doorNPC != null)
        {
            doorNPC.insidePrefab = insideNPCPrefab;
            doorNPC.spawner = this;
        }

        spawnedCount++;
        Debug.Log($"✅ Spawn Outside NPC (Initial) | Spawned: {spawnedCount}/{maxNPCs} | Accepted: {acceptedCount}/{maxNPCs}");

        if (knockSound != null)
            audioSource.PlayOneShot(knockSound);
    }

    // ================= Spawn Inside (บันทึก data) =================
    public IEnumerator SpawnInsideNPC(GameObject prefabOverride = null)
    {
        Debug.Log($"⏳ [Inside] รอ {insideSpawnDelay}s...");

        yield return new WaitForSeconds(insideSpawnDelay);

        GameObject prefab = prefabOverride != null ? prefabOverride : insideNPCPrefab;

        if (insideSpawnPoints == null || insideSpawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ ไม่มี Inside Spawn Points!");
            GameObject fallbackNPC = Instantiate(prefab, transform.position, Quaternion.identity);
            RecordNPCData(prefab, fallbackNPC.transform.position, fallbackNPC.transform.rotation);  // 🔥 บันทึก
            yield break;
        }

        int idx = insideIndex % insideSpawnPoints.Length;
        Transform spawnPoint = insideSpawnPoints[idx];

        GameObject insideNPC = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        insideIndex++;

        // 🔥 NEW: บันทึก data ใน Manager
        RecordNPCData(prefab, insideNPC.transform.position, insideNPC.transform.rotation);

        Debug.Log($"✅ [Inside] Spawn ที่จุด {idx + 1}! (Prefab: {prefab.name})");
    }

    // ================= Spawn NPCdoor ตัวใหม่ =================
    public IEnumerator SpawnNewOutsideNPC()
    {
        if (spawnedCount >= maxNPCs)
        {
            Debug.Log($"🚫 ถึง limit Spawn ({maxNPCs})! | Accepted: {acceptedCount}/{maxNPCs}");
            yield break;
        }

        Debug.Log($"⏳ [New Outside] รอ {newOutsideDelay}s...");

        yield return new WaitForSeconds(newOutsideDelay);

        GameObject prefabToSpawn = nextOutsideNPCPrefab != null ? nextOutsideNPCPrefab : outsideNPCPrefab;
        GameObject newNPC = Instantiate(prefabToSpawn, outsideSpawnPoint.position, outsideSpawnPoint.rotation);

        DoorNPC doorNPC = newNPC.GetComponent<DoorNPC>();
        if (doorNPC == null)
        {
            Debug.LogError($"❌ Prefab {prefabToSpawn.name} ไม่มี DoorNPC!");
            Destroy(newNPC);
            yield break;
        }

        doorNPC.insidePrefab = newInsideNPCPrefab != null ? newInsideNPCPrefab : insideNPCPrefab;
        doorNPC.spawner = this;

        spawnedCount++;
        Debug.Log($"✅ [New Outside] Spawn สำเร็จ! | Spawned: {spawnedCount}/{maxNPCs} | Accepted: {acceptedCount}/{maxNPCs}");

        if (knockSound != null)
            audioSource.PlayOneShot(knockSound);
    }

    public void OnNPCAccepted()
    {
        acceptedCount++;
        Debug.Log($"✅ รับ NPC เข้าแล้ว! | Accepted: {acceptedCount}/{maxNPCs}");

        if (AllNPCsAccepted)
        {
            Debug.Log("🎉 รับ NPC ครบทุกตัว! กด E ที่ประตูเพื่อไป Night Scene");
        }
    }

    // 🔥 NEW: บันทึก data ใน Manager
    private void RecordNPCData(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (NPCDataManager.Instance != null)
        {
            NPCDataManager.Instance.AddAcceptedNPC(prefab, position, rotation);
        }
        else
        {
            Debug.LogWarning("⚠️ NPCDataManager ไม่พบ! ไม่บันทึก data");
        }
    }
}