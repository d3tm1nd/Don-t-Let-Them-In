using UnityEngine;

/// <summary>
/// InteractableHintSource
/// ใส่บน Object ที่ "Interact ได้" (หรือ parent ของมัน)
/// เพื่อให้ระบบ Hint โชว์ข้อความ 1 ครั้ง เมื่อผู้เล่นเดินผ่าน/เข้าใกล้
///
/// ความต้องการ (โหมด 2):
/// - เดินผ่านครั้งแรก → โชว์ข้อความ ~4 วินาที แล้วหาย
/// - เดินผ่านครั้งที่ 2+ → ไม่โชว์อีกเลย "เฉพาะระหว่างเล่นรอบนั้น"
/// - ปิดเกม/เริ่มใหม่ → ข้อความจะกลับมาโชว์ได้อีก
/// </summary>
[DisallowMultipleComponent]
public class InteractableHintSource : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("ต้องไม่ซ้ำกัน ใช้สำหรับจำว่าเคยโชว์แล้วหรือยัง (แนะนำ: SceneName_ObjectName หรือ GUID)")]
    public string hintId;

    [Header("Hint Text")]
    [TextArea(2, 4)]
    public string hintText = "";

    [Tooltip("ระยะเวลาโชว์ (วินาที)")]
    public float showSeconds = 4f;

    [Header("Persistence")]
    [Tooltip("โหมดนี้จะจำเฉพาะระหว่างเล่นรอบนั้นเท่านั้น (ตัวเลือกนี้ถูกเก็บไว้เพื่อ compatibility)")]
    public bool persistAcrossSessions = false;

    [Header("Optional")]
    [Tooltip("ถ้าตั้งไว้ จะจำกัดให้โชว์เฉพาะช่วง phase นี้ (ปล่อยว่าง = ทุกช่วง)")]
    public bool onlyShowInSpecificPhase = false;

    public PhaseManager.GamePhase requiredPhase = PhaseManager.GamePhase.Day;

    public bool CanShowNow()
    {
        if (string.IsNullOrWhiteSpace(hintText)) return false;

        if (onlyShowInSpecificPhase)
        {
            if (PhaseManager.Instance == null) return false;
            if (PhaseManager.Instance.currentPhase != requiredPhase) return false;
        }

        return true;
    }

    void Reset()
    {
        if (string.IsNullOrWhiteSpace(hintId))
            hintId = gameObject.scene.name + "_" + gameObject.name;

        if (showSeconds <= 0f) showSeconds = 4f;
        persistAcrossSessions = false;
    }
}
