using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ตัวช่วย (ออปชัน): เมื่อเข้า Morning (หลังโหลดซีน) ให้เรียก EnsureRolledForToday() กับ NPCCheckProfile ทุกตัว
/// ปกติ NPCCheckProfile จะจัดการเองอยู่แล้ว แต่ตัวนี้ช่วยกรณี NPC ถูก spawn ช้าหรืออยากบังคับ refresh หลังโหลดซีน
/// </summary>
public class NPCCheckRerollOnMorning : MonoBehaviour
{
    private int _lastHandledDay = -1;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PhaseManager.Instance == null) return;
        if (PhaseManager.Instance.currentPhase != PhaseManager.GamePhase.Morning) return;

        int day = Mathf.Max(1, PhaseManager.Instance.currentDay);
        if (day == _lastHandledDay) return;
        _lastHandledDay = day;

        var all = GameObject.FindObjectsOfType<NPCCheckProfile>(true);
        foreach (var p in all)
        {
            if (p != null) p.EnsureRolledForToday();
        }
        Debug.Log($"[NPCCheckRerollOnMorning] Rerolled check profiles for Day {day} (count={all.Length})");
    }
}
