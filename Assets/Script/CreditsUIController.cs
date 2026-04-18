using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ใส่บนฉาก Credits
/// - แสดงชื่อผู้ทำ (ตั้งผ่าน Inspector หรือ hardcode ก็ได้)
/// - ปุ่ม Back to Main Menu
/// </summary>
public class CreditsUIController : MonoBehaviour
{
    [Header("Credits Text")]
    public TextMeshProUGUI creditsText;

    [TextArea(5, 20)]
    public string creditsContent = "CREDITS\n\nCreated by: YOUR NAME\n";

    [Header("Buttons")]
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        if (creditsText != null)
            creditsText.text = creditsContent;

        // ปลดล็อกเมาส์เผื่อให้คลิกปุ่มได้
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // เผื่ออยากให้กด ESC กลับเมนูได้
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            BackToMainMenu();
    }
}
