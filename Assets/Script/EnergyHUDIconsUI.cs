using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// EnergyHUDStackUI (POP VERSION)
///
/// ตามที่ต้องการ:
/// - ถ้ามี energy = 3 charges → โชว์ไอคอน 3 อัน
/// - เมื่อใช้ energy → ลบไอคอนออก 1 อัน (จำนวนไอคอน = Current)
/// - เมื่อเพิ่ม energy → เพิ่มไอคอน 1 อัน และ "เด้ง" (pop) ตอนเพิ่ม
///
/// ใช้ค่า:
/// - EnergyManager.Instance.Current
/// - EnergyManager.Instance.maxCharges
/// - (ถ้ามี) event OnChanged(int cur,int max)
///
/// Notes:
/// - โหมดหลักคือ showMaxAsEmpty=false (โชว์เฉพาะจำนวน Current)
/// - ถ้าอยากโชว์ max แบบ empty ให้เปิด showMaxAsEmpty=true (ไม่ใช่ที่คุณต้องการตอนนี้)
/// </summary>
public class EnergyHUDStackUI_Pop : MonoBehaviour
{
    [Header("UI")]
    public Transform iconContainer;
    public GameObject iconPrefab; // must have Image

    [Header("Icon")]
    public Sprite energySprite;

    [Tooltip("(ไม่ใช่โหมดที่คุณต้องการตอนนี้) ถ้า true: โชว์ครบ max แล้วที่เหลือเป็น emptySprite")]
    public bool showMaxAsEmpty = false;
    public Sprite emptySprite;

    [Header("Optional Text")]
    public TextMeshProUGUI countText;

    [Header("Update")]
    public bool usePollingFallback = true;
    public float pollingInterval = 0.25f;

    [Header("Pop Animation")]
    public bool popOnAdd = true;
    [Tooltip("เวลาที่ใช้เด้ง (วินาที)")]
    public float popDuration = 0.12f;
    [Tooltip("สเกลตอนเด้งขึ้น")]
    public float popScale = 1.25f;

    private int _lastCur = -999;
    private int _lastMax = -999;
    private float _nextPollTime = 0f;

    void Awake()
    {
        if (iconContainer == null) iconContainer = transform;
    }

    void OnEnable()
    {
        TrySubscribe(true);
        Refresh(force: true);
    }

    void OnDisable()
    {
        TrySubscribe(false);
    }

    void Update()
    {
        if (!usePollingFallback) return;
        if (Time.time < _nextPollTime) return;
        _nextPollTime = Time.time + Mathf.Max(0.05f, pollingInterval);
        Refresh(force: false);
    }

    private void TrySubscribe(bool subscribe)
    {
        try
        {
            if (EnergyManager.Instance == null) return;
            if (subscribe) EnergyManager.Instance.OnChanged += OnEnergyChanged;
            else EnergyManager.Instance.OnChanged -= OnEnergyChanged;
        }
        catch { }
    }

    private void OnEnergyChanged(int cur, int max)
    {
        Apply(cur, max);
    }

    private void Refresh(bool force)
    {
        if (EnergyManager.Instance == null) return;

        int cur = EnergyManager.Instance.Current;
        int max = EnergyManager.Instance.maxCharges;

        if (!force && cur == _lastCur && max == _lastMax) return;
        Apply(cur, max);
    }

    private void Apply(int cur, int max)
    {
        _lastCur = cur;
        _lastMax = max;

        if (max < 0) max = 0;
        if (cur < 0) cur = 0;
        if (cur > max) cur = max;

        if (showMaxAsEmpty)
            EnsureIconsMaxMode(cur, max);
        else
            EnsureIconsStackMode(cur);

        if (countText != null)
            countText.text = $"{cur}/{max}";
    }

    // ---------------- Mode A: show only current (REMOVE/ADD icons) ----------------
    private void EnsureIconsStackMode(int cur)
    {
        if (iconPrefab == null || iconContainer == null) return;

        int childCount = iconContainer.childCount;

        // Remove extra icons if current decreased
        for (int i = childCount - 1; i >= cur; i--)
        {
            var t = iconContainer.GetChild(i);
            if (t != null) Destroy(t.gameObject);
        }

        // Add icons if current increased
        int beforeAddCount = iconContainer.childCount;
        for (int i = beforeAddCount; i < cur; i++)
        {
            var go = Instantiate(iconPrefab, iconContainer);
            go.name = $"EnergyIcon_{i}";
            ApplySprite(go, energySprite);

            if (popOnAdd)
                StartCoroutine(Pop(go.transform));
        }

        // Ensure all sprites are correct
        for (int i = 0; i < iconContainer.childCount; i++)
        {
            var go = iconContainer.GetChild(i).gameObject;
            ApplySprite(go, energySprite);
        }
    }

    // ---------------- Mode B: show max with empty (optional) ----------------
    private void EnsureIconsMaxMode(int cur, int max)
    {
        if (iconPrefab == null || iconContainer == null) return;

        // Ensure child count = max
        for (int i = iconContainer.childCount; i < max; i++)
        {
            var go = Instantiate(iconPrefab, iconContainer);
            go.name = $"EnergyIcon_{i}";
        }
        for (int i = iconContainer.childCount - 1; i >= max; i--)
        {
            var t = iconContainer.GetChild(i);
            if (t != null) Destroy(t.gameObject);
        }

        // Fill/empty
        for (int i = 0; i < iconContainer.childCount; i++)
        {
            var go = iconContainer.GetChild(i).gameObject;
            bool filled = i < cur;
            ApplySprite(go, filled ? energySprite : emptySprite);
        }
    }

    private void ApplySprite(GameObject go, Sprite sprite)
    {
        if (go == null) return;
        var img = go.GetComponent<Image>();
        if (img == null) img = go.GetComponentInChildren<Image>(true);
        if (img == null) return;

        img.sprite = sprite;
        img.enabled = (sprite != null);
    }

    private IEnumerator Pop(Transform t)
    {
        if (t == null) yield break;

        Vector3 baseScale = Vector3.one;
        t.localScale = baseScale * Mathf.Max(0.01f, popScale);

        float dur = Mathf.Max(0.01f, popDuration);
        float half = dur * 0.5f;

        // Ease down to 1
        float time = 0f;
        while (time < dur)
        {
            time += Time.unscaledDeltaTime;
            float u = time / dur;
            // simple easeOutBack-ish
            float s = Mathf.Lerp(popScale, 1f, u);
            t.localScale = baseScale * s;
            yield return null;
        }
        t.localScale = baseScale;
    }
}
