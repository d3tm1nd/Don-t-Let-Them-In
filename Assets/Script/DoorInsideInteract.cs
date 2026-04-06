using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteract : MonoBehaviour, IInteractable
{
    [Header("Layer / Detection")]
    [Tooltip("ต้องตรงกับ LayerMask ของ InteractionRay")]
    public string requiredLayerName = "interactable";

    [Header("Animation (optional)")]
    [Tooltip("Animator ของประตู (ถ้าไม่มี จะเปิด/ปิดด้วย Collider อย่างเดียว)")]
    public Animator animator;
    [Tooltip("ชื่อพารามิเตอร์ใน Animator สำหรับเปิด/ปิด")]
    public string animatorBoolParam = "Open";

    [Header("Colliders (optional)")]
    [Tooltip("Collider ที่เป็นบานประตู (ถ้าอยากปิด/เปิดการชนเมื่อเปิดประตู)")]
    public Collider doorLeafCollider;

    [Header("Sounds (optional)")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    AudioSource _audio;

    [Header("Rules")]
    public bool startOpened = false;        // เริ่มเกมเปิดอยู่ไหม
    public bool isLocked = false;           // ล็อกไหม
    public bool disableLeafColliderWhenOpen = true; // เปิดแล้วให้เดินทะลุได้
    [Tooltip("ตั้ง > 0 เพื่อตั้งเวลาปิดเอง (วินาที)")]
    public float autoCloseSeconds = 0f;     // 0 = ไม่ปิดเอง
    [Tooltip("กันสแปมกด")]
    public float interactCooldown = 0.25f;

    bool _isOpen;
    float _nextTime;

    void Reset()
    {
        EnsureLayer();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false; // ให้ InteractionRay โดนง่าย
        if (GetComponent<AudioSource>() == null) gameObject.AddComponent<AudioSource>();
    }

    void Awake()
    {
        EnsureLayer();
        _audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // ตั้งค่าตอนเริ่ม
        SetOpenInstant(startOpened, playAudio: false);
    }

    void EnsureLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer != -1) gameObject.layer = layer;
    }

    public void Interact()
    {
        if (Time.time < _nextTime) return; // กันสแปม
        _nextTime = Time.time + interactCooldown;

        if (isLocked)
        {
            Play(lockedSound);
            return;
        }

        if (_isOpen) Close(); else Open();
    }

    public void Open()
    {
        SetOpenInstant(true, playAudio: true);
        // ตั้งเวลาปิดเอง (ถ้ามี)
        if (autoCloseSeconds > 0f)
        {
            CancelInvoke(nameof(Close));
            Invoke(nameof(Close), autoCloseSeconds);
        }
    }

    public void Close()
    {
        SetOpenInstant(false, playAudio: true);
    }

    public void Lock() { isLocked = true; }
    public void Unlock() { isLocked = false; }

    void SetOpenInstant(bool open, bool playAudio)
    {
        _isOpen = open;

        // Animator
        if (animator != null && !string.IsNullOrEmpty(animatorBoolParam))
        {
            animator.SetBool(animatorBoolParam, _isOpen);
        }

        // Collider ของบานประตู
        if (doorLeafCollider != null && disableLeafColliderWhenOpen)
        {
            doorLeafCollider.enabled = !_isOpen;
        }

        // เสียง
        if (playAudio)
        {
            Play(_isOpen ? openSound : closeSound);
        }
    }

    void Play(AudioClip clip)
    {
        if (clip == null || _audio == null) return;
        _audio.PlayOneShot(clip);
    }
}