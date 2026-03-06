using UnityEngine;
using TMPro;

public class RadioNewsSystem : MonoBehaviour
{
    public TextMeshProUGUI newsText;
    public string[] dailyNews;

    public void PlayNews()
    {
        newsText.text = dailyNews[PhaseManager.Instance.currentDay - 1];
        // เพิ่มเสียงวิทยุที่นี่
    }
}