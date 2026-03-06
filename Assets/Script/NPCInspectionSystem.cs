using UnityEngine;
using UnityEngine.UI;

public class NPCInspectionSystem : MonoBehaviour, IInteractable
{
    [Header("Inspection UI")]
    public GameObject inspectionPanel;
    public Button btnTeeth, btnHands, btnEyes, btnArmpits;

    private ResourceInsideNPC currentNPC;

    void Start()
    {
        btnTeeth.onClick.AddListener(() => Inspect("Teeth"));
        btnHands.onClick.AddListener(() => Inspect("Hands"));
        btnEyes.onClick.AddListener(() => Inspect("Eyes"));
        btnArmpits.onClick.AddListener(() => Inspect("Armpits"));
    }

    public void Interact()
    {
        currentNPC = GetComponent<ResourceInsideNPC>();
        inspectionPanel.SetActive(true);
    }

    void Inspect(string part)
    {
        Debug.Log($"ตรวจ {part} ของ {currentNPC.name}");
        // ถ้าเป็น Stranger → เปิดโอกาสให้รู้ตัว
        if (currentNPC.isStranger)
            Debug.Log("พบ Stranger! (ติดเชื้อ)");

        inspectionPanel.SetActive(false);
        // ลบ NPC ถ้าตรวจแล้ว (หรือส่งไปกินข้าว)
    }
}