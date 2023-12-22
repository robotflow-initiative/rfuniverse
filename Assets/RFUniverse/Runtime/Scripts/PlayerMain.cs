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
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif

namespace RFUniverse
{
    public class PlayerMain : RFUniverseMain
    {
        public int port = 5004;
        public static PlayerMain Instance = null;
        [HideInInspector]
        public int patchNumber;
        public PlayerMainUI playerMainUI;
        public static RFUniverseCommunicator Communicator;
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

        void OnValidate()
        {
            Instance = this;
        }
        DebugManager debugManager;
        protected override void Awake()
        {
            base.Awake();
            patchNumber = PlayerPrefs.GetInt("Patch", 0);
            Instance = this;
            FixedDeltaTime = fixedDeltaTime;
            TimeScale = timeScale;

            playerMainUI.Init(() =>
            {
                Debug.Log("PendDone");
                Communicator.SendObject("Env", "PendDone");
            });

            debugManager = DebugManager.Instance;


            if (Communicator == null)
            {
                string[] commandLineArgs = Environment.GetCommandLineArgs();
                for (int i = 0; i < commandLineArgs.Length; i++)
                {
                    if (commandLineArgs[i].StartsWith("-port:"))
                    {
                        startWithPort = true;
                        if (int.TryParse(commandLineArgs[i].Remove(0, 6), out int value))
                            port = value;
                    }
                }
                Communicator = new RFUniverseCommunicator("localhost", port, false, clientTime, () =>
                {
                    Debug.Log("Connected successfully");
                    OnStepAction += Step;
                    Communicator.OnReceivedData += (data) =>
                    {
                        ReceiveData(data);
                    };
                    Communicator.OnDisconnect += () =>
                    {
#if UNITY_EDITOR
                        EditorApplication.ExitPlaymode();
#else
                        Application.Quit();
#endif
                    };
                });
            }

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

            Communicator?.SendObject("Env", "Close");
            Communicator?.Dispose();
        }

        public Action OnStepAction;
        void FixedUpdate()
        {
            OnStepAction?.Invoke();
        }

        void Step()
        {
            if (Communicator.Connected)
            {
                foreach (var attr in BaseAttr.ActiveAttrs.Values)
                {
                    Dictionary<string, object> data = attr.CollectData();
                    Communicator.SendObject("Instance", attr.ID, attr.GetType().Name, data);
                }
                Communicator.SyncStepEnd();
            }
        }

        private void ReceiveData(object[] data)
        {
            string channel = (string)data[0];
            data = data.Skip(1).ToArray();
            switch (channel)
            {
                case "Env":
                    ReceiveEnvData(data);
                    break;
                case "Instance":
                    ReceiveInstanceData(data);
                    break;
                case "Message":
                    ReceiveMessageData(data);
                    break;
                case "Object":
                    ReceiveObjectData(data);
                    break;
                case "Debug":
                    debugManager.ReceiveDebugData(data);
                    break;
                default:
                    break;
            }
        }
        void ReceiveInstanceData(object[] data)
        {
            int id = (int)data[0];
            data = data.Skip(1).ToArray();
            if (BaseAttr.Attrs.ContainsKey(id))
                BaseAttr.Attrs[id].ReceiveData(data);
            else
                Debug.LogError($"ID:{id} Not Exist");
        }
        void ReceiveEnvData(object[] data)
        {
            string type = (string)data[0];
            data = data.Skip(1).ToArray();
            switch (type)
            {
                case "PreLoadAssetsAsync":
                    PreLoadAssetsAsync(data[0].ConvertType<List<string>>());
                    return;
                case "LoadSceneAsync":
                    LoadSceneAsync((string)data[0]);
                    return;
                case "SwitchSceneAsync":
                    SwitchSceneAsync((string)data[0]);
                    return;
                case "Pend":
                    Pend();
                    return;
                case "AlignCamera":
                    AlignCamera((int)data[0]);
                    return;
                case "SaveScene":
                    SaveScene((string)data[0]);
                    return;
                case "ClearScene":
                    ClearScene();
                    return;
                case "InstanceObject":
                    InstanceObject<BaseAttr>((string)data[0], (int)data[1]);
                    return;
                case "LoadURDF":
                    LoadURDF((int)data[0], (string)data[1], (bool)data[2], (string)data[3]);
                    return;
                case "LoadMesh":
                    LoadMesh((int)data[0], (string)data[1]);
                    return;
                case "IgnoreLayerCollision":
                    IgnoreLayerCollision((int)data[0], (int)data[1], (bool)data[2]);
                    return;
                case "GetCurrentCollisionPairs":
                    GetCurrentCollisionPairs();
                    return;
                case "GetRFMoveColliders":
                    GetRFMoveColliders();
                    return;
                case "SetGravity":
                    SetGravity(data[0].ConvertType<List<float>>());
                    return;
                case "SetGroundActive":
                    SetGroundActive((bool)data[0]);
                    return;
                case "SetGroundPhysicMaterial":
                    SetGroundPhysicMaterial((float)data[0], (float)data[1], (float)data[2], (int)data[3], (int)data[4]);
                    return;
                case "SetTimeStep":
                    SetTimeStep((float)data[0]);
                    return;
                case "SetTimeScale":
                    SetTimeScale((float)data[0]);
                    return;
                case "SetResolution":
                    SetResolution((int)data[0], (int)data[1]);
                    return;
                case "ExportOBJ":
                    ExportOBJ(data[0].ConvertType<List<int>>(), (string)data[1]);
                    return;
                case "SetShadowDistance":
                    SetShadowDistance((float)data[0]);
                    return;
                case "SetViewTransform":
                    SetViewTransform(data[0].ConvertType<List<float>>(), data[1].ConvertType<List<float>>());
                    return;
                case "ViewLookAt":
                    ViewLookAt(data[0].ConvertType<List<float>>(), data[1].ConvertType<List<float>>());
                    return;
                case "SetViewBackGround":
                    SetViewBackGround(data[0].ConvertType<List<float>>());
                    return;
                default:
                    Debug.LogWarning("Dont have mehond:" + type);
                    break;
            }
        }



