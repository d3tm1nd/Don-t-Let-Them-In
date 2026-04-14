using UnityEngine;
using System;

/// <summary>
/// เก็บผลการตรวจ (Eyes/Hands/Temperature) ต่อ NPC
/// - สุ่มใหม่ "ทุกวัน" ตาม PhaseManager.currentDay
/// - ค่าจะคงที่ภายในวันเดียว (ตรวจซ้ำแล้วได้ค่าเดิม)
/// - ใช้ System.Random แยก เพื่อไม่ไปรบกวน UnityEngine.Random ของระบบอื่น
/// </summary>
public class NPCCheckProfile : MonoBehaviour
{
    [Header("Daily Random")]
    [Tooltip("ถ้าเปิด: จะสุ่มใหม่เมื่อ currentDay เปลี่ยน")]
    public bool rerollEachNewDay = true;

    [Tooltip("ID ประจำตัว NPC เพื่อให้ผลสุ่ม 'คงที่ในวันเดียว' และแตกต่างกันระหว่าง NPC (แนะนำให้ตั้งให้ไม่ซ้ำกัน)")]
    public string npcStableId;

    [Header("Temperature Range")]
    public float minTemp = 36f;
    public float maxTemp = 42f;
    [Tooltip("จำนวนทศนิยม เช่น 1 = 38.2 | 0 = 38")]
    public int tempDecimals = 1;

    [Header("Generated (Read Only)")]
    [SerializeField] private int eyesIndex;     // 0..3
    [SerializeField] private int handsIndex;    // 0..3
    [SerializeField] private float temperature; // min..max
    [SerializeField] private int lastRolledDay = -1;

    public int EyesIndex => eyesIndex;
    public int HandsIndex => handsIndex;
    public float Temperature => temperature;
    public int LastRolledDay => lastRolledDay;

    void Awake()
    {
        // ถ้าไม่ได้ตั้ง stableId ให้สร้างจากชื่อ + instance id (ไม่คงข้ามการ reload แต่พอสำหรับ runtime)
        if (string.IsNullOrWhiteSpace(npcStableId))
        {
            npcStableId = gameObject.name + "#" + GetInstanceID();
        }
        EnsureRolledForToday();
    }

    void OnEnable()
    {
        EnsureRolledForToday();
    }

    /// <summary>
    /// เรียกได้ทุกครั้งที่ต้องการให้แน่ใจว่า NPC มีผลสุ่มของ "วันนี้" แล้ว
    /// </summary>
    public void EnsureRolledForToday()
    {
        int today = GetCurrentDaySafe();

        if (!rerollEachNewDay)
        {
            // ถ้าไม่ให้สุ่มรายวัน ก็สุ่มครั้งแรกครั้งเดียว
            if (lastRolledDay == -1) RerollForDay(today);
            return;
        }

        if (today != lastRolledDay)
        {
            RerollForDay(today);
        }
    }

    /// <summary>
    /// บังคับสุ่มใหม่สำหรับวันปัจจุบัน (ใช้เมื่อต้องการ reroll ทันที)
    /// </summary>
    public void ForceRerollToday()
    {
        RerollForDay(GetCurrentDaySafe());
    }

    private int GetCurrentDaySafe()
    {
        // ถ้า PhaseManager ไม่มี ให้ถือว่า Day 1
        if (PhaseManager.Instance == null) return 1;
        return Mathf.Max(1, PhaseManager.Instance.currentDay);
    }

    private void RerollForDay(int day)
    {
        // สร้าง seed จาก (day + stableId) เพื่อให้ "คงที่ในวันเดียว" และไม่เหมือนกันระหว่าง NPC
        int seed = Hash(day, npcStableId);
        var rng = new System.Random(seed);

        eyesIndex = rng.Next(0, 4);
        handsIndex = rng.Next(0, 4);

        double raw = minTemp + (rng.NextDouble() * Math.Max(0.0001, (maxTemp - minTemp)));
        float t = (float)raw;
        float pow = Mathf.Pow(10f, Mathf.Clamp(tempDecimals, 0, 3));
        temperature = Mathf.Round(t * pow) / pow;

        lastRolledDay = day;
    }

    private int Hash(int day, string id)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + day;
            if (!string.IsNullOrEmpty(id))
            {
                for (int i = 0; i < id.Length; i++)
                    h = h * 31 + id[i];
            }
            return h;
        }
    }
}
