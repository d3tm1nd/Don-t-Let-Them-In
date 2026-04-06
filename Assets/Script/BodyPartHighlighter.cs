using UnityEngine;
using System.Collections.Generic;

// ตัวเลือกเสริม: ไฮไลต์ส่วนร่างกายที่ถูกเลือกด้วยการเปิด/ปิด Renderer หรือตั้งสี Emission
public class BodyPartHighlighter : MonoBehaviour
{
    [System.Serializable]
    public class RegionMap
    {
        public BodyRegion region;
        public Renderer[] renderers;         // กลุ่มเมชของส่วนนั้น
    }

    public Color highlightColor = new Color(1f, 0.7f, 0.2f, 1f);
    public List<RegionMap> maps = new List<RegionMap>();

    MaterialPropertyBlock _mpb;
    RegionMap _current;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }

    public void Highlight(BodyRegion region)
    {
        Clear();
        _current = maps.Find(m => m.region == region);
        if (_current != null)
        {
            foreach (var r in _current.renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_EmissionColor", highlightColor);
                r.SetPropertyBlock(_mpb);
                r.material.EnableKeyword("_EMISSION");
            }
        }
    }

    public void Clear()
    {
        if (_current == null) return;
        foreach (var r in _current.renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissionColor", Color.black);
            r.SetPropertyBlock(_mpb);
        }
        _current = null;
    }
}