        private void SetShadowDistance(float dis)
        {
            QualitySettings.shadowDistance = dis;
        }
        private void SetViewTransform(List<float> posiiton, List<float> rotation)
        {
            Debug.Log("SetViewTransform");
            if (posiiton != null)
            {
                MainCamera.transform.position = RFUniverseUtility.ListFloatToVector3(posiiton);
            }
            if (rotation != null)
            {
                MainCamera.transform.eulerAngles = RFUniverseUtility.ListFloatToVector3(rotation);
            }
        }

        public virtual void ViewLookAt(List<float> target, List<float> worldUp)
        {
            MainCamera.transform.LookAt(RFUniverseUtility.ListFloatToVector3(target), RFUniverseUtility.ListFloatToVector3(worldUp));
        }

        private void SetViewBackGround(List<float> color)
        {
            Debug.Log("SetViewBackGround");
            if (color == null)
                MainCamera.clearFlags = CameraClearFlags.Skybox;
            else
            {
                MainCamera.clearFlags = CameraClearFlags.SolidColor;
                MainCamera.backgroundColor = RFUniverseUtility.ListFloatToColor(color);
            }
        }
        public void Pend()
        {
            playerMainUI.ShowPend();
        }
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

        void LoadSceneAsync(string file)
        {
            Debug.Log($"LoadSceneAsync: {file}");
            LoadScene(file);
            SendLoadDoneMsg();
        }

        void AlignCamera(int cameraID)
        {
            Debug.Log("AlignCamera");
            if (!BaseAttr.Attrs.ContainsKey(cameraID))
            {
                Debug.LogWarning($"not find align target camera {cameraID}");
                return;
            }
            BaseAttr camera = BaseAttr.Attrs[cameraID];
            MainCamera.transform.position = camera.transform.position;
            MainCamera.transform.rotation = camera.transform.rotation;
        }
        void SaveScene(string path)
        {
            Debug.Log("SaveScene");
            SaveScene(path, BaseAttr.Attrs.Values.ToList());
        }
        void ClearScene()
        {
            Debug.Log("ClearScene");
            foreach (var item in BaseAttr.Attrs.Values)
            {
                item.Destroy();
            }
        }
        void SwitchSceneAsync(string name)
        {
            Debug.Log("SwitchSceneAsync");
            Addressables.LoadSceneAsync(name).Completed += (_) =>
            {
                SendLoadDoneMsg();
            };
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
            Communicator.SendObject("Env", "LoadDone");
        }

        Dictionary<string, Action<IncomingMessage>> registeredMessages = new Dictionary<string, Action<IncomingMessage>>();
        private void ReceiveMessageData(object[] data)
        {
            string message = (string)data[0];
            data = data.Skip(1).ToArray();
            if (registeredMessages.TryGetValue(message, out Action<IncomingMessage> action))
            {

                action?.Invoke(new IncomingMessage((byte[])data[0]));
            }
        }

        [Obsolete("AddListener is the older interface, and AddListenerObject is the recommended interface for dynamic messaging")]
        public void AddListener(string message, Action<IncomingMessage> action)
        {
            if (registeredMessages.ContainsKey(message))
            {
                registeredMessages[message] = action;
            }
            else
            {
                registeredMessages.Add(message, action);
            }
        }

