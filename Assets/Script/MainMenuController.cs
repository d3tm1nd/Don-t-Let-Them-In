using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;  // ลาก Text สำหรับ Title
    public Button startButton;         // ลาก Button Start
    public Button exitButton;          // ลาก Button Exit
    public AudioSource backgroundAudio; // ลาก AudioSource สำหรับเสียงพื้นหลัง (optional)

    void Start()
    {
        // ตั้งค่า Title
        if (titleText != null)
        {
            titleText.text = "DON’T LET THEM IN";
            titleText.color = Color.red; // หรือสีเขียวแบบ PDF
        }

        // ตั้งค่า Button
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        // เล่นเสียงพื้นหลัง (ถ้ามี)
        if (backgroundAudio != null)
            backgroundAudio.Play();
    }

    void StartGame()
    {
        Debug.Log("Start Game: Load Phase 1 (Morning Scene)");
        SceneManager.LoadScene("MorningScene"); // เปลี่ยนเป็นชื่อ Scene Phase 1 ของคุณ
    }

    void ExitGame()
    {
        Debug.Log("Exit Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}