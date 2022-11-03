using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;
using RFUniverse;
using RFUniverse.Manager;
using RFUniverse.Attributes;
using System.IO;

public class GraspSimAttr : BaseAttr
{
    public override string Type
    {
        get { return "GraspSim"; }
    }

    public override void AnalysisMsg(IncomingMessage msg, string type)
    {
        switch (type)
        {
            case "StartGraspSim":
                StartGraspSim(msg);
                return;
            case "ShowGraspPose":
                ShowGraspPose(msg);
                return;
            case "GenerateGraspPose":
                GenerateGraspPose(msg);
                return;
        }
        base.AnalysisMsg(msg, type);
    }
    public override void CollectData(OutgoingMessage msg)
    {
        base.CollectData(msg);
        msg.WriteBoolean(isDone);
        if (isDone)
        {
            List<Vector3> successPoints = allPoints.Where((s, i) => success[i]).ToList();
            List<Quaternion> successQuaternions = allQuaternions.Where((s, i) => success[i]).ToList();
            List<float> successGripperWidth = gripperWidth.Where((s, i) => success[i]).ToList();
            msg.WriteFloatList(RFUniverseUtility.ListVector3ToListFloat(successPoints));
            msg.WriteFloatList(RFUniverseUtility.ListQuaternionToListFloat(successQuaternions));
            msg.WriteFloatList(successGripperWidth);
            isDone = false;
        }
    }
    bool isDone = false;
    List<Vector3> allPoints = new List<Vector3>();
    List<Quaternion> allQuaternions = new List<Quaternion>();
    List<bool> success = new List<bool>();
    List<float> gripperWidth = new List<float>();
    float depthRangeMin;
    float depthRangeMax;
    int depthLerpCount;
    int angleLerpCount;
    int parallelCount;
    List<Transform> envs = new List<Transform>();
    List<ControllerAttr> grippers = new List<ControllerAttr>();
    List<RigidbodyAttr> targets = new List<RigidbodyAttr>();

