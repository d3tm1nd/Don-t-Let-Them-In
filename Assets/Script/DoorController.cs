using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform doorPivot;
    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;
    private Quaternion closeRot;
    private Quaternion openRot;

    void Start()
    {
        closeRot = doorPivot.localRotation;
        openRot = Quaternion.Euler(0, openAngle, 0) * closeRot;
    }

    public void OpenDoor()
    {
        Debug.Log("DOOR: OpenDoor()");
        isOpen = true;
    }

    public void CloseDoor()
    {
        Debug.Log("DOOR: CloseDoor()");
        isOpen = false;
    }

    void Update()
    {
        if (isOpen)
        {
            doorPivot.localRotation =
                Quaternion.Slerp(doorPivot.localRotation, openRot, Time.deltaTime * speed);
        }
        else
        {
            doorPivot.localRotation =
                Quaternion.Slerp(doorPivot.localRotation, closeRot, Time.deltaTime * speed);
        }
    }
}
