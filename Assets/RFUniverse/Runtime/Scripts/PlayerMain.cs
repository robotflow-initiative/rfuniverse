#if OBI
using Obi;
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif
using Newtonsoft.Json;
using RFUniverse.Attributes;
using RFUniverse.Manager;
using Robotflow.RFUniverse.SideChannels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if TRILIB
using TriLibCore;
using System.Reflection;
#endif

namespace RFUniverse
{
    public enum CommunicationBackend
    {
        TCP,
        gRPC,
    }
    public class PlayerMain : RFUniverseMain<PlayerMain>, IReceiveData, IDistributeData<string>, IHaveAPI, ICollectData
    {
        public const string VERSION = "0.30.0";

        public int port = 5004;

        public CommunicationBackend communicationBackend = CommunicationBackend.TCP;
        [HideInInspector]
        public int patchNumber;
        public PlayerMainUI playerMainUI;
        public static RFUniverseCommunicatorBase Communicator;
        public int clientTime = 30;

        [SerializeField]
        float fixedDeltaTime = 0.02f;

        public float FixedDeltaTime
        {
            get
            {
                return fixedDeltaTime;
            }
            set
            {
                fixedDeltaTime = value;
                Time.fixedDeltaTime = fixedDeltaTime;
            }
        }

        [SerializeField]
        float timeScale = 1;
        public float TimeScale
        {
            get
            {
                return timeScale;
            }
            set
            {
                timeScale = value;
                Time.timeScale = timeScale;
            }
        }

        public ICollectData CollectData => this;

        private void OnValidate()
        {
            if (!Application.isPlaying && Instance != this)
                base.Awake();
        }


