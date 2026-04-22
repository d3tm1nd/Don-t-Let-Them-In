using UnityEngine;

/// <summary>
/// NPCProximityDialogue
/// - เมื่อผู้เล่นเข้าใกล้ NPC จะโชว์ข้อความพูดเป็น Text UI (ผ่าน DialogueUI)
/// - รองรับ 2 วิธีตรวจระยะ:
///   A) Trigger: ใช้ SphereCollider (isTrigger) บน NPC
///   B) Distance check: คำนวณระยะด้วย Transform
///
/// วิธีใช้ (แนะนำ Trigger):
/// 1) ใส่สคริปต์นี้บน NPC
/// 2) Add Component -> SphereCollider, ติ๊ก Is Trigger
/// 3) ตั้ง Radius ตามต้องการ และตั้ง playerTag = "Player" (หรือเปลี่ยน)
/// 4) สร้าง UI ด้วย DialogueUI (ดูไฟล์ DialogueUI.cs)
/// </summary>
public class NPCProximityDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    public string speakerName = "";

    [TextArea(2, 6)]
    public string[] lines;

    [Tooltip("ถ้า true จะสุ่มประโยคทุกครั้งที่เข้าใกล้")]
    public bool randomLine = true;

    [Tooltip("ถ้า true จะแสดงประโยคถัดไปแบบวน เมื่อเข้าใกล้ซ้ำ")]
    public bool cycleLines = false;

    [Header("Activation")]
    public bool useTrigger = true;

    [Tooltip("ถ้าไม่ใช้ Trigger ให้ใส่ Transform ผู้เล่น (ถ้าว่างจะหาโดย tag)")]
    public Transform player;

    public string playerTag = "Player";

    [Tooltip("ใช้เมื่อ useTrigger=false")]
    public float distance = 3f;

    [Tooltip("อัปเดตระยะทุกกี่วินาที (ลดการคำนวณ)")]
    public float checkInterval = 0.1f;

    [Header("Behavior")]
    [Tooltip("คูลดาวน์ (วินาที) กันสแปมตอนเข้าออกเร็ว ๆ")]
    public float cooldown = 1.0f;

    [Tooltip("ถ้า true: ออกจากระยะแล้วซ่อนข้อความ")]
    public bool hideOnExit = true;

    [Tooltip("ถ้าใส่ UI อื่นในลิสต์นี้ (activeInHierarchy) จะไม่โชว์ข้อความ")]
    public GameObject[] blockIfUIActive;

    private bool _inRange;
    private float _nextAllowedTime = 0f;
    private float _nextCheckTime = 0f;
    private int _cycleIndex = 0;

    void Start()
    {
        if (!useTrigger)
        {
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go != null) player = go.transform;
            }
        }

        // ถ้า useTrigger=true แนะนำให้มี SphereCollider isTrigger
        // แต่ไม่บังคับ เพื่อให้คุณเลือกใช้ distance check ได้
    }

    void Update()
    {
        if (useTrigger) return;

        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + Mathf.Max(0.02f, checkInterval);

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) player = go.transform;
            if (player == null) return;
        }

        float d = Vector3.Distance(player.position, transform.position);
        bool nowInRange = d <= distance;

        if (nowInRange && !_inRange)
        {
            _inRange = true;
            TryShowLine();
        }
        else if (!nowInRange && _inRange)
        {
            _inRange = false;
            if (hideOnExit) DialogueUI.InstanceSafeHide();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        _inRange = true;
        TryShowLine();
    }

    void OnTriggerExit(Collider other)
    {
        if (!useTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        _inRange = false;
        if (hideOnExit) DialogueUI.InstanceSafeHide();
    }

    void TryShowLine()
    {
        if (Time.time < _nextAllowedTime) return;
        _nextAllowedTime = Time.time + Mathf.Max(0f, cooldown);

        if (IsBlockedByUI()) return;

        string line = PickLine();
        if (string.IsNullOrEmpty(line)) return;

        DialogueUI.InstanceSafeShow(speakerName, line);
    }

    bool IsBlockedByUI()
    {
        if (blockIfUIActive == null) return false;
        for (int i = 0; i < blockIfUIActive.Length; i++)
        {
            var go = blockIfUIActive[i];
            if (go != null && go.activeInHierarchy) return true;
        }
        return false;
    }

    string PickLine()
    {
        if (lines == null || lines.Length == 0) return "";

        if (cycleLines)
        {
            int idx = Mathf.Clamp(_cycleIndex, 0, lines.Length - 1);
            string result = lines[idx];
            _cycleIndex = (_cycleIndex + 1) % lines.Length;
            return result;
        }

        if (randomLine)
        {
            int idx = Random.Range(0, lines.Length);
            return lines[idx];
        }

        return lines[0];
    }
}
