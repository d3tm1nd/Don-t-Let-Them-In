using UnityEngine;
using TMPro;

public class MorningSceneController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dayText;           // แสดง "DAY 2"
    public TextMeshProUGUI radioText;         // ข่าววิทยุประจำวัน

    [Header("ข่าวประจำวัน")]
    public string[] dailyRadioNews;           // ข่าว Day 1, Day 2, ...

    void Start()
    {
        // อัพเดท Day
        if (dayText != null)
            dayText.text = $"DAY {PhaseManager.Instance.currentDay}";

        // เล่นข่าววิทยุ
        if (radioText != null && dailyRadioNews.Length > 0)
        {
            int index = Mathf.Clamp(PhaseManager.Instance.currentDay - 1, 0, dailyRadioNews.Length - 1);
            radioText.text = dailyRadioNews[index];
        }

        Debug.Log($"MorningScene โหลด Day {PhaseManager.Instance.currentDay}");
    }
}