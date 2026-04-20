using UnityEngine;

/// <summary>
/// ตัวช่วยล็อก/ปลดล็อกประตูตาม Phase (Morning/Day/Night)
/// - ใส่บนประตูเดียวกับ DoorInteract
/// - ตั้งว่าอยากล็อกช่วงไหน
/// </summary>
[RequireComponent(typeof(DoorInteract))]
public class DoorPhaseLock : MonoBehaviour
{
    public bool lockInMorning = false;
    public bool lockInDay = false;
    public bool lockInNight = false;

    public string reasonMorning = "ล็อกช่วงเช้า";
    public string reasonDay = "ล็อกช่วงกลางวัน";
    public string reasonNight = "ล็อกช่วงกลางคืน";

    private DoorInteract door;

    void Awake()
    {
        door = GetComponent<DoorInteract>();
    }

    void Update()
    {
        if (PhaseManager.Instance == null || door == null) return;

        var phase = PhaseManager.Instance.currentPhase;

        if (phase == PhaseManager.GamePhase.Morning && lockInMorning)
            door.SetLocked(true, reasonMorning);
        else if (phase == PhaseManager.GamePhase.Day && lockInDay)
            door.SetLocked(true, reasonDay);
        else if (phase == PhaseManager.GamePhase.Night && lockInNight)
            door.SetLocked(true, reasonNight);
        else
            door.SetLocked(false);
    }
}
