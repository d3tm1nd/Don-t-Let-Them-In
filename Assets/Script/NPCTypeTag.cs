using UnityEngine;

public enum NPCKind { Human, Ghost }

// ใส่สคริปต์นี้ไว้ที่ "Prefab" ของ NPC ผู้มาเยือน (ทั้งคนปกติและผี)
// เพื่อให้ระบบนับจำนวนผี/คนได้ตอนสรุปผล
public class NPCTypeTag : MonoBehaviour
{
    public NPCKind kind = NPCKind.Human;
}
