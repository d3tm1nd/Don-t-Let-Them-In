using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RadioNPCStatusUI_v2 : MonoBehaviour
{
    [Header("UI")]
    public GameObject rootPanel;          // เปิด/ปิด UI ทั้งชุด
    public Transform listContainer;       // พาเรนต์ของ item (เช่น Content ของ ScrollView)
    public NPCStatusItemUI_v2 itemPrefab; // พรีแฟบของแถวรายการ (Name + Status)
    public TextMeshProUGUI emptyText;     // แสดงเมื่อไม่มีข้อมูล

    [Header("Texts")]
    public string aliveText = "มีชีวิต";
    public string deadText  = "เสียชีวิต";

    public void Toggle(bool show)
    {
        if (rootPanel != null) rootPanel.SetActive(show);
        else gameObject.SetActive(show);
    }

    public void ShowEmpty(string message)
    {
        ClearList();
        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(true);
            emptyText.text = message;
        }
    }

    public void PopulateFrom(List<NPCData> data)
    {
        ClearList();

        if (data == null || data.Count == 0)
        {
            ShowEmpty("ยังไม่มี NPC ที่รับเข้าไว้");
            return;
        }
        if (emptyText != null) emptyText.gameObject.SetActive(false);

        foreach (var d in data)
        {
            if (itemPrefab == null) { Debug.LogError("ไม่มี itemPrefab (NPCStatusItemUI_v2)"); break; }
            var item = Instantiate(itemPrefab, listContainer);
            string npcName = d.prefab != null ? d.prefab.name : "(ไม่ทราบชื่อ)";

            // กำหนดสถานะ: ถ้าเจออินสแตนซ์ NPC ในซีน ถือว่า 'มีชีวิต', ถ้าไม่เจอถือว่า 'เสียชีวิต'
            bool isAlive = TryFindInstanceAlive(d);
            item.Set(npcName, isAlive ? aliveText : deadText);
        }
    }

    private bool TryFindInstanceAlive(NPCData d)
    {
        if (d.prefab == null) return false;
        string name = d.prefab.name;
        var go = GameObject.Find(name) ?? GameObject.Find(name + "(Clone)");
        if (go == null) return false; // ไม่พบอินสแตนซ์ในซีน → ถือว่าเสียชีวิต/ไม่อยู่

        // ถ้ามีสคริปต์ provider ในอนาคต เราก็อ่านค่าจริง ๆ ได้ (optional)
        var provider = go.GetComponent<NPCLifeStateProvider>();
        if (provider != null) return provider.IsAlive;

        return true; // พบอินสแตนซ์แต่ไม่มี provider → ถือว่ายังมีชีวิต
    }

    private void ClearList()
    {
        if (listContainer == null) return;
        for (int i = listContainer.childCount - 1; i >= 0; i--)
            Destroy(listContainer.GetChild(i).gameObject);
    }
}
