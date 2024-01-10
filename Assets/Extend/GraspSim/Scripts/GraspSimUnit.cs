using RFUniverse;
using RFUniverse.Attributes;
using System.Collections;
using UnityEngine;

public class GraspSimUnit
{
    GraspSimAttr graspSimAttr;
    ControllerAttr gripper;
    RigidbodyAttr target;
    public void Init(int unitID, ControllerAttr griggerPrefab, RigidbodyAttr objectPrefab, Vector3 positon)
    {
        Transform env = new GameObject($"Env{unitID}").transform;
        env.position = positon;

        gripper = GameObject.Instantiate(griggerPrefab, env);
        gripper.SetTransform(true, true, false, Vector3.zero, Vector3.zero, Vector3.one, false);

        target = GameObject.Instantiate(objectPrefab, env);
        target.SetTransform(true, true, false, Vector3.down, Vector3.zero, Vector3.one, false);
        target.gameObject.AddComponent<CollisionState>();
    }

    IEnumerator StartProcess()
    {
        //while (graspSimAttr.GetGraspData(out Vector3 position, out Quaternion rotation, ))
        //{

        //}
        yield break;
    }
}
