using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// BodyCheckUI (TAG-FIRST)
/// - Shoot spawns corpse/bodybag
/// - NO fade
/// - FOOT-SNAP
///
/// เพิ่มเติมจากเวอร์ชัน Tag-first:
/// - แก้ปัญหา "ถุงดำ spawn ผิดที่" โดยคำนวณตำแหน่งจาก "เท้า" ของ NPC (Collider/CharacterController bounds)
/// - (optional) snap ลงพื้นด้วย raycast เพื่อให้วางบนพื้นพอดี
///
/// การเลือกศพ/ถุงดำ:
/// - Tag = "Ghost" / "Human" (เช็คทั้งตัวเองและ root)
///
/// ✅ เพิ่มใหม่ (ตามที่ขอ):
/// - เล่นเสียงยิง (2D) ทันทีเมื่อกดปุ่ม Shoot/Interact
///   และไม่ผูกกับเสียง 3D ในฉาก
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
    [Tooltip("ปุ่มยิง/เก็บไว้ (สามารถตั้งให้แสดงทันทีหรือรอให้ตรวจครบ)")]
    [SerializeField] private GameObject decisionPanel;

    [Tooltip("ถ้า true: ปิด RaycastTarget ของพื้นหลัง decisionPanel เพื่อไม่ให้บังปุ่มตรวจ (Eyes/Hands/Temp) เมื่อ panel ทับกัน")]
    [SerializeField] private bool disableDecisionPanelBackgroundRaycast = true;

    [SerializeField] private Button shootButton;
    [SerializeField] private Button keepButton;

    [Header("Images (Results)")]
    [SerializeField] private Image eyeLeftImg;
    [SerializeField] private Image eyeRightImg;
    [SerializeField] private Image handLeftImg;
    [SerializeField] private Image handRightImg;
    [SerializeField] private Image tempIconImg;

    [Header("Sprites")]
    [SerializeField] private Sprite eyeNormalSprite;
    [SerializeField] private Sprite eyeAbnormalSprite;
    [SerializeField] private Sprite handNormalSprite;
    [SerializeField] private Sprite handAbnormalSprite;
    [SerializeField] private Sprite tempIconSprite;

    [Header("Disable scripts while UI open (ลากใส่)")]
    [Tooltip("ลาก InteractionRay / MouseLook / PlayerController ที่ชอบล็อกเมาส์ไว้")]
    [SerializeField] private MonoBehaviour[] disableWhileOpen;

    [Header("Decision Options")]
    [Tooltip("ถ้าเปิด: เปิดปุ่มยิง/เก็บไว้ทันทีเมื่อเปิด UI (ไม่ต้องตรวจครบ)")]
    [SerializeField] private bool decisionAvailableImmediately = true;

    [Header("Cursor Settings")]
    [SerializeField] private bool showCursorWhenOpen = true;
    [SerializeField] private CursorLockMode lockModeWhenClosed = CursorLockMode.Locked;
    [SerializeField] private bool hideCursorWhenClosed = true;

    [Header("Shoot Options")]
    [Tooltip("ถ้าเปิด จะพยายาม MarkDead ลง DeathRegistry ด้วย (ถ้ามีในโปรเจกต์)")]
    public bool markDeadInRegistry = true;

    [Header("Shoot Spawn (No Fade)")]
    [Tooltip("Prefab ศพ (corpse model)")]
    [SerializeField] private GameObject corpsePrefab;

    [Tooltip("Prefab ถุงดำ (body bag model)")]
    [SerializeField] private GameObject bodyBagPrefab;

    [Tooltip("ถ้า true: เลือกศพ/ถุงดำตาม Tag (Ghost/Human)\nถ้า false: ใช้ defaultUseBodyBag อย่างเดียว")]
    [SerializeField] private bool autoChooseByNpcTag = true;

    [Tooltip("ค่า fallback ถ้า Tag ไม่ตรงหรือไม่เจอ")]
    [SerializeField] private bool defaultUseBodyBag = true;

    [Tooltip("ผี (Tag=Ghost) ใช้ถุงดำไหม")]
    [SerializeField] private bool ghostUsesBodyBag = true;

    [Tooltip("คน (Tag=Human) ใช้ถุงดำไหม (ปกติ false = spawn ศพ)")]
    [SerializeField] private bool humanUsesBodyBag = false;

    [Tooltip("offset ตอน spawn (ใช้ปรับละเอียด เช่นยกขึ้นเล็กน้อย)")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    [Tooltip("หมุนเพิ่มตอน spawn (องศา)")]
    [SerializeField] private Vector3 spawnEulerOffset = Vector3.zero;

    [Tooltip("ถ้า true จะ destroy NPC ตัวเดิมหลัง spawn")]
    [SerializeField] private bool destroyNpcAfterSpawn = true;

    [Header("Spawn Position Fix")]
    [Tooltip("ถ้า true จะใช้จุดเท้า (bounds.min.y) ของ NPC เป็นจุด spawn")]
    [SerializeField] private bool useFeetPosition = true;

    [Tooltip("ถ้า true จะ raycast ลงพื้นเพื่อ snap ให้พอดีกับพื้น")]
    [SerializeField] private bool snapToGround = true;

    [Tooltip("Layer ที่ถือว่าเป็นพื้นสำหรับ snap (แนะนำ: Default/Terrain)\nถ้าไม่แน่ใจปล่อยเป็น Everything")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Tooltip("ระยะยิง ray ลงพื้นสูงสุด")]
    [SerializeField] private float groundSnapMaxDistance = 3f;

    [Tooltip("เผื่อยกขึ้นจากพื้นเล็กน้อยหลัง snap (กันจม)")]
    [SerializeField] private float groundYOffset = 0.02f;

    [Header("SFX (2D)")]
    [Tooltip("AudioSource สำหรับเล่นเสียง UI/เสียงผู้เล่น (2D). ถ้าไม่ใส่ ระบบจะ Add ให้อัตโนมัติ")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("เสียงยิงตอนกด Shoot")]
    [SerializeField] private AudioClip shootSfx;

    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 1f;

    private NPCHealthProfile _target;

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

        // ✅ Setup SFX source (2D)
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f; // 2D
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

        // ✅ เล่นเสียงยิง (2D) ทันทีตอนกด
        if (shootSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(shootSfx, shootVolume);

        // กันกดซ้ำ
        if (shootButton != null) shootButton.interactable = false;
        if (keepButton != null) keepButton.interactable = false;

        // เก็บตำแหน่ง/หมุนก่อนลบ
        Vector3 pos = GetSpawnPosition(_target.gameObject) + spawnOffset;
        Quaternion rot = _target.transform.rotation * Quaternion.Euler(spawnEulerOffset);

        // เลือกว่าจะ spawn อะไร
        bool useBag = DetermineUseBodyBagByTag(_target.gameObject);
        GameObject prefab = useBag ? bodyBagPrefab : corpsePrefab;
        if (prefab != null)
            Instantiate(prefab, pos, rot);

        // 1) ลบข้อมูลจาก NPCDataManager ก่อน (กัน spawn กลับมา)
        RemoveFromNPCDataManager(_target);

        // 2) optional: mark dead ลง DeathRegistry (ถ้ามี)
        if (markDeadInRegistry)
            TryMarkDead(_target);

        // 3) ลบ GameObject NPC ในฉาก
        if (destroyNpcAfterSpawn && _target != null)
            Destroy(_target.gameObject);

        Close();
    }

    private void OnKeepNPC()
    {
        if (_target == null) return;
        Close();
    }

    // ===================== Tag-first selection =====================
    private bool DetermineUseBodyBagByTag(GameObject npcGO)
    {
        if (!autoChooseByNpcTag)
            return defaultUseBodyBag;

        if (npcGO == null) return defaultUseBodyBag;

        Transform rootT = npcGO.transform != null ? npcGO.transform.root : null;

        // Ghost
        if (npcGO.CompareTag("Ghost") || (rootT != null && rootT.CompareTag("Ghost")))
            return ghostUsesBodyBag;

        // Human
        if (npcGO.CompareTag("Human") || (rootT != null && rootT.CompareTag("Human")))
            return humanUsesBodyBag;

        return defaultUseBodyBag;
    }

    // ===================== Spawn position (feet + ground snap) =====================
    private Vector3 GetSpawnPosition(GameObject npcGO)
    {
        if (npcGO == null) return Vector3.zero;

        // default fallback
        Vector3 p = npcGO.transform.position;

        if (useFeetPosition)
        {
            // 1) CharacterController bounds
            var cc = npcGO.GetComponentInParent<CharacterController>();
            if (cc != null)
            {
                var b = cc.bounds;
                p = b.center;
                p.y = b.min.y;
            }
            else
            {
                // 2) Collider bounds (prefer root collider)
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
            // ยิง ray จากเหนือจุดเท้าลงไป
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

        var instProp = regType.GetProperty("Instance");
        object inst = instProp != null ? instProp.GetValue(null) : null;
        if (inst == null) return;

        var markMethod = regType.GetMethod("MarkDead");
        if (markMethod == null) return;

        string id = string.IsNullOrEmpty(target.npcStableId) ? target.gameObject.name : target.npcStableId;
        markMethod.Invoke(inst, new object[] { id });
    }

    // ===================== Decision Panel Raycast =====================
    private void ApplyDecisionPanelRaycastSetup()
    {
        if (!disableDecisionPanelBackgroundRaycast) return;
        if (decisionPanel == null) return;

        var graphics = decisionPanel.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            var g = graphics[i];
            if (g == null) continue;

            // ถ้า object นี้เป็นปุ่ม ให้ปล่อยไว้
            if (g.GetComponent<Button>() != null)
                continue;

            g.raycastTarget = false;
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
