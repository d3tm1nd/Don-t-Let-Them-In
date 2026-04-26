using UnityEngine;

/// <summary>
/// NPCBodyCheckUISpriteSet (SINGLE)
///
/// ใส่ไว้บน Prefab/ตัว NPC เพื่อกำหนด "รูปตา" และ "รูปมือ" ที่จะแสดงใน BodyCheckUI
/// - ใช้รูปเดียว ไม่แยกซ้าย/ขวา
/// - รองรับ Normal/Abnormal
/// </summary>
[DisallowMultipleComponent]
public class NPCBodyCheckUISpriteSet : MonoBehaviour
{
    [Header("Eyes (single)")]
    public Sprite eyeNormal;
    public Sprite eyeAbnormal;

    [Header("Hands (single)")]
    public Sprite handNormal;
    public Sprite handAbnormal;

    [Header("Temperature Icon (optional)")]
    public Sprite tempIcon;
}
