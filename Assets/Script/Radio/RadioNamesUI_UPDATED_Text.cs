using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// RadioNamesUI (UPDATED to generic text UI)
/// - เดิมใช้โชว์รายชื่อ NPC
/// - ตอนนี้ปรับให้โชว์ "ข้อความธรรมดา" ได้ (เนื้อเรื่อง/ประกาศ)
/// - ปิดด้วย ESC (เหมือนเดิม)
///
/// วิธีใช้:
/// - เรียก ShowText(title, body) จาก RadioInteract_ShowNames (เวอร์ชันใหม่)
/// </summary>
public class RadioNamesUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject rootPanel; // inactive at start
    public TextMeshProUGUI textOutput; // TMP text to show body

    [Header("Optional Title")]
    public TextMeshProUGUI titleOutput; // optional

    [Header("Hint (optional)")]
    public TextMeshProUGUI hintOutput; // optional
    [TextArea(1, 3)]
    public string hint = "ESC: ปิด";

    [Header("Close Behavior")]
    public bool closeOnEsc = true; // Esc to close only

    private bool isOpen = false;

    void Awake()
    {
        if (rootPanel != null) rootPanel.SetActive(false);
    }

    /// <summary>
    /// โหมดใหม่: โชว์ข้อความธรรมดา
    /// </summary>
    public void ShowText(string title, string body)
    {
        if (rootPanel == null || textOutput == null)
        {
            Debug.LogError("❌ RadioNamesUI: rootPanel/textOutput is not assigned.");
            return;
        }

        if (titleOutput != null)
            titleOutput.text = string.IsNullOrEmpty(title) ? "" : title;

        textOutput.text = string.IsNullOrEmpty(body) ? "(ไม่มีข้อความ)" : body;

        if (hintOutput != null)
        {
            hintOutput.gameObject.SetActive(true);
            hintOutput.text = hint;
        }

        Toggle(true);
    }

    /// <summary>
    /// Backward compatible: เดิมเรียก ShowNames(List<string>)
    /// ตอนนี้ยังรองรับ โดยจะแปลง list เป็นข้อความหลายบรรทัด
    /// </summary>
    public void ShowNames(System.Collections.Generic.List<string> names)
    {
        if (names == null || names.Count == 0)
        {
            ShowText("", "(ไม่มีข้อความ)");
            return;
        }
        ShowText("", string.Join("\n", names));
    }

    void Update()
    {
        if (!isOpen) return;
        if (closeOnEsc && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Toggle(false);
        }
    }

    private void Toggle(bool show)
    {
        isOpen = show;
        if (rootPanel != null) rootPanel.SetActive(show);
    }
}
