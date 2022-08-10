using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RFUniverse.Attributes;
using Newtonsoft.Json;

public class SceneExportToJSON : Editor
{
    [MenuItem("RFUniverse/SceneExportToJSON")]
    public static void Export()
    {
        SceneData data = new SceneData();
        List<BaseAttr> attrs = FindObjectsOfType<BaseAttr>().ToList();
        GameObject ground = GameObject.FindGameObjectWithTag("Ground");
        if (ground != null && ground.activeSelf && ground.GetComponent<MeshRenderer>().enabled)
            data.ground = true;
        else
            data.ground = false;
        data.cameraPosition = new float[] { Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z };
        data.cameraRotation = new float[] { Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z };
        List<BaseAttr> attrsTmp = new List<BaseAttr>(attrs);
        foreach (var item in attrsTmp)
        {
            if (item is ControllerAttr)
            {
                foreach (var child in (item as ControllerAttr).childs)
                {
                    attrs.Remove(child);
                }
            }
        }
        foreach (var item in attrs)
        {
            data.assetsData.Add(item.GetAttrData());
        }
        Debug.Log(data.assetsData.Count);
        File.WriteAllText($"{Application.streamingAssetsPath}/SceneData/{UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name}.json", JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        }));
    }
}
