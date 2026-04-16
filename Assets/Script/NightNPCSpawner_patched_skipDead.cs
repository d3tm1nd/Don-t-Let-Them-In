using UnityEngine;

/// <summary>
/// NightNPCSpawner (patched):
/// - Spawn NPCs จาก NPCDataManager.acceptedNPCs ในฉาก Night
/// - ข้าม NPC ที่ถูกฆ่าตายแล้ว (ตาม DeathRegistry + npcStableId ใน NPCHealthProfile)
/// </summary>
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
        if (acceptedNPCs == null || acceptedNPCs.Count == 0)
        {
            Debug.Log("ℹ️ ไม่มี NPC data ที่บันทึกไว้");
            return;
        }

        // Ensure DeathRegistry exists (optional)
        if (DeathRegistry.Instance == null)
        {
            var go = new GameObject("DeathRegistry");
            go.AddComponent<DeathRegistry>();
        }

        foreach (var data in acceptedNPCs)
        {
            if (data.prefab == null) continue;

            // ตรวจว่า prefab นี้ถูก mark ว่าตายแล้วหรือไม่
            var profile = data.prefab.GetComponent<NPCHealthProfile>();
            if (profile != null)
            {
                string id = string.IsNullOrEmpty(profile.npcStableId) ? data.prefab.name : profile.npcStableId;
                if (DeathRegistry.Instance != null && DeathRegistry.Instance.IsDead(id))
                {
                    Debug.Log($"💀 Skip spawn (dead): {profile.npcDisplayName} id={id}");
                    continue;
                }
            }

            var obj = Instantiate(data.prefab, data.position, data.rotation);
            Debug.Log($"✅ Spawn NPC ใน Night: {data.prefab.name} ที่ {data.position}");

            // Safety: ถ้า instance มี DestroyIfDead จะจัดการตัวเองอีกชั้น
            // (ไม่ต้องทำอะไร)
        }

        // Optional: ลบ data หลัง spawn ถ้าต้องการ reset
        // NPCDataManager.Instance.acceptedNPCs.Clear();
    }
}
