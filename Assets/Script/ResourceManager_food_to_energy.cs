using UnityEngine;

/// <summary>
/// Food ordering now supports PLAYER ENERGY only.
/// - Night: Order food (free) -> pendingFood increases
/// - Morning: ApplyMorningDelivery() moves pendingFood -> food (guaranteed)
/// - Food is NOT consumed by NPCs anymore.
/// - Player can convert food -> energy via FoodToEnergyInteract.
/// 
/// Bed-gating flags:
/// - HasOrderedTonight: true if player ordered at least once this night
/// - DeliveryAppliedDay: last day when ApplyMorningDelivery was called
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Food")]
    public int food = 0;              // Available now (player resource)
    public int pendingFood = 0;       // Ordered at night, arrives in morning

    [Header("Limits (balancing)")]
    public int maxFoodStorage = 50;   // Cap for (food + pendingFood)
    public int maxOrderPerNight = 10; // Cap per night

    private int _orderedTonight = 0;

    public int OrderedTonightCount => _orderedTonight;
    public bool HasOrderedTonight => _orderedTonight > 0;

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

    public void ResetNightOrderQuota()
    {
        _orderedTonight = 0;
    }

    public bool OrderFood(int amount)
    {
        if (amount <= 0) return false;

        int remainingQuota = maxOrderPerNight - _orderedTonight;
        if (remainingQuota <= 0) return false;

        int request = Mathf.Min(amount, remainingQuota);

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

    public int ApplyMorningDelivery()
    {
        int delivered = pendingFood;
        food += delivered;
        pendingFood = 0;

        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        deliveryAppliedDay = day;

        Debug.Log($"📦 Delivery Arrived +{delivered} | Food now={food} | DeliveryAppliedDay={deliveryAppliedDay}");
        return delivered;
    }

    // NEW: player consumes food to restore energy
    public bool TryConsumeFood(int amount = 1)
    {
        if (amount <= 0) return true;
        if (food < amount) return false;
        food -= amount;
        return true;
    }
}
