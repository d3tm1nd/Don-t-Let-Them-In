using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// เก็บรายชื่อ NPC (ตาม npcStableId) ที่ "ตายแล้ว" ข้ามซีนได้ (DontDestroy)
/// ใช้เพื่อให้การตายในคืน ส่งผลต่อวันถัดไป/ฉากอื่น
/// </summary>
public class DeathRegistry : MonoBehaviour
{
    public static DeathRegistry Instance { get; private set; }

    private HashSet<string> deadIds = new HashSet<string>();

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

    public bool IsDead(string npcStableId)
    {
        if (string.IsNullOrEmpty(npcStableId)) return false;
        return deadIds.Contains(npcStableId);
    }

    public void MarkDead(string npcStableId)
    {
        if (string.IsNullOrEmpty(npcStableId)) return;
        deadIds.Add(npcStableId);
    }

    public void ClearAll()
    {
        deadIds.Clear();
    }
}
