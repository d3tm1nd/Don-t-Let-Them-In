using UnityEngine;

/// <summary>
/// Tracks how many NPC decisions (Accept/Reject) the player has made in the current day.
/// Hook this from your Accept/Reject buttons by calling RegisterDecision().
/// </summary>
public class DailyNPCDecisionTracker : MonoBehaviour
{
    public static DailyNPCDecisionTracker Instance { get; private set; }

    [Header("Config")]
    public int requiredDecisionsPerDay = 2;

    [Header("Runtime")]
    [SerializeField] private int decisionsMade = 0;
    [SerializeField] private int dayStamp = -1;

    public int DecisionsMade => decisionsMade;

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
        // Auto-reset when day changes
        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        if (dayStamp != day)
        {
            dayStamp = day;
            decisionsMade = 0;
        }
    }

    /// <summary>Call this when player Accepts or Rejects an NPC.</summary>
    public void RegisterDecision(int count = 1)
    {
        if (count <= 0) return;
        decisionsMade += count;
        Debug.Log($"🧾 NPC Decision registered. {decisionsMade}/{requiredDecisionsPerDay}");
    }

    public bool IsCompletedForToday()
    {
        return decisionsMade >= requiredDecisionsPerDay;
    }
}
