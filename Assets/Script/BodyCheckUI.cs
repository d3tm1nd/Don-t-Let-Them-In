using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// BodyCheckUI (patched):
/// - เมื่อเลือก "ยิงทิ้ง" จะลบข้อมูล NPC ออกจาก NPCDataManager.acceptedNPCs ด้วย
/// - ใช้ npcStableId จาก NPCHealthProfile ในการ match (fallback: prefab.name)
/// - (optional) สามารถ mark ตายใน DeathRegistry ได้ถ้ามีในโปรเจกต์
/// </summary>
public class BodyCheckUI : MonoBehaviour
{
    public static BodyCheckUI Instance { get; private set; }

    [Header("Root (Panel)")]
    [SerializeField] private GameObject root;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI tempText;

    [Header("Buttons (Checks)")]
    [SerializeField] private Button eyeButton;
    [SerializeField] private Button handButton;
    [SerializeField] private Button tempButton;
    [SerializeField] private Button closeButton;

    [Header("Decision Panel (หลังตรวจครบ)")]
    [SerializeField] private GameObject decisionPanel;
    [SerializeField] private Button shootButton;
    [SerializeField] private Button keepButton;

    [Header("Images (Results)")]
    [SerializeField] private Image eyeLeftImg;
    [SerializeField] private Image eyeRightImg;
    [SerializeField] private Image handLeftImg;
    [SerializeField] private Image handRightImg;
    [SerializeField] private Image tempIconImg; // optional

    [Header("Sprites")]
    [SerializeField] private Sprite eyeNormalSprite;
    [SerializeField] private Sprite eyeAbnormalSprite;
    [SerializeField] private Sprite handNormalSprite;
    [SerializeField] private Sprite handAbnormalSprite;
    [SerializeField] private Sprite tempIconSprite; // optional

    [Header("Disable scripts while UI open (ลากใส่)")]
    [Tooltip("ลาก InteractionRay / MouseLook / PlayerController ที่ชอบล็อกเมาส์ไว้")]
    [SerializeField] private MonoBehaviour[] disableWhileOpen;

    [Header("Cursor Settings")]
    [SerializeField] private bool showCursorWhenOpen = true;
    [SerializeField] private CursorLockMode lockModeWhenClosed = CursorLockMode.Locked;
    [SerializeField] private bool hideCursorWhenClosed = true;

    [Header("Shoot Options")]
    [Tooltip("ถ้าเปิด จะพยายาม MarkDead ลง DeathRegistry ด้วย (ถ้ามีในโปรเจกต์)")]
    public bool markDeadInRegistry = true;

    private NPCHealthProfile _target;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (root == null) root = gameObject;
        root.SetActive(false);

        if (eyeButton != null) eyeButton.onClick.AddListener(OnCheckEyes);
        if (handButton != null) handButton.onClick.AddListener(OnCheckHands);
        if (tempButton != null) tempButton.onClick.AddListener(OnCheckTemperature);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (shootButton != null) shootButton.onClick.AddListener(OnShootNPC);
        if (keepButton != null) keepButton.onClick.AddListener(OnKeepNPC);

