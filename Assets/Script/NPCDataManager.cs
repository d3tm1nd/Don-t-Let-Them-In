using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct NPCData
{
    public GameObject prefab;
    public Vector3 position;
    public Quaternion rotation;
}

public class NPCDataManager : MonoBehaviour
{
    public static NPCDataManager Instance { get; private set; }

    public List<NPCData> acceptedNPCs = new List<NPCData>();  // 🔥 List ข้อมูล NPC ที่รับเข้า

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // ข้าม scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ================= บันทึก NPC ที่รับเข้า =================
    public void AddAcceptedNPC(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        NPCData data = new NPCData
        {
            prefab = prefab,
            position = position,
            rotation = rotation
        };

        acceptedNPCs.Add(data);
        Debug.Log($"✅ บันทึก NPC Data: {prefab.name} ที่ {position}");
    }
}