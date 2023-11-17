using RFUniverse.Attributes;
using RFUniverse.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RFUniverse
{
    public class SceneJson : Editor
    {
        [MenuItem("RFUniverse/SceneJson/SaveScene")]
        public static void Save()
        {
            string path = EditorUtility.SaveFilePanel("SaveScene", Application.streamingAssetsPath + "/SceneData", "", "json");
            if (path == null) return;
            List<BaseAttr> attrs = FindObjectsOfType<BaseAttr>().ToList();
            PlayerMain.Instance.SaveScene(path, attrs);
        }
        [MenuItem("RFUniverse/SceneJson/LoadScene")]
        public static void Load()
        {
            string path = EditorUtility.OpenFilePanel("SaveScene", Application.streamingAssetsPath + "/SceneData", "json");
            if (path == null) return;
            Clear();
            PlayerMain.Instance.LoadScene(path, true);
        }
        [MenuItem("RFUniverse/SceneJson/ClearScene")]
        public static void Clear()
        {
            List<BaseAttr> attrs = FindObjectsOfType<BaseAttr>().ToList();
            PlayerMain.Instance.ClearScene(attrs);
        }
    }
}
