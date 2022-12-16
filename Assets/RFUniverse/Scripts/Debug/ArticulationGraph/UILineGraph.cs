using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILineGraph : MonoBehaviour
{
    public List<RectTransform> lines = new List<RectTransform>();
    public TextMeshProUGUI titel;

    public RectTransform upper;
    public TextMeshProUGUI upperText;

    public RectTransform middle;

    public RectTransform lower;
    public TextMeshProUGUI lowerText;
    private void Awake()
    {
        lower.anchoredPosition = upper.anchoredPosition * -1;
    }
    public void Init(string titelString, Color color)
    {
        titel.text = titelString;
        foreach (var item in lines[0].GetComponentsInChildren<Image>())
        {
            item.color = color;
        }
    }
    //float max = 0;
    public void SetData(List<float> datas)
    {
        if (datas.Count == 0) return;
        float max = Mathf.Max(Mathf.Abs(datas.Max()), Mathf.Abs(datas.Min()),1f);
        upperText.text = max.ToString("f2");
        lowerText.text = "-" + max.ToString("f2");
        List<float> h = new List<float>();
        List<float> t = new List<float>();
        for (int i = 0; i < datas.Count; i++)
        {
            float data = datas[i];
            h.Add(Mathf.LerpUnclamped(0, upper.anchoredPosition.y, data / max));
            t.Add(middle.sizeDelta.x / (datas.Count-1) * i);
        }
        List<float> l = new List<float>();
        List<float> a = new List<float>();
        for (int i = 0; i < datas.Count-1; i++)
        {
            Vector2 dir = new Vector2(t[i + 1], h[i + 1]) - new Vector2(t[i], h[i]);
            l.Add(dir.magnitude);
            a.Add(Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg);
        }
        for (int i = 0; i < datas.Count - 1; i++)
        {
            while (lines.Count < datas.Count-1)
                lines.Add(Instantiate(lines[0], lines[0].parent));
            lines[i].anchoredPosition = new Vector2(t[i], h[i]);
            lines[i].sizeDelta = new Vector2(l[i], lines[i].sizeDelta.y);
            lines[i].localEulerAngles = Vector3.forward * a[i];
        }
    }
}
