using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMainUI : MonoBehaviour
{
    ScrollView debugView;
    Button pend;
    Label unityVersionUI;
    Label pythonVersionUI;

    public int DebugViewMaxItemCount
    {
        get;
        set;
    }

    public void Init(Action onPendDone)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        DebugViewMaxItemCount = 100;
        unityVersionUI = root.Q<Label>("unity");
        pythonVersionUI = root.Q<Label>("python");
        pend = root.Q<Button>("pend-button");
        pend.clicked += () => onPendDone.Invoke();
        pend.style.display = DisplayStyle.None;
        debugView = root.Q<ScrollView>("debug-view");
        debugView.contentContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
        root.Q<Button>("debug-button").clicked += () =>
        {
            debugView.style.display = 1 - debugView.style.display.value;
        };
        unityVersionUI.text = "RFUniverse Version:" + Application.version;
    }
    public void SetPythonVersion(Version pythonVersion)
    {
        Version unityVersion = new Version(Application.version);
        pythonVersionUI.text = "pyrfuniverse Version:" + pythonVersion;
        if (!(unityVersion.Major == pythonVersion.Major && unityVersion.Minor == pythonVersion.Minor && unityVersion.Build == pythonVersion.Build))
        {
            unityVersionUI.style.color = Color.red;
            pythonVersionUI.style.color = Color.red;
        }
    }
    internal void ShowPend()
    {
        pend.style.display = DisplayStyle.Flex;
    }

    //public void RefreshLogList(string[] items)
    //{
    //    debugView.itemsSource = items;
    //    debugView.makeItem = MakeItem;
    //    debugView.bindItem = BindItem;
    //    debugView.Rebuild();
    //    debugView.ScrollToItem(items.Length);
    //}
    //private void BindItem(VisualElement ve, int index)
    //{
    //    Label label = ve.Q<Label>("text-label");
    //    label.text = (string)debugView.itemsSource[index];
    //}
    //private VisualElement MakeItem()
    //{
    //    TemplateContainer oneObjectItem = Resources.Load<VisualTreeAsset>("debug-item").Instantiate();
    //    return oneObjectItem;
    //}
    bool isBottom;
    public void AddLog(string log)
    {
        isBottom = (!debugView.verticalScroller.enabledInHierarchy) || debugView.verticalScroller.value == debugView.verticalScroller.highValue;
        TemplateContainer oneObjectItem = Resources.Load<VisualTreeAsset>("debug-item").Instantiate();
        Label label = oneObjectItem.Q<Label>("text-label");
        label.text = log;
        debugView.Add(oneObjectItem);
        if (debugView.childCount > DebugViewMaxItemCount)
            debugView.RemoveAt(0);
    }
    private void GeometryChanged(GeometryChangedEvent evt)
    {
        if (isBottom)
            debugView.ScrollTo(debugView.Children().Last());
    }
}
