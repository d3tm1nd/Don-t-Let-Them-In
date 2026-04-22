using UnityEngine;
using TMPro;

/// <summary>
/// SimpleDialogueUI
/// - UI สำหรับแสดงบทสนทนาแบบเรียงลำดับ
/// - ไม่ใช้การคลิกปุ่ม UI (ไม่ต้องปล่อยเมาส์) แต่ให้ DialogueManager คุมด้วยคลิกซ้าย/ESC
/// </summary>
public class SimpleDialogueUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI lineText;

    [Header("Optional")]
    [Tooltip("ถ้าเปิด จะโชว์ hint เช่น 'คลิกซ้าย: ต่อไป | ESC: ออก'")]
    public TextMeshProUGUI hintText;

    [TextArea(1, 3)]
    public string hint = "คลิกซ้าย: ต่อไป  |  ESC: ออก";

    void Awake()
    {
        if (root == null) root = gameObject;
        Hide();
    }

    public void Show(string speaker, string line)
    {
        if (root != null) root.SetActive(true);

        if (speakerText != null)
            speakerText.text = string.IsNullOrEmpty(speaker) ? "" : speaker;

        if (lineText != null)
            lineText.text = line ?? "";

        if (hintText != null)
        {
            hintText.gameObject.SetActive(true);
            hintText.text = hint;
        }
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
