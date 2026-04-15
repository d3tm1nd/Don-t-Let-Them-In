using UnityEngine;
using System;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    [Header("Config")]
    public int maxCharges = 3;

    [SerializeField]
    private int currentCharges;

    public event Action<int, int> OnChanged; // (current, max)

    public int Current => currentCharges;
    public bool HasEnergy => currentCharges > 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (currentCharges <= 0) currentCharges = maxCharges;
            OnChanged?.Invoke(currentCharges, maxCharges);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool TrySpend(int amount = 1)
    {
        if (amount <= 0) return true;
        if (currentCharges < amount) return false;
        currentCharges -= amount;
        OnChanged?.Invoke(currentCharges, maxCharges);
        return true;
    }
    public void AddCharges(int amount = 1)
    {
    int before = currentCharges;
    currentCharges = Mathf.Clamp(currentCharges + amount, 0, maxCharges);
    if (currentCharges != before)
        OnChanged?.Invoke(currentCharges, maxCharges);
     }
    
    public void ResetFull()
    {
        currentCharges = maxCharges;
        OnChanged?.Invoke(currentCharges, maxCharges);
    }
}
