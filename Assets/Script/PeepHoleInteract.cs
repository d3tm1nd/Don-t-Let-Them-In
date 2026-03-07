using UnityEngine;

public class PeepHoleInteract : MonoBehaviour, IInteractable
{
    [Header("Peep Viewpoint")]
    public Transform peepViewPoint;            // จุด/m มองช่องตาแมว
    public float peepFOV = 30f;                // มุมมองตอนส่อง

    [Header("Controls (optional)")]
    [Tooltip("คอมโพเนนต์ที่ต้องปิดชั่วคราวระหว่างส่อง (เช่น ตัวหมุนกล้อง/ตัวเดิน)")]
    public MonoBehaviour[] disableWhilePeeping;

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
            Debug.LogError("❌ PeepHoleInteract: ไม่พบ Camera.main");
            enabled = false;
            return;
        }
        defaultFOV = playerCam.fieldOfView;
    }

    public void Interact()
    {
        // รองรับ toggle ด้วย (แม้ระบบหลักจะใช้ hold F)
        if (!isPeeping) StartPeep(); else StopPeep();
    }

    public void StartPeep()
    {
        if (isPeeping || peepViewPoint == null || playerCam == null) return;
        isPeeping = true;

        // บันทึกสภาพกล้องเดิม
        originalCamPos = playerCam.transform.position;
        originalCamRot = playerCam.transform.rotation;
        originalCamParent = playerCam.transform.parent;

        // ปิดคอมโพเนนต์ควบคุมที่อาจแทรกแซงกล้อง/การเคลื่อนไหว
        SetControlsEnabled(false);

        // ย้ายกล้องไปยังตำแหน่งช่องมอง + ปรับมุมมอง
        playerCam.transform.position = peepViewPoint.position;
        playerCam.transform.rotation = peepViewPoint.rotation;
        playerCam.fieldOfView = peepFOV;

        // (ถ้าต้องให้อิงตามประตูที่ไหว: สามารถ SetParent ได้)
        // playerCam.transform.SetParent(peepViewPoint, true);

        Debug.Log("เข้าโหมดส่องช่องตาแมว");
    }

    public void StopPeep()
    {
        if (!isPeeping || playerCam == null) return;
        isPeeping = false;

        // คืนค่ากล้องเดิม
        playerCam.fieldOfView = defaultFOV;
        // playerCam.transform.SetParent(originalCamParent, true); // ถ้าเคย SetParent
        playerCam.transform.SetPositionAndRotation(originalCamPos, originalCamRot);

        // เปิดคอมโพเนนต์ควบคุมคืน
        SetControlsEnabled(true);

        Debug.Log("ออกจากโหมดส่องช่องตาแมว");
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
        // กันกรณีซีนเปลี่ยน/วัตถุถูกปิดระหว่างส่อง
        if (isPeeping)
        {
            // อย่าเรียก Debug ซ้ำมากไปใน lifecycle
            playerCam.fieldOfView = defaultFOV;
            // playerCam.transform.SetParent(originalCamParent, true);
            playerCam.transform.SetPositionAndRotation(originalCamPos, originalCamRot);
            SetControlsEnabled(true);
            isPeeping = false;
        }
    }
}
