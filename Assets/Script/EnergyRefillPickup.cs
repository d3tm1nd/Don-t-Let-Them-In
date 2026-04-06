using UnityEngine;

// Simple interactable pickup to refill energy to full
[RequireComponent(typeof(Collider))]
public class EnergyRefillPickup : MonoBehaviour, IInteractable
{
    [Tooltip("Destroy this pickup after use")] public bool destroyOnUse = true;
    [Tooltip("Optional sound when refilled")] public AudioClip refillSfx;
    private AudioSource _audio;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = false; // for raycast interact
        _audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Interact()
    {
        EnergyManager.Instance?.ResetFull();
        if (refillSfx != null) _audio.PlayOneShot(refillSfx);
        if (destroyOnUse) Destroy(gameObject);
    }
}
