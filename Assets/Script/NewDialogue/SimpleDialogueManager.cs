using UnityEngine;

/// <summary>
/// SimpleDialogueManager
/// - คุมสถานะบทสนทนา (กำลังคุย/บรรทัดปัจจุบัน)
/// - อินพุต:
///   - Mouse0 (คลิกซ้าย) : ไปบรรทัดถัดไป
///   - ESC : ปิดบทสนทนา
/// - ระหว่างเปิดบทสนทนา สามารถปิดสคริปต์ควบคุม FPS (MouseLook/InteractionRay/Controller) ผ่าน disableWhileOpen
/// </summary>
public class SimpleDialogueManager : MonoBehaviour
{
    public static SimpleDialogueManager Instance { get; private set; }

    [Header("References")]
    public SimpleDialogueUI ui;

    [Header("Input")]
    public KeyCode exitKey = KeyCode.Escape;

    [Header("Disable scripts while dialogue open")]
    public MonoBehaviour[] disableWhileOpen;

    [Header("Block open when these UI are active")]
    public GameObject[] blockIfUIActive;

    private bool _isOpen = false;
    private string _speaker = "";
    private string[] _lines;
    private int _index = 0;

    public bool IsOpen => _isOpen;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (ui == null) ui = FindObjectOfType<SimpleDialogueUI>(true);
        if (ui != null) ui.Hide();
    }

    void Update()
    {
        if (!_isOpen) return;

        // ESC เพื่อออก
        if (Input.GetKeyDown(exitKey))
        {
            Close();
            return;
        }

        // คลิกซ้ายไปต่อ
        if (Input.GetMouseButtonDown(0))
        {
            Next();
        }
    }

    public bool CanOpenDialogue()
    {
        if (blockIfUIActive == null) return true;
        for (int i = 0; i < blockIfUIActive.Length; i++)
        {
            var go = blockIfUIActive[i];
            if (go != null && go.activeInHierarchy)
                return false;
        }
        return true;
    }

    public void StartDialogue(NPCDialogueSource src)
    {
        if (src == null) return;
        if (!CanOpenDialogue()) return;

        var lines = src.GetLines();
        if (lines == null || lines.Length == 0) return;

        _speaker = src.GetSpeaker();
        _lines = lines;
        _index = 0;

        _isOpen = true;
        SetDisableList(true);

        ShowCurrent();
    }

    public void Next()
    {
        if (!_isOpen) return;

        _index++;
        if (_lines == null || _index >= _lines.Length)
        {
            Close();
            return;
        }

        ShowCurrent();
    }

    void ShowCurrent()
    {
        if (ui == null) ui = FindObjectOfType<SimpleDialogueUI>(true);
        if (ui == null) return;

        string line = (_lines != null && _index >= 0 && _index < _lines.Length) ? _lines[_index] : "";
        ui.Show(_speaker, line);
    }

    public void Close()
    {
        if (!_isOpen) return;

        _isOpen = false;
        _speaker = "";
        _lines = null;
        _index = 0;

        if (ui != null) ui.Hide();
        SetDisableList(false);
    }

    void SetDisableList(bool disable)
    {
        if (disableWhileOpen == null) return;
        for (int i = 0; i < disableWhileOpen.Length; i++)
        {
            var mb = disableWhileOpen[i];
            if (mb == null) continue;
            mb.enabled = !disable;
        }
    }
}
