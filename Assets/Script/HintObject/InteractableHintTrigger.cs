using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class InteractableHintTrigger : MonoBehaviour
{
    [Header("Detect")]
    public string playerTag = "Player";

    [Tooltip("ถ้าเว้นว่าง จะหา InteractableHintSource จาก parent")]
    public InteractableHintSource hintSource;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (hintSource == null)
            hintSource = GetComponentInParent<InteractableHintSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (hintSource == null) return;
        if (!hintSource.CanShowNow()) return;

        // โชว์ค้าง (seconds <= 0)
        InteractableHintUI.Instance?.ShowHint(hintSource.hintText, 0f);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // ออกโซนแล้วซ่อน
        InteractableHintUI.Instance?.HideHint();
    }
}
