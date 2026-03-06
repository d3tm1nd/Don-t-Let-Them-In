using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorToNight : MonoBehaviour, IInteractable
{
    [Header("Spawner Reference (ลาก NPCSpawner มา)")]
    public NPCSpawner spawner;

    [Header("Audio (optional)")]
    public AudioClip doorOpenSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Interact()
    {
        if (spawner == null)
        {
            Debug.LogError("❌ ลาก NPCSpawner มาที่ DoorToNight!");
            return;
        }
        if (!spawner.AllNPCsAccepted)
        {
            Debug.Log("ยังไม่ครบ! ต้องรับ NPC เข้าบ้านให้ครบก่อน");
            return;
        }

        if (doorOpenSound != null) audioSource.PlayOneShot(doorOpenSound);

        if (PhaseManager.Instance == null)
        {
            Debug.LogError("❌ ไม่พบ PhaseManager ในซีน!");
            return;
        }

        Debug.Log("🚪 สลับ Scene ไป Night ผ่าน PhaseManager");
        PhaseManager.Instance.LoadNight();
    }
}
