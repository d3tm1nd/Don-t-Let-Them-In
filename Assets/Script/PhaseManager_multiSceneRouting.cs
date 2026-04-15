using UnityEngine;

/// <summary>
/// PhaseManager (Scene-only version)
/// - Manages ONLY phase + day counter
/// - Does NOT auto-advance from gameplay
/// - Scene transitions are controlled by BedInteractSceneFlow_2Days
/// </summary>
public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    public enum GamePhase
    {
        Morning,
        Day,
        Night
    }

    [Header("State")]
    public GamePhase currentPhase = GamePhase.Morning;
    public int currentDay = 1; // Day 1 starts at 1

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

    // -------- Phase setters (called by Bed system) --------

    public void SetPhase(GamePhase phase)
    {
        currentPhase = phase;
        Debug.Log($"📌 Phase set to {currentPhase} (Day {currentDay})");
    }

    public void GoMorning()
    {
        currentDay++;
        currentPhase = GamePhase.Morning;
        Debug.Log($"🌅 Go Morning (Day {currentDay})");
    }

    public void GoDay()
    {
        currentPhase = GamePhase.Day;
        Debug.Log($"☀️ Go Day (Day {currentDay})");
    }

    public void GoNight()
    {
        currentPhase = GamePhase.Night;
        Debug.Log($"🌙 Go Night (Day {currentDay})");
    }
}
