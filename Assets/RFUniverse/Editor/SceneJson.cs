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
            AssetManager.Instance.SaveScene(path, attrs);
        }
        [MenuItem("RFUniverse/SceneJson/LoadScene")]
        public static void Load()
        {
            string path = EditorUtility.OpenFilePanel("SaveScene", Application.streamingAssetsPath + "/SceneData", "json");
            if (path == null) return;
            Clear();
            AssetManager.Instance.LoadScene(path, null, false);
        }
        [MenuItem("RFUniverse/SceneJson/ClearScene")]
        public static void Clear()
        {
            List<BaseAttr> attrs = FindObjectsOfType<BaseAttr>().ToList();
            AssetManager.Instance.ClearScene(attrs);
        }
    }
}