        SetDecisionVisible(false);
    }

    void OnEnable()
    {
        if (EnergyManager.Instance != null)
            EnergyManager.Instance.OnChanged += OnEnergyChanged;
    }

    void OnDisable()
    {
        if (EnergyManager.Instance != null)
            EnergyManager.Instance.OnChanged -= OnEnergyChanged;
    }

    public bool IsOpen => root != null && root.activeSelf;

    public void Open(NPCHealthProfile target)
    {
        _target = target;
        if (_target == null) return;

        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        _target.EnsureGeneratedForDay(day);

        root.SetActive(true);
        EnterUIMode(true);

        if (npcNameText != null) npcNameText.text = _target.npcDisplayName;

        ShowEyes(false);
        ShowHands(false);
        ShowTemp(false);
        if (tempText != null) tempText.text = string.Empty;

        RefreshEnergyUI();
        RefreshLockState();
        RefreshDecisionState();
    }

    public void Close()
    {
        EnterUIMode(false);
        root.SetActive(false);
        _target = null;
        SetDecisionVisible(false);
    }

    // ===================== ตรวจ (ลด Energy 1) =====================

    public void OnCheckEyes()
    {
        if (!TrySpendEnergy()) return;
        if (_target == null) return;

        _target.MarkEyesChecked();

        ShowEyes(true);
        ShowHands(false);
        ShowTemp(false);

        if (eyeLeftImg != null)
            eyeLeftImg.sprite = _target.leftEyeAbnormal ? eyeAbnormalSprite : eyeNormalSprite;
        if (eyeRightImg != null)
            eyeRightImg.sprite = _target.rightEyeAbnormal ? eyeAbnormalSprite : eyeNormalSprite;

        RefreshDecisionState();
    }

    public void OnCheckHands()
    {
        if (!TrySpendEnergy()) return;
        if (_target == null) return;

        _target.MarkHandsChecked();

        ShowEyes(false);
        ShowHands(true);
        ShowTemp(false);

        if (handLeftImg != null)
            handLeftImg.sprite = _target.leftHandAbnormal ? handAbnormalSprite : handNormalSprite;
        if (handRightImg != null)
            handRightImg.sprite = _target.rightHandAbnormal ? handAbnormalSprite : handNormalSprite;

        RefreshDecisionState();
    }

    public void OnCheckTemperature()
    {
        if (!TrySpendEnergy()) return;
        if (_target == null) return;

        _target.MarkTempChecked();

        ShowEyes(false);
        ShowHands(false);
        ShowTemp(true);

        if (tempText != null)
            tempText.text = $"{_target.currentTemperature:F1} °C";

        if (tempIconImg != null)
        {
            tempIconImg.sprite = tempIconSprite;
            tempIconImg.enabled = (tempIconSprite != null);
        }

        RefreshDecisionState();
    }

    // ===================== Decision (ยิง/ไม่ยิง) =====================

    private void RefreshDecisionState()
    {
        if (_target == null)
        {
            SetDecisionVisible(false);
            return;
        }

        bool ready = _target.IsFullyChecked;
        SetDecisionVisible(ready);

        if (shootButton != null) shootButton.interactable = ready;
        if (keepButton != null) keepButton.interactable = ready;
    }

    private void SetDecisionVisible(bool show)
    {
        if (decisionPanel != null) decisionPanel.SetActive(show);
    }

    private void OnShootNPC()
    {
        if (_target == null) return;
        if (!_target.IsFullyChecked) return;

        // 1) ลบข้อมูลจาก NPCDataManager ก่อน (กัน spawn กลับมา)
        RemoveFromNPCDataManager(_target);

        // 2) optional: mark dead ลง DeathRegistry (ถ้ามี)
        if (markDeadInRegistry)
            TryMarkDead(_target);

        // 3) ลบ GameObject ในฉาก
        Destroy(_target.gameObject);

        Close();
    }

    private void OnKeepNPC()
    {
        if (_target == null) return;
        if (!_target.IsFullyChecked) return;
        Close();
    }

    // ===================== Remove from NPCDataManager =====================

    private void RemoveFromNPCDataManager(NPCHealthProfile target)
    {
        if (target == null) return;
        if (NPCDataManager.Instance == null) return;
        if (NPCDataManager.Instance.acceptedNPCs == null) return;

        string victimId = string.IsNullOrEmpty(target.npcStableId) ? target.gameObject.name : target.npcStableId;

        var list = NPCDataManager.Instance.acceptedNPCs;
        int removed = 0;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (entry.prefab == null) continue;

            var prof = entry.prefab.GetComponent<NPCHealthProfile>();
            string id = (prof != null && !string.IsNullOrEmpty(prof.npcStableId)) ? prof.npcStableId : entry.prefab.name;

            if (id == victimId)
            {
                list.RemoveAt(i);
                removed++;
            }
        }

        if (removed > 0)
            Debug.Log($"🗑️ Removed {removed} NPCData entries for id={victimId}");
        else
            Debug.LogWarning($"⚠️ No NPCData entry removed for id={victimId} (check npcStableId on prefab vs instance)");
    }

    private void TryMarkDead(NPCHealthProfile target)
    {
        // ใช้ reflection เพื่อไม่บังคับว่าต้องมี DeathRegistry ในโปรเจกต์
        var regType = System.Type.GetType("DeathRegistry");
        if (regType == null) return;

        // DeathRegistry.Instance
        var instProp = regType.GetProperty("Instance");
        object inst = instProp != null ? instProp.GetValue(null) : null;
        if (inst == null) return;

        var markMethod = regType.GetMethod("MarkDead");
        if (markMethod == null) return;

        string id = string.IsNullOrEmpty(target.npcStableId) ? target.gameObject.name : target.npcStableId;
        markMethod.Invoke(inst, new object[] { id });
    }

    // ===================== Energy / Lock =====================

    private bool TrySpendEnergy()
    {
        if (EnergyManager.Instance == null)
        {
            Debug.LogError("❌ EnergyManager ไม่พบ");
            return false;
        }

        if (EnergyManager.Instance.Current <= 0)
        {
            RefreshLockState();
            return false;
        }

        bool ok = EnergyManager.Instance.TrySpend(1);
        RefreshEnergyUI();
        RefreshLockState();
        return ok;
    }

    private void OnEnergyChanged(int cur, int max)
    {
        RefreshEnergyUI();
        RefreshLockState();
        RefreshDecisionState();
    }

    private void RefreshEnergyUI()
    {
        if (energyText == null || EnergyManager.Instance == null) return;
        energyText.text = $"Energy: {EnergyManager.Instance.Current}/{EnergyManager.Instance.maxCharges}";
    }

    private void RefreshLockState()
    {
        bool canCheck = (EnergyManager.Instance != null && EnergyManager.Instance.Current > 0);

        if (eyeButton != null) eyeButton.interactable = canCheck;
        if (handButton != null) handButton.interactable = canCheck;
        if (tempButton != null) tempButton.interactable = canCheck;
    }

    // ===================== Cursor + Disable Scripts =====================

    private void EnterUIMode(bool uiOpen)
    {
        if (uiOpen && showCursorWhenOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = lockModeWhenClosed;
            Cursor.visible = !hideCursorWhenClosed;
        }

        if (disableWhileOpen != null)
        {
            for (int i = 0; i < disableWhileOpen.Length; i++)
            {
                if (disableWhileOpen[i] != null)
                    disableWhileOpen[i].enabled = !uiOpen;
            }
        }
    }

    // ===================== Show / Hide groups =====================

    private void ShowEyes(bool show)
    {
        if (eyeLeftImg != null) eyeLeftImg.gameObject.SetActive(show);
        if (eyeRightImg != null) eyeRightImg.gameObject.SetActive(show);
    }

    private void ShowHands(bool show)
    {
        if (handLeftImg != null) handLeftImg.gameObject.SetActive(show);
        if (handRightImg != null) handRightImg.gameObject.SetActive(show);
    }

    private void ShowTemp(bool show)
    {
        if (tempText != null) tempText.gameObject.SetActive(show);
        if (tempIconImg != null) tempIconImg.gameObject.SetActive(show);
    }
}
