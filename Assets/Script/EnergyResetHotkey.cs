using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Developer/testing hotkey to refill energy to full
public class EnergyResetHotkey : MonoBehaviour
{
    [Tooltip("Key used to reset energy (Input System new: F6; legacy: KeyCode.F6)")] public KeyCode legacyKey = KeyCode.F6;

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame)
        {
            EnergyManager.Instance?.ResetFull();
        }
#else
        if (Input.GetKeyDown(legacyKey))
        {
            EnergyManager.Instance?.ResetFull();
        }
#endif
    }
}
