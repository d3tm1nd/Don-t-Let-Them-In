using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WinSceneController : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] private string creditSceneName = "CreditScene";

    [Header("Timing")]
    [SerializeField] private float waitSeconds = 3.0f;

    [Header("Skip")]
    [SerializeField] private bool allowSkipWithSpace = true;

    private bool loading;

    private void Start()
    {
        // เผื่อบางระบบมี Time.timeScale = 0 ตอนจบเกม
        Time.timeScale = 1f;

        Invoke(nameof(LoadCredits), waitSeconds);
    }

    private void Update()
    {
        if (!allowSkipWithSpace || loading) return;

        bool pressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            pressed = true;
#else
        if (Input.GetKeyDown(KeyCode.Space))
            pressed = true;
#endif

        if (pressed)
            LoadCredits();
    }

    private void LoadCredits()
    {
        if (loading) return;
        loading = true;

        SceneManager.LoadScene(creditSceneName);
    }
}