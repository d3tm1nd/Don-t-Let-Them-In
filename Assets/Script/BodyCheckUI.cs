using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BodyCheckUI : MonoBehaviour
{
    public static BodyCheckUI Instance { get; private set; }

    [Header("Root (Panel)")]
    [SerializeField] private GameObject root;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI tempText;

    [Header("Buttons")]
    [SerializeField] private Button eyeButton;
    [SerializeField] private Button handButton;
    [SerializeField] private Button tempButton;
    [SerializeField] private Button closeButton;

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

    private NPCHealthProfile _target;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // กันลืมลาก root: ถ้าว่างให้ถือว่า root คือ GameObject นี้
        if (root == null) root = gameObject;

        // ปิด UI ตั้งแต่เริ่มเกม
        root.SetActive(false);

        // ผูกปุ่ม (กันลืมตั้ง OnClick ใน Inspector)
        if (eyeButton != null) eyeButton.onClick.AddListener(OnCheckEyes);
        if (handButton != null) handButton.onClick.AddListener(OnCheckHands);
        if (tempButton != null) tempButton.onClick.AddListener(OnCheckTemperature);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
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

    public void Open(NPCHealthProfile target)
    {
        _target = target;
        if (_target == null) return;

        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        _target.EnsureGeneratedForDay(day);

        root.SetActive(true);
        EnterUIMode(true);

        if (npcNameText != null) npcNameText.text = _target.npcDisplayName;

        // ซ่อนผลทุกอย่างก่อน
        ShowEyes(false);
        ShowHands(false);
        ShowTemp(false);
        if (tempText != null) tempText.text = string.Empty;

        RefreshEnergyUI();
        RefreshLockState();
    }

    public void Close()
    {
        EnterUIMode(false);
        root.SetActive(false);
        _target = null;
    }

    // ===================== ตรวจ: ตา / มือ / อุณหภูมิ =====================

    public void OnCheckEyes()
    {
        if (!TrySpendEnergy()) return;
        if (_target == null) return;

        ShowEyes(true);
        ShowHands(false);
        ShowTemp(false);

        if (eyeLeftImg != null)
            eyeLeftImg.sprite = _target.leftEyeAbnormal ? eyeAbnormalSprite : eyeNormalSprite;

        if (eyeRightImg != null)
            eyeRightImg.sprite = _target.rightEyeAbnormal ? eyeAbnormalSprite : eyeNormalSprite;
    }

    public void OnCheckHands()
    {
        if (!TrySpendEnergy()) return;
        if (_target == null) return;

        ShowEyes(false);
        ShowHands(true);
        ShowTemp(false);

        if (handLeftImg != null)
            handLeftImg.sprite = _target.leftHandAbnormal ? handAbnormalSprite : handNormalSprite;

        if (handRightImg != null)
            handRightImg.sprite = _target.rightHandAbnormal ? handAbnormalSprite : handNormalSprite;
    }

    public void OnCheckTemperature()
    {
        if (!TrySpendEnergy()) return;
        if (_target == null) return;

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

    // ===================== Show / Hide Groups =====================

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