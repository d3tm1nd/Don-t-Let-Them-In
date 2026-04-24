using System.Collections.Generic;

/// <summary>
/// HintSeenRegistry (RUNTIME ONLY)
/// เก็บประวัติว่า hintId ไหน "เคยโชว์แล้ว" เฉพาะระหว่างเล่นรอบนั้นเท่านั้น
/// - ไม่ใช้ PlayerPrefs
/// - ปิดเกม/เริ่มใหม่ → ข้อมูลถูกรีเซ็ต
/// </summary>
public static class HintSeenRegistry
{
    private static readonly HashSet<string> _seen = new HashSet<string>();

    public static bool HasSeen(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return _seen.Contains(id);
    }

    public static void MarkSeen(string id, bool persistIgnored = false)
    {
        if (string.IsNullOrEmpty(id)) return;
        _seen.Add(id);
    }

    /// <summary>
    /// ล้างประวัติทั้งหมด (ใช้ตอน Playtest)
    /// </summary>
    public static void ClearAll()
    {
        _seen.Clear();
    }
}
