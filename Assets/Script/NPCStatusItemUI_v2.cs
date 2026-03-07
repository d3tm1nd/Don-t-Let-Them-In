using UnityEngine;
using TMPro;

public class NPCStatusItemUI_v2 : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;

    public void Set(string name, string status)
    {
        if (nameText != null) nameText.text = name;
        if (statusText != null) statusText.text = status;
    }
}
