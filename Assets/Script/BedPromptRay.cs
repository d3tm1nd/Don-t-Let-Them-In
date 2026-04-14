using UnityEngine;
using TMPro;

/// <summary>
/// Attach this to the player's camera (or the same object as InteractionRay).
/// It raycasts forward and shows a small prompt when looking at the bed.
/// No need to modify InteractionRay.
/// </summary>
public class BedPromptRay : MonoBehaviour
{
    [Header("Ray")]
    public float distance = 2.5f;
    public LayerMask interactLayer; // should include your 'interactable' layer

    [Header("UI")]
    public GameObject promptRoot;          // optional parent to enable/disable
    public TextMeshProUGUI promptText;     // required

    [Header("Behavior")]
    public bool showOnlyWhenLookingAtBed = true;

    void Awake()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
        if (promptText != null) promptText.text = string.Empty;
    }

    void Update()
    {
        bool show = false;
        string text = string.Empty;

        if (Physics.Raycast(new Ray(transform.position, transform.forward), out var hit, distance, interactLayer))
        {
            var bed = hit.collider.GetComponent<BedInteractSceneFlow>();
            if (bed != null)
            {
                // If bed requires completion, check provider
                bool can = true;
                if (bed.requireCompletion && bed.completionProvider != null)
                {
                    var checker = bed.completionProvider as ICompletionChecker;
                    if (checker != null) can = checker.IsCompleted;
                }
                show = true;
                text = BedPromptText.GetPrompt(can);
            }
            else if (!showOnlyWhenLookingAtBed)
            {
                // optional: hide unless bed
                show = false;
            }
        }

        if (promptRoot != null) promptRoot.SetActive(show);
        if (promptText != null) promptText.text = show ? text : string.Empty;
    }
}