    void StartGraspSim(IncomingMessage msg)
    {
        Debug.Log("StartGraspSim");
        string meshPath = msg.ReadString();
        string gripperName = msg.ReadString();
        List<float> points = msg.ReadFloatList().ToList();
        List<float> normals = msg.ReadFloatList().ToList();
        depthRangeMin = msg.ReadFloat32();
        depthRangeMax = msg.ReadFloat32();
        depthLerpCount = msg.ReadInt32();
        angleLerpCount = msg.ReadInt32();
        parallelCount = msg.ReadInt32();
        List<Vector3> pointsV3 = RFUniverseUtility.ListFloatToListVector3(points);
        List<Vector3> normalsV3 = RFUniverseUtility.ListFloatToListVector3(normals);
        GenerateGraspPose(pointsV3, normalsV3);
        success.Clear();
        gripperWidth.Clear();
        envs.Clear();
        grippers.Clear();
        targets.Clear();


        //创建初始环境
        Transform env = new GameObject($"Env0").transform;
        env.SetParent(transform);
        env.localPosition = Vector3.zero;
        envs.Add(env);

        RigidbodyAttr target = AssetManager.Instance.LoadMesh(ID * 10 + 0, meshPath, false);
        target.transform.SetParent(env);
        targets.Add(target);

        AssetManager.Instance.InstanceObject(gripperName, ID * 10 + 1, (attr) =>
        {
            ControllerAttr newGripper = attr as ControllerAttr;

            //计算抓点与Root间旋转
            Transform graspPoint = newGripper.jointParameters.LastOrDefault().body.transform;
            Quaternion graspPointToGripperQuaternion = Quaternion.FromToRotation(graspPoint.transform.eulerAngles, newGripper.transform.eulerAngles);

            newGripper.Init();
            newGripper.transform.SetParent(env);
            grippers.Add(newGripper);
            newGripper.SetTransform(true, true, false, Vector3.up, graspPointToGripperQuaternion * Quaternion.AngleAxis(180, Vector3.left).eulerAngles, Vector3.zero, false);

            graspPointPosition = env.InverseTransformPoint(graspPoint.position);
            graspPointRotaion = graspPoint.rotation * env.rotation;

            StartCoroutine(GraspSim());
        }, false);
    }
    Vector3 graspPointPosition;
    Quaternion graspPointRotaion;
    IEnumerator GraspSim()
    {
        bool sourceGround = PlayerMain.Instance.GroundActive;
        PlayerMain.Instance.GroundActive = false;
        //复制并行环境
        int wCount = Mathf.FloorToInt(Mathf.Sqrt(parallelCount));
        for (int i = 1; i < parallelCount; i++)
        {
            Transform newEnv = new GameObject($"Env{i}").transform;
            newEnv.SetParent(transform);
            int w = i % wCount;
            int h = i / wCount;
            newEnv.localPosition = new Vector3(w * 2, 0, h * 2);
            envs.Add(newEnv);

            RigidbodyAttr newTarget = GameObject.Instantiate(targets[0], newEnv);
            targets.Add(newTarget);

            ControllerAttr newGripper = GameObject.Instantiate(grippers[0], newEnv);
            newGripper.Init();
            newGripper.SetTransform(true, true, false, grippers[0].transform.localPosition, grippers[0].transform.localRotation.eulerAngles, Vector3.zero, false);
            grippers.Add(newGripper);
        }
        //开始循环
        for (int i = 0; i < allPoints.Count; i += parallelCount)
        {
            Vector3 tempGravity = Physics.gravity;
            Physics.gravity = Vector3.zero;
            //打开gripper
            for (int j = 0; j < parallelCount; j++)
            {
                grippers[j].SetJointPosition(new List<float>(new float[] { 0.04f, 0.04f }), ControlMode.Direct);
            }
            yield return new WaitForFixedUpdate();
            //设置物体位置
            List<Vector3> startPositions = new List<Vector3>();
            List<Vector3> startRotations = new List<Vector3>();
            for (int j = 0; j < parallelCount; j++)
            {
                if (i + j >= allPoints.Count) break;
                Vector3 point = allPoints[i + j];
                Quaternion quaternion = allQuaternions[i + j];
                Transform localEnv = envs[j];
                RigidbodyAttr localTarget = targets[j];

                Transform graspPoint = new GameObject("GraspPoint").transform;
                graspPoint.SetParent(localTarget.transform);
                graspPoint.localPosition = point;
                graspPoint.localRotation = quaternion;
                graspPoint.SetParent(localEnv);
                localTarget.transform.SetParent(graspPoint);

                graspPoint.localPosition = graspPointPosition;
                graspPoint.localRotation = graspPointRotaion;
                localTarget.transform.SetParent(localEnv);
                GameObject.Destroy(graspPoint.gameObject);
                localTarget.Rigidbody.velocity = Vector3.zero;
                startPositions.Add(localTarget.transform.position);
                startRotations.Add(localTarget.transform.eulerAngles);
            }
            for (int j = 0; j < 5; j++)
            {
                yield return new WaitForFixedUpdate();
            }
            bool[] envSuccess = new bool[parallelCount];
            //初始状态检查
            for (int j = 0; j < parallelCount; j++)
            {

                //检测第一帧后碰撞状态
                //envSuccess[j] = !targets[j].GetComponent<CollisionState>().collision;

                //检测第一帧后速度
                //envSuccess[j] = (targets[j].Rigidbody.velocity.sqrMagnitude < 0.0001f);

                //检测第5帧后pose
                if (i + j >= allPoints.Count) break;
                envSuccess[j] = Vector3.Distance(targets[j].transform.position, startPositions[j]) < 0.001f;
                envSuccess[j] &= Vector3.Angle(targets[j].transform.eulerAngles, startRotations[j]) < 0.1;
            }
            //闭合gripper开始抓取
            for (int j = 0; j < parallelCount; j++)
            {
                if (!envSuccess[j]) continue;
                grippers[j].SetJointPosition(new List<float>(new float[] { 0, 0 }));
            }
            //等待抓取
            for (int j = 0; j < 100; j++)
            {
                yield return new WaitForFixedUpdate();
            }
            //启用重力 等待掉落
            Physics.gravity = tempGravity;
            for (int j = 0; j < 100; j++)
            {
                yield return new WaitForFixedUpdate();
            }
            //判断结束状态物体速度
            for (int j = 0; j < parallelCount; j++)
            {
                if (!envSuccess[j]) continue;
                envSuccess[j] &= (targets[j].Rigidbody.velocity.sqrMagnitude < 0.01f);
            }
            //写入结果
            for (int j = 0; j < parallelCount; j++)
            {
                if (success.Count >= allPoints.Count) break;
                success.Add(envSuccess[j]);
                Debug.Log(envSuccess[j]);
                if (envSuccess[j])
                {
                    List<float> width = grippers[j].GetJointPositions();
                    gripperWidth.Add(width[0] + width[1]);
                }
                else
                    gripperWidth.Add(0);

            }
        }
        //清理
        for (int i = 0; i < parallelCount; i++)
        {
            GameObject.Destroy(envs[i].gameObject);
        }
        isDone = true;
        PlayerMain.Instance.GroundActive = sourceGround;
        System.GC.Collect();
    }
    void ShowGraspPose(IncomingMessage msg)
    {
        string meshPath = msg.ReadString();
        string gripperName = msg.ReadString();
        List<float> positions = msg.ReadFloatList().ToList();
        List<float> rotations = msg.ReadFloatList().ToList();
        List<Vector3> positionsV3 = RFUniverseUtility.ListFloatToListVector3(positions);
        List<Quaternion> rotationsQ4 = RFUniverseUtility.ListFloatToListQuaternion(rotations);
        ShowGraspPose(meshPath, gripperName, positionsV3, rotationsQ4);
    }
    void ShowGraspPose(string meshPath, string gripperName, List<Vector3> positionsV3, List<Quaternion> rotationsQ4)
    {
        if (positionsV3.Count != rotationsQ4.Count) return;

        RigidbodyAttr target = AssetManager.Instance.LoadMesh(ID * 10 + 2, meshPath, false);
        Transform trans = target.transform;
        GameObject.Destroy(target);
        GameObject.Destroy(target.Rigidbody);
        trans.SetParent(transform);
        trans.position = Vector3.up;

        AssetManager.Instance.GetGameObject(gripperName, (g) =>
        {
            for (int i = 0; i < positionsV3.Count; i++)
            {
                Vector3 position = positionsV3[i];
                Quaternion rotation = rotationsQ4[i];
                GameObject obj = GameObject.Instantiate(g, trans);
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
            }
        });
    }
    void GenerateGraspPose(IncomingMessage msg)
    {
        Debug.Log("GenerateGraspPose");
        string meshPath = msg.ReadString();
        string gripperName = msg.ReadString();
        List<float> points = msg.ReadFloatList().ToList();
        List<float> normals = msg.ReadFloatList().ToList();
        depthRangeMin = msg.ReadFloat32();
        depthRangeMax = msg.ReadFloat32();
        depthLerpCount = msg.ReadInt32();
        angleLerpCount = msg.ReadInt32();
        parallelCount = msg.ReadInt32();
        List<Vector3> pointsV3 = RFUniverseUtility.ListFloatToListVector3(points);
        List<Vector3> normalsV3 = RFUniverseUtility.ListFloatToListVector3(normals);
        GenerateGraspPose(pointsV3, normalsV3);
        ShowGraspPose(meshPath, gripperName, allPoints, allQuaternions);
    }

