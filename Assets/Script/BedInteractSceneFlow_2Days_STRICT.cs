using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// STRICT bed scene flow.
/// - Requires completionProvider and requireCompletion.
/// - If completionProvider is missing or does not implement ICompletionChecker -> blocks.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BedInteractSceneFlow_2Days : MonoBehaviour, IInteractable
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
    public MonoBehaviour completionProvider; // must implement ICompletionChecker

    [Header("End Game")]
    public EndGameEvaluator2Days endEvaluator;

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
        // STRICT: must have completionProvider if requireCompletion
        if (requireCompletion)
        {
            if (completionProvider == null)
            {
                Debug.LogError("❌ BedInteractSceneFlow_2Days: completionProvider is NULL (bed blocked)");
                if (blockedSfx != null) _audio.PlayOneShot(blockedSfx);
                return;
            }

            var checker = completionProvider as ICompletionChecker;
            if (checker == null)
            {
                Debug.LogError("❌ BedInteractSceneFlow_2Days: completionProvider does not implement ICompletionChecker (bed blocked)");
                if (blockedSfx != null) _audio.PlayOneShot(blockedSfx);
                return;
            }

            bool ok = checker.IsCompleted;
            Debug.Log($"🛏️ Bed check: phase={PhaseManager.Instance?.currentPhase.ToString() ?? "NULL"}, day={PhaseManager.Instance?.currentDay.ToString() ?? "-"}, ok={ok}");

            if (!ok)
            {
                if (blockedSfx != null) _audio.PlayOneShot(blockedSfx);
                return;
            }
        }

        if (sleepSfx != null) _audio.PlayOneShot(sleepSfx);

        if (PhaseManager.Instance == null)
        {
            Debug.LogError("❌ BedInteractSceneFlow_2Days: PhaseManager missing (cannot route)");
            return;
        }

        var phase = PhaseManager.Instance.currentPhase;
        int day = Mathf.Max(1, PhaseManager.Instance.currentDay);
        int playableDays = (rules != null) ? Mathf.Max(1, rules.playableDays) : 2;

        if (phase == PhaseManager.GamePhase.Morning)
        {
            PhaseManager.Instance.SetPhase(PhaseManager.GamePhase.Day);
            LoadSceneSafe(daySceneName);
            return;
        }

        if (phase == PhaseManager.GamePhase.Day)
        {
            PhaseManager.Instance.SetPhase(PhaseManager.GamePhase.Night);
            LoadSceneSafe(nightSceneName);
            return;
        }

        if (phase == PhaseManager.GamePhase.Night)
        {
            if (day >= playableDays)
            {
                if (endEvaluator == null) endEvaluator = FindObjectOfType<EndGameEvaluator2Days>(true);
                if (endEvaluator != null) endEvaluator.EvaluateAndLoad();
                else Debug.LogError("❌ EndGameEvaluator2Days not found");
                return;
            }

            PhaseManager.Instance.SetPhase(PhaseManager.GamePhase.Morning);
            LoadSceneSafe(morningSceneName);
            return;
        }

        Debug.LogError($"❌ Unknown phase {phase}");
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
}
