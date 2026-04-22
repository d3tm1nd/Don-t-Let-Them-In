using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InteractionRay (UPDATED)
/// - ยิง Ray จากกล้อง/ผู้เล่นเพื่อหา object ที่ interact ได้
/// - กด E: เรียก IInteractable.Interact() (เหมือนเดิม)
/// - กด F: ถ้าเจอ NPCDialogueSource จะเปิดบทสนทนา (SimpleDialogueManager)
///
/// หมายเหตุ:
/// - ถ้า Dialogue เปิดอยู่ จะบล็อกการ interact อื่นทั้งหมด
/// - ยังรองรับ PeepHoleInteract แบบกดค้าง F ได้ (จะทำงานเมื่อ "ไม่เจอ NPCDialogueSource")
///   ถ้าคุณไม่ใช้ peephole แล้ว สามารถลบส่วนนี้ออกได้
/// </summary>
public class InteractionRay : MonoBehaviour
{
    public float interactDistance = 2.5f;
    public LayerMask interactLayer;

    // Optional: peephole hold
    private PeepHoleInteract currentPeep;

    void Update()
    {
        // ถ้า Dialogue เปิดอยู่: ไม่ให้ interact อย่างอื่น
        if (SimpleDialogueManager.Instance != null && SimpleDialogueManager.Instance.IsOpen)
        {
            // ถ้ากำลังส่องอยู่ให้หยุด
            if (currentPeep != null)
            {
                currentPeep.StopPeep();
                currentPeep = null;
            }
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            // ---------- Dialogue (กด F ครั้งเดียว) ----------
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                var src = hit.collider.GetComponentInParent<NPCDialogueSource>();
                if (src != null)
                {
                    if (SimpleDialogueManager.Instance != null)
                        SimpleDialogueManager.Instance.StartDialogue(src);
                    return; // กันไม่ให้ไปทำอย่างอื่นในเฟรมเดียวกัน
                }
            }

            // ---------- Peep Hole (กดค้าง F) (Optional) ----------
            PeepHoleInteract peep = hit.collider.GetComponent<PeepHoleInteract>();
            if (peep != null)
            {
                if (Keyboard.current != null && Keyboard.current.fKey.isPressed)
                {
                    currentPeep = peep;
                    peep.StartPeep();
                    return; // ไม่ให้ทำอย่างอื่น
                }
                else if (currentPeep != null)
                {
                    currentPeep.StopPeep();
                    currentPeep = null;
                }
            }

            // ---------- Interaction ปกติ (กด E) ----------
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                    interactable.Interact();
            }
        }
        else if (currentPeep != null)
        {
            currentPeep.StopPeep();
            currentPeep = null;
        }
    }
}
