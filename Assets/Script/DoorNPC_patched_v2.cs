using UnityEngine;

public class DoorNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public string dialogueText = "ขอเข้าไปหน่อย";
    public string insultText = "ใจร้ายจริง ๆ!";

    [Header("Spawn Inside NPC")]
    public NPCSpawner spawner;

    [Header("Inside Prefab Override")]
    [Tooltip("Prefab Inside ที่ spawn เมื่อรับเข้า")]
    public GameObject insidePrefab; // Override สำหรับแต่ละตัว

    [Header("Audio")]
    public AudioClip doorOpenSound;
    AudioSource audioSource;

    [Header("Decision Tracking (2-day rules)")]
    [Tooltip("ถ้าเปิด: นับการตัดสินใจ (Accept/Reject) ให้ระบบ 2 วัน")]
    public bool trackDecisionFor2DayRules = true;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Interact()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsTalking) return;
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
            // Spawn inside + record is handled INSIDE NPCSpawner.SpawnInsideNPC via RecordNPCData()
            // (Do NOT double-add to NPCDataManager here)
            spawner.StartCoroutine(spawner.SpawnInsideNPC(insidePrefab));

            // Spawn next outside NPC so the day continues
            spawner.StartCoroutine(spawner.SpawnNewOutsideNPC());

            // Keep your original accepted counter if you still want it
            spawner.OnNPCAccepted();
        }

        // Count decision for 2-day loop (Accept)
        if (trackDecisionFor2DayRules && DailyNPCDecisionTracker2Days.Instance != null)
            DailyNPCDecisionTracker2Days.Instance.RegisterDecision();

        Destroy(gameObject, 1.5f);
    }

    // ================= NO =================
    public void OnNo()
    {
        Debug.Log("NO → NPC Angry");
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(insultText, null);

        // Spawn next outside NPC so the day continues
        if (spawner != null)
            spawner.StartCoroutine(spawner.SpawnNewOutsideNPC());

        // Count decision for 2-day loop (Reject)
        if (trackDecisionFor2DayRules && DailyNPCDecisionTracker2Days.Instance != null)
            DailyNPCDecisionTracker2Days.Instance.RegisterDecision();

        Destroy(gameObject, 1.5f);
    }
}
