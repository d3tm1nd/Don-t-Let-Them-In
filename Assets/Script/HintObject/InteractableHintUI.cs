using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// InteractableHintUI
/// UI แสดงข้อความ hint ชั่วคราวบนหน้าจอ
/// - โชว์ข้อความ X วินาที แล้วซ่อน
/// - ถ้าถูกเรียกซ้ำระหว่างโชว์: จะรีสตาร์ทเวลาใหม่
/// </summary>
public class InteractableHintUI : MonoBehaviour
{
    public static InteractableHintUI Instance { get; private set; }

    [Header("UI")]
    public GameObject root;
    public TextMeshProUGUI hintText;

    [Header("Behavior")]
    [Tooltip("ถ้า true: ใช้เวลาแบบ unscaled (ไม่โดน Time.timeScale) เหมาะกับเกมที่ pause")]
    public bool useUnscaledTime = true;

    private Coroutine _routine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (root == null) root = gameObject;
        HideImmediate();
    }

    public void ShowHint(string text, float seconds)
    {
        // ✅ แก้บรรทัดนี้ให้ถูกต้อง (ของเดิมในไฟล์เหมือนพิมพ์ผิดเป็นคนละบรรทัด)
        if (hintText == null || root == null) return;  // [1](https://rsuac-my.sharepoint.com/personal/6507448_rsu_ac_th/Documents/Microsoft%20Copilot%20Chat%20Files/InteractableHintUI.cs)

        hintText.text = text ?? "";
        root.SetActive(true);

        // ถ้ามี routine ค้างอยู่ ให้หยุดก่อน
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        // ✅ ถ้า seconds <= 0 แปลว่า "โชว์ค้าง" ไม่ต้อง auto-hide
        if (seconds <= 0f)
            return;

        // ✅ ถ้ามีเวลาจริง ค่อยเริ่ม auto-hide
        _routine = StartCoroutine(AutoHide(seconds));
    }

    // ✅ เพิ่มเมธอดนี้เข้าไป (เพื่อให้ Trigger เรียก HideHint() ได้)
    public void HideHint()
    {
        HideImmediate();  // ใช้ของเดิมที่มีอยู่แล้ว [1](https://rsuac-my.sharepoint.com/personal/6507448_rsu_ac_th/Documents/Microsoft%20Copilot%20Chat%20Files/InteractableHintUI.cs)
    }

    public void HideImmediate()
    {
        if (root != null) root.SetActive(false);
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    private IEnumerator AutoHide(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
        root.SetActive(false);
        _routine = null;
    }
}
