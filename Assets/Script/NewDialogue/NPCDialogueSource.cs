using UnityEngine;

/// <summary>
/// NPCDialogueSource
/// - ใส่บน NPC (ตัวที่อยู่ในบ้าน) เพื่อกำหนดบทสนทนาที่จะเล่นเมื่อผู้เล่นกด F
/// - รองรับ 2 วิธีเก็บข้อความ:
///   A) ใช้ ScriptableObject (SimpleDialogueData) แนะนำ
///   B) ใส่ lines ตรงใน Inspector (ถ้า dialogueData ว่าง)
/// </summary>
public class NPCDialogueSource : MonoBehaviour
{
    [Header("Dialogue")]
    public SimpleDialogueData dialogueData;

    [Tooltip("ถ้า dialogueData ไม่ได้ตั้ง จะใช้ข้อความจากตรงนี้แทน")]
    public string speakerName = "";

    [TextArea(2, 6)]
    public string[] lines;

    [Header("Optional")]
    [Tooltip("ถ้า true จะบังคับให้มี collider อยู่บน object นี้หรือ parent เพื่อ raycast โดน")]
    public bool requireCollider = true;

    void Reset()
    {
        // กันเผื่อลืมใส่ collider
        if (requireCollider)
        {
            var col = GetComponent<Collider>();
            if (col == null) gameObject.AddComponent<BoxCollider>();
        }
    }

    public string GetSpeaker()
    {
        if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.speakerName))
            return dialogueData.speakerName;

        return speakerName;
    }

    public string[] GetLines()
    {
        if (dialogueData != null && dialogueData.lines != null && dialogueData.lines.Length > 0)
            return dialogueData.lines;

        return lines;
    }
}
