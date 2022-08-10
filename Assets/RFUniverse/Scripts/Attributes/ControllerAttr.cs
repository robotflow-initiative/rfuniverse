using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using UnityEditor;
using System;
using System.Linq;
using DG.Tweening;

namespace RFUniverse.Attributes
{
    public class ControllerData : ColliderAttrData
    {
        public List<ArticulationData> articulationDatas;
        public ControllerData() : base()
        {
            type = "Controller";
        }
        public ControllerData(BaseAttrData b) : base(b)
        {
            type = "Controller";
        }
    }
    [Serializable]
    public class MyArticulationDrive
    {
        public float lowerLimit;
        public float upperLimit;
        public float stiffness;
        public float damping;
        public float forceLimit;
        public float target;
        public float targetVelocity;
        public MyArticulationDrive(ArticulationDrive drive)
        {
            lowerLimit = drive.lowerLimit;
            upperLimit = drive.upperLimit;
            stiffness = drive.stiffness;
            damping = drive.damping;
            forceLimit = drive.forceLimit;
            target = drive.target;
            targetVelocity = drive.targetVelocity;
        }
        public ArticulationDrive ToArticulationDrive()
        {
            return new ArticulationDrive
            {
                lowerLimit = lowerLimit,
                upperLimit = upperLimit,
                stiffness = stiffness,
                damping = damping,
                forceLimit = forceLimit,
                target = target,
                targetVelocity = targetVelocity
            };
        }
    }
    [Serializable]
    public class ArticulationData
    {
        public string bodyName;
        public float[] anchorPosition;
        public float[] anchorRotation;

        public ArticulationJointType JointType;

        public ArticulationDofLock linearLockX;
        public ArticulationDofLock linearLockY;
        public ArticulationDofLock linearLockZ;
        public ArticulationDofLock swingYLock;
        public ArticulationDofLock swingZLock;
        public ArticulationDofLock twistLock;
        public MyArticulationDrive xDrive;
        public MyArticulationDrive yDrive;
        public MyArticulationDrive zDrive;
        public ArticulationData()
        {
        }
        public ArticulationData(ArticulationBody body)
        {
            bodyName = body.name;
            anchorPosition = new float[] { body.anchorPosition.x, body.anchorPosition.y, body.anchorPosition.z };
            anchorRotation = new float[] { body.anchorRotation.x, body.anchorRotation.y, body.anchorRotation.z, body.anchorRotation.w };
            JointType = body.jointType;

            linearLockX = body.linearLockX;
            linearLockY = body.linearLockY;
            linearLockZ = body.linearLockZ;
            swingYLock = body.swingYLock;
            swingZLock = body.swingZLock;
            twistLock = body.twistLock;

            xDrive = new MyArticulationDrive(body.xDrive);
            yDrive = new MyArticulationDrive(body.yDrive);
            zDrive = new MyArticulationDrive(body.zDrive);
        }
    }
    [Serializable]
    public class ArticulationParameter
    {
        public ArticulationBody body;
        public bool moveable = true;
        public float initPosition = 0;
        //public bool isGraspPoint = false;
    }
    public class ControllerAttr : ColliderAttr
    {
        [SerializeField]
        private List<ArticulationData> articulationDatas;

        [Attr("Articulations")]
        public List<ArticulationData> ArticulationDatas
        {
            get
            {
                return articulationDatas;
            }
            set
            {
                articulationDatas = value;
            }
        }
        private ArticulationBody root;
        public ArticulationBody Root
        {
            get
            {
                if (root == null)
                    root = GetComponentInChildren<ArticulationBody>();
                if (root == null)
                    Debug.LogError("Controller not find ArticulationBody");
                return root;
            }
        }
        public List<ArticulationParameter> jointParameters = new List<ArticulationParameter>();
        protected List<ArticulationBody> joints = new List<ArticulationBody>();
        protected List<ArticulationBody> moveableJoints = new List<ArticulationBody>();
        [HideInInspector]
        public ArticulationBody graspPoint = null;
        private List<float> initJointPositions = new List<float>();

