using UnityEngine;
using UnityEngine.SceneManagement;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance;

    public enum GamePhase { Morning, Event, Night }
    public GamePhase currentPhase = GamePhase.Morning;

    [Header("Gameplay")]
    public int energy = 4;
    public int maxNPCs = 5;
    public int currentDay = 1;              // Day1 เริ่มที่ 1
    public int NPCAcceptedCount = 0;
    public bool DinnerCompleted = false;

    [Header("Scene Names (single)")]
    [Tooltip("ชื่อฉากกลางวันแบบ fallback ถ้าไม่ใช้ per-day scenes หรือ index ว่าง")]
    public string daySceneName = "dayscene";
    public string daySceneName2 = "dayscene2";   // ใช้เฉพาะโหมด single เป็นตัวเลือกหลัง Morning
    public string nightSceneName = "nightscene";
    public string morningSceneName = "morningscene";

    [Header("Routing Options (single)")]
    [Tooltip("ถ้าเปิด: เมื่อจบ Morning จะไป dayscene2 แทน dayscene (ใช้เฉพาะเมื่อปิด per-day scenes)")]
    public bool useDayScene2AfterMorning = false;

    [Header("Per-Day Scenes (แนะนำ)")]
    [Tooltip("เปิดใช้ mapping ตามวัน: dayScenes[n], nightScenes[n], morningScenes[n]")]
    public bool usePerDayScenes = true;
    [Tooltip("ดัชนี 0 = Day1, 1 = Day2, ... ช่องว่างหรือ null จะ fallback ไป single scene")]
    public string[] dayScenes;       // เช่น [0]=dayscene, [1]=dayscene2, ...
    public string[] nightScenes;     // เช่น [0]=nightscene, [1]=nightscene2, ...
    public string[] morningScenes;   // เช่น [0]=morningscene, [1]=morningscene2, ...

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void UseEnergy()
    {
        if (energy > 0)
        {
            energy--;
            Debug.Log($"ใช้ Energy เหลือ {energy}");
            AdvancePhase();
        }
    }

    public void AdvancePhase()
    {
        // Morning -> Event (Day)
        if (currentPhase == GamePhase.Morning && energy == 0)
        {
            currentPhase = GamePhase.Event;
            string nextDay = SelectDaySceneForCurrentDay();
            Debug.Log($"เข้าสู่ Phase 2: Event → โหลดฉากกลางวัน: {nextDay}");
            LoadSceneIfDifferent(nextDay);
        }
        // Event -> Night
        else if (currentPhase == GamePhase.Event && NPCAcceptedCount >= maxNPCs)
        {
            LoadNight();
        }
        // Night -> Morning (วันถัดไป)
        else if (currentPhase == GamePhase.Night && DinnerCompleted)
        {
            LoadMorning();
        }
    }

    // ---------- Phase-specific loaders ----------
    public void LoadNight()
    {
        currentPhase = GamePhase.Night;
        string scene = SelectNightSceneForCurrentDay();
        Debug.Log($"🌙 ไปฉากคืน: {scene}");
        LoadSceneIfDifferent(scene);
    }

    public void LoadMorning()
    {
        currentDay++;                // เพิ่มวันก่อนเลือก morning ของ "วันใหม่"
        currentPhase = GamePhase.Morning;
        energy = 4;
        DinnerCompleted = false;
        string scene = SelectMorningSceneForCurrentDay();
        Debug.Log($"🌅 ไปฉากเช้า (Day {currentDay}): {scene}");
        LoadSceneIfDifferent(scene);
    }

    // ---------- Scene selection helpers ----------
    private string SelectDaySceneForCurrentDay()
    {
        int idx = Mathf.Max(0, currentDay - 1);
        if (usePerDayScenes)
        {
            string byIndex = GetByIndex(dayScenes, idx);
            if (!string.IsNullOrEmpty(byIndex)) return byIndex;
            return daySceneName; // fallback
        }
        else
        {
            return useDayScene2AfterMorning && !string.IsNullOrEmpty(daySceneName2) ? daySceneName2 : daySceneName;
        }
    }

    private string SelectNightSceneForCurrentDay()
    {
        int idx = Mathf.Max(0, currentDay - 1);
        if (usePerDayScenes)
        {
            string byIndex = GetByIndex(nightScenes, idx);
            if (!string.IsNullOrEmpty(byIndex)) return byIndex;
            return nightSceneName; // fallback
        }
        else
        {
            return nightSceneName;
        }
    }

    private string SelectMorningSceneForCurrentDay()
    {
        int idx = Mathf.Max(0, currentDay - 1);
        if (usePerDayScenes)
        {
            string byIndex = GetByIndex(morningScenes, idx);
            if (!string.IsNullOrEmpty(byIndex)) return byIndex;
            return morningSceneName; // fallback
        }
        else
        {
            return morningSceneName;
        }
    }

    private string GetByIndex(string[] arr, int index)
    {
        if (arr == null) return null;
        if (index < 0 || index >= arr.Length) return null;
        return string.IsNullOrEmpty(arr[index]) ? null : arr[index];
    }

    private void LoadSceneIfDifferent(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ Scene name ว่าง! ตรวจการตั้งค่าใน Inspector");
            return;
        }
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