    public void GenerateGraspPose(List<Vector3> pointsV3, List<Vector3> normalsV3)
    {
        if (pointsV3.Count != normalsV3.Count) return;
        allPoints.Clear();
        allQuaternions.Clear();
        for (int i = 0; i < pointsV3.Count; i++)
        {
            Vector3 point = pointsV3[i];
            Vector3 normal = normalsV3[i];
            Quaternion qua = Quaternion.LookRotation(normal);
            qua *= Quaternion.AngleAxis(90, Vector3.left);
            for (int j = 0; j < depthLerpCount; j++)
            {
                Vector3 depthPoint = point + normal * Mathf.Lerp(depthRangeMin, depthRangeMax, j / (float)depthLerpCount);
                for (int k = 0; k < angleLerpCount; k++)
                {
                    Quaternion angle = Quaternion.AngleAxis(k * 360 / angleLerpCount, Vector3.up);
                    allPoints.Add(depthPoint);
                    allQuaternions.Add(qua * angle);
                }
            }
        }
    }

    // void OnDrawGizmos()
    // {
    //     for (int i = 0; i < allPoints.Count; i++)
    //     {
    //         Gizmos.color = Color.white;
    //         Gizmos.DrawSphere(allPoints[i], 0.001f);
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawLine(allPoints[i], allPoints[i] + allQuaternions[i] * Vector3.up * 0.01f);
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawLine(allPoints[i], allPoints[i] + allQuaternions[i] * Vector3.left * 0.01f);
    //     }
    // }
}
