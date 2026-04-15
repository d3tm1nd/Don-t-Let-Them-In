using UnityEngine;

/// <summary>
/// Morning processor (Food->Energy design):
/// - Applies guaranteed delivery from last night (pendingFood -> food)
/// - Does NOT feed NPCs anymore.
/// </summary>
public class MorningResourceProcessor : MonoBehaviour
{
    [Tooltip("Run only once per day even if the MorningScene reloads.")]
    public bool runOncePerDay = true;

    private int _lastProcessedDay = -1;

    void Start()
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("❌ ResourceManager not found.");
            return;
        }

        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        if (runOncePerDay && _lastProcessedDay == day) return;
        _lastProcessedDay = day;

        int delivered = ResourceManager.Instance.ApplyMorningDelivery();
        Debug.Log($"🌅 Morning Delivery | Day {day} | Delivered={delivered} | Food={ResourceManager.Instance.food}");
    }
}
