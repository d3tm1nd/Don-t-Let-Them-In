using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

// ข้ามฉากด้วยการกด E ที่เตียงเท่านั้น และรองรับเกม 2 วัน + ฉากสรุปผล (Day 3)
[RequireComponent(typeof(Collider))]
public class BedInteractSceneFlow : MonoBehaviour, IInteractable
{
    [Header("Layer (match InteractionRay)")]
    public string requiredLayerName = "interactable";

    [Header("Scene Names")]
    public string morningSceneName = "morningscene";
    public string daySceneName = "dayscene";
    public string nightSceneName = "nightscene";

    [Header("Two-Day Rules")]
    public TwoDayRules rules;

    [Header("Completion Provider")]
    public bool requireCompletion = true;
    public MonoBehaviour completionProvider; // should implement ICompletionChecker

    [Header("End Game")]
    public EndGameEvaluator2Days endEvaluator; // assign in inspector or auto-find

    [Header("Audio (optional)")]
    public AudioClip sleepSfx;
    public AudioClip blockedSfx;
    private AudioSource _audio;

    void Reset()
    {
        EnsureLayer();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;
    }

    void Awake()
    {
        EnsureLayer();
        _audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (endEvaluator == null) endEvaluator = FindObjectOfType<EndGameEvaluator2Days>(true);
    }

    void EnsureLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer != -1) gameObject.layer = layer;
    }

    public void Interact()
    {
        // 1) Check completion rules
        if (requireCompletion && completionProvider != null)
        {
            var checker = completionProvider as ICompletionChecker;
            if (checker == null)
            {
                Debug.LogError("❌ BedInteractSceneFlow_2Days: completionProvider ต้อง implements ICompletionChecker");
                return;
            }
            if (!checker.IsCompleted)
            {
                if (blockedSfx != null) _audio.PlayOneShot(blockedSfx);
                Debug.Log("⛔ Bed blocked: requirements not met");
                return;
            }
        }

        if (sleepSfx != null) _audio.PlayOneShot(sleepSfx);

        // 2) Determine phase-based transition
        if (PhaseManager.Instance != null)
        {
            var phase = PhaseManager.Instance.currentPhase;
            int day = Mathf.Max(1, PhaseManager.Instance.currentDay);
            int playableDays = (rules != null) ? Mathf.Max(1, rules.playableDays) : 2;

            // Morning -> Day
            if (phase == PhaseManager.GamePhase.Morning)
            {
                PhaseManager.Instance.currentPhase = PhaseManager.GamePhase.Event;
                LoadSceneSafe(daySceneName);
                return;
            }

            // Day -> Night
            if (phase == PhaseManager.GamePhase.Event)
            {
                PhaseManager.Instance.currentPhase = PhaseManager.GamePhase.Night;
                CallMethodIfExists(PhaseManager.Instance, "LoadNight");
                LoadSceneSafe(nightSceneName);
                return;
            }

            // Night -> Next (Morning or End)
            if (phase == PhaseManager.GamePhase.Night)
            {
                // If this is the last playable day night, go to ending instead of Morning
                if (day >= playableDays)
                {
                    // Day 3 is summary/end scene (no loop)
                    if (endEvaluator == null) endEvaluator = FindObjectOfType<EndGameEvaluator2Days>(true);
                    if (endEvaluator != null)
                    {
                        endEvaluator.EvaluateAndLoad();
                    }
                    else
                    {
                        Debug.LogError("❌ EndGameEvaluator2Days not found");
                    }
                    return;
                }

                // Otherwise proceed to next morning (increment day)
                // Prefer PhaseManager.LoadMorning if exists
                if (CallMethodIfExists(PhaseManager.Instance, "LoadMorning"))
                    return;

                PhaseManager.Instance.currentDay += 1;
                PhaseManager.Instance.currentPhase = PhaseManager.GamePhase.Morning;
                LoadSceneSafe(morningSceneName);
                return;
            }
        }

        // Fallback by active scene
        string active = SceneManager.GetActiveScene().name;
        if (active == morningSceneName) LoadSceneSafe(daySceneName);
        else if (active == daySceneName) LoadSceneSafe(nightSceneName);
        else if (active == nightSceneName) LoadSceneSafe(morningSceneName);
        else LoadSceneSafe(daySceneName);
    }

    void LoadSceneSafe(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ BedInteractSceneFlow_2Days: sceneName is empty");
            return;
        }
        Debug.Log($"🛏️ Load Scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    bool CallMethodIfExists(object target, string methodName)
    {
        if (target == null) return false;
        var mi = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (mi == null) return false;
        mi.Invoke(target, null);
        return true;
    }
}
