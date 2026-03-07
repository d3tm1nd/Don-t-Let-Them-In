using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class RadioInteract_ShowNPCStatus : MonoBehaviour, IInteractable
{
    [Header("Interact Layer Settings")]
    [Tooltip("ชื่อเลเยอร์ที่ใช้กับ InteractionRay")] public string requiredLayerName = "interactable";
    [Tooltip("ตั้งเลเยอร์ให้ลูกทั้งหมดด้วยหรือไม่")] public bool applyLayerToChildren = true;

    [Header("UI References")]
    public RadioNPCStatusUI_v2 statusUI; // อ้างอิง UI หลัก
    public bool autoFindUIInScene = true; // หา UI อัตโนมัติถ้าไม่อ้างอิง

    [Header("Audio (optional)")]
    public AudioClip interactSound;   // เสียงกด E ที่วิทยุ
    private AudioSource audioSource;

    void Reset()
    {
        EnsureInteractLayer();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false; // ให้ Raycast โดนง่ายแบบเดียวกับประตู
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        EnsureInteractLayer();
    }

    private void EnsureInteractLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer == -1)
        {
            Debug.LogWarning($"⚠️ ไม่พบเลเยอร์ '{requiredLayerName}' — โปรดสร้างเลเยอร์นี้และตั้ง InteractionRay ให้ครอบคลุม");
            return;
        }
        if (applyLayerToChildren)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layer;
        }
        else
        {
            gameObject.layer = layer;
        }
    }

    public void Interact()
    {
        if (autoFindUIInScene && statusUI == null)
            statusUI = FindObjectOfType<RadioNPCStatusUI_v2>(true);

        if (statusUI == null)
        {
            Debug.LogError("❌ ไม่พบ RadioNPCStatusUI_v2 ในซีน — โปรดเพิ่ม UI แล้วอ้างอิง");
            return;
        }

        if (interactSound != null) audioSource.PlayOneShot(interactSound);

        // ดึงข้อมูล NPC จาก DataManager
        List<NPCData> src = null;
        if (NPCDataManager.Instance != null)
        {
            src = NPCDataManager.Instance.acceptedNPCs;
        }

        statusUI.PopulateFrom(src);
        statusUI.Toggle(true);
    }
}
