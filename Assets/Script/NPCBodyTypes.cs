using UnityEngine;

public enum BodyRegion { Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg }

[System.Serializable]
public struct RegionStatus
{
    public bool hasWound;     // แผล
    public bool hasRash;      // ผื่น/จุดแดง
    public bool isCoughing;   // ไอ (อาการ)
    public bool highFever;    // ไข้สูง
    [TextArea(1,3)] public string note; // บันทึกสั้น ๆ
}
