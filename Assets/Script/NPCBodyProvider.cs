using UnityEngine;

// ใส่สคริปต์นี้บน NPC อินสแตนซ์ (รากของตัวละคร)
// ตั้งค่าภาพรวมสถานะร่างกายใน Inspector ต่อส่วนต่าง ๆ ของร่างกาย
public class NPCBodyProvider : MonoBehaviour
{
    [Header("สถานะร่างกายต่อส่วน")]
    public RegionStatus head;
    public RegionStatus torso;
    public RegionStatus leftArm;
    public RegionStatus rightArm;
    public RegionStatus leftLeg;
    public RegionStatus rightLeg;

    public RegionStatus Get(BodyRegion region)
    {
        switch (region)
        {
            case BodyRegion.Head: return head;
            case BodyRegion.Torso: return torso;
            case BodyRegion.LeftArm: return leftArm;
            case BodyRegion.RightArm: return rightArm;
            case BodyRegion.LeftLeg: return leftLeg;
            case BodyRegion.RightLeg: return rightLeg;
            default: return torso;
        }
    }
}
