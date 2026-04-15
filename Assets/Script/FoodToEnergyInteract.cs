using UnityEngine;

/// <summary>
/// Day-only Food -> Energy station.
/// Press E to consume food and restore energy, ONLY during Day phase.
/// Note: In this project, Day phase is PhaseManager.GamePhase.Event.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FoodToEnergyInteract : MonoBehaviour, IInteractable
{
    [Header("Layer (match InteractionRay)")]
    public string requiredLayerName = "interactable";

    [Header("Config")]
    public int foodPerUse = 1;
    public int energyGain = 1;

    [Header("Restrictions")]
    [Tooltip("If true, can only be used during Day phase (GamePhase.Event).")]
    public bool dayOnly = true;

    [Header("Audio (optional)")]
    public AudioClip eatSfx;
    public AudioClip failSfx;
    public AudioClip wrongTimeSfx;
    private AudioSource _audio;

    void Reset()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer != -1) gameObject.layer = layer;
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;
    }

    void Awake()
    {
        _audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Interact()
    {
        // Day-only gate
        if (dayOnly && PhaseManager.Instance != null)
        {
            // In your loop, Day = GamePhase.Event
            if (PhaseManager.Instance.currentPhase != PhaseManager.GamePhase.Day)
            {
                if (wrongTimeSfx != null) _audio.PlayOneShot(wrongTimeSfx);
                Debug.Log("⛔ You can only use food to restore energy during the day.");
                return;
            }
        }

        if (ResourceManager.Instance == null || EnergyManager.Instance == null)
        {
            Debug.LogError("❌ Missing ResourceManager or EnergyManager");
            return;
        }

        // Already full
        if (EnergyManager.Instance.Current >= EnergyManager.Instance.maxCharges)
        {
            Debug.Log("ℹ️ Energy is already full.");
            return;
        }

        // Need enough food
        if (!ResourceManager.Instance.TryConsumeFood(foodPerUse))
        {
            if (failSfx != null) _audio.PlayOneShot(failSfx);
            Debug.Log("⚠️ Not enough food.");
            return;
        }

        EnergyManager.Instance.AddCharges(energyGain);
        if (eatSfx != null) _audio.PlayOneShot(eatSfx);

        Debug.Log($"🍞 Used food -{foodPerUse}, Energy +{energyGain} | Food={ResourceManager.Instance.food} | Energy={EnergyManager.Instance.Current}/{EnergyManager.Instance.maxCharges}");
    }
}
