using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;
using RFUniverse;
using RFUniverse.Manager;
using RFUniverse.Attributes;
using System.IO;
using DG.Tweening;
using System;

public class GraspSimShow : MonoBehaviour
{
    List<Vector3> allPoints = new();
    List<Quaternion> allQuaternions = new();
    List<List<float>> listJoint = new();
    List<bool> success = new();
    int parallelCount;
    List<Transform> envs = new();
    List<ControllerAttr> grippers = new();
    List<RigidbodyAttr> targets = new();

    public List<GameObject> objs = new();
    public int jointCount = 16;
    public Vector3 gripperPosition = new Vector3(0, 0, 90);
    private void Start()
    {
        AssetManager.Instance.AddListener("Test", StartGraspTest);
        //AssetManager.Instance.AddListener("Show", ShowGraspPose);
    }
    void StartGraspTest(IncomingMessage msg)
    {
        Debug.Log("StartGraspTest");
        allPoints.Clear();
        allQuaternions.Clear();
        string meshPath = msg.ReadString();
        string gripperName = msg.ReadString();
        List<float> pose = msg.ReadFloatList().ToList();
        List<float> joint = msg.ReadFloatList().ToList();
        parallelCount = msg.ReadInt32();
        List<List<float>> listPose = RFUniverseUtility.ListFloatSlicer(pose, 16);
        foreach (var item in listPose)
        {
            Matrix4x4 matrix = RFUniverseUtility.ListFloatToMatrix(item);

            Vector3 point = matrix.transpose.GetPosition();
            point = new Vector3(-point.x, point.y, point.z);

            Quaternion quaternion = matrix.transpose.rotation;
            quaternion = new Quaternion(quaternion.x, -quaternion.y, -quaternion.z, quaternion.w);
            quaternion = quaternion * Quaternion.AngleAxis(-90, Vector3.up);
            quaternion = quaternion * Quaternion.AngleAxis(-90, Vector3.forward);

            allPoints.Add(point);
            allQuaternions.Add(quaternion);
        }
        for (int i = 0; i < joint.Count; i++)
        {
            joint[i] *= Mathf.Rad2Deg;
        }
        listJoint = RFUniverseUtility.ListFloatSlicer(joint, jointCount);

        success.Clear();
        envs.Clear();
        grippers.Clear();
        targets.Clear();


        //创建初始环境
        Transform env = new GameObject($"Env0").transform;
        env.SetParent(transform);
        env.localPosition = Vector3.zero;
        envs.Add(env);

        GameObject select = objs.First(s => s.name == meshPath);
        GameObject target = Instantiate(select);
        Destroy(target.GetComponent<RigidbodyAttr>());
        Destroy(target.GetComponent<Rigidbody>());
        target.transform.SetParent(env);

        AssetManager.Instance.InstanceObject(gripperName, 987654, (attr) =>
        {
            foreach (var item in allPoints)
            {
                GameObject gg = Instantiate(attr.gameObject);
                gg.transform.SetParent(target.transform);
                gg.transform.localPosition = item;
                gg.transform.localRotation = allQuaternions[allPoints.IndexOf(item)];
            }
        }, false);


    }
}
