using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class EndCutsceneToCredits : MonoBehaviour
{
    public PlayableDirector director;
    public string creditsSceneName = "Credits";

    [Header("Skip")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;
    public float holdToSkipSeconds = 1.0f;

    float holdTimer = 0f;

    void Awake()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
        if (director != null)
            director.stopped += OnStopped;
    }

    void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnStopped;
    }

    void Update()
    {
        if (!allowSkip) return;

        if (Input.GetKey(skipKey))
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdToSkipSeconds)
            {
                GoCredits();
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    void OnStopped(PlayableDirector d)
    {
        GoCredits();
    }

    public void GoCredits()
    {
        SceneManager.LoadScene(creditsSceneName);
    }
}