using UnityEngine;

public class PeepHoleInteract : MonoBehaviour, IInteractable
{
    public Transform peepViewPoint;
    public float peepFOV = 30f;

    private Camera playerCam;
    private float defaultFOV;
    private bool isPeeping = false;

    void Start()
    {
        playerCam = Camera.main;
        defaultFOV = playerCam.fieldOfView;
    }

    public void Interact()
    {
        if (!isPeeping)
        {
            StartPeep();
        }
        else
        {
            StopPeep();
        }
    }

    public void StartPeep()
    {
        if (isPeeping) return;
        isPeeping = true;

        playerCam.transform.position = peepViewPoint.position;
        playerCam.transform.rotation = peepViewPoint.rotation;
        playerCam.fieldOfView = peepFOV;
        Debug.Log("เข้าโหมดส่องช่องตาแมว");
    }

    public void StopPeep()
    {
        if (!isPeeping) return;
        isPeeping = false;

        playerCam.fieldOfView = defaultFOV;
        Debug.Log("ออกจากโหมดส่องช่องตาแมว");
    }

}
