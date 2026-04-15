using UnityEngine;

/// <summary>
/// Builds English prompt text for the bed transition rules.
/// Uses current Phase + your rule set:
/// - Night: must order food (at least once)
/// - Morning: must receive delivery AND spend all energy (Energy=0)
/// - Day/Event: must accept/reject visitors until required count
/// </summary>
public static class BedPromptText
{
    public static string GetPrompt(bool canInteract)
    {
        if (PhaseManager.Instance == null)
            return canInteract ? "Press E" : "";

        var phase = PhaseManager.Instance.currentPhase;
        int day = Mathf.Max(1, PhaseManager.Instance.currentDay);

        // When allowed
        if (canInteract)
        {
            if (phase == PhaseManager.GamePhase.Night) return "Press E to sleep (go to Morning)";
            if (phase == PhaseManager.GamePhase.Morning) return "Press E to continue (go to Day)";
            if (phase == PhaseManager.GamePhase.Day) return "Press E to sleep (go to Night)";
            return "Press E";
        }

        // When blocked: explain why + show progress
        if (phase == PhaseManager.GamePhase.Night)
        {
            // Must order food
            bool ordered = ResourceManager.Instance != null && ResourceManager.Instance.HasOrderedTonight;
            int orderedCount = ResourceManager.Instance != null ? ResourceManager.Instance.OrderedTonightCount : 0;
            return ordered
                ? "Press E to sleep"
                : $"Order food before sleeping. (Ordered: {orderedCount}/1)";
        }

        if (phase == PhaseManager.GamePhase.Morning)
        {
            // Must receive delivery + spend all energy
            bool deliveryOk = ResourceManager.Instance != null && ResourceManager.Instance.DeliveryAppliedDay == day;
            int curEnergy = EnergyManager.Instance != null ? EnergyManager.Instance.Current : 0;
            int maxEnergy = EnergyManager.Instance != null ? EnergyManager.Instance.maxCharges : 3;

            string deliveryPart = deliveryOk ? "Delivery: OK" : "Receive delivery first";
            string energyPart = curEnergy == 0 ? "Energy: OK" : $"Spend all energy ({curEnergy}/{maxEnergy})";

            return $"{deliveryPart} | {energyPart}";
        }

        if (phase == PhaseManager.GamePhase.Day)
        {
            // Must decide visitors
            int made = DailyNPCDecisionTracker.Instance != null ? DailyNPCDecisionTracker.Instance.DecisionsMade : 0;
            int req = DailyNPCDecisionTracker.Instance != null ? DailyNPCDecisionTracker.Instance.requiredDecisionsPerDay : 2;
            if (made >= req) return "Press E to sleep";
            return $"Decide all visitors first ({made}/{req})";
        }

        return "";
    }
}
