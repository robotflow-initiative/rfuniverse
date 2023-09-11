using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;
using RFUniverse.Manager;

namespace RFUniverse.Attributes
{
    public class GraspSimAttr : BaseAttr
    {
        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "StartGraspSim":
                    StartGraspSim(data);
                    return;
                case "StartGraspTest":
                    StartGraspTest(data);
                    return;
                case "ShowGraspPose":
                    ShowGraspPose(data);
                    return;
                case "GenerateGraspPose":
                    GenerateGraspPose(data);
                    return;
            }
            base.AnalysisData(type, data);
        }
        int mode = 0;
        public override Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = base.CollectData();
            data.Add("done", isDone);
            if (isDone)
            {
                if (mode == 0)
                {
                    List<Vector3> successPoints = allPoints.Where((s, i) => success[i]).ToList();
                    List<Quaternion> successQuaternions = allQuaternions.Where((s, i) => success[i]).ToList();
                    List<float> successGripperWidth = gripperWidth.Where((s, i) => success[i]).ToList();
                    data.Add("points", successPoints);
                    data.Add("quaternions", successQuaternions);
                    data.Add("width", successGripperWidth);
                }
                if (mode == 1)
                {
                    data.Add("success", success);
                }
                isDone = false;
            }
            return data;
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

        void StartGraspSim(object[] data)
        {
            mode = 0;
            Debug.Log("StartGraspSim");
            string meshPath = (string)data[0];
            string gripperName = (string)data[1];
            List<float> points = RFUniverseUtility.ConvertType<List<float>>(data[2]);
            List<float> normals = RFUniverseUtility.ConvertType<List<float>>(data[3]);
            depthRangeMin = (float)data[4];
            depthRangeMax = (float)data[5];
            depthLerpCount = (int)data[6];
            angleLerpCount = (int)data[7];
            parallelCount = (int)data[8];
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

            RigidbodyAttr target = PlayerMain.Instance.LoadMesh(ID * 10 + 0, meshPath, false);
            target.gameObject.AddComponent<CollisionState>();
            target.transform.SetParent(env);
            targets.Add(target);

            var newGripper = PlayerMain.Instance.InstanceObject<ControllerAttr>(gripperName, ID * 10 + 1, false);

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
        }
        void StartGraspTest(object[] data)
        {
            mode = 1;
            Debug.Log("StartGraspTest");
            string meshPath = (string)data[0];
            string gripperName = (string)data[1];
            List<float> points = RFUniverseUtility.ConvertType<List<float>>(data[2]);
            List<float> quaternions = RFUniverseUtility.ConvertType<List<float>>(data[3]);
            parallelCount = (int)data[4];
            List<Vector3> pointsV3 = RFUniverseUtility.ListFloatToListVector3(points);
            List<Quaternion> quaternionsQ4 = RFUniverseUtility.ListFloatToListQuaternion(quaternions);

            //allPoints = pointsV3;
            //allQuaternions = quaternionsQ4;
            ProcessGraspPose(pointsV3, quaternionsQ4);

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

            RigidbodyAttr target = PlayerMain.Instance.LoadMesh(ID * 10 + 0, meshPath, false);
            target.gameObject.AddComponent<CollisionState>();
            target.transform.SetParent(env);
            targets.Add(target);

            var newGripper = PlayerMain.Instance.InstanceObject<ControllerAttr>(gripperName, ID * 10 + 1, false);

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
                newEnv.localPosition = new Vector3(w * 5, 0, h * 5);
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
                List<Vector3> startPositions = new List<Vector3>();
                List<Quaternion> startRotations = new List<Quaternion>();
                Vector3 tempGravity = Physics.gravity;
                Physics.gravity = Vector3.zero;
                //打开gripper
                for (int j = 0; j < parallelCount; j++)
                {
                    grippers[j].SetJointPosition(new List<float>(new float[] { 0.04f, 0.04f }), ControlMode.Direct);
                }
                yield return new WaitForFixedUpdate();
                //设置物体位置

                for (int j = 0; j < parallelCount; j++)
                {
                    if (i + j >= allPoints.Count) break;
                    Vector3 point = allPoints[i + j];
                    Quaternion quaternion = allQuaternions[i + j];
                    Transform localEnv = envs[j];
                    RigidbodyAttr localTarget = targets[j];
                    localTarget.Rigidbody.isKinematic = true;
                    foreach (var item in localTarget.GetComponentsInChildren<Collider>())
                    {
                        item.isTrigger = true;
                    }

                    if (localTarget.GetComponent<CollisionState>() == null)
                        localTarget.gameObject.AddComponent<CollisionState>();
                    localTarget.GetComponent<CollisionState>().collision = false;
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

                    startPositions.Add(targets[j].transform.position);
                    startRotations.Add(targets[j].transform.rotation);

                    localTarget.transform.position -= Vector3.up * 0.1f;
                    //localTarget.Rigidbody.velocity = Vector3.zero;
                    //localTarget.Rigidbody.Sleep();

                }
                for (int j = 0; j < 100; j++)
                {
                    for (int k = 0; k < parallelCount; k++)
                    {
                        targets[k].transform.position += Vector3.up * (0.1f / 100);
                    }
                    yield return new WaitForFixedUpdate();
                }

                bool[] envSuccess = new bool[parallelCount];
                //初始状态检查
                for (int j = 0; j < parallelCount; j++)
                {
                    if (i + j >= allPoints.Count) break;
                    //检测碰撞状态
                    envSuccess[j] = !targets[j].GetComponent<CollisionState>().collision;
                }
                //设置物体刚体 还原位置
                for (int j = 0; j < parallelCount; j++)
                {
                    if (!envSuccess[j]) continue;
                    if (i + j >= allPoints.Count) break;
                    targets[j].Rigidbody.isKinematic = false;
                    foreach (var item in targets[j].GetComponentsInChildren<Collider>())
                    {
                        item.isTrigger = false;
                    }
                    targets[j].transform.position = startPositions[j];
                    targets[j].transform.rotation = startRotations[j];
                }
                for (int j = 0; j < 5; j++)
                {
                    yield return new WaitForFixedUpdate();
                }
                for (int j = 0; j < parallelCount; j++)
                {
                    if (!envSuccess[j]) continue;
                    if (i + j >= allPoints.Count) break;
                    //检测第5帧后pose
                    envSuccess[j] &= Vector3.Distance(targets[j].transform.position, startPositions[j]) < 0.001f;
                    envSuccess[j] &= Quaternion.Angle(targets[j].transform.rotation, startRotations[j]) < 0.1f;
                }
                //闭合gripper开始抓取
                for (int j = 0; j < parallelCount; j++)
                {
                    if (!envSuccess[j]) continue;
                    if (i + j >= allPoints.Count) break;
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
                    if (i + j >= allPoints.Count) break;
                    envSuccess[j] &= Mathf.Abs(targets[j].transform.position.y - startPositions[j].y) < 0.1f;
                    //envSuccess[j] &= (targets[j].Rigidbody.velocity.sqrMagnitude < 0.01f);
                }
                //写入结果
                for (int j = 0; j < parallelCount; j++)
                {
                    if (i + j >= allPoints.Count) break;
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
        void ShowGraspPose(object[] data)
        {
            string meshPath = (string)data[0];
            string gripperName = (string)data[1];
            List<float> positions = RFUniverseUtility.ConvertType<List<float>>(data[2]);
            List<float> rotations = RFUniverseUtility.ConvertType<List<float>>(data[3]);
            List<Vector3> positionsV3 = RFUniverseUtility.ListFloatToListVector3(positions);
            List<Quaternion> rotationsQ4 = RFUniverseUtility.ListFloatToListQuaternion(rotations);
            //ProcessGraspPose(positionsV3, rotationsQ4);
            //ShowGraspPose(meshPath, gripperName, allPoints, allQuaternions);
            ShowGraspPose(meshPath, gripperName, positionsV3, rotationsQ4);
        }
        void ShowGraspPose(string meshPath, string gripperName, List<Vector3> positionsV3, List<Quaternion> rotationsQ4)
        {
            if (positionsV3.Count != rotationsQ4.Count) return;

            RigidbodyAttr target = PlayerMain.Instance.LoadMesh(ID * 10 + 2, meshPath, false);
            Transform trans = target.transform;
            Destroy(target);
            Destroy(target.Rigidbody);
            trans.SetParent(transform);
            trans.position = Vector3.up;

            var g = PlayerMain.Instance.GetGameObject(gripperName);
            for (int i = 0; i < positionsV3.Count; i++)
            {
                Vector3 position = positionsV3[i];
                Quaternion rotation = rotationsQ4[i];
                GameObject obj = Instantiate(g, trans);
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
            }
        }
        void GenerateGraspPose(object[] data)
        {
            Debug.Log("GenerateGraspPose");
            string meshPath = (string)data[0];
            string gripperName = (string)data[1];
            List<float> points = RFUniverseUtility.ConvertType<List<float>>(data[2]);
            List<float> normals = RFUniverseUtility.ConvertType<List<float>>(data[3]);
            depthRangeMin = (float)data[4];
            depthRangeMax = (float)data[5];
            depthLerpCount = (int)data[6];
            angleLerpCount = (int)data[7];
            parallelCount = (int)data[8];
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
                        //Quaternion angle = Quaternion.AngleAxis(k * 360 / angleLerpCount, Vector3.up);
                        //allPoints.Add(depthPoint);
                        //allQuaternions.Add(qua * angle);
                        Quaternion currentQuat;
                        Quaternion angle = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
                        currentQuat = qua * angle;
                        float dotFF = Mathf.Abs(Vector3.Dot(currentQuat * Vector3.forward, Vector3.forward));
                        float dotFL = Mathf.Abs(Vector3.Dot(currentQuat * Vector3.forward, Vector3.left));
                        float dotLF = Mathf.Abs(Vector3.Dot(currentQuat * Vector3.left, Vector3.forward));
                        float dotLL = Mathf.Abs(Vector3.Dot(currentQuat * Vector3.left, Vector3.left));
                        float dotLU = Mathf.Abs(Vector3.Dot(currentQuat * Vector3.left, Vector3.up));
                        float dotFU = Mathf.Abs(Vector3.Dot(currentQuat * Vector3.forward, Vector3.up));
                        if (dotFF > 0.98f || dotFL > 0.98f || dotLF > 0.98f || dotLL > 0.98f || dotLU > 0.98f || dotFU > 0.98f)
                        {
                            allPoints.Add(depthPoint);
                            allQuaternions.Add(currentQuat);
                        }
                        // else
                        // {
                        //     Debug.Log(Quaternion.Angle(currentQuat, Quaternion.LookRotation(Vector3.left)));
                        //     Debug.Log(Quaternion.Angle(currentQuat, Quaternion.LookRotation(Vector3.forward)));
                        //     k--;
                        // }
                    }
                }
            }
        }

        public void ProcessGraspPose(List<Vector3> pointsV3, List<Quaternion> quaternionsQ4)
        {
            if (pointsV3.Count != quaternionsQ4.Count) return;
            allPoints.Clear();
            allQuaternions.Clear();
            for (int i = 0; i < pointsV3.Count; i++)
            {
                Vector3 point = pointsV3[i];
                point = new Vector3(-point.x, point.y, point.z);
                Quaternion quaternion = quaternionsQ4[i];
                quaternion = new Quaternion(quaternion.x, -quaternion.y, -quaternion.z, quaternion.w);
                //quaternion = Quaternion.Euler(new Vector3(-quaternion.eulerAngles.x, quaternion.eulerAngles.y, quaternion.eulerAngles.z));
                quaternion *= Quaternion.AngleAxis(90, Vector3.forward);
                point += quaternion * Vector3.up * 0.04f;
                allPoints.Add(point);
                allQuaternions.Add(quaternion);
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
}