        protected override void Awake()
        {
            base.Awake();

            //加载所有catalog
#if !UNITY_EDITOR
#if UNITY_STANDALONE_WIN
            string path = $"{Addressables.RuntimePath}/StandaloneWindows64";
#elif UNITY_STANDALONE_LINUX
            string path = $"{Addressables.RuntimePath}/StandaloneLinux64";
#endif

            foreach (var item in Directory.GetFiles(path, "catalog_*.json"))
            {
                Addressables.LoadContentCatalogAsync(item).WaitForCompletion();
            }
#endif

            (this as IDistributeData<string>).RegisterReceiver("Env", ReceiveEnvData);
            (this as IDistributeData<string>).RegisterReceiver("PhysicsScene", PhysicsSceneManager.Instance.ReceiveData);
            (this as IDistributeData<string>).RegisterReceiver("Debug", DebugManager.Instance.ReceiveData);
            (this as IDistributeData<string>).RegisterReceiver("Instance", InstanceManager.Instance.ReceiveData);
            (this as IDistributeData<string>).RegisterReceiver("Message", MessageManager.Instance.ReceiveMessageData);
            (this as IDistributeData<string>).RegisterReceiver("Object", MessageManager.Instance.ReceiveData);

            patchNumber = PlayerPrefs.GetInt("Patch", 0);
            FixedDeltaTime = fixedDeltaTime;
            TimeScale = timeScale;

            playerMainUI.Init();
            playerMainUI.OnPendDone = () =>
            {
                Debug.Log("PendDone");
                CollectData.AddDataNextStep("pend_done", null);
            };

            (this as IHaveAPI).RegisterAPI();

            BaseAttr[] sceneAttrs = FindObjectsOfType<BaseAttr>(true);
            List<BaseAttr> noParentAttr = new List<BaseAttr>(sceneAttrs);
            foreach (var item in sceneAttrs)
            {
                foreach (var child in item.childs)
                {
                    noParentAttr.Remove(child);
                }
            }
            foreach (var item in noParentAttr)
            {
                if (item != null)
                    item.Instance();
            }

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor("_IDColor", Color.black);
            foreach (var render in Ground.GetComponentsInChildren<Renderer>())
            {
                render.SetPropertyBlock(mpb);
            }
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i].StartsWith("-port:"))
                {
                    startWithPort = true;
                    if (int.TryParse(commandLineArgs[i].Remove(0, 6), out int value))
                        port = value;
                }
                if (commandLineArgs[i].StartsWith("-grpc"))
                {
                    communicationBackend = CommunicationBackend.gRPC;
                }
                else if (commandLineArgs[i].StartsWith("-tcp"))
                {
                    communicationBackend = CommunicationBackend.TCP;
                }
            }

            if (Communicator == null)
            {
                switch (communicationBackend)
                {
                    default:
                    case CommunicationBackend.TCP:
                        Communicator = new RFUniverseCommunicatorTCP("localhost", port, clientTime, InitCommunicator);
                        break;
                    case CommunicationBackend.gRPC:
                        Communicator = new RFUniverseCommunicatorGRPC("localhost", port, clientTime, InitCommunicator);
                        break;
                }
            }
            else if (Communicator.Connected)
            {
                InitCommunicator();
            }
            else
            {
                Debug.LogWarning("Communicator is Disconnect");
                QuitApp();
            }
        }

        void InitCommunicator()
        {
            Debug.Log("Connected successfully");
            Physics.simulationMode = SimulationMode.Script;

            OnStepAction += Step;

            Communicator.OnReceivedData = (data) =>
            {
                (this as IReceiveData).ReceiveData(data);
            };
            Communicator.OnDisconnect = () =>
            {
                QuitApp();
            };
            CollectData.AddDataNextStep("scene_init", null);
            CollectData.AddDataNextStep("rfu_version", VERSION);
        }

        void QuitApp()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        public class ConfigData
        {
            public string assets_path;
            public string executable_file;
        }

        bool startWithPort = false;
        void OnApplicationQuit()
        {
            if (!startWithPort)
            {
                string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string configPath = $"{userPath}/.rfuniverse/config.json";
                if (File.Exists(configPath))
                {
                    string configString = File.ReadAllText(configPath);
                    ConfigData config = JsonConvert.DeserializeObject<ConfigData>(configString);
#if UNITY_EDITOR
                    config.executable_file = "@editor";
#else
                    string platformExtend = Application.platform == RuntimePlatform.WindowsPlayer ? "exe" : "x86_64";
                    config.executable_file = $"{Application.dataPath.Replace("_Data", "")}.{platformExtend}";
#endif
                    configString = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(configPath, configString);
                }
            }
            CollectData.AddDataNextStep("close", null);
            Communicator?.Dispose();
        }

        public Action OnStepAction;

        void FixedUpdate()
        {
            OnStepAction?.Invoke();
        }

        public void Step()
        {
            //if (autoSimulate)
            //Physics.Simulate(fixedDeltaTime);
            Communicator?.SyncStepEnd();
        }
        //[RFUAPI]
        //public void SetAutoSimulate(bool auto)
        //{
        //    autoSimulate = auto;
        //}

        [RFUAPI(null, false)]
        public void Simulate(float fixedDeltaTime = -1, int count = 1)
        {
            if (Physics.simulationMode != SimulationMode.Script) return;
            if (fixedDeltaTime <= 0)
                fixedDeltaTime = Time.fixedDeltaTime;
            count = Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                Physics.Simulate(fixedDeltaTime);
            }
        }

        //[RFUAPI]
        //public void SetAutoCollect(bool auto)
        //{
        //    autoCollect = auto;
        //}

        [RFUAPI(null, false)]
        public void Collect()
        {
            InstanceManager.Instance.CollectAllAttrData();
            Communicator?.SendObject("Env", CollectData.CollectData());
        }

        Dictionary<string, Action<object[]>> IDistributeData<string>.Receiver { get; set; }

        void IReceiveData.ReceiveData(object[] data)
        {
            string hand = (string)data[0];
            data = data.Skip(1).ToArray();
            (this as IDistributeData<string>).DistributeData(hand, data);
        }

        void ReceiveEnvData(object[] data)
        {
            string hand = (string)data[0];
            data = data.Skip(1).ToArray();
            (this as IHaveAPI).CallAPI(hand, data);
        }
        void ICollectData.AddPermanentData(Dictionary<string, object> data)
        {
            data["time_scale"] = TimeScale;
            data["fixed_delta_time"] = FixedDeltaTime;
        }

        Dictionary<string, object> ICollectData.TemporaryData { get; set; }

        [RFUAPI]
        public void SetShadowDistance(float dis)
        {
            QualitySettings.shadowDistance = dis;
        }
        [RFUAPI]
        public void SetViewTransform(List<float> posiiton, List<float> rotation)
        {
            if (posiiton != null)
            {
                MainCamera.transform.position = RFUniverseUtility.ListFloatToVector3(posiiton);
            }
            if (rotation != null)
            {
                MainCamera.transform.eulerAngles = RFUniverseUtility.ListFloatToVector3(rotation);
            }
        }
        [RFUAPI]
        public void GetViewTransform()
        {
            CollectData.AddDataNextStep("view_position", MainCamera.transform.position);
            CollectData.AddDataNextStep("view_rotation", MainCamera.transform.eulerAngles);
            CollectData.AddDataNextStep("view_quaternion", MainCamera.transform.rotation);
        }
        [RFUAPI]
        public virtual void ViewLookAt(List<float> target, List<float> worldUp)
        {
            MainCamera.transform.LookAt(RFUniverseUtility.ListFloatToVector3(target), RFUniverseUtility.ListFloatToVector3(worldUp));
        }
        [RFUAPI]
        public void SetViewBackGround(List<float> color)
        {
            if (color == null)
                MainCamera.clearFlags = CameraClearFlags.Skybox;
            else
            {
                MainCamera.clearFlags = CameraClearFlags.SolidColor;
                MainCamera.backgroundColor = RFUniverseUtility.ListFloatToColor(color);
            }
        }

        [RFUAPI]
        public void Pend()
        {
            playerMainUI.ShowPend();
        }
        [RFUAPI]
        public void PreLoadAssetsAsync(List<string> names, Action onCompleted = null, bool sendDoneMsg = true)
        {
            names = names.Distinct().ToList();
            int loadedCount = 0;
            foreach (string name in names)
            {
                PreLoadAssetAsync(name, () =>
                {
                    loadedCount++;
                    if (loadedCount == names.Count)
                    {
                        if (sendDoneMsg)
                        {
                            SendLoadDoneMsg();
                        }
                        onCompleted?.Invoke();
                    }
                });
            }
        }
        public void PreLoadAssetAsync(string name, Action onCompleted = null)
        {
            if (assets.ContainsKey(name))
            {
                onCompleted?.Invoke();
            }
            else
            {
                Debug.Log("LoadAsset:" + name);
#if UNITY_EDITOR
                AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;
                List<AddressableAssetEntry> entrys = new List<AddressableAssetEntry>();
                setting.GetAllAssets(entrys, false);
                AddressableAssetEntry entry = entrys.FirstOrDefault(s => s.address == name);
                if (entry == null) return;
                GameObject asset = (GameObject)entry.MainAsset;
                if (asset != null)
                {
                    assets.Add(name, asset);
                    onCompleted?.Invoke();
                }
                else
                {
                    Debug.LogWarning($"Not Find Editor Asset: {name}");
                }
#else
                Addressables.LoadAssetAsync<GameObject>(name).Completed += (handle) =>
                {
                    if (!assets.ContainsKey(name))
                        assets.Add(name, handle.Result);
                    onCompleted?.Invoke();
                };
#endif
            }
        }
        [RFUAPI]
        public void LoadSceneAsync(string file)
        {
            LoadScene(file);
            SendLoadDoneMsg();
        }
        [RFUAPI]
        public void AlignCamera(int cameraID)
        {
            if (!BaseAttr.Attrs.ContainsKey(cameraID))
            {
                Debug.LogWarning($"not find align target camera {cameraID}");
                return;
            }
            BaseAttr camera = BaseAttr.Attrs[cameraID];
            MainCamera.transform.position = camera.transform.position;
            MainCamera.transform.rotation = camera.transform.rotation;
        }
        [RFUAPI]
        public void SaveScene(string path)
        {
            SaveScene(path, BaseAttr.Attrs.Values.ToList());
        }
        [RFUAPI]
        public void ClearScene()
        {
            foreach (var item in BaseAttr.Attrs.Values)
            {
                item.Destroy();
            }
        }
        [RFUAPI]
        public void SwitchSceneAsync(string name)
        {
            Addressables.LoadSceneAsync(name).WaitForCompletion();
        }
        public void ClearScene(List<BaseAttr> attrs)
        {
            foreach (var item in attrs)
            {
                item.Destroy();
            }
            GroundActive = true;
        }
        void SendLoadDoneMsg()
        {
            CollectData.AddDataNextStep("load_done", null);
        }

        [RFUAPI]
        public void InstanceObject(string name, int id)
        {
            InstanceObject<BaseAttr>(name, id, true);
        }
        public T InstanceObject<T>(string name, int id, bool callInstance = true) where T : BaseAttr
        {
            Debug.Log(name);
            GameObject gameObject = GetGameObject(name);
            if (gameObject == null)
            {
                Debug.LogWarning($"Asset {name} is not exist");
                return null;
            }
            gameObject = Instantiate(gameObject);
            gameObject.name = gameObject.name.Replace("(Clone)", "");
            T attr = gameObject.GetComponent<T>();
            if (attr == null)
            {
                Debug.LogWarning($"{name} is not {nameof(T)}");
                return null;
            }
            attr.ID = id;
            attr.Name = name;
            Debug.Log("Instance Done " + attr.Name + " ID:" + attr.ID);
            if (callInstance)
                attr.Instance();
            return attr;
        }
        [RFUAPI]
        public void LoadURDF(int id, string path, bool nativeIK, string axis)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);
            Debug.Log($"LoadURDF: {path}");
            ImportSettings setting = new ImportSettings();
            setting.chosenAxis = axis == "z" ? ImportSettings.axisType.zAxis : ImportSettings.axisType.yAxis;
            setting.convexMethod = ImportSettings.convexDecomposer.unity;
            GameObject robot = UrdfRobotExtensions.CreateRuntime(path, setting);
            robot.transform.SetParent(null);
            ControllerAttr attr = RFUniverseUtility.NormalizeRFUniverseArticulation(robot);
            CreateReference newReference = new CreateReference(attr);
            attr.CreateReference = newReference;
            attr.ID = id;
            attr.Name = Path.GetFileNameWithoutExtension(path);
            attr.initBioIK = nativeIK;
            attr.Instance();
        }

        [RFUAPI]
        public RigidbodyAttr LoadMesh(int id, string path, bool autoInstance = true, string colliderMode = "")
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);
            Debug.Log($"LoadMesh: {path}");
            GameObject obj;
