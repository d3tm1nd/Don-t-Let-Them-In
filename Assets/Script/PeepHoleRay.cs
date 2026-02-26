using UnityEngine;
using UnityEngine.InputSystem;

public class PeepHoleRay : MonoBehaviour
{
    public float distance = 2f;
    public LayerMask peepLayer;

    private PeepHoleInteract currentPeep;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance, peepLayer))
        {
            PeepHoleInteract peep =
                hit.collider.GetComponent<PeepHoleInteract>();

            if (peep != null)
            {
                if (Keyboard.current.fKey.isPressed)
                {
                    currentPeep = peep;
                    peep.Interact();
                }
                else if (currentPeep != null)
                {
                    currentPeep.StopPeep();
                    currentPeep = null;
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
