using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RFUniverse
{
    public class PlayerMainUI : MonoBehaviour
    {
        DebugWindow debugWindow;
        ArticulationWindow articulationWindow;
        Button pend;
        Label unityVersionUI;
        Label pythonVersionUI;

        public void Init(Action onPendDone)
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            unityVersionUI = root.Q<Label>("unity");
            pythonVersionUI = root.Q<Label>("python");
            pend = root.Q<Button>("pend-button");
            pend.clicked += () => onPendDone.Invoke();
            pend.style.display = DisplayStyle.None;
            debugWindow = root.Q<DebugWindow>("debug-window");
            articulationWindow = root.Q<ArticulationWindow>("articulation-window");
            articulationWindow.style.display = DisplayStyle.None;
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

        public void ShowArticulationParameter(List<ArticulationBody> bodys)
        {
            articulationWindow.style.display = DisplayStyle.Flex;
            articulationWindow.Refresh(bodys);
        }

        public void ShowPend()
        {
            pend.style.display = DisplayStyle.Flex;
        }

        public void AddLog(string log)
        {
            debugWindow.AddLog(log);
        }

    }
}
