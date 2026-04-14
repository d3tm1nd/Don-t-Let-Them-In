using UnityEngine;

// ตั้งกติกาตามวัน (Day 1, Day 2) สำหรับจำนวนผู้มาเยือน
// Day 3 เป็นฉากสรุปผล (ไม่ต้องมีการรัน loop)
[CreateAssetMenu(menuName = "Game/Two Day Rules", fileName = "TwoDayRules")]
public class TwoDayRules : ScriptableObject
{
    [Header("Total days to play (loop)")]
    public int playableDays = 2; // เล่นแค่ 2 วัน

    [Header("Day 1 Visitors")]
    public int day1Humans = 2;
    public int day1Ghosts = 2;

    [Header("Day 2 Visitors")]
    public int day2Humans = 2;
    public int day2Ghosts = 1;

    public int GetRequiredDecisionsForDay(int day)
    {
        if (day <= 1) return day1Humans + day1Ghosts;
        if (day == 2) return day2Humans + day2Ghosts;
        return 0;
    }
}