        public bool initBioIK = false;
        Transform iKTarget;
        protected override void Init()
        {
            base.Init();
            joints = jointParameters.Select(s => s.body).ToList();
            moveableJoints = jointParameters.Where(s => s.moveable).Select(s => s.body).ToList();
            foreach (var item in moveableJoints)
            {
                if (!item.GetComponent<ArticulationUnit>())
                    item.gameObject.AddComponent<ArticulationUnit>();
            }
            initJointPositions = jointParameters.Where(s => s.moveable).Select(s => s.initPosition).ToList();
            if (jointParameters.Count > 0)
            {
                graspPoint = jointParameters.Last()?.body;
            }
            if (initBioIK)
            {
                InitBioIK();
            }
        }
        public override BaseAttrData GetAttrData()
        {
            ControllerData data = new ControllerData(base.GetAttrData());

            data.articulationDatas = ArticulationDatas;

            return data;
        }
        public override void SetAttrData(BaseAttrData setData)
        {
            base.SetAttrData(setData);
            if (setData is ControllerData)
            {
                ControllerData data = setData as ControllerData;

                ArticulationDatas = data.articulationDatas;
                SetArticulationDatas(articulationDatas);
            }
        }
        public List<ArticulationData> GetArticulationDatas()
        {
            List<ArticulationData> datas = new List<ArticulationData>();
            List<ArticulationBody> bodys = new List<ArticulationBody>(GetComponentsInChildren<ArticulationBody>());
            if (bodys.Count == 0) return null;
            bodys.RemoveAt(0);
            foreach (var item in bodys)
            {
                datas.Add(new ArticulationData(item));
            }
            return datas;
        }
        private void SetArticulationDatas(List<ArticulationData> datas)
        {
            foreach (var data in datas)
            {
                ArticulationBody body = FindChlid(transform, data.bodyName, true).GetComponent<ArticulationBody>();

                if (body == null) continue;

                body.anchorPosition = new Vector3(data.anchorPosition[0], data.anchorPosition[1], data.anchorPosition[2]);
                body.anchorRotation = new Quaternion(data.anchorRotation[0], data.anchorRotation[1], data.anchorRotation[2], data.anchorRotation[3]);

                body.jointType = data.JointType;

                body.linearLockX = data.linearLockX;
                body.linearLockY = data.linearLockY;
                body.linearLockZ = data.linearLockZ;
                body.swingYLock = data.swingYLock;
                body.swingZLock = data.swingZLock;
                body.twistLock = data.twistLock;

                body.xDrive = data.xDrive.ToArticulationDrive();
                body.yDrive = data.yDrive.ToArticulationDrive();
                body.zDrive = data.zDrive.ToArticulationDrive();
            }
        }

