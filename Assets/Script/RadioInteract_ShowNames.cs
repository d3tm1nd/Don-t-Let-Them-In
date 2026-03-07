using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class RadioInteract_ShowNames : MonoBehaviour, IInteractable
{
    [Header("Layer (must match InteractionRay)")]
    public string requiredLayerName = "interactable";

    [Header("UI")]
    public RadioNamesUI namesUI;                 // assign in Inspector or enable auto-find
    public bool autoFindUIInScene = true;

    void Reset()
    {
        EnsureLayer();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;  // make raycast friendly
    }

    void Awake()
    {
        EnsureLayer();
    }

    private void EnsureLayer()
    {
        int layer = LayerMask.NameToLayer(requiredLayerName);
        if (layer == -1) { Debug.LogWarning($"RadioInteract_ShowNames: Layer '{requiredLayerName}' not found."); return; }
        gameObject.layer = layer;
    }

    public void Interact()
    {
        if (autoFindUIInScene && namesUI == null)
            namesUI = FindObjectOfType<RadioNamesUI>(true);

        if (namesUI == null)
        {
            Debug.LogError("❌ RadioInteract_ShowNames: RadioNamesUI not found in scene (assign it or enable autoFindUIInScene)");
            return;
        }

        List<string> names = new List<string>();
        if (NPCDataManager.Instance != null)
        {
            var list = NPCDataManager.Instance.acceptedNPCs;
            for (int i = 0; i < list.Count; i++)
            {
                var d = list[i];
                string n = (d.prefab != null) ? d.prefab.name : "(ไม่มีชื่อ)";
                names.Add(n);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่มี NPCDataManager.Instance ในซีน — จะแสดงเป็นรายการว่าง");
        }

        namesUI.ShowNames(names);
    }
}
