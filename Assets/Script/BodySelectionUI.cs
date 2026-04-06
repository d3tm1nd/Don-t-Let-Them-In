using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

// UI for selecting NPC body parts and showing their status (with Energy spend on part selection)
public class BodySelectionUI : MonoBehaviour
{
    [Header("Root Panel")]
    public GameObject rootPanel;      // inactive at start

    [Header("Header")]
    public TextMeshProUGUI npcNameText;    // NPC display name

    [Header("Buttons (assign in Inspector)")]
    public Button headBtn;
    public Button torsoBtn;
    public Button leftArmBtn;
    public Button rightArmBtn;
    public Button leftLegBtn;
    public Button rightLegBtn;

    [Header("Detail Pane")]
    public TextMeshProUGUI partTitleText;  // e.g., "Head"
    public TextMeshProUGUI detailText;     // bullet list

    [Header("Highlight (optional)")]
    public BodyPartHighlighter highlighter; // optional: highlight selected part on the 3D model

    [Header("Behavior")]
    public bool closeOnEsc = true;

    [Header("Player Control (optional)")]
    [Tooltip("Scripts to disable while UI is open (e.g., camera look / movement).")]
    public MonoBehaviour[] disableWhileOpen;

    [Header("Energy Spend")]
    [Tooltip("Spend 1 energy when selecting a body part")]
    public bool spendOnSelect = true;
    public AudioClip spendOKSfx;            // หักสำเร็จ
    public AudioClip noEnergySfx;           // Energy ไม่พอ
    private AudioSource _audio;

    // ------- Localization (English) -------
    [Header("Localization (English)")]
    public string bulletWoundYes = "• Wound present";
    public string bulletWoundNo = "• No wound";
    public string bulletRashYes = "• Rash / redness present";
    public string bulletRashNo = "• No rash / redness";
    public string bulletCoughYes = "• Coughing / irregular breathing";
    public string bulletCoughNo = "• Breathing normal";
    public string bulletFeverYes = "• High fever";
    public string bulletFeverNo = "• Temperature normal";
    public string bulletNotePrefix = "• Note: ";

    public string labelHead = "Head";
    public string labelTorso = "Torso";
    public string labelLeftArm = "Left Arm";
    public string labelRightArm = "Right Arm";
    public string labelLeftLeg = "Left Leg";
    public string labelRightLeg = "Right Leg";

    // ---- runtime ----
    NPCBodyProvider _currentProvider;
    BodyRegion _currentRegion = BodyRegion.Torso;
    bool _open;

    // remember cursor state
    CursorLockMode _prevLock;
    bool _prevVisible;

