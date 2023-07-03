using Newtonsoft.Json;
using RFUniverse.Attributes;
using RFUniverse.Manager;
using Robotflow.RFUniverse.SideChannels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Robotics.UrdfImporter;
using UnityEditor;
using UnityEngine;

namespace RFUniverse
{
    public class PlayerMain : RFUniverseMain
    {
        public int port = 5004;
        public static PlayerMain Instance = null;
        [SerializeField]
        private PlayerMainUI playerMainUI;

        private static RFUniverseCommunicator Communicator;

        public Version pythonVersion
        {
            set
            {
                playerMainUI.SetVersion(value);
            }
        }

        Queue<string> logList = new Queue<string>();
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
        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            FixedDeltaTime = fixedDeltaTime;
            TimeScale = timeScale;

            if (Communicator == null)
            {
                string[] commandLineArgs = Environment.GetCommandLineArgs();
                for (int i = 0; i < commandLineArgs.Length; i++)
                {
                    if (commandLineArgs[i].StartsWith("-port:"))
                    {
                        if (int.TryParse(commandLineArgs[i].Remove(0, 6), out int value))
                            port = value;
                    }
                }
                Communicator = new RFUniverseCommunicator("localhost", port, false);
            }

            if (Communicator.connected)
            {
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

            playerMainUI.Init(() => SendPendDoneMsg());
        }


        public Action OnStepAction;
        void FixedUpdate()
        {
            OnStepAction?.Invoke();
        }

        void Step()
        {
            if (Communicator.connected)
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
                    DebugManager.Instance.ReceiveDebugData(data);
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
            else if (waitingMsg.ContainsKey(id))
            {
                Debug.LogWarning($"ID:{id} is loading, add in waiting msg");
                waitingMsg[id].Enqueue(data);
            }
            else
                Debug.LogError($"ID:{id} not exist");
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
                    InstanceObject((string)data[0], (int)data[1]);
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
                case "SetViewBackGround":
                    SetViewBackGround(data[0].ConvertType<List<float>>());
                    return;
                default:
                    return;
            }
        }

