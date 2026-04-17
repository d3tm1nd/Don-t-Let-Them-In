using UnityEngine;

public class PeepHoleInteract : MonoBehaviour, IInteractable
{
    [Header("Peep Viewpoint")]
    public Transform peepViewPoint;      // Position/rotation of peephole view
    public float peepFOV = 30f;          // FOV while peeping

    [Header("Controls (optional)")]
    [Tooltip("Components to disable while peeping (e.g., movement/look controllers)")]
    public MonoBehaviour[] disableWhilePeeping;

    [Header("Camera Lock (auto)")]
    [Tooltip("If assigned, will set MouseLook.canLook=false while peeping, then true on exit. If left empty, it will try to auto-find on Camera.main.")]
    public MouseLook cameraLook;
    public bool useCanLookFlag = true;

    private Camera playerCam;
    private float defaultFOV;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private Transform originalCamParent;

    private bool isPeeping = false;
    public bool IsPeeping => isPeeping;

    void Start()
    {
        playerCam = Camera.main;
        if (playerCam == null)
        {
            Debug.LogError("❌ PeepHoleInteract: Camera.main not found");
            enabled = false;
            return;
        }
        defaultFOV = playerCam.fieldOfView;

        // Auto-bind MouseLook on the camera if not set
        if (cameraLook == null)
        {
            cameraLook = playerCam.GetComponent<MouseLook>();
            if (cameraLook == null)
            {
                // try in parents as fallback
                cameraLook = playerCam.GetComponentInParent<MouseLook>();
            }
        }
    }

    public void Interact()
    {
        // Support toggle too (even though hold-to-peek is handled by PeepHoleRay)
        if (!isPeeping) StartPeep(); else StopPeep();
    }

    public void StartPeep()
    {
        if (isPeeping || peepViewPoint == null || playerCam == null) return;
        isPeeping = true;

        // Save original camera state
        originalCamPos = playerCam.transform.position;
        originalCamRot = playerCam.transform.rotation;
        originalCamParent = playerCam.transform.parent;

        // Disable conflicting controllers (if any)
        SetControlsEnabled(false);

        // 🔒 Lock camera look via MouseLook.canLook
        if (useCanLookFlag && cameraLook != null)
        {
            cameraLook.canLook = false;
        }

        // Move camera to peephole view
        playerCam.transform.position = peepViewPoint.position;
        playerCam.transform.rotation = peepViewPoint.rotation;
        playerCam.fieldOfView = peepFOV;

        // If you need to follow moving door/peephole, you can parent the camera:
        // playerCam.transform.SetParent(peepViewPoint, true);

        Debug.Log("Enter peephole mode");
    }

    public void StopPeep()
    {
        if (!isPeeping || playerCam == null) return;
        isPeeping = false;

        // Restore camera
        playerCam.fieldOfView = defaultFOV;
        // playerCam.transform.SetParent(originalCamParent, true); // if you parented in StartPeep
        playerCam.transform.SetPositionAndRotation(originalCamPos, originalCamRot);

        // 🔓 Unlock camera look via MouseLook.canLook
        if (useCanLookFlag && cameraLook != null)
        {
            cameraLook.canLook = true;
        }

        // Re-enable controllers
        SetControlsEnabled(true);

        Debug.Log("Exit peephole mode");
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (disableWhilePeeping == null) return;
        foreach (var c in disableWhilePeeping)
        {
            if (c != null) c.enabled = enabled;
        }
    }

    void OnDisable()
    {
        // Safety: if object disabled while peeping, restore camera & controls
        if (isPeeping)
        {
            if (playerCam != null)
            {
                playerCam.fieldOfView = defaultFOV;
                playerCam.transform.SetPositionAndRotation(originalCamPos, originalCamRot);
            }

            if (useCanLookFlag && cameraLook != null)
            {
                cameraLook.canLook = true;
            }

            SetControlsEnabled(true);
            isPeeping = false;
        }
    }
}
