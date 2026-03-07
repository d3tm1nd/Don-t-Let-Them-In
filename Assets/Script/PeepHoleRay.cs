using UnityEngine;
using UnityEngine.InputSystem;

public class PeepHoleRay : MonoBehaviour
{
    public float distance = 2f;
    public LayerMask peepLayer;

    private PeepHoleInteract currentPeep; // peep ที่กำลังใช้งาน

    void Update()
    {
        // ถ้ายังไม่ได้ส่อง: รอการเริ่มส่องเมื่อกด F + Ray โดนเป้าหมาย
        if (currentPeep == null || !currentPeep.IsPeeping)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (Physics.Raycast(new Ray(transform.position, transform.forward), out var hit, distance, peepLayer))
                {
                    var peep = hit.collider.GetComponent<PeepHoleInteract>();
                    if (peep != null)
                    {
                        currentPeep = peep;
                        currentPeep.StartPeep();
                    }
                }
            }
        }
        else // กำลังส่องอยู่: เลิกส่องเมื่อปล่อยปุ่ม F (ไม่ต้องพึ่ง Ray อีกแล้ว)
        {
            if (Keyboard.current.fKey.wasReleasedThisFrame)
            {
                currentPeep.StopPeep();
                currentPeep = null;
            }
        }

        // กันกรณี object ถูกปิด/ถูกลบระหว่างส่อง
        if (currentPeep != null && !currentPeep.isActiveAndEnabled)
        {
            currentPeep = null;
        }
    }
}
