using UnityEngine;
using TMPro;

public class ResourceInsideNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [TextArea(3, 10)]
    public string[] dialogueLines = new string[]
    {
        "ขอบคุณที่รับฉันเข้ามา...",
        "ที่นี่ปลอดภัยจริง ๆ ...",
        "พรุ่งนี้เจอกันใหม่นะ"
    };

    [Header("Stranger Flag")]
    public bool isStranger = false;

    [Header("UI")]
    public GameObject dialoguePanel;           // ลาก Pause Panel หรือ Dialogue Panel มา
    public TextMeshProUGUI dialogueText;

    private int index = 0;
    private bool isTalking = false;

    public void Interact()
    {
        if (isTalking || ResourceDialogueManager.Instance.IsTalking) return;
        StartDialogue();
    }

    private void StartDialogue()
    {
        isTalking = true;
        index = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogueLines[0];
        ResourceDialogueManager.Instance.Freeze();   // หยุดเวลา + แสดง Cursor
    }

    void Update()
    {
        if (!isTalking) return;

        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            index++;
            if (index >= dialogueLines.Length)
            {
                EndDialogue();
            }
            else
            {
                dialogueText.text = dialogueLines[index];
            }
        }
    }

    private void EndDialogue()
    {
        isTalking = false;
        dialoguePanel.SetActive(false);
        ResourceDialogueManager.Instance.Unfreeze();
        Debug.Log($"✅ คุยกับ {gameObject.name} เสร็จแล้ว");
    }
}