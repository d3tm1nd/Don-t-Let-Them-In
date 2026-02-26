using UnityEngine;

public class DoorNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public string dialogueText = "ขอเข้าไปหน่อย";
    public string insultText = "ใจร้ายจริง ๆ!";

    [Header("Spawn Inside NPC")]
    public NPCSpawner spawner;

    [Header("Inside Prefab Override")]
    [Tooltip("Prefab Inside ที่ spawn เมื่อรับเข้า (ตัวใหม่จะใช้ newInsideNPCPrefab)")]
    public GameObject insidePrefab;  // Override สำหรับแต่ละตัว

    [Header("Audio")]
    public AudioClip doorOpenSound;

    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Interact()
    {
        if (DialogueManager.Instance.IsTalking) return;

        DialogueManager.Instance.StartDialogue(dialogueText, this);
    }

    // ================= YES =================
    public void OnYes()
    {
        Debug.Log("YES → Open Door");

        if (doorOpenSound != null)
            audioSource.PlayOneShot(doorOpenSound);

        if (spawner != null)
        {
            spawner.StartCoroutine(spawner.SpawnInsideNPC(insidePrefab));
            spawner.StartCoroutine(spawner.SpawnNewOutsideNPC());

            // 🔥 NEW: นับรับเข้า
            spawner.OnNPCAccepted();
        }

        Destroy(gameObject, 1.5f);
    }

    // ================= NO =================
    public void OnNo()
    {
        Debug.Log("NO → NPC Angry");

        DialogueManager.Instance.StartDialogue(insultText, null);

        Destroy(gameObject, 1.5f);
    }
}