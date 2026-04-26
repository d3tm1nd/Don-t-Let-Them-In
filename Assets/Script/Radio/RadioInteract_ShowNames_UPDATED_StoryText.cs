using UnityEngine;

/// <summary>
/// RadioInteract_ShowNames (UPDATED to show story text)
/// - โชว์ข้อความเนื้อเรื่องผ่าน RadioNamesUI
///
/// ✅ ปรับตามที่ขอ:
/// - เวลา Interact (กด E) ให้เล่นเสียงวิทยุ "ครั้งเดียว" (PlayOneShot)
/// - ถ้าผู้เล่นมากด Interact ใหม่ ก็จะได้ยินอีกครั้ง
/// - และถ้า UI ปิด (กด ESC) ให้หยุดเสียงทันที (Stop)
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class RadioInteract_ShowNames : MonoBehaviour, IInteractable
{
    [Header("Layer (must match InteractionRay)")]
    public string requiredLayerName = "interactable";

    [Header("UI")]
    public RadioNamesUI namesUI;               // assign in Inspector or enable auto-find
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

    [Header("Radio SFX (One Shot)")]
    [Tooltip("AudioSource ที่ใช้เล่นเสียง (ถ้าไม่ใส่จะใช้ของ GameObject นี้)")]
    public AudioSource radioSource;

    [Tooltip("เสียงวิทยุ (เล่นครั้งเดียวตอน Interact)")]
    public AudioClip radioSfx;

    [Range(0f, 1f)]
    public float radioVolume = 0.9f;

    [Tooltip("0 = 2D, 1 = 3D (แนะนำ 1 เพราะเป็นเสียงจากวิทยุในฉาก)")]
    [Range(0f, 1f)]
    public float spatialBlend = 1f;

    // กัน subscribe ซ้ำ
    private bool _subscribed = false;

    // กันหลายวิทยุใช้ UI เดียว: ให้หยุดเสียงเฉพาะตัวที่เปิดล่าสุด
    private static RadioInteract_ShowNames _currentOwner;

    void Reset()
    {
        EnsureLayer();

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false; // make raycast friendly

        radioSource = GetComponent<AudioSource>();
        SetupAudio();
    }

    void Awake()
    {
        EnsureLayer();

        if (radioSource == null)
            radioSource = GetComponent<AudioSource>();

        SetupAudio();
    }

    void OnDisable()
    {
        UnsubscribeUI();
        if (_currentOwner == this) _currentOwner = null;
        StopRadioImmediately();
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

    private void SetupAudio()
    {
        if (radioSource == null) return;
        radioSource.playOnAwake = false;
        radioSource.loop = false;
        radioSource.spatialBlend = spatialBlend;
        radioSource.volume = radioVolume;
    }

    public void Interact()
    {
        // หา UI
        if (autoFindUIInScene && namesUI == null)
            namesUI = FindObjectOfType<RadioNamesUI>(true);

        if (namesUI == null)
        {
            Debug.LogError("❌ RadioInteract_ShowNames: RadioNamesUI not found in scene (assign it or enable autoFindUIInScene)");
            return;
        }

        // subscribe event UI ปิด (เพื่อหยุดเสียงทันที)
        SubscribeUI();

        // ตัวนี้เป็นเจ้าของล่าสุด
        _currentOwner = this;

        // เล่นเสียงวิทยุแบบครั้งเดียว
        PlayRadioOneShot();

        // ---- ของเดิม: โชว์ข้อความ ----
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

        namesUI.ShowText(title, pages[idx]);
    }

    private void PlayRadioOneShot()
    {
        if (radioSource == null)
            radioSource = GetComponent<AudioSource>();

        if (radioSource == null) return;

        radioSource.spatialBlend = spatialBlend;
        radioSource.volume = radioVolume;

        if (radioSfx == null) return;

        // ถ้ากำลังเล่นอยู่แล้ว ให้หยุดก่อน เพื่อให้กดใหม่แล้วได้ยินชัด (ไม่ซ้อนหลายเสียง)
        radioSource.Stop();
        radioSource.PlayOneShot(radioSfx, radioVolume);
    }

    private void StopRadioImmediately()
    {
        if (radioSource == null) return;
        radioSource.Stop();
    }

    private void SubscribeUI()
    {
        if (_subscribed) return;
        namesUI.OnClosed += HandleUIClosed;
        _subscribed = true;
    }

    private void UnsubscribeUI()
    {
        if (!_subscribed || namesUI == null) return;
        namesUI.OnClosed -= HandleUIClosed;
        _subscribed = false;
    }

    private void HandleUIClosed()
    {
        // หยุดเสียงทันที เฉพาะวิทยุที่เปิดล่าสุด
        if (_currentOwner == this)
        {
            StopRadioImmediately();
            _currentOwner = null;
        }
    }
}
