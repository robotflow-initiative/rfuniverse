using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse
{
    public class RFUniverseMain : MonoBehaviour
    {

        [SerializeField]
        private Camera mainCamera;
        public Camera MainCamera
        {
            get
            {
                return mainCamera;
            }
        }

        [SerializeField]
        private Camera axisCamera;
        public Camera AxisCamera
        {
            get
            {
                return axisCamera;
            }
        }

        [SerializeField]
        private GameObject ground;
        public GameObject Ground
        {
            get
            {
                return ground;
            }
        }

        public bool GroundActive
        {
            get
            {
                if (ground == null) return false;
                return ground.activeSelf;
            }
            set
            {
                ground.SetActive(value);
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

        class ConfigData
        {
            public string assets_path = "";
            public string executable_file = "";
        }
        protected virtual void Awake()
        {
            Application.targetFrameRate = 60;
            string userPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            string configPath = $"{userPath}/.rfuniverse/config.json";
            if (System.IO.File.Exists(configPath))
            {
                string configString = System.IO.File.ReadAllText(configPath);
                ConfigData config = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigData>(configString);
                if (Application.isEditor)
                    config.executable_file = "";
                else
                    config.executable_file = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                configString = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(configPath, configString);
            }

            AxisCamera.cullingMask = 1 << axisLayer;
        }
    }
}
