using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionRay : MonoBehaviour
{
    public float interactDistance = 2.5f;
    public LayerMask interactLayer;

    private PeepHoleInteract currentPeep;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // ---------- Peep Hole (กดค้าง F) ----------
            PeepHoleInteract peep =
                hit.collider.GetComponent<PeepHoleInteract>();

            if (peep != null)
            {
                if (Keyboard.current.fKey.isPressed)
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
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                IInteractable interactable =
                    hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
        else if (currentPeep != null)
        {
            currentPeep.StopPeep();
            currentPeep = null;
        }
    }
}
