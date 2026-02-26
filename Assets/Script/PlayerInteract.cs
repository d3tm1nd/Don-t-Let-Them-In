using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float distance = 3f;
    public Camera cam;

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // กด E
            if (Input.GetKeyDown(KeyCode.E))
            {
                DoorNPC npc = hit.collider.GetComponent<DoorNPC>();

                if (npc != null)
                {
                    Debug.Log("Talk to NPC");
                    npc.Interact(); // <<< สำคัญ
                }
            }
        }
    }
}
