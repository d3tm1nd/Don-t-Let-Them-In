using UnityEngine;

/// <summary>
/// Implements ICompletionChecker for BedInteractSceneFlow.
/// Rules:
/// 1) Night: must order food at least once (ResourceManager.HasOrderedTonight)
/// 2) Morning: must have received delivery (ResourceManager.DeliveryAppliedDay == currentDay) AND spend all energy (EnergyManager.Current == 0)
/// 3) Day/Event: must accept/reject NPC until required count (DailyNPCDecisionTracker.IsCompletedForToday)
/// </summary>
public class BedPhaseCompletionProvider : MonoBehaviour, ICompletionChecker
{
    public bool IsCompleted
    {
        get
        {
            if (PhaseManager.Instance == null) return true;

            var phase = PhaseManager.Instance.currentPhase;
            int day = Mathf.Max(1, PhaseManager.Instance.currentDay);

            // Night -> must order food
            if (phase == PhaseManager.GamePhase.Night)
            {
                return ResourceManager.Instance != null && ResourceManager.Instance.HasOrderedTonight;
            }

            // Morning -> must receive delivery + spend all energy
            if (phase == PhaseManager.GamePhase.Morning)
            {
                bool deliveryOk = ResourceManager.Instance != null && ResourceManager.Instance.DeliveryAppliedDay == day;
                bool energyOk = EnergyManager.Instance != null && EnergyManager.Instance.Current == 0;
                return deliveryOk && energyOk;
            }

            // Day/Event -> must process NPC decisions
            if (phase == PhaseManager.GamePhase.Event)
            {
                return DailyNPCDecisionTracker.Instance != null && DailyNPCDecisionTracker.Instance.IsCompletedForToday();
            }

            // default allow
            return true;
        }
    }
}