        public void InitBioIK()
        {
            BioIK.BioIK bioIK = GetComponent<BioIK.BioIK>() ?? gameObject.AddComponent<BioIK.BioIK>();
            bioIK.SetGenerations(1);
            bioIK.SetPopulationSize(30);
            bioIK.SetElites(1);
            bioIK.Smoothing = 0;
            for (int i = 0; i < jointParameters.Count; i++)
            {
                ArticulationParameter item = jointParameters[i];
                if (item.moveable)
                {
                    BioIK.BioJoint joint = bioIK.FindSegment(item.body.transform).AddJoint();
                    joint.SetAnchor(item.body.anchorPosition);
                    joint.SetOrientation(item.body.anchorRotation.eulerAngles);
                    joint.SetDefaultFrame(item.body.transform.localPosition, item.body.transform.localRotation);
                    switch (item.body.jointType)
                    {
                        case ArticulationJointType.RevoluteJoint:
                            joint.JointType = BioIK.JointType.Rotational;
                            joint.X.Enabled = true;
                            switch (item.body.twistLock)
                            {
                                case ArticulationDofLock.FreeMotion:
                                    joint.X.Constrained = false;
                                    break;
                                case ArticulationDofLock.LimitedMotion:
                                    joint.X.Constrained = true;
                                    joint.X.SetUpperLimit(item.body.xDrive.upperLimit);
                                    joint.X.SetLowerLimit(item.body.xDrive.lowerLimit);
                                    joint.X.SetTargetValue(item.body.xDrive.target);
                                    break;
                            }
                            joint.Y.Enabled = false;
                            joint.Z.Enabled = false;
                            break;
                        case ArticulationJointType.PrismaticJoint:
                            joint.JointType = BioIK.JointType.Translational;
                            switch (item.body.linearLockX)
                            {
                                case ArticulationDofLock.LockedMotion:
                                    joint.X.Enabled = false;
                                    break;
                                case ArticulationDofLock.FreeMotion:
                                    joint.X.Enabled = true;
                                    joint.X.Constrained = false;
                                    break;
                                case ArticulationDofLock.LimitedMotion:
                                    joint.X.Enabled = true;
                                    joint.X.Constrained = true;
                                    joint.X.SetUpperLimit(item.body.xDrive.upperLimit);
                                    joint.X.SetLowerLimit(item.body.xDrive.lowerLimit);
                                    joint.X.SetTargetValue(item.body.xDrive.target);
                                    break;
                            }
                            switch (item.body.linearLockY)
                            {
                                case ArticulationDofLock.LockedMotion:
                                    joint.Y.Enabled = false;
                                    break;
                                case ArticulationDofLock.FreeMotion:
                                    joint.Y.Enabled = true;
                                    joint.Y.Constrained = false;
                                    break;
                                case ArticulationDofLock.LimitedMotion:
                                    joint.Y.Enabled = true;
                                    joint.Y.Constrained = true;
                                    joint.Y.SetUpperLimit(item.body.yDrive.upperLimit);
                                    joint.Y.SetLowerLimit(item.body.yDrive.lowerLimit);
                                    joint.Y.SetTargetValue(item.body.yDrive.target);
                                    break;
                            }
                            switch (item.body.linearLockZ)
                            {
                                case ArticulationDofLock.LockedMotion:
                                    joint.Z.Enabled = false;
                                    break;
                                case ArticulationDofLock.FreeMotion:
                                    joint.Z.Enabled = true;
                                    joint.Z.Constrained = false;
                                    break;
                                case ArticulationDofLock.LimitedMotion:
                                    joint.Z.Enabled = true;
                                    joint.Z.Constrained = true;
                                    joint.Z.SetUpperLimit(item.body.zDrive.upperLimit);
                                    joint.Z.SetLowerLimit(item.body.zDrive.lowerLimit);
                                    joint.Z.SetTargetValue(item.body.zDrive.target);
                                    break;
                            }
                            break;
                        case ArticulationJointType.SphericalJoint:
                            joint.JointType = BioIK.JointType.Rotational;
                            switch (item.body.twistLock)
                            {
                                case ArticulationDofLock.LockedMotion:
                                    joint.X.Enabled = false;
                                    break;
                                case ArticulationDofLock.FreeMotion:
                                    joint.X.Enabled = true;
                                    joint.X.Constrained = false;
                                    break;
                                case ArticulationDofLock.LimitedMotion:
                                    joint.X.Enabled = true;
                                    joint.X.Constrained = true;
                                    joint.X.SetUpperLimit(item.body.xDrive.upperLimit);
                                    joint.X.SetLowerLimit(item.body.xDrive.lowerLimit);
                                    joint.X.SetTargetValue(item.body.xDrive.target);
                                    break;
                            }
                            switch (item.body.swingYLock)
                            {
                                case ArticulationDofLock.LockedMotion:
                                    joint.Y.Enabled = false;
                                    break;
                                case ArticulationDofLock.FreeMotion:
                                    joint.Y.Enabled = true;
                                    joint.Y.Constrained = false;
                                    break;
                                case ArticulationDofLock.LimitedMotion:
                                    joint.Y.Enabled = true;
                                    joint.Y.Constrained = true;
                                    joint.Y.SetUpperLimit(item.body.yDrive.upperLimit);
                                    joint.Y.SetLowerLimit(item.body.yDrive.lowerLimit);
                                    joint.Y.SetTargetValue(item.body.yDrive.target);
                                    break;
                            }
                            switch (item.body.swingZLock)
                            {
                                case ArticulationDofLock.LockedMotion:
                                    joint.Z.Enabled = false;
                                    break;
                                case ArticulationDofLock.FreeMotion:
                                    joint.Z.Enabled = true;
                                    joint.Z.Constrained = false;
                                    break;
                                case ArticulationDofLock.LimitedMotion:
                                    joint.Z.Enabled = true;
                                    joint.Z.Constrained = true;
                                    joint.Z.SetUpperLimit(item.body.zDrive.upperLimit);
                                    joint.Z.SetLowerLimit(item.body.zDrive.lowerLimit);
                                    joint.Z.SetTargetValue(item.body.zDrive.target);
                                    break;
                            }
                            break;
                    }
                }
            }
            if (jointParameters.Count > 0)
            {
                Transform end = jointParameters.Last().body.transform;
                BioIK.BioSegment segment = bioIK.FindSegment(end);
                segment.Objectives = new BioIK.BioObjective[] { };
                iKTarget = new GameObject("iKTarget").transform;
                iKTarget.parent = jointParameters.First().body.transform;
                iKTarget.position = end.position;
                iKTarget.rotation = end.rotation;
                BioIK.BioObjective positionObjective = segment.AddObjective(BioIK.ObjectiveType.Position);
                ((BioIK.Position)positionObjective).SetTargetTransform(iKTarget);
                BioIK.BioObjective orientationObjective = segment.AddObjective(BioIK.ObjectiveType.Orientation);
                ((BioIK.Orientation)orientationObjective).SetTargetTransform(iKTarget);
            }
            bioIK.Refresh();
        }
        public override void CollectData(OutgoingMessage msg)
        {
            msg.WriteString("Controller");
            // ID
            msg.WriteInt32(ID);
            // Name
            msg.WriteString(Name);
            // Position
            msg.WriteFloatList(GetPositions());
            // Rotation
            msg.WriteFloatList(GetRotations());
            // Quaternion
            msg.WriteFloatList(GetRotationsQuaternion());
            // Velocity
            msg.WriteFloatList(GetVelocities());
            // GraspPointPosition
            msg.WriteFloatList(GetGraspPointPosition());
            // GraspPointRotation
            msg.WriteFloatList(GetGraspPointRotation());
            // GraspPointRotationQuaternion
            msg.WriteFloatList(GetGraspPointRotationQuaternion());
            // Number of articulation parts
            msg.WriteInt32(GetNumberOfJoints());
            // Each part's joint position
            msg.WriteFloatList(GetJointPositions());
            // Each part's joint velocity
            msg.WriteFloatList(GetJointVelocities());
            // Whether all parts are stable
            msg.WriteBoolean(AllStable());
        }

