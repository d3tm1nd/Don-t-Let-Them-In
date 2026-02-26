using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject panel;
    public TextMeshProUGUI dialogueText;
    public Button yesButton;
    public Button noButton;

    private DoorNPC currentNPC;

    public bool IsTalking { get; private set; }

    void Awake()
    {
        Instance = this;

        panel.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }

    public void StartDialogue(string text, DoorNPC npc)
    {
        if (IsTalking) return;

        IsTalking = true;
        currentNPC = npc;

        panel.SetActive(true);
        dialogueText.text = text;

        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(Yes);
        noButton.onClick.AddListener(No);

        Freeze();
    }

    void Yes()
    {
        currentNPC?.OnYes();
        End();
    }

    void No()
    {
        StartCoroutine(NoRoutine());
    }

    IEnumerator NoRoutine()
    {
        // ปิดปุ่ม
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        // ให้ NPC ด่า
        if (currentNPC != null)
            dialogueText.text = currentNPC.insultText;

        yield return new WaitForSecondsRealtime(2f);

        // ลบ NPC
        currentNPC?.OnNo();

        End();
    }

    void End()
    {
        IsTalking = false;
        currentNPC = null;

        panel.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        Unfreeze();
    }

    void Freeze()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Unfreeze()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowSingleText(string text)
    {
        Freeze();

        panel.SetActive(true);
        dialogueText.text = text;

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        Invoke(nameof(HideSingleText), 2f);
    }

    void HideSingleText()
    {
        panel.SetActive(false);
        Unfreeze();
    }

}
