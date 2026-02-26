using UnityEngine;

public class ResourceDoorNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public string dialogueText = "เอาของกินหน่อยมั้ย?";
    public string insultText = "ใจร้ายจริง ๆ!";

    [Header("Item to Offer")]
    [Tooltip("Item ที่นำเสนอ (ลาก ScriptableObject มา)")]
    public ResourceItem offeredItem;

    [Header("Spawner Reference")]
    public ResourceNPCSpawner spawner;

    [Header("Audio")]
    public AudioClip doorSound;

    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Interact()
    {
        if (ResourceDialogueManager.Instance.IsTalking) return;  // 🔥 เปลี่ยนเป็น ResourceDialogueManager

        // แสดงชื่อ item ใน dialogue
        string fullText = $"{dialogueText} ({offeredItem?.itemName ?? "ไม่ระบุ"})";

        ResourceDialogueManager.Instance.StartDialogue(fullText, this);  // 🔥 เปลี่ยนเป็น ResourceDialogueManager
    }

    // ================= YES: ได้ item =================
    public void OnYes()
    {
        Debug.Log("YES → ได้ของกิน");

        if (doorSound != null)
            audioSource.PlayOneShot(doorSound);

        if (offeredItem != null && ResourceInventoryManager.Instance != null)
        {
            ResourceInventoryManager.Instance.AddItem(offeredItem);
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่มี Item หรือ Inventory!");
        }

        if (spawner != null)
        {
            spawner.StartCoroutine(spawner.SpawnNewOutsideNPC());
        }

        Destroy(gameObject, 1.5f);
    }

    // ================= NO =================
    public void OnNo()
    {
        Debug.Log("NO → NPC Angry");

        ResourceDialogueManager.Instance.StartDialogue(insultText, null);  // 🔥 เปลี่ยนเป็น ResourceDialogueManager

        Destroy(gameObject, 1.5f);
    }
}