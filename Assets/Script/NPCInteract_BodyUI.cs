using UnityEngine;

// ติดที่ GameObject ของ NPC (ต้องมี Collider) เพื่อให้กด E ผ่าน InteractionRay แล้วเปิด UI เลือกส่วนร่างกาย
// ต้องมี NPCBodyProvider อยู่บนรากเดียวกัน (หรือพาเรนต์)
[RequireComponent(typeof(Collider))]
public class NPCInteract_BodyUI : MonoBehaviour, IInteractable
{
    [Header("Layer (ต้องสอดคล้องกับ InteractionRay)")]
    public string requiredLayerName = "interactable";

    [Header("Body Provider (optional)")]
    public NPCBodyProvider provider; // ถ้าเว้นว่างจะค้นหาจากราก

    [Header("UI")]
    public BodySelectionUI selectionUI; // ถ้าเว้นว่างจะค้นหาอัตโนมัติในซีน
    public bool autoFindUIInScene = true;

    [Header("Info")]
    public string displayNameOverride; // ชื่อที่จะแสดงใน UI ถ้าเว้นว่างจะใช้ name ของ GameObject

    [Header("Energy Gate")]
    public AudioClip noEnergySfx;      // เสียงเตือนเมื่อ Energy = 0
    private AudioSource _audio;

    void Reset()
    {
        EnsureLayer();
        if (provider == null) provider = GetComponentInParent<NPCBodyProvider>();
    }

    void Awake()
    {
        EnsureLayer();
        if (provider == null) provider = GetComponentInParent<NPCBodyProvider>();
        _audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void EnsureLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer != -1) gameObject.layer = layer;
    }

    // ถูกเรียกโดย InteractionRay เมื่อกด E ที่ Collider ของ NPC
    public void Interact()
    {
        if (provider == null)
        {
            Debug.LogWarning("NPCInteract_BodyUI: ไม่พบ NPCBodyProvider บนราก NPC");
            return;
        }

        if (autoFindUIInScene && selectionUI == null)
        {
            selectionUI = GameObject.FindObjectOfType<BodySelectionUI>(true);
        }
        if (selectionUI == null)
        {
            Debug.LogError("NPCInteract_BodyUI: ไม่พบ BodySelectionUI ในซีน");
            return;
        }

        // --- ENERGY GATE: Energy = 0 ห้ามเปิด UI ---
        if (EnergyManager.Instance != null && !EnergyManager.Instance.HasEnergy)
        {
            if (noEnergySfx != null) _audio.PlayOneShot(noEnergySfx);
            Debug.Log("Not enough energy to open inspection UI.");
            return;
        }

        string shownName = string.IsNullOrEmpty(displayNameOverride) ? gameObject.name : displayNameOverride;
        selectionUI.Open(provider, shownName);         // **ไม่หัก energy ตรงนี้**
    }
}
