using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMainUI : MonoBehaviour
{
    ListView debugView;
    Button pend;
    public void Init(Version pythonVersion, Action onPendDone)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Label unityVersionUI = root.Q<Label>("unity");
        Label pythonVersionUI = root.Q<Label>("python");
        pend = root.Q<Button>("pend-button");
        pend.clicked += () => onPendDone.Invoke();
        pend.style.display = DisplayStyle.None;
        debugView = root.Q<ListView>("debug-view");
        root.Q<Button>("debug-button").clicked += () => debugView.style.display = 1 - debugView.style.display.value;


        Version unityVersion = new Version(Application.version);
        unityVersionUI.text = "RFUniverse Version:" + unityVersion;
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
    public void RefreshLogList(string[] items)
    {
        debugView.itemsSource = items;
        debugView.makeItem = MakeItem;
        debugView.bindItem = BindItem;
    }
    private void BindItem(VisualElement ve, int index)
    {
        Label label = ve.Q<Label>("text-label");
        label.text = (string)debugView.itemsSource[index];
    }

    private VisualElement MakeItem()
    {
        TemplateContainer oneObjectItem = Resources.Load<VisualTreeAsset>("debug-item").Instantiate();
        return oneObjectItem;
    }
}