#if TRILIB
            AssetLoaderOptions assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            obj = AssetLoader.LoadModelFromFileNoThread(path, null, null, assetLoaderOptions).RootGameObject;
#else
            obj = UnityMeshImporter.MeshImporter.Load(path);
#endif
            if (!obj) return null;
            RigidbodyAttr attr = obj.AddComponent<RigidbodyAttr>();
            CreateReference newReference = new CreateReference(attr);
            attr.CreateReference = newReference;
            foreach (var item in obj.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in item.materials)
                {
                    mat.SetFloat("_Metallic", 0);
                    mat.SetFloat("_Glossiness", 0);
                }
            }
            attr.ID = id;
            attr.Name = Path.GetFileNameWithoutExtension(path);
            switch (colliderMode)
            {
                case "VHACD":
                    attr.GenerateVHACDCollider();
                    break;
                case "CoACD":
                    attr.GenerateCoACDCollider();
                    break;
                case "Convex":
                    attr.GenerateConvexCollider();
                    break;
            }

            if (autoInstance)
                attr.Instance();
            return attr;
        }

#if OBI
        [RFUAPI]
        public ClothAttr LoadCloth(string path, int id)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);
            Debug.Log($"LoadCloth: {path}");
            GameObject obj = UnityMeshImporter.MeshImporter.Load(path);
            Mesh mesh = obj.GetComponentInChildren<MeshFilter>().sharedMesh;
            ObiSolver obiSolver = Addressables.LoadAssetAsync<GameObject>("ObiClothStencil").WaitForCompletion().GetComponent<ObiSolver>();
            obiSolver.GetComponentInChildren<ObiCloth>().clothBlueprint = ObiUtility.GenerateClothBlueprints(mesh);
            Destroy(obj);
            ObiSolver instance = Instantiate(obiSolver);
            ClothAttr attr = instance.GetComponent<ClothAttr>();
            CreateReference newReference = new CreateReference(attr);
            attr.CreateReference = newReference;
            attr.ID = id;
            attr.Instance();
            return attr;
        }
