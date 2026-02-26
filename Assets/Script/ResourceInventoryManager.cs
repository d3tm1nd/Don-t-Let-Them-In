using UnityEngine;
using System.Collections.Generic;

public class ResourceInventoryManager : MonoBehaviour
{
    public static ResourceInventoryManager Instance { get; private set; }

    public List<ResourceItem> items = new List<ResourceItem>();  // List ของกินที่ได้รับ

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // ถ้าต้องการข้าม scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(ResourceItem newItem)
    {
        items.Add(newItem);
        Debug.Log($"✅ ได้ {newItem.itemName} เข้า inventory! | Total: {items.Count}");
        // Optional: แสดง UI
    }
}