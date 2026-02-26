using UnityEngine;

public class NightNPCSpawner : MonoBehaviour
{
    void Start()
    {
        if (NPCDataManager.Instance == null)
        {
            Debug.LogWarning("⚠️ NPCDataManager ไม่พบ! ไม่ spawn NPC ใน Night");
            return;
        }

        var acceptedNPCs = NPCDataManager.Instance.acceptedNPCs;

        if (acceptedNPCs.Count == 0)
        {
            Debug.Log("ℹ️ ไม่มี NPC data ที่บันทึกไว้");
            return;
        }

        foreach (var data in acceptedNPCs)
        {
            if (data.prefab == null) continue;

            Instantiate(
                data.prefab,
                data.position,
                data.rotation
            );

            Debug.Log($"✅ Spawn NPC ใน Night: {data.prefab.name} ที่ {data.position}");
        }

        // Optional: ลบ data หลัง spawn ถ้าต้องการ reset
        // NPCDataManager.Instance.acceptedNPCs.Clear();
    }
}