using UnityEngine;

/// <summary>
/// Mode A: Press E on the phone to order a fixed amount of food each time.
/// Requires InteractionRay + IInteractable (your existing system).
/// Put this object on layer 'interactable' (or whatever your InteractionRay LayerMask uses).
/// </summary>
[RequireComponent(typeof(Collider))]
public class NightPhoneOrderInteract : MonoBehaviour, IInteractable
{
    [Header("Order Settings")]
    public int orderAmountPerPress = 3; // Mode A: fixed amount per E

    [Header("Layer (match InteractionRay)")]
    public string requiredLayerName = "interactable";

    [Header("Audio (optional)")]
    public AudioClip orderSfx;
    public AudioClip failSfx;
    private AudioSource _audio;

    void Reset()
    {
        EnsureLayer();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false; // raycast-friendly
    }

    void Awake()
    {
        EnsureLayer();
        _audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void EnsureLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer != -1) gameObject.layer = layer;
    }

    public void Interact()
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("❌ ResourceManager not found. Add it to the first scene and mark DontDestroyOnLoad.");
            return;
        }

        bool ok = ResourceManager.Instance.OrderFood(orderAmountPerPress);

        if (ok)
        {
            if (orderSfx != null) _audio.PlayOneShot(orderSfx);
        }
        else
        {
            if (failSfx != null) _audio.PlayOneShot(failSfx);
            Debug.Log("⚠️ Cannot order more food (quota/storage reached). ");
        }
    }
}
