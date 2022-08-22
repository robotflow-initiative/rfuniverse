using System.Collections.Generic;
using RFUniverse.Attributes;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

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

        public AssetManager(string channel_id) : base(channel_id)
        {
        }
        public override void ReceiveData(IncomingMessage msg)
        {
            string type = msg.ReadString();
            switch (type)
            {
                case "PreLoadAssetsAsync":
                    PreLoadAssetsAsync(msg);
                    break;
                case "LoadSceneAsync":
                    LoadSceneAsync(msg);
                    break;
                case "SendMessage":
                    SendMessage(msg);
                    break;
                case "InstanceObject":
                    InstanceObject(msg);
                    break;
                case "IgnoreLayerCollision":
                    IgnoreLayerCollision(msg);
                    break;
                case "GetCurrentCollisionPairs":
                    GetCurrentCollisionPairs();
                    break;
                case "GetRFMoveColliders":
                    GetRFMoveColliders();
                    break;
                case "SetGravity":
                    SetGravity(msg);
                    break;
                case "SetGroundPhysicMaterial":
                    SetGroundPhysicMaterial(msg);
                    break;
                case "SetTimeStep":
                    SetTimeStep(msg);
                    break;
                case "SetTimeScale":
                    SetTimeScale(msg);
                    break;
                default:
                    Debug.Log("Dont have mehond:" + type);
                    break;
            }
        }

        void PreLoadAssetsAsync(IncomingMessage msg, Action OnCompleted = null, bool sendDoneMsg = true)
        {
            int count = msg.ReadInt32();
            List<string> names = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string n = msg.ReadString();
                if (!names.Contains(n))
                    names.Add(n);
            }
            PreLoadAssetsAsync(names, OnCompleted, sendDoneMsg);
        }
        public void PreLoadAssetsAsync(List<string> names, Action OnCompleted = null, bool sendDoneMsg = true)
        {
            int loadedCount = 0;
            foreach (string name in names)
            {
                Debug.Log("PreLoadAssets:" + name);
                UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(name).Completed += (handle) =>
                {
                    if (!assets.ContainsKey(name))
                        assets.Add(name, handle.Result);
                    loadedCount++;
                    if (loadedCount == names.Count)
                    {
                        if (sendDoneMsg)
                        {
                            OutgoingMessage metaData = new OutgoingMessage();
                            metaData.WriteString("PreLoad Done");
                            channel.SendMetaDataToPython(metaData);
                        }
                        OnCompleted?.Invoke();
                    }
                };
            }
        }
        public void LoadSceneAsync(IncomingMessage msg, Action OnCompleted = null, bool sendDoneMsg = true)
        {
            string fileName = msg.ReadString();
            LoadSceneAsync(fileName, OnCompleted, sendDoneMsg);
        }
        void LoadSceneAsync(string fileName, Action OnCompleted = null, bool sendDoneMsg = true)
        {
            string filePath = $"{Application.streamingAssetsPath}/SceneData/{fileName}";
            string dataString = File.ReadAllText(filePath);
            SceneData data = JsonConvert.DeserializeObject<SceneData>(dataString, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            PreLoadAssetsAsync(data.assetsData.Select((a) => a.name).ToList(), () =>
            {
                List<int> headID = new List<int>();
                BaseAttrData temp;
                bool dirty = true;
                while (dirty)
                {
                    dirty = false;
                    for (int i = 0; i < data.assetsData.Count; i++)
                    {
                        if (data.assetsData[i].parentID > 0 && !headID.Contains(data.assetsData[i].parentID))
                        {
                            temp = data.assetsData[i];
                            data.assetsData.Remove(temp);
                            data.assetsData.Add(temp);
                            dirty = true;
                            i--;
                        }
                        else
                        {
                            headID.Add(data.assetsData[i].id);
                        }
                    }
                }
                GameObject ground = GameObject.FindGameObjectWithTag("Ground");
                if (ground != null) ground.SetActive(data.ground);
                Camera.main.transform.position = new Vector3(data.cameraPosition[0], data.cameraPosition[1], data.cameraPosition[2]);
                Camera.main.transform.eulerAngles = new Vector3(data.cameraRotation[0], data.cameraRotation[1], data.cameraRotation[2]);
                foreach (var item in data.assetsData)
                {
                    InstanceObject(item);
                }
                if (sendDoneMsg)
                {
                    OutgoingMessage metaData = new OutgoingMessage();
                    metaData.WriteString("PreLoad Done");
                    channel.SendMetaDataToPython(metaData);
                }
                OnCompleted?.Invoke();
            }, false);
        }

        Dictionary<string, List<Action>> Messages = new Dictionary<string, List<Action>>();
        private void SendMessage(IncomingMessage msg)
        {
            string msgName = msg.ReadString();
            Debug.Log($"Message : {msgName}");
            if (Messages.TryGetValue(msgName, out List<Action> actions))
            {
                foreach (var item in actions)
                {
                    item?.Invoke();
                }
            }
        }
        public void AddListener(string msgName, Action action)
        {
            if (Messages.TryGetValue(msgName, out List<Action> actions))
            {
                if (!actions.Contains(action))
                    actions.Add(action);
            }
            else
            {
                Messages.Add(msgName, new List<Action>() { action });
            }
        }
        public void RemoveListener(string msgName, Action action)
        {
            if (Messages.TryGetValue(msgName, out List<Action> actions))
            {
                if (actions.Contains(action))
                    actions.Remove(action);
            }
        }
        public void GetGameObject(string name, Action<GameObject> OnCompleted = null)
        {
            if (assets.TryGetValue(name, out GameObject gameObject))
                OnCompleted(gameObject);
            else
            {
                Debug.LogWarning($"GameObject {name} not preload");
                UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(name).Completed += (handle) =>
                {
                    if (!assets.ContainsKey(name))
                        assets.Add(name, handle.Result);
                    OnCompleted?.Invoke(assets[name]);
                };
            }
        }

        void InstanceObject(IncomingMessage msg)
        {
            string name = msg.ReadString();
            int id = msg.ReadInt32();
            InstanceObject(name, id);
        }
        public Dictionary<int, List<IncomingMessage>> waitingMsg = new Dictionary<int, List<IncomingMessage>>();
        public void InstanceObject(string name, int id)
        {
            Debug.Log("InstanceObject:" + name);
            switch (name)
            {
                case "Camera":
                    CreateCamera(id);
                    break;
                default:
                    waitingMsg.Add(id, new List<IncomingMessage>());
                    GetGameObject(name, (gameObject) =>
                    {
                        gameObject = GameObject.Instantiate(gameObject);
                        gameObject.name = gameObject.name.Replace("(Clone)", "");
                        BaseAttr attr = gameObject.GetComponent<BaseAttr>();
                        attr.ID = id;
                        attr.Name = name;
                        Debug.Log("Instance Done " + attr.Name + " id:" + attr.ID);
                        attr.Instance();
                        foreach (var item in waitingMsg[id])
                        {
                            BaseAttr.Attrs[attr.ID].ReceiveData(item);
                        }
                        waitingMsg.Remove(id);
                    });
                    break;
            }
        }
        public void InstanceObject(BaseAttrData baseAttrData)
        {
            GetGameObject(baseAttrData.name, gameObject =>
            {
                gameObject = GameObject.Instantiate(gameObject);
                gameObject.name = gameObject.name.Replace("(Clone)", "");
                BaseAttr attr = gameObject.GetComponent<BaseAttr>();
                attr.SetAttrData(baseAttrData);
                Debug.Log("Instance Done " + attr.Name + " id:" + attr.ID);
                attr.Instance();
            });
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
        void SetGroundPhysicMaterial(IncomingMessage msg)
        {
            PlayerMain.Instance.ground.GetComponent<Collider>().material = new PhysicMaterial
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
        private void CreateCamera(int id)
        {
            GameObject cam = new GameObject("Camera", typeof(CameraAttr));
            CameraAttr cameraAttr = cam.GetComponent<CameraAttr>();
            cameraAttr.ID = id;
            cameraAttr.Instance();
        }
    }
}
