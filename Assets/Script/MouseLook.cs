using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;

    public bool canLook = true;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        // 🔴 ถ้า Freeze → หยุดทันที
        if (!canLook) return;

        float mouseX = Mouse.current.delta.ReadValue().x * sensitivity * Time.deltaTime;
        float mouseY = Mouse.current.delta.ReadValue().y * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
