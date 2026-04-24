using UnityEngine;

/// <summary>
/// NPCPhaseAnimator
/// เปลี่ยนแอนิเมชันของ NPC ตาม Phase (Morning/Day/Night) แบบ "ไม่สุ่ม"
///
/// วิธีทำงาน:
/// - อ่าน PhaseManager.Instance.currentPhase ทุกเฟรม (หรือเมื่อเปลี่ยนค่า)
/// - ถ้า Phase เปลี่ยน → สั่ง Animator ให้เล่น state ที่กำหนดไว้ใน Inspector
///
/// ใช้ได้ดีเมื่อ:
/// - คุณมี Animator Controller เดียว และมี state แยกชื่อไว้ (เช่น "Idle_Morning", "Idle_Day", "Idle_Night")
///
/// ข้อแนะนำ (สำหรับ Artist):
/// - ทำ state ใน Animator Controller ให้ครบ 3 state
/// - ปิด Loop/ตั้ง Transition ตามต้องการใน Animator
/// </summary>
[DisallowMultipleComponent]
public class NPCPhaseAnimator : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Tooltip("ถ้าเว้นว่าง จะพยายามหา Animator จากตัวเอง/ลูก")]
    public bool autoFindAnimator = true;

    [Header("State Names (Animator States)")]
    [Tooltip("ชื่อ state ใน Animator Controller สำหรับ Morning")]
    public string morningState = "Idle_Morning";

    [Tooltip("ชื่อ state ใน Animator Controller สำหรับ Day")]
    public string dayState = "Idle_Day";

    [Tooltip("ชื่อ state ใน Animator Controller สำหรับ Night")]
    public string nightState = "Idle_Night";

    [Header("Play Settings")]
    [Tooltip("Crossfade ช่วยให้เปลี่ยนท่าลื่นขึ้น")]
    public bool useCrossFade = true;

    [Tooltip("ระยะเวลา crossfade (วินาที)")]
    public float crossFadeTime = 0.15f;

    [Tooltip("เล่นที่ layer ไหนของ Animator")]
    public int layer = 0;

    [Tooltip("ถ้า true จะเล่น state อีกครั้งแม้ชื่อเดิม (ใช้กรณีอยากรีสตาร์ทคลิป)")]
    public bool replaySameStateOnPhaseChange = false;

    private PhaseManager.GamePhase _lastPhase = (PhaseManager.GamePhase)(-1);
    private string _lastPlayedState = "";

    void Awake()
    {
        if (animator == null && autoFindAnimator)
            animator = GetComponentInChildren<Animator>(true);
    }

    void Start()
    {
        ApplyPhaseAnimation(force: true);
    }

    void Update()
    {
        ApplyPhaseAnimation(force: false);
    }

    private void ApplyPhaseAnimation(bool force)
    {
        if (PhaseManager.Instance == null) return;
        if (animator == null) return;

        var phase = PhaseManager.Instance.currentPhase;
        if (!force && phase == _lastPhase) return;

        _lastPhase = phase;

        string targetState = GetStateNameForPhase(phase);
        if (string.IsNullOrEmpty(targetState)) return;

        if (!replaySameStateOnPhaseChange && targetState == _lastPlayedState)
            return;

        _lastPlayedState = targetState;

        if (useCrossFade)
            animator.CrossFadeInFixedTime(targetState, Mathf.Max(0f, crossFadeTime), layer);
        else
            animator.Play(targetState, layer, 0f);
    }

    private string GetStateNameForPhase(PhaseManager.GamePhase phase)
    {
        switch (phase)
        {
            case PhaseManager.GamePhase.Morning: return morningState;
            case PhaseManager.GamePhase.Day: return dayState;
            case PhaseManager.GamePhase.Night: return nightState;
        }
        return dayState;
    }
}
