using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EndGameEvaluator2Days (UPDATED RULES)
///
/// กติกาใหม่:
/// 1) ถ้ามีผีอย่างน้อย 1 ตัว => แพ้ (ไม่สนจำนวนคน)
/// 2) ถ้าผี = 0 และมีคนอย่างน้อย 3 คน => ชนะ
/// 3) กรณีอื่น ๆ => แพ้
///
/// แหล่งข้อมูล:
/// - อ่านจำนวนผู้รอด/อยู่ในบ้านจาก NPCDataManager.Instance.acceptedNPCs (จำนวนรายการ)
/// - นับผีจาก NPCTypeTag.kind == NPCKind.Ghost บน prefab ที่ถูกบันทึกใน NPCData
/// </summary>
public class EndGameEvaluator2Days : MonoBehaviour
{
    [Header("Rules")]
    public int minHumansToWinWhenNoGhost = 3;

    [Header("Scenes")]
    public string winSceneName = "EndWin";
    public string loseSceneName = "EndLose";

    public void EvaluateAndLoad()
    {
        int total = 0;
        int ghosts = 0;

        if (NPCDataManager.Instance != null && NPCDataManager.Instance.acceptedNPCs != null)
        {
            var list = NPCDataManager.Instance.acceptedNPCs;
            total = list.Count;

            for (int i = 0; i < list.Count; i++)
            {
                var d = list[i];
                if (d.prefab == null) continue;

                var tag = d.prefab.GetComponent<NPCTypeTag>();
                if (tag != null && tag.kind == NPCKind.Ghost)
                    ghosts++;
            }
        }

        // กติกาใหม่
        bool win = (ghosts == 0) && (total >= minHumansToWinWhenNoGhost);
        bool lose = !win; // กรณีอื่น ๆ แพ้

        Debug.Log($"🏁 END CHECK | total={total}, ghosts={ghosts} => {(win ? "WIN" : "LOSE")}");
        SceneManager.LoadScene(win ? winSceneName : loseSceneName);
    }
}