        private List<float> GetPositions()
        {
            List<float> positions = new List<float>();
            for (int i = 0; i < joints.Count; i++)
            {
                Vector3 position = joints[i].transform.position;
                positions.Add(position.x);
                positions.Add(position.y);
                positions.Add(position.z);
            }
            return positions;
        }
        private List<float> GetRotations()
        {
            List<float> rotations = new List<float>();
            for (int i = 0; i < joints.Count; i++)
            {
                Vector3 rotation = joints[i].transform.eulerAngles;
                rotations.Add(rotation.x);
                rotations.Add(rotation.y);
                rotations.Add(rotation.z);
            }
            return rotations;
        }
        private List<float> GetRotationsQuaternion()
        {
            List<float> quaternions = new List<float>();
            for (int i = 0; i < joints.Count; i++)
            {
                Quaternion quaternion = joints[i].transform.rotation;
                quaternions.Add(quaternion.x);
                quaternions.Add(quaternion.y);
                quaternions.Add(quaternion.z);
                quaternions.Add(quaternion.w);
            }
            return quaternions;
        }
        private List<float> GetVelocities()
        {
            List<float> velocities = new List<float>();
            for (int i = 0; i < joints.Count; i++)
            {
                Vector3 velocity = joints[i].velocity;
                velocities.Add(velocity.x);
                velocities.Add(velocity.y);
                velocities.Add(velocity.z);
            }
            return velocities;
        }
        private List<float> GetGraspPointPosition()
        {
            List<float> position = new List<float>();
            if (graspPoint == null) return position;
            position.Add(graspPoint.transform.position.x);
            position.Add(graspPoint.transform.position.y);
            position.Add(graspPoint.transform.position.z);
            return position;
        }
        private List<float> GetGraspPointRotation()
        {
            List<float> rotation = new List<float>();
            if (graspPoint == null) return rotation;
            rotation.Add(graspPoint.transform.eulerAngles.x);
            rotation.Add(graspPoint.transform.eulerAngles.y);
            rotation.Add(graspPoint.transform.eulerAngles.z);
            return rotation;
        }
        private List<float> GetGraspPointRotationQuaternion()
        {
            List<float> quaternion = new List<float>();
            if (graspPoint == null) return quaternion;
            quaternion.Add(graspPoint.transform.rotation.x);
            quaternion.Add(graspPoint.transform.rotation.y);
            quaternion.Add(graspPoint.transform.rotation.z);
            quaternion.Add(graspPoint.transform.rotation.w);
            return quaternion;
        }
        private int GetNumberOfJoints()
        {
            return joints.Count;
        }
        public List<float> GetJointPositions()
        {
            List<float> jointPositions = new List<float>();
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                jointPositions.Add(moveableJoints[i].GetComponent<ArticulationUnit>().CalculateCurrentJointPosition());
            }
            return jointPositions;
        }
        public List<float> GetJointVelocities()
        {
            List<float> jointVelocities = new List<float>();
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                jointVelocities.Add(moveableJoints[i].jointVelocity[0]);
            }

