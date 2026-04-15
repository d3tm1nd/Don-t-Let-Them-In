using UnityEngine;

/// <summary>
/// Put this in NightScene (or call from PhaseManager EnterNight).
/// Resets the per-night order quota.
/// </summary>
public class NightEntry_ResetOrderQuota : MonoBehaviour
{
    void Start()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.ResetNightOrderQuota();
    }
}
