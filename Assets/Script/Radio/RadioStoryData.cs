using UnityEngine;

/// <summary>
/// RadioStoryData
/// - เก็บข้อความเนื้อเรื่อง/ประกาศ ที่จะโชว์ผ่านวิทยุ
/// - รองรับหลายหน้า (pages) หรือหลายบรรทัด
/// </summary>
[CreateAssetMenu(menuName = "Radio/Story Data", fileName = "RadioStoryData")]
public class RadioStoryData : ScriptableObject
{
    [Header("Meta")]
    public string storyId = "";

    [Header("Optional Title")]
    public string title = "";

    [Header("Pages")]
    [Tooltip("แต่ละ element คือ 1 หน้า/1 ชุดข้อความ")]
    [TextArea(3, 10)]
    public string[] pages;

    [Header("Behavior")]
    [Tooltip("ถ้า true: กดวิทยุแต่ละครั้งจะสุ่มหน้า")]
    public bool randomPage = false;

    [Tooltip("ถ้า true: กดวิทยุแต่ละครั้งจะวนหน้าไปเรื่อย ๆ")]
    public bool cyclePages = true;
}
