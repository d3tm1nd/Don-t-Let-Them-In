using UnityEngine;

// Auto-resets energy to full when PhaseManager enters Morning once per day
public class EnergyResetOnMorning : MonoBehaviour
{
    [Tooltip("Reset the energy when entering Morning phase")] public bool resetOnMorning = true;
    [Tooltip("Also reset on scene start if already Morning")] public bool resetOnStartIfAlreadyMorning = true;

    private bool _didThisMorning;

    void Start()
    {
        TryResetIfMorning(initial:true);
    }

    void Update()
    {
        TryResetIfMorning(initial:false);
    }

    void TryResetIfMorning(bool initial)
    {
        if (!resetOnMorning) return;
        if (PhaseManager.Instance == null) return;

        bool isMorning = PhaseManager.Instance.currentPhase == PhaseManager.GamePhase.Morning;
        if (initial)
        {
            if (isMorning && resetOnStartIfAlreadyMorning)
            {
                EnergyManager.Instance?.ResetFull();
                _didThisMorning = true;
            }
            return;
        }

        if (isMorning && !_didThisMorning)
        {
            EnergyManager.Instance?.ResetFull();
            _didThisMorning = true;
        }
        else if (!isMorning)
        {
            _didThisMorning = false; // allow next morning to reset again
        }
    }
}