    void Awake()
    {
        if (rootPanel != null) rootPanel.SetActive(false);
        WireButtons();
        _audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    void OnEnable()
    {
        if (EnergyManager.Instance != null)
            EnergyManager.Instance.OnChanged += HandleEnergyChanged;
        RefreshButtonsByEnergy();
    }

    void OnDisable()
    {
        if (EnergyManager.Instance != null)
            EnergyManager.Instance.OnChanged -= HandleEnergyChanged;
    }

    void HandleEnergyChanged(int cur, int max) => RefreshButtonsByEnergy();

    void RefreshButtonsByEnergy()
    {
        bool canClick = (EnergyManager.Instance == null) ? true : EnergyManager.Instance.HasEnergy;
        SetButtonsInteractable(canClick);
    }

    void SetButtonsInteractable(bool enabled)
    {
        if (headBtn) headBtn.interactable = enabled;
        if (torsoBtn) torsoBtn.interactable = enabled;
        if (leftArmBtn) leftArmBtn.interactable = enabled;
        if (rightArmBtn) rightArmBtn.interactable = enabled;
        if (leftLegBtn) leftLegBtn.interactable = enabled;
        if (rightLegBtn) rightLegBtn.interactable = enabled;
    }

    void WireButtons()
    {
        if (headBtn) headBtn.onClick.AddListener(() => SelectRegion(BodyRegion.Head));
        if (torsoBtn) torsoBtn.onClick.AddListener(() => SelectRegion(BodyRegion.Torso));
        if (leftArmBtn) leftArmBtn.onClick.AddListener(() => SelectRegion(BodyRegion.LeftArm));
        if (rightArmBtn) rightArmBtn.onClick.AddListener(() => SelectRegion(BodyRegion.RightArm));
        if (leftLegBtn) leftLegBtn.onClick.AddListener(() => SelectRegion(BodyRegion.LeftLeg));
        if (rightLegBtn) rightLegBtn.onClick.AddListener(() => SelectRegion(BodyRegion.RightLeg));
    }

    public void Open(NPCBodyProvider provider, string npcName)
    {
        _currentProvider = provider;
        if (npcNameText) npcNameText.text = npcName;
        Toggle(true);

        // default selection (ไม่หัก energy ตอนเปิด)
        SelectRegionInternal(BodyRegion.Torso, spend: false);
        RefreshButtonsByEnergy();
    }

    public void Close()
    {
        Toggle(false);
    }

    // >>>>>>> ONLY ONE Toggle(bool) EXISTS <<<<<<<
    private void Toggle(bool show)
    {
        // show/hide panel
        if (rootPanel) rootPanel.SetActive(show);

        // cursor lock/visibility
        if (show)
        {
            _prevLock = Cursor.lockState;
            _prevVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = _prevLock;
            Cursor.visible = _prevVisible;
        }

        // disable/enable player controllers while UI open
        if (disableWhileOpen != null)
        {
            foreach (var c in disableWhileOpen)
            {
                if (c == null) continue;
                c.enabled = !show;
            }
        }

        // clear highlight when closing
        if (!show && highlighter != null) highlighter.Clear();

        _open = show;
    }

    void Update()
    {
        if (!_open) return;
        if (closeOnEsc && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    // --------- PUBLIC from buttons (หัก energy) -----------
    public void SelectRegion(BodyRegion region) => SelectRegionInternal(region, spend: true);

    // --------- CORE: select + optional spend ---------------
    void SelectRegionInternal(BodyRegion region, bool spend)
    {
        // 1) หัก Energy ตอนกดปุ่ม
        if (spendOnSelect && spend && EnergyManager.Instance != null)
        {
            if (!EnergyManager.Instance.TrySpend(1))
            {
                if (noEnergySfx != null) _audio.PlayOneShot(noEnergySfx);
                RefreshButtonsByEnergy();
                Debug.Log("Not enough energy to inspect this part.");
                return;
            }
            else
            {
                if (spendOKSfx != null) _audio.PlayOneShot(spendOKSfx);
            }
        }

        // 2) เปลี่ยนส่วน + แสดงรายละเอียด
        if (_currentProvider == null) return;
        _currentRegion = region;
        var status = _currentProvider.Get(region);

        if (partTitleText) partTitleText.text = RegionToEnglish(region);
        if (detailText) detailText.text = BuildDetail(status);
        if (highlighter != null) highlighter.Highlight(region);

        // 3) อัปเดตปุ่มตาม Energy ล่าสุด (กรณีเหลือ 0 หลังเพิ่งกด)
        RefreshButtonsByEnergy();
    }

    string BuildDetail(RegionStatus s)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(s.hasWound ? bulletWoundYes : bulletWoundNo);
        sb.AppendLine(s.hasRash ? bulletRashYes : bulletRashNo);
        sb.AppendLine(s.isCoughing ? bulletCoughYes : bulletCoughNo);
        sb.AppendLine(s.highFever ? bulletFeverYes : bulletFeverNo);
        if (!string.IsNullOrWhiteSpace(s.note))
            sb.AppendLine(bulletNotePrefix + s.note);
        return sb.ToString();
    }

    string RegionToEnglish(BodyRegion r)
    {
        switch (r)
        {
            case BodyRegion.Head: return labelHead;
            case BodyRegion.Torso: return labelTorso;
            case BodyRegion.LeftArm: return labelLeftArm;
            case BodyRegion.RightArm: return labelRightArm;
            case BodyRegion.LeftLeg: return labelLeftLeg;
            case BodyRegion.RightLeg: return labelRightLeg;
            default: return r.ToString();
        }
    }
}
