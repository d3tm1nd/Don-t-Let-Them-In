using UnityEngine;
using System;

/// <summary>
/// โปรไฟล์การตรวจร่างกายของ NPC (สุ่มใหม่ทุกวัน แต่คงที่ภายในวันเดียว)
/// ตรวจ 3 อย่าง: ตา (ซ้าย/ขวา), มือ (ซ้าย/ขวา), อุณหภูมิ (ตัวเลข)
///
/// เงื่อนไขสุ่ม:
/// - ตา/มือมี 4 แบบ (2 บิต):
///   00 = ปกติทั้งสองข้าง, 01 = ผิดปกติขวา, 10 = ผิดปกติซ้าย, 11 = ผิดปกติทั้งสองข้าง
/// - อุณหภูมิสุ่มจากช่วงที่กำหนดต่อ NPC (minTemperature..maxTemperature)
///
/// เงื่อนไขการ "ตรวจครบ":
/// - ต้องตรวจ ตา + มือ + อุณหภูมิ อย่างน้อยอย่างละ 1 ครั้งในวันนั้น
/// - ค่าการตรวจ (checked*) จะถูกรีเซ็ตเมื่อขึ้นวันใหม่
/// </summary>
public class NPCHealthProfile : MonoBehaviour
{
    [Header("ข้อมูล NPC")]
    public string npcDisplayName = "NPC";

    [Tooltip("ไอดีคงที่สำหรับสุ่มแบบ deterministic (ถ้าว่างจะใช้ชื่อ GameObject)\nแนะนำ: human_jone, human_penny, ghost_g3 เป็นต้น")]
    public string npcStableId = "";

    [Header("ช่วงอุณหภูมิ (สุ่มทุกวัน)")]
    public float minTemperature = 36f;
    public float maxTemperature = 42f;

    [Header("ความละเอียดทศนิยมของอุณหภูมิ")]
    [Range(0,3)]
    public int temperatureDecimalPlaces = 1;

    // ---- ผลตรวจของ "วันปัจจุบัน" ----
    [NonSerialized] public float currentTemperature;
    [NonSerialized] public bool leftEyeAbnormal;
    [NonSerialized] public bool rightEyeAbnormal;
    [NonSerialized] public bool leftHandAbnormal;
    [NonSerialized] public bool rightHandAbnormal;

    // ---- สถานะ "ตรวจแล้วหรือยัง" (รีเซ็ตทุกวัน) ----
    [NonSerialized] public bool checkedEyes;
    [NonSerialized] public bool checkedHands;
    [NonSerialized] public bool checkedTemp;

    private int _generatedDay = -1;
    public int GeneratedDay => _generatedDay;

    public bool IsFullyChecked => checkedEyes && checkedHands && checkedTemp;

    public void MarkEyesChecked()  => checkedEyes = true;
    public void MarkHandsChecked() => checkedHands = true;
    public void MarkTempChecked()  => checkedTemp = true;

    /// <summary>
    /// สุ่มผลตรวจสำหรับวันนั้น ๆ (ถ้าวันเดิมจะไม่สุ่มซ้ำ)
    /// และรีเซ็ต checked* เมื่อขึ้นวันใหม่
    /// </summary>
    public void EnsureGeneratedForDay(int day)
    {
        if (_generatedDay == day) return;

        _generatedDay = day;

        // รีเซ็ตสถานะการตรวจเมื่อขึ้นวันใหม่
        checkedEyes = false;
        checkedHands = false;
        checkedTemp = false;

        // สุ่มแบบ deterministic ให้คงที่ในวันเดียว (แม้โหลดซีนใหม่)
        string id = string.IsNullOrEmpty(npcStableId) ? gameObject.name : npcStableId;
        int seed = HashToSeed(id, day);
        var rng = new System.Random(seed);

        // Temperature
        float t = Lerp((float)rng.NextDouble(), minTemperature, maxTemperature);
        currentTemperature = RoundToDecimals(t, temperatureDecimalPlaces);

        // Eyes/Hands 4 แบบ (2 บิต)
        int eyeBits = rng.Next(0, 4);   // 0..3
        int handBits = rng.Next(0, 4);  // 0..3

        leftEyeAbnormal  = (eyeBits & 0b10) != 0;
        rightEyeAbnormal = (eyeBits & 0b01) != 0;

        leftHandAbnormal  = (handBits & 0b10) != 0;
        rightHandAbnormal = (handBits & 0b01) != 0;

        Debug.Log($"🧬 [BodyCheck] {npcDisplayName} | Day {day} | Temp={currentTemperature:F1} | Eye(L,R)=({leftEyeAbnormal},{rightEyeAbnormal}) | Hand(L,R)=({leftHandAbnormal},{rightHandAbnormal})");
    }

    private static float Lerp(float u, float a, float b) => a + (b - a) * Mathf.Clamp01(u);

    private static float RoundToDecimals(float value, int decimals)
    {
        decimals = Mathf.Clamp(decimals, 0, 3);
        float p = Mathf.Pow(10f, decimals);
        return Mathf.Round(value * p) / p;
    }

    private static int HashToSeed(string id, int day)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + (id != null ? id.GetHashCode() : 0);
            h = h * 31 + day;
            return h;
        }
    }
}
