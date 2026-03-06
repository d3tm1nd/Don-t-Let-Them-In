using UnityEngine;

public class PersistentGameManager : MonoBehaviour
{
    public static PersistentGameManager Instance;

    public bool strangerAccepted = false;   // เก็บว่าผู้เล่นรับ Stranger หรือไม่
    public int currentDay = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // เรียกตอนกดเปลี่ยนฉาก Day → Night
    public void CheckForGameOver()
    {
        if (strangerAccepted)
        {
            Debug.Log("💀 รับ Stranger (ผี) เข้า��้านแล้ว → GAME OVER");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene"); // หรือ Scene Game Over ของคุณ
        }
    }
}