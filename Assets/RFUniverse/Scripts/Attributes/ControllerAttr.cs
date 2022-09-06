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
        //public float initPosition = 0;
        //public bool isGraspPoint = false;
    }
    public class ControllerAttr : ColliderAttr
    {
        public override string Type
        {
            get { return "Controller"; }
        }
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
        [HideInInspector]
        public List<ArticulationBody> joints = new List<ArticulationBody>();
        [HideInInspector]
        public List<ArticulationBody> moveableJoints = new List<ArticulationBody>();

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
            List<ArticulationBody> bodys = this.GetChildComponentFilter<ArticulationBody>();
            if (bodys.Count > 0)
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
                ArticulationBody body = transform.FindChlid(data.bodyName, true).GetComponent<ArticulationBody>();
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
#if BIOIK
            BioIK.BioIK bioIK = GetComponent<BioIK.BioIK>() ?? gameObject.AddComponent<BioIK.BioIK>();
            bioIK.isArticulations = true;
            bioIK.SetGenerations(3);
            bioIK.SetPopulationSize(50);
            bioIK.SetElites(1);
            bioIK.Smoothing = 1;
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
                iKTarget.parent = transform;
                ResetIKTarget();
                BioIK.BioObjective positionObjective = segment.AddObjective(BioIK.ObjectiveType.Position);
                ((BioIK.Position)positionObjective).SetTargetTransform(iKTarget);
                BioIK.BioObjective orientationObjective = segment.AddObjective(BioIK.ObjectiveType.Orientation);
                ((BioIK.Orientation)orientationObjective).SetTargetTransform(iKTarget);
            }
            bioIK.Refresh();
#endif
        }
        void ResetIKTarget()
        {
            if (jointParameters.Count > 0)
            {
                Transform end = jointParameters.Last().body.transform;
                iKTarget.position = end.position;
                iKTarget.rotation = end.rotation;
            }
        }
        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
            // Number of Articulation Joints
            msg.WriteInt32(joints.Count);
            // Position
            msg.WriteFloatList(GetPositions());
            // Rotation
            msg.WriteFloatList(GetRotations());
            // Quaternion
            msg.WriteFloatList(GetRotationsQuaternion());
            // LocalPosition
            msg.WriteFloatList(GetLocalPositions());
            // LocalRotation
            msg.WriteFloatList(GetLocalRotations());
            // LocalQuaternion
            msg.WriteFloatList(GetLocalRotationsQuaternion());
            // Velocity
            msg.WriteFloatList(GetVelocities());
            // Number of Articulation Moveable Joints
            msg.WriteInt32(moveableJoints.Count);
            // Each part's joint position
            msg.WriteFloatList(GetJointPositions());
            // Each part's joint velocity
            msg.WriteFloatList(GetJointVelocities());
            // Whether all parts are stable
            msg.WriteBoolean(AllStable());
            msg.WriteBoolean(moveDone);
            msg.WriteBoolean(rotateDone);
#if  UNITY_2022_1_OR_NEWER
            if (sendJointInverseDynamicsForce)
            {
                msg.WriteBoolean(true);
                List<float> gravityForces = new List<float>();
                joints[0].GetJointGravityForces(gravityForces);
                msg.WriteFloatList(gravityForces);
                List<float> coriolisCentrifugalForces = new List<float>();
                joints[0].GetJointCoriolisCentrifugalForces(coriolisCentrifugalForces);
                msg.WriteFloatList(coriolisCentrifugalForces);
                List<float> driveForces = new List<float>();
                joints[0].GetDriveForces(driveForces);
                msg.WriteFloatList(driveForces);
            }
            else
            {
                msg.WriteBoolean(false);
            }
#else
            msg.WriteBoolean(false);
