using UnityEngine;

/// <summary>
/// ติดกับ prefab ของ NPC ทุกตัว (Human/Ghost) เพื่อกันการเกิดซ้ำของคนที่ "ตายแล้ว"
/// - ถ้า DeathRegistry ระบุว่า npcStableId นี้ตายแล้ว => Destroy(gameObject)
///
/// ข้อดี:
/// - ไม่ต้องแก้ทุก spawner ทุกตัว
/// - แค่ให้ prefab มี NPCHealthProfile.npcStableId ที่ไม่ซ้ำ
/// </summary>
[RequireComponent(typeof(NPCHealthProfile))]
public class DestroyIfDead : MonoBehaviour
{
    void Start()
    {
        if (DeathRegistry.Instance == null) return;

        var prof = GetComponent<NPCHealthProfile>();
        if (prof == null) return;

        string id = string.IsNullOrEmpty(prof.npcStableId) ? gameObject.name : prof.npcStableId;
        if (DeathRegistry.Instance.IsDead(id))
        {
            Debug.Log($"💀 DestroyIfDead: Destroy {prof.npcDisplayName} (id={id})");
            Destroy(gameObject);
        }
    }
}