            return jointVelocities;
        }

        internal bool AllStable()
        {
            foreach (ArticulationBody joint in joints)
            {
                if (joint.TryGetComponent(out ArticulationUnit unit))
                    if (unit.GetMovingDirection() != MovingDirection.None)
                        return false;
            }
            return true;
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "SetJointPosition":
                    SetJointPosition(msg);
                    return;
                case "SetJointPositionDirectly":
                    SetJointPositionDirectly(msg);
                    return;
                case "SetJointPositionContinue":
                    StartCoroutine(SetJointPositionContinue(msg));
                    return;
                case "SetJointVelocity":
                    SetJointVelocity(msg);
                    return;
                case "AddJointForce":
                    AddJointForce(msg);
                    return;
                case "AddJointForceAtPosition":
                    AddJointForceAtPosition(msg);
                    return;
                case "AddTorque":
                    AddJointTorque(msg);
                    return;
                case "SetImmovable":
                    SetImmovable(msg);
                    return;
                case "GetJointInverseDynamicsForce":
                    GetJointInverseDynamicsForce();
                    return;
                case "EnableNativeIK":
                    EnabledNativeIK(msg);
                    return;
                case "IKTargetDoMove":
                    IKTargetDoMove(msg);
                    return;
                case "IKTargetDoRotateQuaternion":
                    IKTargetDoRotateQuaternion(msg);
                    return;
                case "IKTargetDoKill":
                    IKTargetDoKill();
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        private void AddJointForce(IncomingMessage msg)
        {
            int jointCount = msg.ReadInt32();
            if (moveableJoints.Count != jointCount)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointCount, moveableJoints.Count));
                return;
            }
            List<Vector3> jointForces = new List<Vector3>();
            for (int i = 0; i < jointCount; i++)
            {
                Vector3 force = new Vector3();
                force.x = msg.ReadFloat32();
                force.y = msg.ReadFloat32();
                force.z = msg.ReadFloat32();
                jointForces.Add(force);
            }
            AddJointForce(jointForces);
        }
        private void AddJointForceAtPosition(IncomingMessage msg)
        {
            int jointCount = msg.ReadInt32();
            if (moveableJoints.Count != jointCount)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointCount, moveableJoints.Count));
                return;
            }
            List<Vector3> jointForces = new List<Vector3>();
            List<Vector3> forcesPosition = new List<Vector3>();
            for (int i = 0; i < jointCount; i++)
            {
                Vector3 force = new Vector3();
                force.x = msg.ReadFloat32();
                force.y = msg.ReadFloat32();
                force.z = msg.ReadFloat32();
                jointForces.Add(force);
                Vector3 position = new Vector3();
                position.x = msg.ReadFloat32();
                position.y = msg.ReadFloat32();
                position.z = msg.ReadFloat32();
                forcesPosition.Add(position);
            }
            AddJointForceAtPosition(jointForces, forcesPosition);
        }
        private void AddJointTorque(IncomingMessage msg)
        {
            int jointCount = msg.ReadInt32();
            if (moveableJoints.Count != jointCount)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointCount, moveableJoints.Count));
                return;
            }
            List<Vector3> jointForces = new List<Vector3>();
            for (int i = 0; i < jointCount; i++)
            {
                Vector3 force = new Vector3();
                force.x = msg.ReadFloat32();
                force.y = msg.ReadFloat32();
                force.z = msg.ReadFloat32();
                jointForces.Add(force);
            }
            AddJointTorque(jointForces);
        }
        private void SetImmovable(IncomingMessage msg)
        {
            bool immovable = msg.ReadBoolean();
            ArticulationBody first = GetComponentInChildren<ArticulationBody>();
            if (first.isRoot)
                first.immovable = immovable;
            else
                Debug.LogWarning($"Controller ID:{ID},Name:{Name},is not root ArticulationBody");
        }
        private void EnabledNativeIK(IncomingMessage msg)
        {
            bool enabled = msg.ReadBoolean();
            BioIK.BioIK bioIK = GetComponent<BioIK.BioIK>();
            if (bioIK == null)
                Debug.LogWarning($"Controller ID:{ID},Name:{Name},Dont have IK compenent");
            bioIK.enabled = enabled;
        }
        private void IKTargetDoMove(IncomingMessage msg)
        {
            if (iKTarget == null) return;
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            iKTarget.DOComplete();
            iKTarget.DOMove(new Vector3(x, y, z), speed).SetSpeedBased(true).SetEase(Ease.Linear);
        }
        private void IKTargetDoRotateQuaternion(IncomingMessage msg)
        {
            if (iKTarget == null) return;
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();
            float w = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            iKTarget.DOComplete();
            iKTarget.DORotateQuaternion(new Quaternion(), speed).SetSpeedBased(true).SetEase(Ease.Linear);
        }
        private void IKTargetDoKill()
        {
            if (iKTarget == null) return;
            iKTarget.DOKill();
        }
        private void GetJointInverseDynamicsForce()
        {
#if  UNITY_2022_1_OR_NEWER
            OutgoingMessage msg = new OutgoingMessage();
            msg.WriteString("JointInverseDynamicsForce");
            List<float> gravityForces = new List<float>();
            joints[0].GetJointGravityForces(gravityForces);
            msg.WriteFloatList(gravityForces);
            List<float> coriolisCentrifugalForces = new List<float>();
            joints[0].GetJointCoriolisCentrifugalForces(coriolisCentrifugalForces);
            msg.WriteFloatList(coriolisCentrifugalForces);
            List<float> driveForces = new List<float>();
            joints[0].GetDriveForces(driveForces);
            msg.WriteFloatList(driveForces);
            InstanceManager.Instance.channel.SendMetaDataToPython(msg);
#else
            Debug.LogWarning($"Controller ID:{ID},Name:{Name},Current Unity version dont support GetJointInverseDynamicsForce API");
#endif
        }
        public override void SetTransform(bool set_position, bool set_rotation, bool set_scale, Vector3 position, Vector3 rotation, Vector3 scale, bool worldSpace = false)
        {
            Debug.Log("SetTransform");
            if (set_position)
            {
                if (worldSpace)
                    transform.position = position;
                else
                    transform.localPosition = position;
            }
            if (set_rotation)
            {
                if (worldSpace)
                    transform.eulerAngles = rotation;
                else
                    transform.localEulerAngles = rotation;
            }
            Root.TeleportRoot(transform.position, Quaternion.Euler(transform.eulerAngles));
        }
        private void SetJointPosition(IncomingMessage msg)
        {
            //Debug.Log("SetJointPosition");
            int jointCount = msg.ReadInt32();
            if (moveableJoints.Count != jointCount)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointCount, moveableJoints.Count));
                return;
            }

            List<float> jointPositions = msg.ReadFloatList().ToList();
            List<float> speedScales = msg.ReadFloatList().ToList();

            SetJointPosition(jointPositions, speedScales, ControlMode.Target);
        }

        private void SetJointPositionDirectly(IncomingMessage msg)
        {
            //Debug.Log("SetJointPositionDirectly");
            int jointCount = msg.ReadInt32();
            if (moveableJoints.Count != jointCount)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointCount, moveableJoints.Count));
                return;
            }

            List<float> jointPositions = msg.ReadFloatList().ToList();
            List<float> speedScales = new List<float>();
            for (int i = 0; i < moveableJoints.Count; ++i)
            {
                speedScales.Add(1.0f);
            }
            SetJointPosition(jointPositions, speedScales, ControlMode.Direct);
        }

        private IEnumerator SetJointPositionContinue(IncomingMessage msg)
        {
            Debug.Log("SetJointPositionContinue");
            float startTime = Time.time * 1000;
            int timeCount = msg.ReadInt32();
            int jointCount = msg.ReadInt32();
            int interval = msg.ReadInt32();
            List<List<float>> jointPositions = new List<List<float>>();
            for (int i = 0; i < timeCount; i++)
            {
                jointPositions.Add(msg.ReadFloatList().ToList());
            }
            if (moveableJoints.Count != jointCount)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointCount, moveableJoints.Count));
                yield break;
            }
            List<float> speedScales = new List<float>();
            for (int j = 0; j < jointCount; j++)
            {
                speedScales.Add(1.0f);
            }
            for (int i = 0; i < timeCount; i++)
            {
                while ((Time.time * 1000 - startTime) < interval * i)
                {
                    yield return 0;
                }
                SetJointPosition(jointPositions[i], speedScales, ControlMode.Target);
            }
        }
        private void SetJointVelocity(IncomingMessage msg)
        {
            Debug.Log("SetJointVelocity");
            int jointCount = msg.ReadInt32();
            if (moveableJoints.Count != jointCount)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointCount, moveableJoints.Count));
                return;
            }
            List<float> jointTargetVelocitys = msg.ReadFloatList().ToList();
            SetJointVelocity(jointTargetVelocitys);
        }
        private void SetJointVelocity(List<float> jointTargetVelocitys)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetComponent<ArticulationUnit>().SetJointTargetVelocity(jointTargetVelocitys[i]);
            }
        }
        private void SetJointPosition(List<float> jointTargetPositions, List<float> speedScales, ControlMode mode)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetComponent<ArticulationUnit>().SetJointTargetPosition(jointTargetPositions[i], mode, speedScales[i]);
            }
        }
        private void AddJointForce(List<Vector3> jointForces)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetComponent<ArticulationUnit>().AddJointForce(jointForces[i]);
            }
        }
        private void AddJointForceAtPosition(List<Vector3> jointForces, List<Vector3> forcesPosition)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetComponent<ArticulationUnit>().AddJointForceAtPosition(jointForces[i], forcesPosition[i]);
            }
        }
        private void AddJointTorque(List<Vector3> jointForces)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetComponent<ArticulationUnit>().AddJointTorque(jointForces[i]);
            }
        }
        public void ReSetJointPosition()
        {
            List<float> speedScales = new List<float>() { };
            for (int i = 0; i < joints.Count; ++i)
            {
                speedScales.Add(0);
            }
            SetJointPosition(initJointPositions, speedScales, ControlMode.Direct);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ControllerAttr), true)]
    public class BaseControllerEditor : ColliderAttrEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ControllerAttr script = target as ControllerAttr;
            if (GUILayout.Button("Get Articulation Datas"))
            {
                script.ArticulationDatas = script.GetArticulationDatas();
                EditorUtility.SetDirty(script);
            }
            if (GUILayout.Button("Get Joint Parameters"))
            {
                script.jointParameters = new List<ArticulationParameter>();
                List<ArticulationBody> bodys = script.GetComponentsInChildren<ArticulationBody>().ToList();
                foreach (var item in script.childs)
                {
                    ArticulationBody[] childsBody = item.GetComponentsInChildren<ArticulationBody>();
                    foreach (var i in childsBody)
                    {
                        if (bodys.Contains(i))
                            bodys.Remove(i);
                    }
                }

                foreach (var item in bodys)
                {
                    if (!item.GetComponent<ArticulationUnit>())
                        item.gameObject.AddComponent<ArticulationUnit>();
                    script.jointParameters.Add(new ArticulationParameter
                    {
                        body = item,
                        moveable = item.jointType != ArticulationJointType.FixedJoint && item.GetComponent<MimicJoint>()?.Parent == null,
                        initPosition = 0,
                        //isGraspPoint = false
                    });
                }
                EditorUtility.SetDirty(script);
            }
            // if (GUILayout.Button("Add BioIK"))
            // {
            //     script.InitBioIK();
            // }
        }
    }
#endif
}
