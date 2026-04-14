using UnityEngine;

/// <summary>
/// Utility to get a human-readable reason why bed transition is blocked.
/// Optional: call from UI prompt.
/// </summary>
public static class BedBlockReason
{
    public static string GetReason()
    {
        if (PhaseManager.Instance == null) return "";
        var phase = PhaseManager.Instance.currentPhase;
        int day = Mathf.Max(1, PhaseManager.Instance.currentDay);

        if (phase == PhaseManager.GamePhase.Night)
        {
            if (ResourceManager.Instance == null) return "ResourceManager missing";
            if (!ResourceManager.Instance.HasOrderedTonight) return "You must order food before sleeping.";
        }

        if (phase == PhaseManager.GamePhase.Morning)
        {
            if (ResourceManager.Instance == null) return "ResourceManager missing";
            if (ResourceManager.Instance.DeliveryAppliedDay != day) return "You must receive the delivery first.";
            if (EnergyManager.Instance == null) return "EnergyManager missing";
            if (EnergyManager.Instance.Current != 0) return "You must spend all energy first.";
        }

        if (phase == PhaseManager.GamePhase.Event)
        {
            if (DailyNPCDecisionTracker.Instance == null) return "DecisionTracker missing";
            if (!DailyNPCDecisionTracker.Instance.IsCompletedForToday()) return "You must accept/reject all visitors for today.";
        }

        return "";
    }
}
