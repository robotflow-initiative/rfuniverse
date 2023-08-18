using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RFUniverse
{
    public class DebugWindow : VisualElement
    {
        ScrollView debugView;
        public new class UxmlFactory : UxmlFactory<DebugWindow> { }

        public int DebugViewMaxItemCount
        {
            get;
            set;
        }
        public DebugWindow()
        {
            DebugViewMaxItemCount = 100;
            Resources.Load<VisualTreeAsset>("debug-window").CloneTree(this);
            debugView = this.Q<ScrollView>("debug-view");
            debugView.contentContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            this.Q<Button>("debug-button").clicked += () =>
            {
                debugView.style.display = 1 - debugView.style.display.value;
            };
        }
        bool isBottom;
        public void AddLog(string log)
        {
            isBottom = (!debugView.verticalScroller.enabledInHierarchy) || debugView.verticalScroller.value == debugView.verticalScroller.highValue;
            TemplateContainer oneObjectItem = Resources.Load<VisualTreeAsset>("debug-item").Instantiate();
            Label label = oneObjectItem.Q<Label>("text-label");
            label.text = log.Replace('\\', '/');
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
}
