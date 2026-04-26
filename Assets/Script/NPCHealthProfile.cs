using UnityEngine;

/// <summary>
/// NPCHealthProfile (SINGLE)
/// เก็บข้อมูลผลตรวจร่างกายของ NPC แบบ "ไม่แยกซ้าย/ขวา"
/// - ตา: ปกติ/ผิดปกติ (ค่าเดียว)
/// - มือ: ปกติ/ผิดปกติ (ค่าเดียว)
/// - อุณหภูมิ
///
/// ✅ รองรับ 2 โหมด:
/// 1) โหมดสุ่มแบบ deterministic ต่อวัน (ใช้ npcStableId + day เป็น seed)
/// 2) โหมดตั้งค่าคงที่ (Fixed) ต่อ NPC เพื่อไม่ให้สุ่ม
/// </summary>
public class NPCHealthProfile : MonoBehaviour
{
    [Header("ข้อมูล NPC")]
    public string npcDisplayName = "NPC";
    public string npcStableId = ""; // ใช้เป็น seed ให้ผลตรวจคงที่ข้ามซีน

    [Header("ค่าอุณหภูมิ (สุ่มในช่วง)")]
    public float minTemperature = 36.0f;
    public float maxTemperature = 38.5f;
    public int temperatureDecimalPlaces = 1;

    [Header("ตั้งค่าผลตรวจเอง (ไม่สุ่ม)")]
    [Tooltip("ถ้าเปิด จะไม่สุ่มผลตรวจรายวัน และจะใช้ค่าที่ตั้งไว้ด้านล่างแทน")]
    public bool useFixedResults = false;

    public enum SimpleState { Normal, Abnormal }

    [Tooltip("สถานะตาที่ต้องการ (ปกติ/ผิดปกติ)")]
    public SimpleState fixedEyes = SimpleState.Normal;

    [Tooltip("สถานะมือที่ต้องการ (ปกติ/ผิดปกติ)")]
    public SimpleState fixedHands = SimpleState.Normal;

    [Tooltip("ถ้าเปิด จะใช้อุณหภูมิที่ตั้งเองด้านล่าง แทนการสุ่มในช่วง")]
    public bool useFixedTemperature = false;

    public float fixedTemperature = 37.0f;

    // ===== ผลตรวจ (Generated) =====

    [Header("ผลตรวจ (Generated)")]
    public float currentTemperature;

    [Tooltip("ตาผิดปกติหรือไม่ (ค่าเดียว)")]
    public bool eyeAbnormal;

    [Tooltip("มือผิดปกติหรือไม่ (ค่าเดียว)")]
    public bool handAbnormal;

    // ===== สถานะว่าตรวจแล้วหรือยัง =====

    [Header("สถานะการตรวจ")]
    public bool checkedEyes;
    public bool checkedHands;
    public bool checkedTemp;

    private int _generatedDay = -1;

    public bool IsFullyChecked => checkedEyes && checkedHands && checkedTemp;

    public void MarkEyesChecked() => checkedEyes = true;
    public void MarkHandsChecked() => checkedHands = true;
    public void MarkTempChecked() => checkedTemp = true;

    /// <summary>
    /// ทำให้ผลตรวจถูกสร้างให้กับวันนั้น ๆ
    /// - ถ้า useFixedResults = true -> ใช้ค่าที่ตั้งไว้ (ไม่สุ่ม)
    /// - ถ้า false -> สุ่มแบบ deterministic (คงที่ต่อวัน)
    /// </summary>
    public void EnsureGeneratedForDay(int day)
    {
        if (_generatedDay == day) return;
        _generatedDay = day;

        // รีเซ็ตสถานะการตรวจเมื่อขึ้นวันใหม่
        checkedEyes = false;
        checkedHands = false;
        checkedTemp = false;

        if (useFixedResults)
        {
            eyeAbnormal = (fixedEyes == SimpleState.Abnormal);
            handAbnormal = (fixedHands == SimpleState.Abnormal);

            float temp = useFixedTemperature
                ? fixedTemperature
                : (minTemperature + maxTemperature) * 0.5f;

            currentTemperature = RoundToDecimals(Mathf.Clamp(temp, minTemperature, maxTemperature), temperatureDecimalPlaces);
            return;
        }

        // โหมดสุ่มแบบ deterministic
        string id = string.IsNullOrEmpty(npcStableId) ? gameObject.name : npcStableId;
        int seed = HashToSeed(id, day);
        var rng = new System.Random(seed);

        // Temperature
        float t = Lerp01((float)rng.NextDouble(), minTemperature, maxTemperature);
        currentTemperature = RoundToDecimals(t, temperatureDecimalPlaces);

        // Eyes/Hands: 1 บิต (0/1)
        eyeAbnormal = rng.Next(0, 2) == 1;
        handAbnormal = rng.Next(0, 2) == 1;
    }

    private static float Lerp01(float u, float a, float b)
    {
        u = Mathf.Clamp01(u);
        return a + (b - a) * u;
    }

    private static float RoundToDecimals(float value, int decimals)
    {
        decimals = Mathf.Clamp(decimals, 0, 4);
        float m = Mathf.Pow(10f, decimals);
        return Mathf.Round(value * m) / m;
    }

    /// <summary>
    /// ทำ seed ให้คงที่จาก id + day (ไม่ใช้ string.GetHashCode เพราะต่างกันตาม runtime)
    /// </summary>
    private static int HashToSeed(string id, int day)
    {
        unchecked
        {
            // FNV-1a 32-bit
            const int fnvPrime = 16777619;
            int hash = (int)2166136261;
            for (int i = 0; i < id.Length; i++)
            {
                hash ^= id[i];
                hash *= fnvPrime;
            }
            hash ^= day;
            hash *= fnvPrime;
            return hash;
        }
    }
}
