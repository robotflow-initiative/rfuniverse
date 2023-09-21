using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
            Addressables.LoadAssetAsync<VisualTreeAsset>("UITookit/debug-window").WaitForCompletion().CloneTree(this);
            debugView = this.Q<ScrollView>("debug-view");
            debugView.contentContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            this.Q<Button>("debug-button").clicked += () =>
            {
                debugView.style.display = 1 - debugView.resolvedStyle.display;
            };
        }
        bool isBottom;
        VisualTreeAsset logItem;
        public void AddLog(string log)
        {
            isBottom = debugView.verticalScroller.value == debugView.verticalScroller.highValue;
            if(logItem == null)
            {
                logItem = Addressables.LoadAssetAsync<VisualTreeAsset>("UITookit/debug-item").WaitForCompletion();
            }
            TemplateContainer oneLogItem = logItem.Instantiate();
            Label label = oneLogItem.Q<Label>("text-label");
            label.text = log.Replace('\\', '/');
            debugView.Add(oneLogItem);
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
