using UnityEngine;

/// <summary>
/// SimpleDialogueData
/// - เก็บบทสนทนาแบบเรียงลำดับ (Linear)
/// - ใช้กับระบบกด F เปิดบทสนทนา + คลิกซ้ายไปประโยคถัดไป + ESC ออก
/// </summary>
[CreateAssetMenu(menuName = "Dialogue/Simple Dialogue Data", fileName = "SimpleDialogueData")]
public class SimpleDialogueData : ScriptableObject
{
    [Header("Meta")]
    public string dialogueId = "";

    [Header("Speaker")]
    public string speakerName = "";

    [TextArea(2, 6)]
    public string[] lines;
}
