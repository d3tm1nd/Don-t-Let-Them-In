using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DinnerTableToMorning_v3 : MonoBehaviour, IInteractable
{
    [Header("Interact Layer Settings")]
    [Tooltip("ชื่อเลเยอร์ที่ระบบ InteractionRay ใช้ยิง Ray")] 
    public string requiredLayerName = "interactable";
    [Tooltip("ถ้าเปิด จะตั้งเลเยอร์ให้ลูกทั้งหมดด้วย")] 
    public bool applyLayerToChildren = true;

    [Header("Options")]
    [Tooltip("จำกัดให้ใช้ได้เฉพาะตอน Night phase เท่านั้น")] 
    public bool onlyInNightPhase = true;
    [Tooltip("กันการกดซ้ำหลายครั้ง")] 
    public bool interactOnce = true;

    [Header("Audio (optional)")]
    public AudioClip confirmSound;
    private AudioSource audioSource;

    private bool _used = false;

    void Reset()
    {
        EnsureInteractLayer();
        EnsureColliderSetup();
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        EnsureInteractLayer();
    }

    private void EnsureColliderSetup()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;
        // ระบบ Raycast ส่วนใหญ่ทำงานกับทั้ง Trigger/Non-Trigger ได้ แต่ส่วนมากประตูใช้ Non-Trigger
        // เพื่อความเสถียร ให้ตั้งเป็น non-trigger เหมือนประตู (ถ้าต้องการ Trigger ให้ปรับเองได้)
        col.isTrigger = false; 
    }

    private void EnsureInteractLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer == -1)
        {
            Debug.LogWarning($"⚠️ ไม่พบเลเยอร์ '{requiredLayerName}' ใน Project Settings > Tags and Layers — โปรดสร้างเลเยอร์นี้ แล้วตั้งค่า LayerMask ของ InteractionRay ให้ครอบคลุม");
            return;
        }

        if (applyLayerToChildren)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = layer;
            }
        }
        else
        {
            gameObject.layer = layer;
        }
    }

    public void Interact()
    {
        if (_used && interactOnce) return;

        if (PhaseManager.Instance == null)
        {
            Debug.LogError("❌ ไม่พบ PhaseManager ในซีน");
            return;
        }

        if (onlyInNightPhase && PhaseManager.Instance.currentPhase != PhaseManager.GamePhase.Night)
        {
            Debug.Log("⏳ ยังไม่ใช่ช่วง Night — ยังไม่ข้ามไป Morning");
            return;
        }

        _used = true;
        if (confirmSound != null) audioSource.PlayOneShot(confirmSound);

        // ยกเลิกตัวตั้งเวลาจากโต๊ะ (ถ้ามี) เพื่อป้องกันโหลดซ้ำ
        var dinnerSystem = FindObjectOfType<DinnerTableSystem>();
        if (dinnerSystem != null)
        {
            dinnerSystem.CancelInvoke("EndDinner");
        }

        PhaseManager.Instance.DinnerCompleted = true;
        PhaseManager.Instance.LoadMorning();
    }
}
