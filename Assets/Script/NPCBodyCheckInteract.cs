using UnityEngine;

/// <summary>
/// กด E ที่ NPC เพื่อเปิด UI ตรวจร่างกาย (ไม่ลด Energy)
/// เงื่อนไข: ถ้า Energy หมด จะห้ามเปิด UI และ "กด E ที่ NPC ไม่ได้"
/// </summary>
public class NPCBodyCheckInteract : MonoBehaviour, IInteractable
{
    public NPCHealthProfile healthProfile;

    [Tooltip("ถ้าไม่ใส่ จะใช้ BodyCheckUI.Instance")]
    public BodyCheckUI ui;

    public void Interact()
    {
        // ถ้า Energy หมด: ไม่ให้กด E ที่ NPC ได้
        if (EnergyManager.Instance != null && EnergyManager.Instance.Current <= 0)
        {
            Debug.Log("⛔ Energy หมด: ไม่สามารถตรวจร่างกาย NPC ได้");
            return;
        }

        if (healthProfile == null)
        {
            Debug.LogError("❌ NPCBodyCheckInteract: healthProfile ไม่ถูกตั้งค่า");
            return;
        }

        var panel = ui != null ? ui : BodyCheckUI.Instance;
        if (panel == null)
        {
            // เผื่อ instance ยังไม่ถูกสร้าง
            panel = FindObjectOfType<BodyCheckUI>(true);
        }

        if (panel == null)
        {
            Debug.LogError("❌ BodyCheckUI ไม่พบในฉาก");
            return;
        }

        panel.Open(healthProfile);
    }
}
