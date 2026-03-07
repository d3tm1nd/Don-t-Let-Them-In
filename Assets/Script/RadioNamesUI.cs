using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Text;

public class RadioNamesUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject rootPanel;          // inactive at start
    public TextMeshProUGUI textOutput;    // TMP text to show names

    [Header("Close Behavior")]
    public bool closeOnEsc = true;        // per requirement: Esc to close only

    private bool isOpen = false;

    void Awake()
    {
        if (rootPanel != null) rootPanel.SetActive(false);
    }

    public void ShowNames(List<string> names)
    {
        if (rootPanel == null || textOutput == null)
        {
            Debug.LogError("❌ RadioNamesUI: rootPanel/textOutput is not assigned.");
            return;
        }

        if (names == null || names.Count == 0)
        {
            textOutput.text = "(ยังไม่มีรายชื่อ NPC)";
        }
        else
        {
            var sb = new StringBuilder();
            for (int i = 0; i < names.Count; i++)
            {
                sb.AppendLine($"• {names[i]}");
            }
            textOutput.text = sb.ToString();
        }

        Toggle(true);
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
