using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using RFUniverse.Attributes;

namespace RFUniverse.Manager
{
    public class PhysicsSceneManager : SingletonBase<PhysicsSceneManager>, IHaveAPI, IReceiveData
    {
        private PhysicsSceneManager()
        {
            (this as IHaveAPI).RegisterAPI();
        }

        Dictionary<int, Scene> PhysicsScenes = new();
        Dictionary<Scene, List<BaseAttr>> PhysicsSceneAttrs = new();

        public void ReceiveData(object[] data)
        {
            string hand = (string)data[0];
            data = data.Skip(1).ToArray();
            (this as IHaveAPI).CallAPI(hand, data);
        }

        [RFUAPI]
        public void NewPhysicsScene(int id)
        {
            if (PhysicsScenes.Count != 0)
            {
                Debug.LogWarning($"PhysicsScene is not empty");
                return;
            }
            Scene physicsScene = SceneManager.CreateScene($"PhysicsScene{id}", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            PhysicsScenes[id] = physicsScene;
            PhysicsSceneAttrs[physicsScene] = new List<BaseAttr>();
            foreach (var item in InstanceManager.Instance.Attrs.Values)
            {
                string resultString = id.ToString() + item.ID.ToString();
                int resultNumber = int.Parse(resultString);
                BaseAttr newAttr = item.Copy(resultNumber);
                PhysicsSceneAttrs[physicsScene].Add(newAttr);
                item.Destroy();
                SceneManager.MoveGameObjectToScene(newAttr.gameObject, physicsScene);
            }
        }


        [RFUAPI]
        public void CopyPhysicsScene(int newID, int copyID)
        {
            if (PhysicsScenes.ContainsKey(newID))
            {
                Debug.LogWarning($"PhysicsScene ID: {newID} is exits");
                return;
            }
            if (!PhysicsScenes.ContainsKey(copyID))
            {
                Debug.LogWarning($"PhysicsScene ID: {newID} not exits");
                return;
            }

            Scene physicsScene = SceneManager.CreateScene($"PhysicsScene{newID}", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            PhysicsScenes[newID] = physicsScene;
            PhysicsSceneAttrs[physicsScene] = new List<BaseAttr>();

            foreach (var item in PhysicsSceneAttrs[PhysicsScenes[copyID]])
            {
                string resultString = newID.ToString() + item.ID.ToString().TrimStart(copyID.ToString().ToArray());
                int resultNumber = int.Parse(resultString);
                BaseAttr newAttr = item.Copy(resultNumber);
                PhysicsSceneAttrs[physicsScene].Add(newAttr);
                SceneManager.MoveGameObjectToScene(newAttr.gameObject, physicsScene);
            }
        }

        [RFUAPI(null, false)]
        public void SimulatePhysicsScene(int id, float fixedDeltaTime = -1, int count = 1)
        {
            if (!PhysicsScenes.ContainsKey(id))
            {
                Debug.LogWarning($"PhysicsScene ID: {id} not exits");
                return;
            }
            PhysicsScene physicsScene = PhysicsScenes[id].GetPhysicsScene();
            if (!physicsScene.IsValid()) return;
            if (fixedDeltaTime <= 0)
                fixedDeltaTime = Time.fixedDeltaTime;
            count = Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                physicsScene.Simulate(fixedDeltaTime);
            }
        }
    }
}