#endif
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
        private List<float> GetLocalPositions()
        {
            List<float> positions = new List<float>();
            for (int i = 0; i < joints.Count; i++)
            {
                Vector3 position = joints[i].transform.localPosition;
                positions.Add(position.x);
                positions.Add(position.y);
                positions.Add(position.z);
            }
            return positions;
        }
        private List<float> GetLocalRotations()
        {
            List<float> rotations = new List<float>();
            for (int i = 0; i < joints.Count; i++)
            {
                Vector3 rotation = joints[i].transform.localEulerAngles;
                rotations.Add(rotation.x);
                rotations.Add(rotation.y);
                rotations.Add(rotation.z);
            }
            return rotations;
        }
        private List<float> GetLocalRotationsQuaternion()
        {
            List<float> quaternions = new List<float>();
            for (int i = 0; i < joints.Count; i++)
            {
                Quaternion quaternion = joints[i].transform.localRotation;
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
                case "AddJointTorque":
                    AddJointTorque(msg);
                    return;
                case "SetImmovable":
                    SetImmovable(msg);
                    return;
                case "GetJointInverseDynamicsForce":
                    GetJointInverseDynamicsForce();
                    return;
                case "MoveForward":
                    MoveForward(msg);
                    return;
                case "MoveBack":
                    MoveBack(msg);
                    return;
                case "TurnLeft":
                    TurnLeft(msg);
                    return;
                case "TurnRight":
                    TurnRight(msg);
                    return;
                case "EnabledNativeIK":
                    EnabledNativeIK(msg);
                    return;
                case "IKTargetDoMove":
                    IKTargetDoMove(msg);
                    return;
                case "IKTargetDoRotateQuaternion":
                    IKTargetDoRotateQuaternion(msg);
                    return;
                case "IKTargetDoComplete":
                    IKTargetDoComplete();
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
#if BIOIK
            bool enabled = msg.ReadBoolean();
            BioIK.BioIK bioIK = GetComponent<BioIK.BioIK>();
            if (bioIK == null)
            {
                Debug.LogWarning($"Controller ID:{ID},Name:{Name},Dont have IK compenent");
                return;
            }
            if (enabled)
            {
                ResetIKTarget();
            }
            bioIK.enabled = enabled;
#endif
        }
        bool moveDone;
        private void IKTargetDoMove(IncomingMessage msg)
        {
            Debug.Log("IKTargetDoMove");
            if (iKTarget == null) return;
            moveDone = false;
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();
            float duration = msg.ReadFloat32();
            bool isSpeedBased = msg.ReadBoolean();
            bool isRelative = msg.ReadBoolean();
            iKTarget.DOMove(new Vector3(x, y, z), duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
            {
                moveDone = true;
            };
        }
        bool rotateDone;
        private void IKTargetDoRotateQuaternion(IncomingMessage msg)
        {
            Debug.Log("IKTargetDoRotateQuaternion");
            if (iKTarget == null) return;
            rotateDone = false;
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();
            float w = msg.ReadFloat32();
            float duration = msg.ReadFloat32();
            bool isSpeedBased = msg.ReadBoolean();
            bool isRelative = msg.ReadBoolean();
            iKTarget.DORotateQuaternion(new Quaternion(x, y, z, w), duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
            {
                rotateDone = true;
            };
        }
        private void IKTargetDoComplete()
        {
            if (iKTarget == null) return;
            iKTarget.DOComplete();
        }
        private void IKTargetDoKill()
        {
            if (iKTarget == null) return;
            iKTarget.DOKill();
        }

        bool sendJointInverseDynamicsForce = false;
        private void GetJointInverseDynamicsForce()
        {
#if  UNITY_2022_1_OR_NEWER
            sendJointInverseDynamicsForce = true;
#else
            Debug.LogWarning($"Controller ID:{ID},Name:{Name},Current Unity version dont support GetJointInverseDynamicsForce API");
#endif
        }

        private void MoveForward(IncomingMessage msg)
        {
            float distance = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            GetComponent<ICustomMove>().Forward(distance, speed);
        }
        private void MoveBack(IncomingMessage msg)
        {
            float distance = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            GetComponent<ICustomMove>().Back(distance, speed);
        }
        private void TurnLeft(IncomingMessage msg)
        {
            float angle = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            GetComponent<ICustomMove>().Left(angle, speed);
        }
        private void TurnRight(IncomingMessage msg)
        {
            float angle = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            GetComponent<ICustomMove>().Right(angle, speed);
        }
        public override void SetTransform(bool set_position, bool set_rotation, bool set_scale, Vector3 position, Vector3 rotation, Vector3 scale, bool worldSpace = true)
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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ControllerAttr), true)]
    public class ControllerAttrEditor : ColliderAttrEditor
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
                foreach (var item in script.GetChildComponentFilter<ArticulationBody>())
                {
                    if (!item.GetComponent<ArticulationUnit>())
                        item.gameObject.AddComponent<ArticulationUnit>();
                    script.jointParameters.Add(new ArticulationParameter
                    {
                        body = item,
                        moveable = item.jointType != ArticulationJointType.FixedJoint && item.GetComponent<MimicJoint>()?.Parent == null,
                        //initPosition = 0,
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
