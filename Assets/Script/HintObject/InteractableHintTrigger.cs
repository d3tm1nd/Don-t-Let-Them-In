using UnityEngine;

/// <summary>
/// InteractableHintTrigger
/// - ใช้ Trigger Collider ตรวจเมื่อ Player "เดินผ่าน" object
/// - เมื่อเข้า trigger ครั้งแรก → ขอให้ HintUI โชว์ข้อความ 4 วินาที
/// - ครั้งต่อไป → ไม่โชว์อีก
///
/// วิธีใช้:
/// - ใส่บน object เดียวกับ InteractableHintSource หรือ child ที่มี Trigger Collider
/// - Collider ต้องติ๊ก IsTrigger
/// - Player ควรมี Rigidbody (isKinematic=true) หรือ collider ที่ทำให้ trigger event ทำงาน
/// </summary>
[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class InteractableHintTrigger : MonoBehaviour
{
    [Header("Detect")]
    public string playerTag = "Player";

    [Tooltip("ถ้าเว้นว่าง จะหา InteractableHintSource จาก parent")]
    public InteractableHintSource hintSource;

    [Tooltip("ถ้า true: จะโชว์เมื่อ OnTriggerEnter เท่านั้น | ถ้า false: สามารถใช้ OnTriggerStay ด้วย (ไม่แนะนำ)")]
    public bool onlyOnEnter = true;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (hintSource == null)
            hintSource = GetComponentInParent<InteractableHintSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hintSource == null) return;
        if (!other.CompareTag(playerTag)) return;

        TryShow();
    }

    private void OnTriggerStay(Collider other)
    {
        if (onlyOnEnter) return;
        if (hintSource == null) return;
        if (!other.CompareTag(playerTag)) return;

        TryShow();
    }

    private void TryShow()
    {
        // กันโชว์ซ้ำ
        if (HintSeenRegistry.HasSeen(hintSource.hintId))
            return;

        if (!hintSource.CanShowNow())
            return;

        // แสดง
        if (InteractableHintUI.Instance != null)
            InteractableHintUI.Instance.ShowHint(hintSource.hintText, hintSource.showSeconds);

        // mark seen
        HintSeenRegistry.MarkSeen(hintSource.hintId, hintSource.persistAcrossSessions);
    }
}
