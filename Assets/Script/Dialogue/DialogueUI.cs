using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// DialogueUI
/// - แสดงข้อความพูดของ NPC แบบ Text UI
/// - รองรับ: name + line, typewriter effect, auto hide
/// - ออกแบบให้ใช้แบบ Singleton ง่าย ๆ
/// </summary>
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI")]
    public GameObject root;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI lineText;

    [Header("Behavior")]
    public bool useTypewriter = true;
    public float typeSpeed = 0.02f;

    [Tooltip("ถ้า >0 จะซ่อนอัตโนมัติหลังโชว์ครบ")]
    public float autoHideSeconds = 0f;

    [Tooltip("ถ้า true: ข้อความใหม่จะตัดข้อความเก่าทันที")]
    public bool interruptPrevious = true;

    Coroutine _typing;
    Coroutine _autoHide;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (root == null) root = gameObject;
        Hide();
    }

    public void Show(string speaker, string line)
    {
        if (root != null) root.SetActive(true);

        if (nameText != null)
            nameText.text = string.IsNullOrEmpty(speaker) ? "" : speaker;

        if (interruptPrevious)
        {
            if (_typing != null) StopCoroutine(_typing);
            if (_autoHide != null) StopCoroutine(_autoHide);
        }

        if (!useTypewriter || lineText == null)
        {
            if (lineText != null) lineText.text = line;
            StartAutoHide();
            return;
        }

        _typing = StartCoroutine(TypeRoutine(line));
    }

    public void Hide()
    {
        if (_typing != null) StopCoroutine(_typing);
        if (_autoHide != null) StopCoroutine(_autoHide);

        if (root != null) root.SetActive(false);
    }

    IEnumerator TypeRoutine(string full)
    {
        lineText.text = "";
        for (int i = 0; i < full.Length; i++)
        {
            lineText.text += full[i];
            yield return new WaitForSeconds(Mathf.Max(0.001f, typeSpeed));
        }
        StartAutoHide();
    }

    void StartAutoHide()
    {
        if (autoHideSeconds <= 0f) return;
        _autoHide = StartCoroutine(AutoHideRoutine());
    }

    IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSeconds(autoHideSeconds);
        Hide();
    }

    // ---------- Static helpers (กัน null) ----------

    public static void InstanceSafeShow(string speaker, string line)
    {
        if (Instance == null)
        {
            var ui = FindObjectOfType<DialogueUI>(true);
            if (ui == null) return;
            Instance = ui;
        }
        Instance.Show(speaker, line);
    }

    public static void InstanceSafeHide()
    {
        if (Instance == null)
        {
            var ui = FindObjectOfType<DialogueUI>(true);
            if (ui == null) return;
            Instance = ui;
        }
        Instance.Hide();
    }
}
