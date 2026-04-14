using UnityEngine;

// นับจำนวนครั้งที่ผู้เล่น "ตัดสินใจ" (Accept/Reject) ในวันปัจจุบัน
// ให้ครบจำนวนผู้มาเยือนของวันนั้น (Day1=4, Day2=3)
public class DailyNPCDecisionTracker2Days : MonoBehaviour
{
    public static DailyNPCDecisionTracker2Days Instance { get; private set; }

    [Header("Rules")]
    public TwoDayRules rules;

    [Header("Runtime")]
    [SerializeField] private int decisionsMade = 0;
    [SerializeField] private int requiredToday = 0;
    [SerializeField] private int dayStamp = -1;

    public int DecisionsMade => decisionsMade;
    public int RequiredToday => requiredToday;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        if (dayStamp != day)
        {
            dayStamp = day;
            decisionsMade = 0;
            requiredToday = (rules != null) ? rules.GetRequiredDecisionsForDay(day) : 0;
        }
    }

    // เรียกจากปุ่ม Accept/Reject ทุกครั้งที่ผู้เล่นตัดสินใจ
    public void RegisterDecision(int count = 1)
    {
        if (count <= 0) return;
        decisionsMade += count;
        Debug.Log($"🧾 Decisions: {decisionsMade}/{requiredToday}");
    }

    public bool IsCompletedForToday()
    {
        return requiredToday > 0 && decisionsMade >= requiredToday;
    }
}
