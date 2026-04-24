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
        if (hintText == null || root == null) return;

        hintText.text = text ?? "";
        root.SetActive(true);

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(AutoHide(Mathf.Max(0.1f, seconds)));
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
