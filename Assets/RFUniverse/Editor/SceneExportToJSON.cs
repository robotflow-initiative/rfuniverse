using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RFUniverse.Attributes;
using Newtonsoft.Json;


namespace RFUniverse
{
    public class SceneExportToJSON : Editor
    {
        [MenuItem("RFUniverse/SceneExportToJSON")]
        public static void Export()
        {
            SceneData data = new SceneData();
            List<BaseAttr> attrs = FindObjectsOfType<BaseAttr>().ToList();
            data.ground = PlayerMain.Instance.GroundActive;
            if (!PlayerMain.Instance.mainCamera)
            {
                UnityEngine.Debug.LogError("No Camera");
                return;
            }
            data.cameraPosition = new float[] { PlayerMain.Instance.mainCamera.transform.position.x, PlayerMain.Instance.mainCamera.transform.position.y, PlayerMain.Instance.mainCamera.transform.position.z };
            data.cameraRotation = new float[] { PlayerMain.Instance.mainCamera.transform.eulerAngles.x, PlayerMain.Instance.mainCamera.transform.eulerAngles.y, PlayerMain.Instance.mainCamera.transform.eulerAngles.z };
            if (data.ground)
                data.groundPosition = new float[] { PlayerMain.Instance.Ground.transform.position.x, PlayerMain.Instance.Ground.transform.position.y, PlayerMain.Instance.Ground.transform.position.z };
            List<BaseAttr> attrsTmp = new List<BaseAttr>(attrs);
            foreach (var item in attrsTmp)
            {
                foreach (var child in item.childs)
                {
                    attrs.Remove(child);
                }
            }
            foreach (var item in attrs)
            {
                data.assetsData.Add(item.GetAttrData());
            }
            UnityEngine.Debug.Log(data.assetsData.Count);
            File.WriteAllText($"{Application.streamingAssetsPath}/SceneData/{UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name}.json", JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            }));
        }
    }
}
