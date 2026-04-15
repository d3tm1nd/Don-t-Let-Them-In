using UnityEngine;

/// <summary>
/// STRICT bed completion rules.
/// IMPORTANT: If required managers are missing, this returns FALSE (blocks bed).
/// Phases: Morning / Day / Night (no Event)
/// </summary>
public class BedPhaseCompletionProvider : MonoBehaviour, ICompletionChecker
{
    public bool IsCompleted
    {
        get
        {
            // If PhaseManager missing -> block (so you notice the setup issue)
            if (PhaseManager.Instance == null)
            {
                Debug.LogError("❌ BedPhaseCompletionProvider: PhaseManager.Instance is NULL (bed will be blocked)");
                return false;
            }

            var phase = PhaseManager.Instance.currentPhase;
            int day = Mathf.Max(1, PhaseManager.Instance.currentDay);

            // Night: must order food
            if (phase == PhaseManager.GamePhase.Night)
            {
                if (ResourceManager.Instance == null)
                {
                    Debug.LogError("❌ BedPhaseCompletionProvider: ResourceManager missing (Night requirement)");
                    return false;
                }
                return ResourceManager.Instance.HasOrderedTonight;
            }

            // Morning: must receive delivery
            if (phase == PhaseManager.GamePhase.Morning)
            {
                if (ResourceManager.Instance == null)
                {
                    Debug.LogError("❌ BedPhaseCompletionProvider: ResourceManager missing (Morning requirement)");
                    return false;
                }
                return ResourceManager.Instance.DeliveryAppliedDay == day;
            }

            // Day: must decide all visitors
            if (phase == PhaseManager.GamePhase.Day)
            {
                if (DailyNPCDecisionTracker2Days.Instance == null)
                {
                    Debug.LogError("❌ BedPhaseCompletionProvider: DailyNPCDecisionTracker2Days missing (Day requirement)");
                    return false;
                }
                return DailyNPCDecisionTracker2Days.Instance.IsCompletedForToday();
            }

            // Unknown phase -> block
            Debug.LogError($"❌ BedPhaseCompletionProvider: Unknown phase '{phase}' (bed blocked)");
            return false;
        }
    }
}
