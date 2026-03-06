using UnityEngine;
using System.Collections.Generic;

public class DinnerTableSystem : MonoBehaviour
{
    [Header("Seating")]
    public Transform[] seatPositions; // ตำแหน่งรอบโต๊ะ
    public float dinnerDuration = 8f;

    [Tooltip("ถ้า true จะ Instantiate จากข้อมูลใน NPCDataManager เมื่อไม่พบอินสแตนซ์ในซีน")]
    public bool spawnIfMissing = true;

    [Header("Scene Flow")]
    [Tooltip("โหลดฉากเช้าเมื่อมื้อค่ำสิ้นสุด (แนะนำให้ปิดถ้าใช้ PhaseManager เป็นศูนย์กลาง)")]
    public bool loadMorningOnEnd = false; // ปิดเป็นค่าเริ่มต้น
    [Tooltip("ชื่อ Scene สำหรับเช้า (ต้องตรง Build Settings)")]
    public string morningSceneName = "morningscene";

    public void StartDinner()
    {
        if (NPCDataManager.Instance == null)
        {
            Debug.LogWarning("⚠️ ไม่พบ NPCDataManager — ไม่สามารถจัดที่นั่งได้");
            Invoke(nameof(EndDinner), dinnerDuration);
            return;
        }

        List<NPCData> accepted = NPCDataManager.Instance.acceptedNPCs;
        if (accepted == null || accepted.Count == 0)
        {
            Debug.Log("ℹ️ ไม่มี NPC ที่รับเข้าไว้ — โต๊ะว่าง");
            Invoke(nameof(EndDinner), dinnerDuration);
            return;
        }

        int seatIndex = 0;
        for (int i = 0; i < accepted.Count && seatIndex < seatPositions.Length; i++)
        {
            var data = accepted[i];
            if (data.prefab == null) { continue; }

            GameObject instance = FindExistingInstanceByName(data.prefab.name);
            if (instance == null && spawnIfMissing)
            {
                instance = Instantiate(data.prefab);
            }

            if (instance != null)
            {
                Transform seat = seatPositions[seatIndex];
                instance.transform.SetPositionAndRotation(seat.position, seat.rotation);
                seatIndex++;
            }
        }

        Invoke(nameof(EndDinner), dinnerDuration);
    }

    private GameObject FindExistingInstanceByName(string prefabName)
    {
        GameObject go = GameObject.Find(prefabName);
        if (go == null)
        {
            go = GameObject.Find(prefabName + "(Clone)");
        }
        return go;
    }

    void EndDinner()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.DinnerCompleted = true;
            PhaseManager.Instance.AdvancePhase();
        }

        if (loadMorningOnEnd)
        {
            if (string.IsNullOrEmpty(morningSceneName))
            {
                Debug.LogError("❌ ไม่ได้ตั้งชื่อ Morning Scene ใน DinnerTableSystem");
            }
            else
            {
                Debug.Log($"🌅 ไปฉากเช้า (ตรงจาก DinnerTable): {morningSceneName}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(morningSceneName);
            }
        }
    }
}
