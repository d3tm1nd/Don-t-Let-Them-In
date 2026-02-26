using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceItem", menuName = "Resource/Item")]
public class ResourceItem : ScriptableObject
{
    public string itemName = "ของกิน";
    public string description = "รายละเอียด";
    // เพิ่ม field อื่น (e.g. icon, value)
}