#endif

        [RFUAPI]
        public void EnabledGroundObiCollider(bool enabled)
        {
#if OBI
            ObiCollider collider = Ground.GetComponent<ObiCollider>() ?? Ground.AddComponent<ObiCollider>();
            collider.enabled = enabled;
#endif
        }

        [RFUAPI]
        public void IgnoreLayerCollision(int layer1, int layer2, bool ignore)
        {
            Physics.IgnoreLayerCollision(layer1, layer2, ignore);
        }
        [RFUAPI]
        public void GetCurrentCollisionPairs()
        {
            CollectData.AddDataNextStep("current_collisio_pairs", ColliderAttr.CollisionPairs);
        }
        [RFUAPI]
        public void GetRFMoveColliders()
        {
            Dictionary<int, List<Dictionary<string, object>>> rfmoveColliders = new();
            List<ColliderAttr> colliderAttrs = BaseAttr.ActiveAttrs.Values.Select(s => s as ColliderAttr).Where(s => s.IsRFMoveCollider).ToList();
            foreach (var attr in colliderAttrs)
            {
                List<Dictionary<string, object>> oneAttr = new();
                rfmoveColliders.Add(attr.ID, oneAttr);
                List<Collider> colliders = attr.GetComponentsInChildren<Collider>().Where(s => s.enabled && s.gameObject.activeInHierarchy && !s.isTrigger).ToList();
                foreach (var collider in colliders)
                {
                    Dictionary<string, object> oneAttrColliders = new();
                    oneAttr.Add(oneAttrColliders);
                    Vector3 pos;
                    switch (collider)
                    {
                        case BoxCollider boxCollider:
                            oneAttrColliders.Add("type", "box");
                            pos = boxCollider.transform.TransformPoint(boxCollider.center);
                            oneAttrColliders.Add("position", pos);
                            oneAttrColliders.Add("rotation", collider.transform.rotation);
                            oneAttrColliders.Add("size", new Vector3(boxCollider.transform.lossyScale.x * boxCollider.size.x, boxCollider.transform.lossyScale.y * boxCollider.size.y, boxCollider.transform.lossyScale.z * boxCollider.size.z));
                            break;
                        case SphereCollider sphereCollider:
                            oneAttrColliders.Add("type", "sphere");
                            pos = sphereCollider.transform.TransformPoint(sphereCollider.center);
                            oneAttrColliders.Add("position", pos);
                            oneAttrColliders.Add("radius", sphereCollider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y, collider.transform.lossyScale.z));
                            break;
                        case CapsuleCollider capsuleCollider:
                            oneAttrColliders.Add("type", "capsule");
                            pos = capsuleCollider.transform.TransformPoint(capsuleCollider.center);
                            oneAttrColliders.Add("position", pos);
                            oneAttrColliders.Add("rotation", collider.transform.rotation);
                            oneAttrColliders.Add("direction", capsuleCollider.direction);
                            switch (capsuleCollider.direction)
                            {
                                case 0:
                                    oneAttrColliders.Add("radius", capsuleCollider.radius * Mathf.Max(collider.transform.lossyScale.y, collider.transform.lossyScale.z));
                                    oneAttrColliders.Add("height", capsuleCollider.height * collider.transform.lossyScale.x);
                                    break;
                                case 1:
                                    oneAttrColliders.Add("radius", capsuleCollider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.z));
                                    oneAttrColliders.Add("height", capsuleCollider.height * collider.transform.lossyScale.y);
                                    break;
                                case 2:
                                default:
                                    oneAttrColliders.Add("radius", capsuleCollider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y));
                                    oneAttrColliders.Add("height", capsuleCollider.height * collider.transform.lossyScale.z);
                                    break;
                            }
                            break;
                        default:
                            oneAttrColliders.Add("type", "box");
                            oneAttrColliders.Add("position", collider.bounds.center);
                            oneAttrColliders.Add("rotation", Quaternion.identity);
                            oneAttrColliders.Add("size", collider.bounds.size);
                            break;
                    }
                }
            }
            CollectData.AddDataNextStep("rfmove_colliders", rfmoveColliders);
        }
        [RFUAPI]
        public void SetGravity(List<float> gravity)
        {
            if (gravity.Count != 3) return;
            Physics.gravity = new Vector3(gravity[0], gravity[1], gravity[2]);
        }
        [RFUAPI]
        public void SetGroundActive(bool actice)
        {
            GroundActive = actice;
        }
        [RFUAPI]
        public void SetGroundPhysicMaterial(float bounciness, float dynamicFriction, float staticFriction, int frictionCombine, int bounceCombine)
        {
            Ground.GetComponent<Collider>().material = new PhysicMaterial
            {
                bounciness = bounciness,
                dynamicFriction = dynamicFriction,
                staticFriction = staticFriction,
                frictionCombine = (PhysicMaterialCombine)frictionCombine,
                bounceCombine = (PhysicMaterialCombine)bounceCombine
            };
        }
        [RFUAPI]
        public void SetTimeStep(float timeStep)
        {
            FixedDeltaTime = timeStep;
        }
        [RFUAPI]
        public void SetTimeScale(float timeScale)
        {
            TimeScale = timeScale;
        }
        [RFUAPI]
        public void SetResolution(int width, int height)
        {
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
        }
        [RFUAPI]
        public void ExportOBJ(List<int> ids, string path)
        {
            ExportOBJ(BaseAttr.Attrs.Where((s) => ids.Contains(s.Key)).Select((s) => s.Value.gameObject).ToArray(), path);
        }

        public void ExportOBJ(GameObject[] meshs, string path)
        {
            new OBJExporter().Export(meshs, path);
        }


        [Obsolete("AddListener is the older interface, and AddListenerObject is the recommended interface for dynamic messaging")]
        public void AddListener(string message, Action<IncomingMessage> action)
        {
            MessageManager.Instance.AddListener(message, action);
        }

        [Obsolete("RemoveListener is the older interface, and RemoveListenerObject is the recommended interface for dynamic messaging")]
        public void RemoveListener(string message)
        {
            MessageManager.Instance.RemoveListener(message);
        }
        [Obsolete("SendMessage is the older interface, and SendObject is the recommended interface for dynamic messaging")]
        public void SendMessage(string message, params object[] objects)
        {
            MessageManager.Instance.SendMessage(message, objects);
        }

        public void AddListenerObject(string head, Action<object[]> action)
        {
            MessageManager.Instance.AddListenerObject(head, action);
        }
        public void RemoveListenerObject(string head)
        {
            MessageManager.Instance.RemoveListenerObject(head);
        }
        public void SendObject(string head, params object[] objects)
        {
            MessageManager.Instance.SendObject(head, objects);
        }
    }
}