using UnityEngine;
using UnityEngine.SceneManagement;

// ประเมินแพ้/ชนะหลังจบ Day 2 (กลางคืนรอบที่ 2)
// เงื่อนไขจากผู้ใช้:
// - ต้องมี NPC อยู่ในบ้านอย่างน้อย 3 ตัว
// - ถ้ามีผีอยู่ในกลุ่ม "แค่ 1 ตัว" = แพ้
// หมายเหตุ: ตามข้อความที่ให้มา 0 ผีถือว่าไม่เข้าเงื่อนไขแพ้ (ชนะได้) และ 2+ ผีก็ไม่เข้าเงื่อนไขแพ้
public class EndGameEvaluator2Days : MonoBehaviour
{
    [Header("Rules")]
    public int minNPCInHouse = 3;
    public int loseIfGhostCountEquals = 1;

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
                if (d.prefab == null || d.prefab == null) continue;
                var tag = d.prefab.GetComponent<NPCTypeTag>();
                if (tag != null && tag.kind == NPCKind.Ghost) ghosts++;
            }
        }

        bool lose = (total < minNPCInHouse) || (ghosts == loseIfGhostCountEquals);

        Debug.Log($"🏁 END CHECK | total={total}, ghosts={ghosts} => {(lose ? "LOSE" : "WIN")}");

        SceneManager.LoadScene(lose ? loseSceneName : winSceneName);
    }
}
