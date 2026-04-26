using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// BodyCheckUI (SINGLE)
/// - ตรวจตา/มือ/อุณหภูมิ (ใช้พลังงาน)
/// - UI โชว์รูป "ตา 1 รูป" และ "มือ 1 รูป" (ไม่แยกซ้าย/ขวา)
/// - รูปที่ใช้โชว์ ดึงจากตัว NPC ผ่านคอมโพเนนต์ NPCBodyCheckUISpriteSet
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

    [Header("Decision Panel")]
    [SerializeField] private GameObject decisionPanel;
    [SerializeField] private bool disableDecisionPanelBackgroundRaycast = true;
    [SerializeField] private Button shootButton;
    [SerializeField] private Button keepButton;

    [Header("Images (Results)")]
    [SerializeField] private Image eyeImg;
    [SerializeField] private Image handImg;
    [SerializeField] private Image tempIconImg;

    [Header("NPC UI Sprite Set")]
    [SerializeField] private bool warnIfMissingNpcSpriteSet = true;

    [Header("Disable scripts while UI open (ลากใส่)")]
    [SerializeField] private MonoBehaviour[] disableWhileOpen;

    [Header("Decision Options")]
    [SerializeField] private bool decisionAvailableImmediately = true;

    [Header("Cursor Settings")]
    [SerializeField] private bool showCursorWhenOpen = true;
    [SerializeField] private CursorLockMode lockModeWhenClosed = CursorLockMode.Locked;
    [SerializeField] private bool hideCursorWhenClosed = true;

    [Header("Shoot Spawn")]
    [SerializeField] private GameObject corpsePrefab;
    [SerializeField] private GameObject bodyBagPrefab;
    [SerializeField] private bool autoChooseByNpcTag = true;
    [SerializeField] private bool defaultUseBodyBag = true;
    [SerializeField] private bool ghostUsesBodyBag = true;
    [SerializeField] private bool humanUsesBodyBag = false;

    [Header("Spawn Adjust")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    [SerializeField] private Vector3 spawnEulerOffset = Vector3.zero;
    [SerializeField] private bool destroyNpcAfterSpawn = true;

    [Header("Spawn Position Fix")]
    [SerializeField] private bool useFeetPosition = true;
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundSnapMaxDistance = 3f;
    [SerializeField] private float groundYOffset = 0.02f;

    [Header("SFX (2D)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip shootSfx;
    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 1f;

    private NPCHealthProfile _target;
    private NPCBodyCheckUISpriteSet _spriteSetCache;

    public bool IsOpen => root != null && root.activeSelf;

    private void Awake()
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

        ApplyDecisionPanelRaycastSetup();
        SetDecisionVisible(false);

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        if (EnergyManager.Instance != null)
            EnergyManager.Instance.OnChanged += OnEnergyChanged;
    }

    private void OnDisable()
    {
        if (EnergyManager.Instance != null)
            EnergyManager.Instance.OnChanged -= OnEnergyChanged;
    }

    public void Open(NPCHealthProfile target)
    {
        _target = target;
        _spriteSetCache = null;

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
        _spriteSetCache = null;
        SetDecisionVisible(false);
    }

    public void OnCheckEyes()
    {
        if (!TrySpendEnergy()) return;
        if (_target == null) return;

        _target.MarkEyesChecked();

        ShowEyes(true);
        ShowHands(false);
        ShowTemp(false);

        var set = GetSpriteSet();
        if (set != null && eyeImg != null)
            eyeImg.sprite = _target.eyeAbnormal ? set.eyeAbnormal : set.eyeNormal;

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

        var set = GetSpriteSet();
        if (set != null && handImg != null)
            handImg.sprite = _target.handAbnormal ? set.handAbnormal : set.handNormal;

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

        var set = GetSpriteSet();
        if (tempIconImg != null)
        {
            tempIconImg.sprite = (set != null) ? set.tempIcon : null;
            tempIconImg.enabled = (tempIconImg.sprite != null);
        }

        RefreshDecisionState();
    }

    private void RefreshDecisionState()
    {
        if (_target == null)
        {
            SetDecisionVisible(false);
            return;
        }

        bool ready = decisionAvailableImmediately || _target.IsFullyChecked;

        SetDecisionVisible(ready);
        if (shootButton != null) shootButton.interactable = ready;
        if (keepButton != null) keepButton.interactable = ready;
    }

    private void SetDecisionVisible(bool show)
    {
        if (decisionPanel != null) decisionPanel.SetActive(show);
        if (show) ApplyDecisionPanelRaycastSetup();
    }

    private void OnShootNPC()
    {
        if (_target == null) return;

        if (shootSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(shootSfx, shootVolume);

        if (shootButton != null) shootButton.interactable = false;
        if (keepButton != null) keepButton.interactable = false;

        Vector3 pos = GetSpawnPosition(_target.gameObject) + spawnOffset;
        Quaternion rot = _target.transform.rotation * Quaternion.Euler(spawnEulerOffset);

        bool useBag = DetermineUseBodyBagByTag(_target.gameObject);
        GameObject prefab = useBag ? bodyBagPrefab : corpsePrefab;
        if (prefab != null)
            Instantiate(prefab, pos, rot);

        RemoveFromNPCDataManager(_target);

        if (destroyNpcAfterSpawn && _target != null)
            Destroy(_target.gameObject);

        Close();
    }

    private void OnKeepNPC()
    {
        if (_target == null) return;
        Close();
    }

    private bool DetermineUseBodyBagByTag(GameObject npcGO)
    {
        if (!autoChooseByNpcTag)
            return defaultUseBodyBag;

        if (npcGO == null) return defaultUseBodyBag;

        Transform rootT = npcGO.transform.root;

        if (npcGO.CompareTag("Ghost") || (rootT != null && rootT.CompareTag("Ghost")))
            return ghostUsesBodyBag;

        if (npcGO.CompareTag("Human") || (rootT != null && rootT.CompareTag("Human")))
            return humanUsesBodyBag;

        return defaultUseBodyBag;
    }

    private Vector3 GetSpawnPosition(GameObject npcGO)
    {
        if (npcGO == null) return Vector3.zero;

        Vector3 p = npcGO.transform.position;

        if (useFeetPosition)
        {
            var cc = npcGO.GetComponentInParent<CharacterController>();
            if (cc != null)
            {
                var b = cc.bounds;
                p = b.center;
                p.y = b.min.y;
            }
            else
            {
                var col = npcGO.GetComponentInParent<Collider>();
                if (col != null)
                {
                    var b = col.bounds;
                    p = b.center;
                    p.y = b.min.y;
                }
            }
        }

        if (snapToGround)
        {
            Vector3 origin = p + Vector3.up * 0.5f;
            float maxDist = Mathf.Max(0.1f, groundSnapMaxDistance);

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDist, groundMask, QueryTriggerInteraction.Ignore))
            {
                p = hit.point;
                p.y += groundYOffset;
            }
        }

        return p;
    }

    private void RemoveFromNPCDataManager(NPCHealthProfile target)
    {
        if (target == null) return;
        if (NPCDataManager.Instance == null) return;
        if (NPCDataManager.Instance.acceptedNPCs == null) return;

        string victimId = string.IsNullOrEmpty(target.npcStableId) ? target.gameObject.name : target.npcStableId;
        var list = NPCDataManager.Instance.acceptedNPCs;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (entry.prefab == null) continue;

            var prof = entry.prefab.GetComponent<NPCHealthProfile>();
            string id = (prof != null && !string.IsNullOrEmpty(prof.npcStableId)) ? prof.npcStableId : entry.prefab.name;

            if (id == victimId)
                list.RemoveAt(i);
        }
    }

    private void ApplyDecisionPanelRaycastSetup()
    {
        if (!disableDecisionPanelBackgroundRaycast) return;
        if (decisionPanel == null) return;

        var graphics = decisionPanel.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            var g = graphics[i];
            if (g == null) continue;

            if (g.GetComponent<Button>() != null)
                continue;

            g.raycastTarget = false;
        }
    }

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

    private void ShowEyes(bool show)
    {
        if (eyeImg != null) eyeImg.gameObject.SetActive(show);
    }

    private void ShowHands(bool show)
    {
        if (handImg != null) handImg.gameObject.SetActive(show);
    }

    private void ShowTemp(bool show)
    {
        if (tempText != null) tempText.gameObject.SetActive(show);
        if (tempIconImg != null) tempIconImg.gameObject.SetActive(show);
    }

    private NPCBodyCheckUISpriteSet GetSpriteSet()
    {
        if (_target == null) return null;
        if (_spriteSetCache != null) return _spriteSetCache;

        _spriteSetCache = _target.GetComponentInChildren<NPCBodyCheckUISpriteSet>(true);
        if (_spriteSetCache == null && warnIfMissingNpcSpriteSet)
            Debug.LogWarning($"⚠️ BodyCheckUI: NPC '{_target.name}' has no NPCBodyCheckUISpriteSet. UI images will not update.");

        return _spriteSetCache;
    }
}
