using Newtonsoft.Json;
using RFUniverse.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RFUniverse.Manager;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RFUniverse
{
    public abstract class RFUniverseMain<T> : SingletonMono<T> where T : class
    {
        [SerializeField]
        private Camera mainCamera;
        public Camera MainCamera => mainCamera;

        [SerializeField]
        private Camera axisCamera;

        [SerializeField]
        private GameObject ground;
        public GameObject Ground => ground;

        public bool GroundActive
        {
            get
            {
                if (Ground == null) return false;
                return Ground.activeSelf;
            }
            set
            {
                Ground.SetActive(value);
            }
        }

        [SerializeField]
        private Light sun;
        public Light Sun
        {
            get
            {
                return sun;
            }
        }
        public LayerMask simulationLayer = 1 << 0;//常规显示层
        public int axisLayer = 6;//debug显示层
        public int tempLayer = 21;//相机渲染临时层
        public LayerMask managedLayer;


        LayerManager layerManager;

        protected override void Awake()
        {
            base.Awake();
            //JsonConvert.DefaultSettings = () => RFUniverseUtility.JsonSerializerSettings;
            //Application.targetFrameRate = 60;
            axisCamera.cullingMask = 1 << axisLayer;
            layerManager = LayerManager.Instance;
            layerManager.SetLayerPool(managedLayer);
        }

        protected Dictionary<string, GameObject> assets = new Dictionary<string, GameObject>();
        public K InstanceObject<K>(BaseAttrData baseAttrData, bool callInstance = true) where K : BaseAttr
        {
            Debug.Log("InstanceObject:" + baseAttrData.name);
            GameObject gameObject = GetGameObject(baseAttrData.name);
            //#if UNITY_EDITOR
            //            gameObject = (GameObject)PrefabUtility.InstantiatePrefab(gameObject);
            //#else
            gameObject = Instantiate(gameObject);
            //#endif
            gameObject.name = gameObject.name.Replace("(Clone)", "");
            K attr = gameObject.GetComponent<K>();
            if (attr != null)
            {
                baseAttrData.SetAttrData(attr);
                Debug.Log("Instance Done " + attr.Name + " ID:" + attr.ID);
                if (callInstance)
                    attr.Instance();
            }
            return attr;
        }

        public GameObject GetGameObject(string name)
        {
            if (assets.TryGetValue(name, out GameObject gameObject))
            {
                return gameObject;
            }
            else
            {
                var locations = Addressables.LoadResourceLocationsAsync(name).WaitForCompletion();
                if (locations.Count > 0)
                {
                    GameObject obj = Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
                    assets.Add(name, obj);
                    return obj;
                }
            }
            return null;
        }

        public List<BaseAttr> LoadScene(string file, bool callInstance = true)
        {
            if (!file.Contains('/') && !file.Contains('\\'))
                file = $"{Application.streamingAssetsPath}/SceneData/{file}";
            if (!file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                file += ".json";
            if (!File.Exists(file))
            {
                Debug.LogWarning($"Not Find Scene Json File: {file}");
                return null;
            }
            SceneData data = JsonConvert.DeserializeObject<SceneData>(File.ReadAllText(file), RFUniverseUtility.JsonSerializerSettings);
            data.assetsData = RFUniverseUtility.SortByParent(data.assetsData);
            List<BaseAttr> attrs = new List<BaseAttr>();
            foreach (var item in data.assetsData)
            {
                BaseAttr one = InstanceObject<BaseAttr>(item, callInstance);
                if (one)
                    attrs.Add(one);
            }
            GroundActive = data.ground;
            MainCamera.transform.position = new Vector3(data.cameraPosition[0], data.cameraPosition[1], data.cameraPosition[2]);
            MainCamera.transform.eulerAngles = new Vector3(data.cameraRotation[0], data.cameraRotation[1], data.cameraRotation[2]);
            Ground.transform.position = new Vector3(data.groundPosition[0], data.groundPosition[1], data.groundPosition[2]);
            return attrs;
        }
        public void SaveScene(string file, List<BaseAttr> attrs)
        {
            if (!file.Contains('/') && !file.Contains('\\'))
                file = $"{Application.streamingAssetsPath}/SceneData/{file}";
            if (!file.EndsWith(".json"))
                file += ".json";

            SceneData data = new SceneData();
            if (MainCamera)
            {
                data.cameraPosition = new float[] { MainCamera.transform.position.x, MainCamera.transform.position.y, MainCamera.transform.position.z };
                data.cameraRotation = new float[] { MainCamera.transform.eulerAngles.x, MainCamera.transform.eulerAngles.y, MainCamera.transform.eulerAngles.z };
            }
            data.ground = GroundActive;
            if (data.ground)
                data.groundPosition = new float[] { Ground.transform.position.x, Ground.transform.position.y, Ground.transform.position.z };
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
            data.assetsData = RFUniverseUtility.SortByParent(data.assetsData);
            Debug.Log(data.assetsData.Count);
            File.WriteAllText(file, JsonConvert.SerializeObject(data, Formatting.Indented, RFUniverseUtility.JsonSerializerSettings));
        }
    }
}
