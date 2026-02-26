using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] lines;

    private int index = 0;
    private bool isTalking = false;

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI dialogueText;

    void Start()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        if (!isTalking) return;

        // คลิกซ้าย → ประโยคถัดไป
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    // เรียกตอนเริ่มคุย (จาก Player / Ray)
    public void StartDialogue()
    {
        if (isTalking) return;

        isTalking = true;
        index = 0;

        panel.SetActive(true);
        dialogueText.text = lines[index];

        FreezePlayer();
    }

    void NextLine()
    {
        index++;

        // หมดบทพูด → ปิด
        if (index >= lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = lines[index];
    }

    void EndDialogue()
    {
        isTalking = false;

        panel.SetActive(false);

        UnfreezePlayer();
    }

    void FreezePlayer()
    {
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UnfreezePlayer()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
