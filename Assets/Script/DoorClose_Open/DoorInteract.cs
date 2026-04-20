using UnityEngine;

/// <summary>
/// ระบบกด E เปิด/ปิดประตู (ใช้กับ InteractionRay ที่เรียก IInteractable.Interact())
///
/// รองรับ 2 โหมด:
/// 1) Hinge Rotate: หมุนบานประตูด้วย pivot (ไม่ต้องมี Animator)
/// 2) Animator: สั่ง Animator bool/trigger เพื่อเล่นอนิเมชันเปิด-ปิด
///
/// ฟีเจอร์:
/// - Toggle เปิด/ปิด
/// - Lock/Unlock + เหตุผล
/// - ปิด/เปิด Collider กันเดินทะลุ (optional)
/// - เล่นเสียง (optional)
/// - Auto-close (optional)
///
/// แนะนำใช้งาน:
/// - ใส่สคริปต์นี้บน GameObject ที่ถูก Raycast โดน (เช่น ลูกบิด/ประตู/pivot)
/// - ถ้าใช้ Hinge Rotate: ตั้ง doorPivot ให้เป็น Transform ของบานที่หมุน
/// - ถ้าใช้ Animator: ใส่ animator และตั้ง parameter
/// </summary>
public class DoorInteract : MonoBehaviour, IInteractable 
{
    public enum DoorMode { HingeRotate, Animator }

    [Header("Mode")]
    public DoorMode mode = DoorMode.HingeRotate;

    [Header("State")]
    public bool isOpen = false;
    public bool isLocked = false;

    [Tooltip("ข้อความเหตุผลตอนล็อก เช่น \"ล็อกอยู่\" / \"ต้องใช้กุญแจ\"")]
    public string lockedReason = "ล็อกอยู่";

    [Header("Prompt")]
    public string promptOpenText = "E: เปิดประตู";
    public string promptCloseText = "E: ปิดประตู";

    // -------- Hinge Rotate --------
    [Header("Hinge Rotate Settings")]
    [Tooltip("Transform ของบานประตูที่หมุน (ถ้าว่างจะใช้ transform นี้)")]
    public Transform doorPivot;

    [Tooltip("มุมเปิด (องศา) เมื่อเปิดประตู")]
    public float openYawAngle = 90f;

    [Tooltip("ความเร็วการหมุน (องศาต่อวินาที)")]
    public float rotateSpeed = 240f;

    [Tooltip("ให้เด้งกลับไปมุมเดิมเมื่อปิด")]
    public bool closeToDefaultRotation = true;

    // -------- Animator --------
    [Header("Animator Settings")]
    public Animator animator;

    [Tooltip("ชนิดพารามิเตอร์ใน Animator")]
    public bool useBoolParameter = true;

    [Tooltip("ชื่อ Bool หรือ Trigger")]
    public string animatorParam = "Open";

    // -------- Collision --------
    [Header("Collision (optional)")]
    [Tooltip("Collider ของบานประตู (ถ้าใส่ จะ enable/disable ตามเปิด-ปิด)")]
    public Collider doorCollider;

    [Tooltip("ถ้า true: เปิดประตูแล้วปิด collider (ให้เดินผ่านได้) / ปิดแล้วเปิด collider")]
    public bool disableColliderWhenOpen = false;

    // -------- Audio --------
    [Header("Audio (optional)")]
    public AudioSource audioSource;
    public AudioClip openSfx;
    public AudioClip closeSfx;
    public AudioClip lockedSfx;

    // -------- Auto Close --------
    [Header("Auto Close (optional)")]
    public bool autoClose = false;
    public float autoCloseDelay = 3f;

    // internal
    private Quaternion _closedRot;
    private Quaternion _openRot;
    private float _autoCloseAt = -1f;

    void Awake()
    {
        if (doorPivot == null) doorPivot = transform;

        // จำมุมปิด (เริ่มต้น)
        _closedRot = doorPivot.localRotation;
        _openRot = _closedRot * Quaternion.Euler(0f, openYawAngle, 0f);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        ApplyColliderState();
        ApplyAnimatorStateInstant();
    }

    void Update()
    {
        // Hinge rotate smoothing
        if (mode == DoorMode.HingeRotate)
        {
            Quaternion target = isOpen ? _openRot : _closedRot;
            doorPivot.localRotation = Quaternion.RotateTowards(doorPivot.localRotation, target, rotateSpeed * Time.deltaTime);
        }

        // Auto close
        if (autoClose && isOpen && _autoCloseAt > 0f && Time.time >= _autoCloseAt)
        {
            _autoCloseAt = -1f;
            Close();
        }
    }

    // ===================== IInteractable =====================

    public void Interact()
    {
        if (isLocked)
        {
            Play(lockedSfx);
            return;
        }

        Toggle();
    }

    // ===================== IInteractablePrompt =====================

    public string GetPromptText()
    {
        return isOpen ? promptCloseText : promptOpenText;
    }

    public bool CanInteract(out string reason)
    {
        if (isLocked)
        {
            reason = string.IsNullOrEmpty(lockedReason) ? "ล็อกอยู่" : lockedReason;
            return false;
        }

        reason = "";
        return true;
    }

    // ===================== Public API =====================

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (isLocked) { Play(lockedSfx); return; }

        isOpen = true;
        ApplyAnimatorState();
        ApplyColliderState();
        Play(openSfx);

        if (autoClose)
            _autoCloseAt = Time.time + Mathf.Max(0.1f, autoCloseDelay);
    }

    public void Close()
    {
        isOpen = false;
        ApplyAnimatorState();
        ApplyColliderState();
        Play(closeSfx);
    }

    public void SetLocked(bool locked, string reason = null)
    {
        isLocked = locked;
        if (!string.IsNullOrEmpty(reason)) lockedReason = reason;
    }

    // ===================== Helpers =====================

    private void ApplyAnimatorState()
    {
        if (mode != DoorMode.Animator) return;
        if (animator == null) return;

        if (useBoolParameter)
        {
            animator.SetBool(animatorParam, isOpen);
        }
        else
        {
            // trigger toggle: ยิง trigger เปิด/ปิดคนละชื่อ
            animator.SetTrigger(animatorParam);
        }
    }

    private void ApplyAnimatorStateInstant()
    {
        if (mode != DoorMode.Animator) return;
        if (animator == null) return;

        if (useBoolParameter)
        {
            animator.SetBool(animatorParam, isOpen);
        }
    }

    private void ApplyColliderState()
    {
        if (doorCollider == null) return;

        if (!disableColliderWhenOpen)
        {
            // collider ตลอด
            doorCollider.enabled = true;
            return;
        }

        // เปิดแล้วปิด collider, ปิดแล้วเปิด collider
        doorCollider.enabled = !isOpen;
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
}
