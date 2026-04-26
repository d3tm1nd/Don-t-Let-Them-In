using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float gravity = -12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Footsteps")]
    [Tooltip("AudioSource สำหรับเล่นเสียงเท้า (ถ้าไม่ใส่ ระบบจะหา/สร้างให้เอง)")]
    public AudioSource footstepSource;

    [Tooltip("ใส่เสียงเท้าได้หลายไฟล์ ระบบจะสุ่มเล่น")]
    public AudioClip[] footstepClips;

    [Tooltip("จำนวนก้าวต่อวินาที เมื่อเดินเต็มสปีด (ยิ่งมาก เสียงยิ่งถี่)")]
    public float stepRate = 2.0f;

    [Tooltip("Pitch สุ่มเพื่อไม่ให้เสียงซ้ำ")]
    public Vector2 randomPitchRange = new Vector2(0.95f, 1.05f);

    [Range(0f, 1f)]
    public float footstepVolume = 0.9f;

    [Tooltip("0 = 2D, 1 = 3D (แนะนำ 1 ถ้าอยากให้เสียงมาจากตัวผู้เล่น)")]
    [Range(0f, 1f)]
    public float footstepSpatialBlend = 1f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector2 moveInput;

    public bool canMove = true;

    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetupFootstepSource();
    }

    void Update()
    {
        // ✅ ถ้าห้ามขยับ ให้หยุดอินพุตและไม่เล่นเสียงเท้า
        if (!canMove)
        {
            moveInput = Vector2.zero;
            stepTimer = 0f;
            GroundCheck();
            ApplyGravity();
            return;
        }

        ReadInput();
        GroundCheck();
        Move();
        ApplyGravity();
        HandleFootsteps();
    }

    void ReadInput()
    {
        // อ่านค่า WASD จาก Keyboard (Input System)
        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed) z -= 1f;
            if (Keyboard.current.wKey.isPressed) z += 1f;
        }

        moveInput = new Vector2(x, z).normalized;
    }

    void GroundCheck()
    {
        if (groundCheck == null) return;

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void Move()
    {
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ===================== Footsteps =====================

    void SetupFootstepSource()
    {
        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>();
            if (footstepSource == null)
                footstepSource = gameObject.AddComponent<AudioSource>();
        }

        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
        footstepSource.spatialBlend = footstepSpatialBlend;
    }

    void HandleFootsteps()
    {
        if (!isGrounded) { stepTimer = 0f; return; }
        if (moveInput.sqrMagnitude < 0.01f) { stepTimer = 0f; return; }
        if (footstepSource == null) return;
        if (footstepClips == null || footstepClips.Length == 0) return;

        // ปรับความถี่ตามความเร็วเดิน (แบบง่าย)
        float rate = Mathf.Max(0.5f, stepRate);
        float interval = 1f / rate;

        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            PlayFootstep();
        }
    }

    void PlayFootstep()
    {
        int index = Random.Range(0, footstepClips.Length);
        AudioClip clip = footstepClips[index];
        if (clip == null) return;

        footstepSource.spatialBlend = footstepSpatialBlend;
        footstepSource.pitch = Random.Range(randomPitchRange.x, randomPitchRange.y);
        footstepSource.PlayOneShot(clip, footstepVolume);
    }
}