        private Dictionary<string, GameObject> assets = new Dictionary<string, GameObject>();
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
        private void SetViewBackGround(List<float> color)
        {
            Debug.Log("SetViewBackGround");
            if (color == null)
                MainCamera.clearFlags = CameraClearFlags.Skybox;
            else
                MainCamera.backgroundColor = RFUniverseUtility.ListFloatToColor(color);
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
                if (UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(name).IsDone)
                {
                    onCompleted?.Invoke();
                    return;
                }
#endif
                UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(name).Completed += (handle) =>
                {
                    if (!assets.ContainsKey(name))
                        assets.Add(name, handle.Result);
                    onCompleted?.Invoke();
                };
            }
        }
        public void GetGameObject(string name, Action<GameObject> onCompleted = null)
        {
            if (assets.TryGetValue(name, out GameObject gameObject))
            {
                onCompleted?.Invoke(assets[name]);
            }
            else
            {
                Debug.LogWarning($"GameObject {name} not preload");
                PreLoadAssetAsync(name, () =>
                {
                    onCompleted?.Invoke(assets[name]);
                });
            }
        }
        void LoadSceneAsync(string file)
        {
            Debug.Log("LoadSceneAsync");
            LoadScene(file, (_) =>
            {
                SendLoadDoneMsg();
            });
        }
        public void SendPendDoneMsg()
        {
            Debug.Log("PendDone");
            Communicator.SendObject("Env", "PendDone");
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
            PlayerMain.Instance.MainCamera.transform.position = camera.transform.position;
            PlayerMain.Instance.MainCamera.transform.rotation = camera.transform.rotation;
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
            UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(name).Completed += (_) =>
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

        public void InstanceObject(BaseAttrData baseAttrData, Action<BaseAttr> onCompleted = null, bool callInstance = true)
        {
            Debug.Log("InstanceObject:" + baseAttrData.name);
            waitingMsg.TryAdd(baseAttrData.id, new Queue<object[]>());
            GetGameObject(baseAttrData.name, gameObject =>
            {
                gameObject = GameObject.Instantiate(gameObject);
                gameObject.name = gameObject.name.Replace("(Clone)", "");
                BaseAttr attr = gameObject.GetComponent<BaseAttr>();
                baseAttrData.SetAttrData(attr);
                Debug.Log("Instance Done " + attr.Name + " id:" + attr.ID);

                if (callInstance)
                {
                    attr.Instance();
                    foreach (var item in waitingMsg[baseAttrData.id])
                    {
                        Debug.Log("run waiting msg");
                        BaseAttr.Attrs[baseAttrData.id].ReceiveData(item);
                    }
                }
                waitingMsg.Remove(baseAttrData.id);
                onCompleted?.Invoke(attr);
            });
        }
        public Dictionary<int, Queue<object[]>> waitingMsg = new Dictionary<int, Queue<object[]>>();
        public void InstanceObject(string name, int id, Action<BaseAttr> onCompleted = null, bool callInstance = true)
        {
            Debug.Log("InstanceObject:" + name);
            waitingMsg.TryAdd(id, new Queue<object[]>());
            GetGameObject(name, (gameObject) =>
            {
                gameObject = GameObject.Instantiate(gameObject);
                gameObject.name = gameObject.name.Replace("(Clone)", "");
                BaseAttr attr = gameObject.GetComponent<BaseAttr>();
                attr.ID = id;
                attr.Name = name;
                Debug.Log("Instance Done " + attr.Name + " id:" + attr.ID);
                if (callInstance)
                {
                    attr.Instance();
                    foreach (var item in waitingMsg[id])
                    {
                        Debug.Log("run waiting msg");
                        BaseAttr.Attrs[id].ReceiveData(item);
                    }
                }
                waitingMsg.Remove(id);
                onCompleted?.Invoke(attr);
            });
        }
        void LoadURDF(int id, string path, bool nativeIK, string axis)
        {
            Debug.Log("LoadURDF:" + path);
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
            List<Dictionary<string, object>> rfmoveColliders = new List<Dictionary<string, object>>();
            List<BaseAttr> colliderAttrs = BaseAttr.ActiveAttrs.Values.Where(s => s.IsRFMoveCollider).ToList();
            foreach (var attr in colliderAttrs)
            {
                Dictionary<string, object> oneAttr = new Dictionary<string, object>();
                rfmoveColliders.Add(oneAttr);
                oneAttr.Add("object_id", attr.ID);
                Dictionary<string, object> oneAttrColliders = new Dictionary<string, object>();
                oneAttr.Add("collider", oneAttrColliders);
                List<Collider> colliders = attr.GetComponentsInChildren<Collider>().Where(s => s.enabled && s.gameObject.activeInHierarchy && !s.isTrigger).ToList();
                foreach (var collider in colliders)
                {
                    if (collider is BoxCollider)
                    {
                        oneAttrColliders.Add("type", "box");
                        BoxCollider boxCollider = collider as BoxCollider;
                        Vector3 pos = boxCollider.transform.position + boxCollider.transform.TransformPoint(boxCollider.center);
                        oneAttrColliders.Add("position", pos);
                        oneAttrColliders.Add("rotation", collider.transform.rotation);
                        oneAttrColliders.Add("size", new float[] { boxCollider.transform.lossyScale.x * boxCollider.size.x, boxCollider.transform.lossyScale.y * boxCollider.size.y, boxCollider.transform.lossyScale.z * boxCollider.size.z });
                    }
                    else if (collider is SphereCollider)
                    {
                        oneAttrColliders.Add("type", "sphere");
                        SphereCollider sphereCollider = collider as SphereCollider;
                        Vector3 pos = sphereCollider.transform.position + sphereCollider.transform.TransformPoint(sphereCollider.center);
                        oneAttrColliders.Add("position", pos);
                        oneAttrColliders.Add("radius", sphereCollider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y, collider.transform.lossyScale.z));
                    }
                    else if (collider is CapsuleCollider)
                    {
                        oneAttrColliders.Add("type", "capsule");
                        CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                        Vector3 pos = capsuleCollider.transform.position + capsuleCollider.transform.TransformPoint(capsuleCollider.center);
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
                    }
                    else
                    {
                        oneAttrColliders.Add("type", "box");
                        oneAttrColliders.Add("position", collider.bounds.center);
                        oneAttrColliders.Add("rotation", Quaternion.identity);
                        oneAttrColliders.Add("size", collider.bounds.size);
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
            PlayerMain.Instance.GroundActive = actice;
        }
        void SetGroundPhysicMaterial(float bounciness, float dynamicFriction, float staticFriction, int frictionCombine, int bounceCombine)
        {
            PlayerMain.Instance.Ground.GetComponent<Collider>().material = new PhysicMaterial
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
            PlayerMain.Instance.FixedDeltaTime = timeStep;
        }
        void SetTimeScale(float timeScale)
        {
            PlayerMain.Instance.TimeScale = timeScale;
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

        public void LoadScene(string file, Action<List<BaseAttr>> onCompleted = null, bool callInstance = true)
        {
            if (!file.Contains('/') && !file.Contains('\\'))
                file = $"{Application.streamingAssetsPath}/SceneData/{file}";
            if (!file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                file += ".json";
            if (!File.Exists(file)) return;
            SceneData data = JsonConvert.DeserializeObject<SceneData>(File.ReadAllText(file), RFUniverseUtility.JsonSerializerSettings);
            List<string> names = data.assetsData.Select((a) => a.name).ToList();
            PreLoadAssetsAsync(names, () =>
            {
                data.assetsData = RFUniverseUtility.SortByParent(data.assetsData);
                GroundActive = data.ground;
                MainCamera.transform.position = new Vector3(data.cameraPosition[0], data.cameraPosition[1], data.cameraPosition[2]);
                MainCamera.transform.eulerAngles = new Vector3(data.cameraRotation[0], data.cameraRotation[1], data.cameraRotation[2]);
                Ground.transform.position = new Vector3(data.groundPosition[0], data.groundPosition[1], data.groundPosition[2]);
                List<BaseAttr> attrs = new List<BaseAttr>();
                foreach (var item in data.assetsData)
                {
                    InstanceObject(item, (attr) =>
                    {
                        attrs.Add(attr);
                    }, callInstance);
                }
                onCompleted?.Invoke(attrs);
            }, false);
        }
        public void SaveScene(string file, List<BaseAttr> attrs)
        {
            if (!file.Contains('/') && !file.Contains('\\'))
                file = $"{Application.streamingAssetsPath}/SceneData/{file}";
            if (!file.EndsWith(".json"))
                file += ".json";

            SceneData data = new SceneData();
            if (PlayerMain.Instance.MainCamera)
            {
                data.cameraPosition = new float[] { PlayerMain.Instance.MainCamera.transform.position.x, PlayerMain.Instance.MainCamera.transform.position.y, PlayerMain.Instance.MainCamera.transform.position.z };
                data.cameraRotation = new float[] { PlayerMain.Instance.MainCamera.transform.eulerAngles.x, PlayerMain.Instance.MainCamera.transform.eulerAngles.y, PlayerMain.Instance.MainCamera.transform.eulerAngles.z };
            }
            data.ground = PlayerMain.Instance.GroundActive;
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
            data.assetsData = RFUniverseUtility.SortByParent(data.assetsData);
            Debug.Log(data.assetsData.Count);
            File.WriteAllText(file, JsonConvert.SerializeObject(data, Formatting.Indented, RFUniverseUtility.JsonSerializerSettings));
        }


        public void AddLog<T>(T log)
        {
            logList.Enqueue(log.ToString());
            if (logList.Count > 50)
                logList.Dequeue();
            playerMainUI.RefreshLogList(logList.ToArray());
        }
    }
}