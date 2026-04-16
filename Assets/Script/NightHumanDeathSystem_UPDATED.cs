using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ระบบ "จบคืนแล้วสุ่มคนตาย" (2 รอบสุ่ม)
///
/// เงื่อนไข:
/// - ทำงานตอน "จบ Night" (ก่อนจะไป Morning)
/// - จะทำงานก็ต่อเมื่อมี Ghost อยู่ในบ้าน AND มี Human อยู่ในบ้าน
///
/// สุ่ม 2 รอบ:
/// 1) สุ่มว่ามีการตายไหม (deathChance)
/// 2) ถ้าตาย สุ่มเลือกว่าคนไหนตาย (จากรายชื่อ Human)
///
/// ผลลัพธ์:
/// - MarkDead ลง DeathRegistry (ใช้ npcStableId จาก NPCHealthProfile)
/// - ลบ entry ของคนที่ตายใน NPCDataManager.acceptedNPCs (เพื่อไม่ให้ spawn อีก)
/// - Destroy GameObject ของคนที่ถูกฆ่าในฉากปัจจุบัน
/// </summary>
public class NightHumanDeathSystem : MonoBehaviour
{
    public static NightHumanDeathSystem Instance { get; private set; }

    [Header("Chance")]
    [Range(0f, 1f)]
    public float deathChance = 0.35f;

    [Tooltip("ถ้าเปิด: โอกาสตายจะเพิ่มตามจำนวนผี (deathChance + (ghostCount-1)*extraPerGhost)")]
    public bool scaleChanceWithGhostCount = true;

    [Range(0f, 1f)]
    public float extraPerGhost = 0.10f;

    [Header("Debug")]
    public bool debugLogs = true;

    private int lastProcessedDay = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ProcessEndOfNight()
    {
        int day = (PhaseManager.Instance != null) ? Mathf.Max(1, PhaseManager.Instance.currentDay) : 1;
        if (lastProcessedDay == day)
        {
            if (debugLogs) Debug.Log($"🌙 [NightDeath] Already processed for Day {day}");
            return;
        }
        lastProcessedDay = day;

        if (DeathRegistry.Instance == null)
        {
            if (debugLogs) Debug.LogWarning("⚠️ [NightDeath] DeathRegistry missing. Creating one.");
            var go = new GameObject("DeathRegistry");
            go.AddComponent<DeathRegistry>();
        }

        // หา NPC ทั้งหมดที่มี NPCHealthProfile ในฉาก
        var profiles = GameObject.FindObjectsOfType<NPCHealthProfile>(true);
        var humans = new List<NPCHealthProfile>();
        var ghosts = new List<NPCHealthProfile>();

        foreach (var p in profiles)
        {
            if (p == null) continue;

            string id = string.IsNullOrEmpty(p.npcStableId) ? p.gameObject.name : p.npcStableId;
            if (DeathRegistry.Instance != null && DeathRegistry.Instance.IsDead(id))
                continue;

            bool isGhost = DetectIsGhost(p.gameObject);
            if (isGhost) ghosts.Add(p);
            else humans.Add(p);
        }

        if (debugLogs)
            Debug.Log($"🌙 [NightDeath] Day {day} | Humans={humans.Count} | Ghosts={ghosts.Count}");

        if (ghosts.Count <= 0 || humans.Count <= 0)
        {
            if (debugLogs) Debug.Log("🌙 [NightDeath] No death check (need both Ghost and Human present)");
            return;
        }

        float chance = deathChance;
        if (scaleChanceWithGhostCount)
            chance = Mathf.Clamp01(deathChance + Mathf.Max(0, ghosts.Count - 1) * extraPerGhost);

        float roll = UnityEngine.Random.value;
        if (debugLogs) Debug.Log($"🎲 [NightDeath] Roll1 death? roll={roll:F2} vs chance={chance:F2}");

        if (roll > chance)
        {
            if (debugLogs) Debug.Log("✅ [NightDeath] No one died tonight.");
            return;
        }

        int victimIndex = UnityEngine.Random.Range(0, humans.Count);
        var victim = humans[victimIndex];
        if (victim == null) return;

        string victimId = string.IsNullOrEmpty(victim.npcStableId) ? victim.gameObject.name : victim.npcStableId;
        if (DeathRegistry.Instance != null)
            DeathRegistry.Instance.MarkDead(victimId);

        if (debugLogs)
            Debug.Log($"💀 [NightDeath] Victim: {victim.npcDisplayName} (id={victimId})");

        // ลบออกจาก NPCDataManager.acceptedNPCs ด้วย (ถ้ามี)
        RemoveFromNPCDataManager(victimId);

        Destroy(victim.gameObject);
    }

    private void RemoveFromNPCDataManager(string victimId)
    {
        if (string.IsNullOrEmpty(victimId)) return;
        if (NPCDataManager.Instance == null) return;
        if (NPCDataManager.Instance.acceptedNPCs == null) return;

        var list = NPCDataManager.Instance.acceptedNPCs;
        int removed = 0;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var entry = list[i];
            if (entry.prefab == null) continue;

            var prof = entry.prefab.GetComponent<NPCHealthProfile>();
            if (prof == null) continue;

            string id = string.IsNullOrEmpty(prof.npcStableId) ? entry.prefab.name : prof.npcStableId;
            if (id == victimId)
            {
                list.RemoveAt(i);
                removed++;
            }
        }

        if (debugLogs && removed > 0)
            Debug.Log($"🗑️ [NightDeath] Removed {removed} entries from NPCDataManager for id={victimId}");
    }

    // ---------- Ghost detection (reflection) ----------

    private bool DetectIsGhost(GameObject go)
    {
        if (go == null) return false;

        var comps = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();
            if (t.Name != "NPCTypeTag") continue;

            object val = GetMemberValue(c, t, "kind") ?? GetMemberValue(c, t, "npcKind");
            if (val != null)
            {
                string s = val.ToString();
                if (!string.IsNullOrEmpty(s) && s.IndexOf("Ghost", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (!string.IsNullOrEmpty(s) && s.IndexOf("Human", StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
            }

            object b = GetMemberValue(c, t, "isGhost");
            if (b is bool) return (bool)b;
        }

        if (go.CompareTag("Ghost")) return true;
        return false;
    }

    private object GetMemberValue(object obj, Type t, string name)
    {
        var f = t.GetField(name);
        if (f != null) return f.GetValue(obj);
        var p = t.GetProperty(name);
        if (p != null && p.CanRead) return p.GetValue(obj);
        return null;
    }
}