        [Obsolete("RemoveListener is the older interface, and RemoveListenerObject is the recommended interface for dynamic messaging")]
        public void RemoveListener(string message)
        {
            if (registeredMessages.ContainsKey(message))
            {
                registeredMessages.Remove(message);
            }
        }
        [Obsolete("SendMessage is the older interface, and SendObject is the recommended interface for dynamic messaging")]
        public void SendMessage(string message, params object[] objects)
        {
            OutgoingMessage msg = new OutgoingMessage();
            foreach (var item in objects)
            {
                if (item is int)
                    msg.WriteInt32((int)item);
                if (item is float)
                    msg.WriteFloat32((float)item);
                if (item is string)
                    msg.WriteString((string)item);
                if (item is bool)
                    msg.WriteBoolean((bool)item);
                if (item is List<float>)
                    msg.WriteFloatList((List<float>)item);
                if (item is List<bool>)
                {
                    List<bool> data = (List<bool>)item;
                    msg.WriteInt32(data.Count);
                    foreach (var i in data)
                    {
                        msg.WriteBoolean(i);
                    }
                }
            }
            Communicator.SendObject("Message", message, msg.buffer);
        }

        Dictionary<string, Action<object[]>> registeredObjects = new Dictionary<string, Action<object[]>>();
        private void ReceiveObjectData(object[] data)
        {
            string head = (string)data[0];
            data = data.Skip(1).ToArray();
            if (registeredObjects.TryGetValue(head, out Action<object[]> action))
            {
                action?.Invoke(data);
            }
        }
        public void AddListenerObject(string head, Action<object[]> action)
        {
            if (registeredObjects.ContainsKey(head))
            {
                registeredObjects[head] = action;
            }
            else
            {
                registeredObjects.Add(head, action);
            }
        }
        public void RemoveListenerObject(string head)
        {
            if (registeredObjects.ContainsKey(head))
            {
                registeredObjects.Remove(head);
            }
        }
        public void SendObject(string head, params object[] objects)
        {
            object[] data = new[] { "Object", head };
            Communicator.SendObject(data.Concat(objects).ToArray());
        }



        public T InstanceObject<T>(string name, int id, bool callInstance = true) where T : BaseAttr
        {
            Debug.Log("InstanceObject:" + name);
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

        void LoadURDF(int id, string path, bool nativeIK, string axis)
        {
            Debug.Log($@"LoadURDF: {path}");
            ImportSettings setting = new ImportSettings();
            setting.chosenAxis = axis == "z" ? ImportSettings.axisType.zAxis : ImportSettings.axisType.yAxis;
            setting.convexMethod = ImportSettings.convexDecomposer.unity;
            GameObject robot = UrdfRobotExtensions.CreateRuntime(path, setting);
            robot.transform.SetParent(null);
            ControllerAttr attr = RFUniverseUtility.NormalizeRFUniverseArticulation(robot);
            attr.ID = id;
            attr.Name = Path.GetFileNameWithoutExtension(path);
            attr.initBioIK = nativeIK;
            attr.Instance();
        }

        public RigidbodyAttr LoadMesh(int id, string path, bool autoInstance = true)
        {
            Debug.Log("LoadMesh:" + path);
            GameObject obj = UnityMeshImporter.MeshImporter.Load(path);
            RigidbodyAttr attr = obj.AddComponent<RigidbodyAttr>();
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
            attr.GenerateVHACDCollider();
            if (autoInstance)
                attr.Instance();
            return attr;
        }

        void IgnoreLayerCollision(int layer1, int layer2, bool ignore)
        {
            Physics.IgnoreLayerCollision(layer1, layer2, ignore);
        }

        void GetCurrentCollisionPairs()
        {
            Communicator.SendObject("Env", "CurrentCollisionPairs", BaseAttr.CollisionPairs);
        }
        void GetRFMoveColliders()
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
            Communicator.SendObject("Env", "RFMoveColliders", rfmoveColliders);
        }
        void SetGravity(List<float> gravity)
        {
            if (gravity.Count != 3) return;
            Debug.Log("SetGravity");
            Physics.gravity = new Vector3(gravity[0], gravity[1], gravity[2]);
        }

        void SetGroundActive(bool actice)
        {
            GroundActive = actice;
        }

        void SetGroundPhysicMaterial(float bounciness, float dynamicFriction, float staticFriction, int frictionCombine, int bounceCombine)
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

        void SetTimeStep(float timeStep)
        {
            FixedDeltaTime = timeStep;
        }

        void SetTimeScale(float timeScale)
        {
            TimeScale = timeScale;
        }

        void SetResolution(int width, int height)
        {
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
        }

        void ExportOBJ(List<int> ids, string path)
        {
            ExportOBJ(BaseAttr.Attrs.Where((s) => ids.Contains(s.Key)).Select((s) => s.Value.gameObject).ToArray(), path);
        }

        public void ExportOBJ(GameObject[] meshs, string path)
        {
            new OBJExporter().Export(meshs, path);
        }
    }
}