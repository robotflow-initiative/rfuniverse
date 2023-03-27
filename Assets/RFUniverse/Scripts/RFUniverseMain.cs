using Newtonsoft.Json;
using RFUniverse.Attributes;
using RFUniverse.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RFUniverse
{
    public class RFUniverseMain : MonoBehaviour
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

        class ConfigData
        {
            public string assets_path = "";
            public string executable_file = "";
        }
        protected virtual void Awake()
        {
            Application.targetFrameRate = 60;
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string configPath = $"{userPath}/.rfuniverse/config.json";
            if (File.Exists(configPath))
            {
                string configString = File.ReadAllText(configPath);
                ConfigData config = JsonConvert.DeserializeObject<ConfigData>(configString, RFUniverseUtility.JsonSerializerSettings);
                if (Application.isEditor)
                    config.executable_file = "";
                else
                    config.executable_file = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                configString = JsonConvert.SerializeObject(config, Formatting.Indented, RFUniverseUtility.JsonSerializerSettings);
                File.WriteAllText(configPath, configString);
            }

            axisCamera.cullingMask = 1 << axisLayer;
        }
    }
}
