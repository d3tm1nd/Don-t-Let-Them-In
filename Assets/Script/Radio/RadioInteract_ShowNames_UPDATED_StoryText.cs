using UnityEngine;

/// <summary>
/// RadioInteract_ShowNames (UPDATED to show story text)
/// - เดิม: โชว์ชื่อ NPC จาก NPCDataManager
/// - ตอนนี้: โชว์ "ข้อความเนื้อเรื่อง" แบบธรรมดา (ไม่ใช้ NPCDataManager แล้ว)
///
/// วิธีใช้:
/// 1) ใส่สคริปต์นี้บน GameObject วิทยุ (ต้องมี Collider)
/// 2) ตั้ง requiredLayerName ให้ตรงกับ InteractionRay (เช่น "interactable")
/// 3) ใส่ RadioNamesUI (UI) ในฉาก
/// 4) ตั้งข้อความผ่าน:
///    A) storyData (แนะนำ) หรือ
///    B) inlinePages (ใส่ใน Inspector)
/// </summary>
[RequireComponent(typeof(Collider))]
public class RadioInteract_ShowNames : MonoBehaviour, IInteractable
{
    [Header("Layer (must match InteractionRay)")]
    public string requiredLayerName = "interactable";

    [Header("UI")]
    public RadioNamesUI namesUI; // assign in Inspector or enable auto-find
    public bool autoFindUIInScene = true;

    [Header("Story Content (recommended)")]
    public RadioStoryData storyData;

    [Header("Story Content (inline fallback)")]
    [Tooltip("ถ้าไม่ใส่ storyData จะใช้ pages ตรงนี้แทน")]
    [TextArea(3, 10)]
    public string[] inlinePages;

    [Header("Behavior")]
    public bool randomPage = false;
    public bool cyclePages = true;

    private int _pageIndex = 0;

    void Reset()
    {
        EnsureLayer();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false; // make raycast friendly
    }

    void Awake()
    {
        EnsureLayer();
    }

    private void EnsureLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer == -1)
        {
            Debug.LogWarning($"RadioInteract_ShowNames: Layer '{requiredLayerName}' not found.");
            return;
        }
        gameObject.layer = layer;
    }

    public void Interact()
    {
        if (autoFindUIInScene && namesUI == null)
            namesUI = FindObjectOfType<RadioNamesUI>(true);

        if (namesUI == null)
        {
            Debug.LogError("❌ RadioInteract_ShowNames: RadioNamesUI not found in scene (assign it or enable autoFindUIInScene)");
            return;
        }

        string title = storyData != null ? storyData.title : "";
        string[] pages = storyData != null ? storyData.pages : inlinePages;

        if (pages == null || pages.Length == 0)
        {
            namesUI.ShowText(title, "(ไม่มีข้อความ)");
            return;
        }

        bool rnd = storyData != null ? storyData.randomPage : randomPage;
        bool cyc = storyData != null ? storyData.cyclePages : cyclePages;

        int idx;
        if (rnd)
        {
            idx = Random.Range(0, pages.Length);
        }
        else if (cyc)
        {
            idx = Mathf.Clamp(_pageIndex, 0, pages.Length - 1);
            _pageIndex = (_pageIndex + 1) % pages.Length;
        }
        else
        {
            idx = 0;
        }

        string body = pages[idx];
        namesUI.ShowText(title, body);
    }
}
