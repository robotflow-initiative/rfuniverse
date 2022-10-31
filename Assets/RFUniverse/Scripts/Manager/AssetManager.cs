using System.Collections.Generic;
using RFUniverse.Attributes;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Unity.Robotics.UrdfImporter;

namespace RFUniverse.Manager
{
    public class AssetManager : BaseManager
    {
        const string UUID = "d587efc8-9eb7-11ec-802a-18c04d443e7d";
        private static AssetManager instance = null;
        public static AssetManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssetManager(UUID);
                }
                return instance;
            }
        }
        //protected AssetBundle assetBundle;
        private Dictionary<string, GameObject> assets = new Dictionary<string, GameObject>();

        private AssetManagerExt ext = null;
        public AssetManager(string channel_id) : base(channel_id)
        {
            ext = new AssetManagerExt();
        }
        public override void ReceiveData(IncomingMessage msg)
        {
            string type = msg.ReadString();
            AnalysisMsg(msg, type);
        }
        void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "PreLoadAssetsAsync":
                    PreLoadAssetsAsync(msg);
                    return;
                case "LoadSceneAsync":
                    LoadSceneAsync(msg);
                    return;
                case "SendMessage":
                    ReceiveMessage(msg);
                    return;
                case "InstanceObject":
                    InstanceObject(msg);
                    return;
                case "LoadURDF":
                    LoadURDF(msg);
                    return;
                case "LoadMesh":
                    LoadMesh(msg);
                    return;
                case "IgnoreLayerCollision":
                    IgnoreLayerCollision(msg);
                    return;
                case "GetCurrentCollisionPairs":
                    GetCurrentCollisionPairs();
                    return;
                case "GetRFMoveColliders":
                    GetRFMoveColliders();
                    return;
                case "SetGravity":
                    SetGravity(msg);
                    return;
                case "SetGroundActive":
                    SetGroundActive(msg);
                    return;
                case "SetGroundPhysicMaterial":
                    SetGroundPhysicMaterial(msg);
                    return;
                case "SetTimeStep":
                    SetTimeStep(msg);
                    return;
                case "SetTimeScale":
                    SetTimeScale(msg);
                    return;
                default:
                    ext.AnalysisMsg(msg, type);
                    return;
            }
        }
        void PreLoadAssetsAsync(IncomingMessage msg, Action onCompleted = null, bool sendDoneMsg = true)
        {
            int count = msg.ReadInt32();
            List<string> names = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string n = msg.ReadString();
                if (!names.Contains(n))
                    names.Add(n);
            }
            PreLoadAssetsAsync(names, onCompleted, sendDoneMsg);
        }
        public void PreLoadAssetsAsync(List<string> names, Action onCompleted = null, bool sendDoneMsg = true)
        {
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
                Debug.Log("LoadedAsset:" + name);
                onCompleted?.Invoke();
            }
            else if (name == "Camera")
            {
                Debug.Log("LoadAsset:" + name);
                GameObject camera = new GameObject("Camera", typeof(CameraAttr));
                //camera.SetActive(false);
                assets.Add(name, camera);
                onCompleted?.Invoke();
            }
            else
            {
                Debug.Log("LoadAsset:" + name);
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
        public void LoadSceneAsync(IncomingMessage msg, Action onCompleted = null, bool sendDoneMsg = true)
        {
            string fileName = msg.ReadString();
            LoadSceneAsync(fileName, onCompleted, sendDoneMsg);
        }
        void LoadSceneAsync(string fileName, Action onCompleted = null, bool sendDoneMsg = true)
        {
            string filePath = $"{Application.streamingAssetsPath}/SceneData/{fileName}";
            string dataString = File.ReadAllText(filePath);
            SceneData data = JsonConvert.DeserializeObject<SceneData>(dataString, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            List<string> names = data.assetsData.Select((a) => a.name).ToList();
            PreLoadAssetsAsync(names, (Action)(() =>
            {
                data.assetsData = RFUniverseUtility.SortByParent(data.assetsData);
                PlayerMain.Instance.GroundActive = data.ground;
                PlayerMain.Instance.MainCamera.transform.position = new Vector3(data.cameraPosition[0], data.cameraPosition[1], data.cameraPosition[2]);
                PlayerMain.Instance.MainCamera.transform.eulerAngles = new Vector3(data.cameraRotation[0], data.cameraRotation[1], data.cameraRotation[2]);
                PlayerMain.Instance.Ground.transform.position = new Vector3(data.groundPosition[0], data.groundPosition[1], data.groundPosition[2]);
                foreach (var item in data.assetsData)
                {
                    InstanceObject(item);
                }
                if (sendDoneMsg)
                {
                    SendLoadDoneMsg();
                }
                onCompleted?.Invoke();
            }), false);
        }
        void SendLoadDoneMsg()
        {
            OutgoingMessage metaData = new OutgoingMessage();
            metaData.WriteString("PreLoadDone");
            channel.SendMetaDataToPython(metaData);
        }
        Dictionary<string, List<Action<IncomingMessage>>> registeredMessages = new Dictionary<string, List<Action<IncomingMessage>>>();
        private void ReceiveMessage(IncomingMessage msg)
        {
            string message = msg.ReadString();
            //Debug.Log($"Message : {message}");
            if (registeredMessages.TryGetValue(message, out List<Action<IncomingMessage>> actions))
            {
                foreach (var item in actions)
                {
                    item?.Invoke(msg);
                }
            }
        }
        public void AddListener(string message, Action<IncomingMessage> action)
        {
            if (registeredMessages.TryGetValue(message, out List<Action<IncomingMessage>> actions))
            {
                if (!actions.Contains(action))
                    actions.Add(action);
            }
            else
            {
                registeredMessages.Add(message, new List<Action<IncomingMessage>>() { action });
            }
        }
        public void RemoveListener(string message, Action<IncomingMessage> action)
        {
            if (registeredMessages.TryGetValue(message, out List<Action<IncomingMessage>> actions))
            {
                if (actions.Contains(action))
                    actions.Remove(action);
                if (actions.Count == 0)
                    registeredMessages.Remove(message);
            }
        }


        public void SendMessage(string message, params object[] objects)
        {
            OutgoingMessage msg = new OutgoingMessage();
            msg.WriteString(message);
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
            }
            channel.SendMetaDataToPython(msg);
        }
        public void SendMessage(OutgoingMessage msg)
        {
            channel.SendMetaDataToPython(msg);
        }
        void InstanceObject(IncomingMessage msg)
        {
            string name = msg.ReadString();
            int id = msg.ReadInt32();
            InstanceObject(name, id);
        }
        public void InstanceObject(BaseAttrData baseAttrData, Action<BaseAttr> onCompleted = null)
        {
            GetGameObject(baseAttrData.name, gameObject =>
            {
                gameObject = GameObject.Instantiate(gameObject);
                gameObject.name = gameObject.name.Replace("(Clone)", "");
                BaseAttr attr = gameObject.GetComponent<BaseAttr>();
                attr.SetAttrData(baseAttrData);
                Debug.Log("Instance Done " + attr.Name + " id:" + attr.ID);
                attr.Instance();
                onCompleted?.Invoke(attr);
            });
        }
        public Dictionary<int, List<IncomingMessage>> waitingMsg = new Dictionary<int, List<IncomingMessage>>();
        public void InstanceObject(string name, int id, Action<BaseAttr> onCompleted = null, bool callInstance = true)
        {
            Debug.Log("InstanceObject:" + name);
            waitingMsg.Add(id, new List<IncomingMessage>());
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
                        InstanceManager.Instance.ReceiveData(item);
                        item.Dispose();
                    }
                }
                waitingMsg.Remove(id);
                onCompleted?.Invoke(attr);
            });
        }
        void LoadURDF(IncomingMessage msg)
        {
            int id = msg.ReadInt32();
            string path = msg.ReadString();
            bool nativeIK = msg.ReadBoolean();
            Debug.Log("LoadURDF:" + path);
            ImportSettings setting = new ImportSettings();
            setting.convexMethod = ImportSettings.convexDecomposer.unity;
            GameObject robot = UrdfRobotExtensions.CreateRuntime(path, setting);
            robot.transform.SetParent(null);
            ControllerAttr attr = RFUniverseUtility.NormalizeRFUniverseArticulation(robot);
            attr.GetJointParameters();
            attr.ID = id;
            attr.Name = Path.GetFileNameWithoutExtension(path);
            attr.initBioIK = nativeIK;
            attr.Instance();
        }
        void LoadMesh(IncomingMessage msg)
        {
            int id = msg.ReadInt32();
            string path = msg.ReadString();
            LoadMesh(id, path);
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
        void IgnoreLayerCollision(IncomingMessage msg)
        {
            int layer1 = msg.ReadInt32();
            int layer2 = msg.ReadInt32();
            bool b = msg.ReadBoolean();
            Physics.IgnoreLayerCollision(layer1, layer2, b);
        }
        void GetCurrentCollisionPairs()
        {
            OutgoingMessage msg = new OutgoingMessage();
            msg.WriteString("CurrentCollisionPairs");
            msg.WriteInt32(BaseAttr.CollisionPairs.Count);
            foreach (var item in BaseAttr.CollisionPairs)
            {
                msg.WriteInt32(item.Item1);
                msg.WriteInt32(item.Item2);
            }
            channel.SendMetaDataToPython(msg);
        }
        void GetRFMoveColliders()
        {
            OutgoingMessage msg = new OutgoingMessage();
            msg.WriteString("RFMoveColliders");
            List<BaseAttr> colliderAttrs = BaseAttr.Attrs.Values.Where(s => s.IsRFMoveCollider).ToList();
            msg.WriteInt32(colliderAttrs.Count);
            foreach (var attr in colliderAttrs)
            {
                msg.WriteInt32(attr.ID);
                List<Collider> colliders = attr.GetComponentsInChildren<Collider>().Where(s => s.enabled && s.gameObject.activeInHierarchy && !s.isTrigger).ToList();
                msg.WriteInt32(colliders.Count);
                foreach (var collider in colliders)
                {
                    if (collider is BoxCollider)
                    {
                        msg.WriteString("box");
                        BoxCollider boxCollider = collider as BoxCollider;
                        Vector3 pos = boxCollider.transform.position + boxCollider.transform.TransformVector(boxCollider.center);
                        msg.WriteFloat32(pos.x);
                        msg.WriteFloat32(pos.y);
                        msg.WriteFloat32(pos.z);
                        msg.WriteFloat32(collider.transform.rotation.x);
                        msg.WriteFloat32(collider.transform.rotation.y);
                        msg.WriteFloat32(collider.transform.rotation.z);
                        msg.WriteFloat32(collider.transform.rotation.w);
                        msg.WriteFloat32(boxCollider.transform.lossyScale.x * boxCollider.size.x);
                        msg.WriteFloat32(boxCollider.transform.lossyScale.y * boxCollider.size.y);
                        msg.WriteFloat32(boxCollider.transform.lossyScale.z * boxCollider.size.z);
                    }
                    else if (collider is SphereCollider)
                    {
                        msg.WriteString("sphere");
                        SphereCollider sphereCollider = collider as SphereCollider;
                        Vector3 pos = sphereCollider.transform.position + sphereCollider.transform.TransformVector(sphereCollider.center);
                        msg.WriteFloat32(pos.x);
                        msg.WriteFloat32(pos.y);
                        msg.WriteFloat32(pos.z);
                        msg.WriteFloat32(sphereCollider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y, collider.transform.lossyScale.z));
                    }
                    else if (collider is CapsuleCollider)
                    {
                        msg.WriteString("capsule");
                        CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                        Vector3 pos = capsuleCollider.transform.position + capsuleCollider.transform.TransformVector(capsuleCollider.center);
                        msg.WriteFloat32(pos.x);
                        msg.WriteFloat32(pos.y);
                        msg.WriteFloat32(pos.z);
                        msg.WriteFloat32(collider.transform.rotation.x);
                        msg.WriteFloat32(collider.transform.rotation.y);
                        msg.WriteFloat32(collider.transform.rotation.z);
                        msg.WriteFloat32(collider.transform.rotation.w);
                        msg.WriteInt32(capsuleCollider.direction);
                        switch (capsuleCollider.direction)
                        {
                            case 0:
                                msg.WriteFloat32(capsuleCollider.radius * Mathf.Max(collider.transform.lossyScale.y, collider.transform.lossyScale.z));
                                msg.WriteFloat32(capsuleCollider.height * collider.transform.lossyScale.x);
                                break;
                            case 1:
                                msg.WriteFloat32(capsuleCollider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.z));
                                msg.WriteFloat32(capsuleCollider.height * collider.transform.lossyScale.y);
                                break;
                            case 2:
                                msg.WriteFloat32(capsuleCollider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y));
                                msg.WriteFloat32(capsuleCollider.height * collider.transform.lossyScale.z);
                                break;
                        }
                        // switch (capsuleCollider.direction)
                        // {
                        //     case 0:
                        //         msg.WriteFloat32(collider.transform.lossyScale.x * (capsuleCollider.radius * 2 + capsuleCollider.height));
                        //         msg.WriteFloat32(collider.transform.lossyScale.y * capsuleCollider.radius * 2);
                        //         msg.WriteFloat32(collider.transform.lossyScale.z * capsuleCollider.radius * 2);
                        //         break;
                        //     case 1:
                        //         msg.WriteFloat32(collider.transform.lossyScale.x * capsuleCollider.radius * 2);
                        //         msg.WriteFloat32(collider.transform.lossyScale.y * (capsuleCollider.radius * 2 + capsuleCollider.height));
                        //         msg.WriteFloat32(collider.transform.lossyScale.z * capsuleCollider.radius * 2);
                        //         break;
                        //     case 2:
                        //         msg.WriteFloat32(collider.transform.lossyScale.x * capsuleCollider.radius * 2);
                        //         msg.WriteFloat32(collider.transform.lossyScale.y * capsuleCollider.radius * 2);
                        //         msg.WriteFloat32(collider.transform.lossyScale.z * (capsuleCollider.radius * 2 + capsuleCollider.height));
                        //         break;
                        // }
                    }
                    else
                    {
                        msg.WriteString("box");
                        msg.WriteFloat32(collider.bounds.center.x);
                        msg.WriteFloat32(collider.bounds.center.y);
                        msg.WriteFloat32(collider.bounds.center.z);
                        msg.WriteFloat32(0);
                        msg.WriteFloat32(0);
                        msg.WriteFloat32(0);
                        msg.WriteFloat32(0);
                        msg.WriteFloat32(collider.bounds.size.x);
                        msg.WriteFloat32(collider.bounds.size.y);
                        msg.WriteFloat32(collider.bounds.size.z);
                    }
                }
            }
            channel.SendMetaDataToPython(msg);
        }
        void SetGravity(IncomingMessage msg)
        {
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();
            Physics.gravity = new Vector3(x, y, z);
        }
        void SetGroundActive(IncomingMessage msg)
        {
            PlayerMain.Instance.GroundActive = msg.ReadBoolean();
        }
        void SetGroundPhysicMaterial(IncomingMessage msg)
        {
            PlayerMain.Instance.Ground.GetComponent<Collider>().material = new PhysicMaterial
            {
                bounciness = msg.ReadFloat32(),
                dynamicFriction = msg.ReadFloat32(),
                staticFriction = msg.ReadFloat32(),
                frictionCombine = (PhysicMaterialCombine)msg.ReadInt32(),
                bounceCombine = (PhysicMaterialCombine)msg.ReadInt32()
            };
        }
        void SetTimeStep(IncomingMessage msg)
        {
            BaseAgent.Instance.FixedDeltaTime = msg.ReadFloat32();
        }
        void SetTimeScale(IncomingMessage msg)
        {
            BaseAgent.Instance.TimeScale = msg.ReadFloat32();
        }

    }
}
