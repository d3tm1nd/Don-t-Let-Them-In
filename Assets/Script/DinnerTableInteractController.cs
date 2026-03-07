using UnityEngine;

// วางสคริปต์นี้แทนที่คอมโพเนนต์โต๊ะอาหารเดิมทั้งหมดที่ implements IInteractable
// รวมพฤติกรรม: กด E ครั้งแรก = เริ่มมื้อค่ำ (StartDinner)
//              กด E ระหว่างมื้อ/กดอีกครั้ง = ข้ามไปเช้า (CancelInvoke + LoadMorning)
[RequireComponent(typeof(Collider))]
public class DinnerTableInteractController : MonoBehaviour, IInteractable
{
    [Header("References")]
    public DinnerTableSystem dinnerSystem;

    [Header("Interact Layer Settings")]
    public string requiredLayerName = "interactable";
    public bool applyLayerToChildren = true;

    [Header("Rules")]
    [Tooltip("ให้กดได้เฉพาะตอนเฟสกลางคืน")]
    public bool onlyAtNight = true;
    [Tooltip("อนุญาตให้กด E อีกครั้งเพื่อข้ามไปเช้า")]
    public bool allowSkipToMorning = true;

    [Header("Audio (optional)")]
    public AudioClip startSound;
    public AudioClip skipSound;
    private AudioSource audioSource;

    // สถานะภายในของคอนโทรลเลอร์ (ไม่ขึ้นกับ DinnerTableSystem)
    private bool dinnerStarted = false;

    void Reset()
    {
        EnsureInteractLayer();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false; // ให้ Raycast โดนง่าย
        if (GetComponent<AudioSource>() == null) gameObject.AddComponent<AudioSource>();
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        EnsureInteractLayer();
        if (dinnerSystem == null)
            dinnerSystem = FindObjectOfType<DinnerTableSystem>(true);
    }

    private void EnsureInteractLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer == -1)
        {
            Debug.LogWarning($"⚠️ ไม่พบเลเยอร์ '{requiredLayerName}' — โปรดสร้างเลเยอร์นี้และเพิ่มใน LayerMask ของ InteractionRay");
            return;
        }
        if (applyLayerToChildren)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
        }
        else
        {
            gameObject.layer = layer;
        }
    }

    // เรียกจาก InteractionRay เมื่อกด E แล้วยิงโดนโต๊ะ
    public void Interact()
    {
        // ตรวจ Phase
        if (onlyAtNight && PhaseManager.Instance != null && PhaseManager.Instance.currentPhase != PhaseManager.GamePhase.Night)
        {
            Debug.Log("⏳ ยังไม่ใช่ช่วง Night — ยังไม่เริ่ม/ข้ามมื้อค่ำ");
            return;
        }

        if (dinnerSystem == null)
        {
            Debug.LogError("❌ DinnerTableInteractController: ไม่พบ DinnerTableSystem ในซีน");
            return;
        }

        if (!dinnerStarted)
        {
            // เริ่มมื้อค่ำ (สคริปต์โต๊ะจะจัดการสปอว์น NPC จาก NPCDataManager และจัดที่นั่ง)
            dinnerSystem.StartDinner();
            dinnerStarted = true;
            if (startSound != null) audioSource.PlayOneShot(startSound);
            Debug.Log("🍽️ StartDinner() โดยกด E ครั้งแรก");
        }
        else if (allowSkipToMorning)
        {
            // ข้ามไปเช้า: ยกเลิกตัวตั้งเวลา แล้วให้ PhaseManager ไป Morning
            dinnerSystem.CancelInvoke("EndDinner");
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.DinnerCompleted = true;
                PhaseManager.Instance.LoadMorning();
            }
            dinnerStarted = false; // reset สำหรับคืนถัดไป
            if (skipSound != null) audioSource.PlayOneShot(skipSound);
            Debug.Log("🌅 ข้ามมื้อค่ำไป Morning ด้วยการกด E ซ้ำ");
        }
        else
        {
            Debug.Log("ℹ️ มื้อค่ำกำลังดำเนินอยู่ — ปิดการข้าม (allowSkipToMorning=false)");
        }
    }
}
