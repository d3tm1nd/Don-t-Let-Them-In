using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorToNight : MonoBehaviour, IInteractable
{
    [Header("Spawner Reference (ลาก NPCSpawner มา)")]
    public NPCSpawner spawner;

    [Header("Night Scene")]
    [Tooltip("ชื่อ Scene กลางคืน (ต้อง add ใน Build Settings)")]
    public string nightSceneName = "NightScene";

    [Header("Audio (optional)")]
    public AudioClip doorOpenSound;

    AudioSource audioSource;

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
            // 🔥 FIX: อย่าเข้าถึง acceptedCount โดยตรง (private) → คำนวณจาก public fields/methods
            // ถ้าต้องแสดงจำนวนเหลือ → ทำให้ acceptedCount เป็น public ใน NPCSpawner.cs ก่อน
            // ตอนนี้เปลี่ยน log เป็น general (หรือแก้ NPCSpawner ให้ public int acceptedCount)
            Debug.Log("ยังไม่ครบ! ต้องรับ NPC เข้าบ้านให้ครบก่อน");
            return;
        }

        // เล่นเสียง (optional)
        if (doorOpenSound != null)
            audioSource.PlayOneShot(doorOpenSound);

        Debug.Log($"🚪 สลับ Scene: {nightSceneName}");
        SceneManager.LoadScene(nightSceneName);
    }
}