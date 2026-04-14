using UnityEngine;

/// <summary>
/// Resource manager for Food ordering loop:
/// - Night: Order food (free) -> pendingFood increases
/// - Morning: ApplyMorningDelivery() moves pendingFood -> food (guaranteed)
/// - Morning: ConsumeFoodForAliveNPC() reduces food by alive NPC count
/// 
/// Added flags for bed-gating:
/// - HasOrderedTonight: true if player ordered at least once this night
/// - DeliveryAppliedDay: last day when ApplyMorningDelivery was called
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Food")]
    public int food = 0;              // Available now
    public int pendingFood = 0;       // Ordered at night, arrives in morning

    [Header("Limits (balancing)")]
    public int maxFoodStorage = 50;   // Cap for (food + pendingFood)
    public int maxOrderPerNight = 10; // Cap per night

    private int _orderedTonight = 0;

    // --- Bed gating flags ---
    public int OrderedTonightCount => _orderedTonight;
    public bool HasOrderedTonight => _orderedTonight > 0;

    // Track when delivery was applied (Day number)
    [SerializeField] private int deliveryAppliedDay = -1;
    public int DeliveryAppliedDay => deliveryAppliedDay;

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

    /// <summary>Call when entering Night to reset the per-night order quota.</summary>
    public void ResetNightOrderQuota()
    {
        _orderedTonight = 0;
    }

    /// <summary>
    /// Free phone order at Night. Adds to pendingFood.
    /// Returns true if any amount was added.
    /// </summary>
    public bool OrderFood(int amount)
    {
        if (amount <= 0) return false;

        // Night quota
        int remainingQuota = maxOrderPerNight - _orderedTonight;
        if (remainingQuota <= 0) return false;

        int request = Mathf.Min(amount, remainingQuota);

        // Storage cap
        int total = food + pendingFood;
        int spaceLeft = maxFoodStorage - total;
        if (spaceLeft <= 0) return false;

        int add = Mathf.Min(request, spaceLeft);
        if (add <= 0) return false;

        pendingFood += add;
        _orderedTonight += add;

        Debug.Log($"📞 Ordered Food +{add} | Pending={pendingFood} | Food={food} | TonightUsed={_orderedTonight}/{maxOrderPerNight}");
        return true;
    }

    /// <summary>
    /// Morning delivery: guaranteed. Moves pendingFood into food.
    /// Returns delivered amount.
    /// </summary>
    public int ApplyMorningDelivery()
    {
        int delivered = pendingFood;
        food += delivered;
        pendingFood = 0;

        // mark delivery applied for this day
        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        deliveryAppliedDay = day;

        Debug.Log($"📦 Delivery Arrived +{delivered} | Food now={food} | DeliveryAppliedDay={deliveryAppliedDay}");
        return delivered;
    }

    /// <summary>
    /// Morning consumption: consumes 1 food per alive NPC (or less if shortage).
    /// Returns used amount.
    /// </summary>
    public int ConsumeFoodForAliveNPC(int aliveCount)
    {
        if (aliveCount <= 0) return 0;
        int used = Mathf.Min(food, aliveCount);
        food -= used;
        Debug.Log($"🍽 Consumed Food -{used} for AliveNPC={aliveCount} | Food left={food}");
        return used;
    }
}
