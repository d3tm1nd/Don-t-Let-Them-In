using UnityEngine;

/// <summary>
/// ตัวช่วย: เพิ่ม SphereCollider (isTrigger) ให้อัตโนมัติให้ NPCProximityDialogue
/// ใช้เมื่อคุณเลือก useTrigger=true
/// </summary>
[RequireComponent(typeof(NPCProximityDialogue))]
public class NPCDialogueAutoTrigger : MonoBehaviour
{
    public float radius = 3f;

    void Reset()
    {
        Ensure();
    }

    void Awake()
    {
        Ensure();
    }

    void Ensure()
    {
        var col = GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = Mathf.Max(0.1f, radius);
    }
}
