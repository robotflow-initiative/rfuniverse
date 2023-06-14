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
        public List<ArticulationData> articulationDatas = new List<ArticulationData>();
        public ControllerData() : base()
        {
            type = "Controller";
        }
        public ControllerData(BaseAttrData b) : base(b)
        {
            type = "Controller";
        }
        public override void SetAttrData(BaseAttr attr)
        {
            base.SetAttrData(attr);
            ControllerAttr controllerAttr = attr as ControllerAttr;
            controllerAttr.SetArticulationDatas(articulationDatas);
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
        public List<int> artIndexQueue = new List<int>();
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
        [NonSerialized]
        public List<ArticulationBody> joints = new List<ArticulationBody>();
        [NonSerialized]
        public List<ArticulationBody> moveableJoints = new List<ArticulationBody>();

        public bool initBioIK = false;
        public bool iKTargetOrientation = true;
        //[HideInInspector]
        [NonSerialized]
        public Transform iKFollow;
        //[HideInInspector]
        [NonSerialized]
        public Transform iKTarget;
        public override void Init()
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
            data.articulationDatas = GetArticulationDatas();
            return data;
        }

        private List<ArticulationData> articulationDatas;
        [EditableAttr("Articulations")]
        [EditAttr("Articulations", "RFUniverse.EditMode.ArticulationAttrUI")]
        public List<ArticulationData> ArticulationDatas
        {
            get
            {
                if (articulationDatas == null)
                    articulationDatas = GetArticulationDatas();
                return articulationDatas;
            }
            set
            {
                articulationDatas = value;
            }
        }
        public List<ArticulationData> GetArticulationDatas()
        {
            List<ArticulationData> datas = new List<ArticulationData>();
            List<ArticulationBody> bodys = this.GetChildComponentFilter<ArticulationBody>();
            if (bodys.Count > 0)
                bodys.RemoveAt(0);
            foreach (var body in bodys)
            {
                ArticulationData data = new ArticulationData();
                datas.Add(data);
                data.artIndexQueue = transform.GetChildIndexQueue(body.transform);
                data.anchorPosition = new float[] { body.anchorPosition.x, body.anchorPosition.y, body.anchorPosition.z };
                data.anchorRotation = new float[] { body.anchorRotation.x, body.anchorRotation.y, body.anchorRotation.z, body.anchorRotation.w };
                data.JointType = body.jointType;

                data.linearLockX = body.linearLockX;
                data.linearLockY = body.linearLockY;
                data.linearLockZ = body.linearLockZ;
                data.swingYLock = body.swingYLock;
                data.swingZLock = body.swingZLock;
                data.twistLock = body.twistLock;

                data.xDrive = new MyArticulationDrive(body.xDrive);
                data.yDrive = new MyArticulationDrive(body.yDrive);
                data.zDrive = new MyArticulationDrive(body.zDrive);
            }
            return datas;
        }
        public void SetArticulationDatas(List<ArticulationData> datas)
        {
            foreach (var data in datas)
            {
                Transform trans = transform.FindChildIndexQueue(data.artIndexQueue);
                if (trans == null) continue;
                ArticulationBody body = trans.GetComponent<ArticulationBody>();
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
        public void GetJointParameters()
        {
            jointParameters = new List<ArticulationParameter>();
            foreach (var item in this.GetChildComponentFilter<ArticulationBody>())
            {
                if (item.GetComponent<ArticulationUnit>() == null)
                    item.gameObject.AddComponent<ArticulationUnit>();
                jointParameters.Add(new ArticulationParameter
                {
                    body = item,
                    moveable = item.jointType != ArticulationJointType.FixedJoint && item.GetComponent<MimicJoint>()?.Parent == null,
                    //initPosition = 0,
                    //isGraspPoint = false
                });
            }
        }
#if BIOIK
        private BioIK.BioIK bioIK = null;
#endif

        private Dictionary<Transform, ArticulationBody> iKCopy = new();
        public void InitBioIK()
        {
#if BIOIK
            if (jointParameters.Count == 0) return;

            Transform first = jointParameters.First().body.transform;
            Transform end = jointParameters.Last().body.transform;
            iKFollow = new GameObject("iKFollowPoint").transform;
            iKFollow.SetParent(end);
            iKFollow.localPosition = Vector3.zero;
            iKFollow.localRotation = Quaternion.identity;

            iKTarget = new GameObject("iKTargetPoint").transform;
            iKTarget.parent = transform;
            ResetIKTarget();

            bioIK = first.GetComponent<BioIK.BioIK>() ?? first.gameObject.AddComponent<BioIK.BioIK>();
            bioIK.isArticulations = true;
            bioIK.SetGenerations(3);
            bioIK.SetPopulationSize(50);
            bioIK.SetElites(1);
            bioIK.Smoothing = 0;
            for (int i = 0; i < jointParameters.Count; i++)
            {
                ArticulationParameter item = jointParameters[i];
                if (item.moveable)
                {
                    BioIK.BioJoint joint = bioIK.FindSegment(item.body.transform).AddJoint();
                    iKCopy.Add(item.body.transform, item.body);
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

            if (iKFollow != null)
            {
                BioIK.BioSegment segment = bioIK.FindSegment(iKFollow);
                segment.Objectives = new BioIK.BioObjective[] { };
                BioIK.BioObjective positionObjective = segment.AddObjective(BioIK.ObjectiveType.Position);
                ((BioIK.Position)positionObjective).SetTargetTransform(iKTarget);
                //((BioIK.Position)positionObjective).SetMaximumError(0.03d);
                if (iKTargetOrientation)
                {
                    BioIK.BioObjective orientationObjective = segment.AddObjective(BioIK.ObjectiveType.Orientation);
                    ((BioIK.Orientation)orientationObjective).SetTargetTransform(iKTarget);
                }
                //((BioIK.Orientation)orientationObjective).SetMaximumError(1d);
            }
            bioIK.Refresh();
#endif
        }
        void ResetIKTarget()
        {
            if (iKFollow != null && iKTarget != null)
            {
                Transform last = jointParameters.Last().body.transform;
                iKTarget.position = iKFollow.position;
                iKTarget.rotation = iKFollow.rotation;
                //iKTarget.position = last.TransformPoint(iKFollow.localPosition);
                //iKTarget.rotation = last.rotation * iKFollow.localRotation;
            }
        }

        private void FixedUpdate()
        {
#if BIOIK
            if (bioIK != null && bioIK.enabled)
            {
                if (tempIKTargetPosition != null || tempIKTargetRotation != null)
                {
                    if (tempIKTargetPosition == null) tempIKTargetPosition = iKTarget.position;
                    if (tempIKTargetRotation == null) tempIKTargetRotation = iKTarget.rotation;
                    DirectlyIK(tempIKTargetPosition.Value, tempIKTargetRotation.Value);
                    tempIKTargetPosition = null;
                    tempIKTargetRotation = null;
                }
                else
                {
                    bioIK.FixedUpdate1();
                    foreach (var item in bioIK.targets)
                    {
                        ArticulationDrive drive = iKCopy[item.Key].xDrive;
                        drive.target = item.Value;
                        iKCopy[item.Key].xDrive = drive;
                    }
                }
            }
#endif
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
            // Each part's joint acceleration
            msg.WriteFloatList(GetJointAcceleration());
            // Each part's joint lower limit
            msg.WriteFloatList(GetJointLowerLimit());
            // Each part's joint upper limit
            msg.WriteFloatList(GetJointUpperLimit());
            // Whether all parts are stable
            msg.WriteBoolean(AllStable());
            msg.WriteBoolean(moveDone);
            msg.WriteBoolean(rotateDone);
#if UNITY_2022_1_OR_NEWER
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
        public List<float> GetJointAcceleration()
        {
            List<float> jointAcceleration = new List<float>();
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                jointAcceleration.Add(moveableJoints[i].jointAcceleration[0]);
            }
            return jointAcceleration;
        }

        public List<float> GetJointLowerLimit()
        {
            List<float> jointLowerLimit = new List<float>();
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                jointLowerLimit.Add(moveableJoints[i].xDrive.lowerLimit);
            }
            return jointLowerLimit;
        }

        public List<float> GetJointUpperLimit()
        {
            List<float> jointUpperLimit = new List<float>();
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                jointUpperLimit.Add(moveableJoints[i].xDrive.upperLimit);
            }
            return jointUpperLimit;
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
                case "SetIndexJointPosition":
                    SetIndexJointPosition(msg);
                    return;
                case "SetIndexJointPositionDirectly":
                    SetIndexJointPositionDirectly(msg);
                    return;
                case "SetJointPositionContinue":
                    SetJointPositionContinue(msg);
                    return;
                case "SetJointVelocity":
                    SetJointVelocity(msg);
                    return;
                case "SetIndexJointVelocity":
                    SetIndexJointVelocity(msg);
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
                case "GripperOpen":
                    GripperOpen();
                    return;
                case "GripperClose":
                    GripperClose();
                    return;
                case "EnabledNativeIK":
                    EnabledNativeIK(msg);
                    return;
                case "IKTargetDoMove":
                    IKTargetDoMove(msg);
                    return;
                case "IKTargetDoRotate":
                    IKTargetDoRotate(msg);
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
                case "SetIKTargetOffset":
                    SetIKTargetOffset(msg);
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
            EnabledNativeIK(enabled);
        }
        public void EnabledNativeIK(bool enabled)
        {
#if BIOIK
            if (bioIK == null)
            {
                Debug.LogWarning($"Controller ID:{ID},Name:{Name},Dont have IK compenent");
                return;
            }
            if (enabled)
            {
                // iKFollow.position = jointParameters.Last().body.transform.TransformPoint(iKFollow.localPosition);
                // iKFollow.rotation = jointParameters.Last().body.transform.rotation * iKFollow.localRotation;
                ResetIKTarget();
                tempIKTargetPosition = iKTarget.position;
                tempIKTargetRotation = iKTarget.rotation;
                //DirectlyRotate(iKTarget.rotation);
                //DirectlyMove(iKTarget.position);
                //directly = true;
            }
            bioIK.enabled = enabled;
#endif
        }

        Vector3? tempIKTargetPosition;
        Quaternion? tempIKTargetRotation;

        bool moveDone = true;
        bool rotateDone = true;
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
            Vector3 pos = new Vector3(x, y, z);
            if (duration == 0)
            {
                //Vector3 tempIKTargetPosition;
                //directly = true;
                if (isRelative)
                    tempIKTargetPosition = iKTarget.transform.position + pos;
                //iKTarget.transform.position += pos;
                else
                    tempIKTargetPosition = pos;
                //iKTarget.transform.position = pos;
                //DirectlyMove(tempIKTargetPosition);
            }
            else
                iKTarget.DOMove(pos, duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
                {
                    moveDone = true;
                };
        }

        private void IKTargetDoRotate(IncomingMessage msg)
        {
            if (iKTarget == null) return;
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();
            float duration = msg.ReadFloat32();
            bool isSpeedBased = msg.ReadBoolean();
            bool isRelative = msg.ReadBoolean();
            Quaternion target = Quaternion.Euler(x, y, z);
            IKTargetDoRotateQuaternion(target, duration, isSpeedBased, isRelative);
        }
        private void IKTargetDoRotateQuaternion(IncomingMessage msg)
        {
            if (iKTarget == null) return;
            float x = msg.ReadFloat32();
            float y = msg.ReadFloat32();
            float z = msg.ReadFloat32();
            float w = msg.ReadFloat32();
            float duration = msg.ReadFloat32();
            bool isSpeedBased = msg.ReadBoolean();
            bool isRelative = msg.ReadBoolean();
            Quaternion target = new Quaternion(x, y, z, w);
            IKTargetDoRotateQuaternion(target, duration, isSpeedBased, isRelative);
        }
        private void IKTargetDoRotateQuaternion(Quaternion target, float duration, bool isSpeedBased = true, bool isRelative = false)
        {
            Debug.Log("IKTargetDoRotateQuaternion");
            if (iKTarget == null) return;
            rotateDone = false;
            if (duration == 0)
            {
                //Quaternion tempIKTargetRotation;
                //directly = true;
                if (isRelative)
                    tempIKTargetRotation = iKTarget.transform.rotation * target;
                //iKTarget.transform.rotation *= rot;
                else
                    tempIKTargetRotation = target;
                //iKTarget.transform.rotation = rot;
                //DirectlyRotate(tempIKTargetRotation);
            }
            else
                iKTarget.DORotateQuaternion(target, duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
                {
                    rotateDone = true;
                };
        }
        void DirectlyIK(Vector3 targetPos, Quaternion targetRot)
        {
#if BIOIK
            Debug.Log("DirectlyIK");
            iKTarget.DOKill();
            float disPos = Vector3.Distance(iKTarget.position, targetPos);
            float disRot = Quaternion.Angle(iKTarget.rotation, targetRot);
            int lerpCountPos = (int)(disPos / 0.01f) + 1;
            int lerpCountRot = (int)(disRot / 1f) + 1;
            int lerpCount = Mathf.Max(lerpCountPos, lerpCountRot, 5);
            Vector3 startPosition = iKTarget.position;
            Quaternion startQuaternion = iKTarget.rotation;
            for (int i = 0; i < lerpCount; i++)
            {
                iKTarget.position = Vector3.Lerp(startPosition, targetPos, i + 1 / (float)lerpCount);
                iKTarget.rotation = Quaternion.Lerp(startQuaternion, targetRot, i + 1 / (float)lerpCount);
                bioIK.FixedUpdate1();
            }
            foreach (var item in bioIK.targets)
            {
                iKCopy[item.Key].GetUnit().SetJointPositionDirectly(item.Value);
            }
            moveDone = true;
            rotateDone = true;
#endif
        }

        private void IKTargetDoComplete()
        {
            if (iKTarget == null) return;
            iKTarget.DOComplete();
            moveDone = true;
            rotateDone = true;
        }
        private void IKTargetDoKill()
        {
            if (iKTarget == null) return;
            iKTarget.DOKill();
            moveDone = true;
            rotateDone = true;
        }
        private void SetIKTargetOffset(IncomingMessage msg)
        {
            if (iKFollow == null) return;
            iKFollow.localPosition = new Vector3(msg.ReadFloat32(), msg.ReadFloat32(), msg.ReadFloat32());
            if (msg.ReadBoolean())
                iKFollow.localRotation = new Quaternion(msg.ReadFloat32(), msg.ReadFloat32(), msg.ReadFloat32(), msg.ReadFloat32());
            else
                iKFollow.localEulerAngles = new Vector3(msg.ReadFloat32(), msg.ReadFloat32(), msg.ReadFloat32());
            ResetIKTarget();
        }
        bool sendJointInverseDynamicsForce = false;
        private void GetJointInverseDynamicsForce()
        {
#if UNITY_2022_1_OR_NEWER
            sendJointInverseDynamicsForce = true;
#else
            Debug.LogWarning($"Controller ID:{ID},Name:{Name},Current Unity version dont support GetJointInverseDynamicsForce API");
#endif
        }

        private void MoveForward(IncomingMessage msg)
        {
            float distance = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            GetComponent<ICustomMove>()?.Forward(distance, speed);
        }
        private void MoveBack(IncomingMessage msg)
        {
            float distance = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            GetComponent<ICustomMove>()?.Back(distance, speed);
        }
        private void TurnLeft(IncomingMessage msg)
        {
            float angle = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            GetComponent<ICustomMove>()?.Left(angle, speed);
        }
        private void TurnRight(IncomingMessage msg)
        {
            float angle = msg.ReadFloat32();
            float speed = msg.ReadFloat32();
            GetComponent<ICustomMove>()?.Right(angle, speed);
        }
        public void GripperOpen()
        {
            //Debug.Log("GripperOpen");
            GetComponent<ICustomGripper>()?.Open();
        }
        public void GripperClose()
        {
            //Debug.Log("GripperClose");
            GetComponent<ICustomGripper>()?.Close();
        }
        public override void SetTransform(bool setPosition, bool setRotation, bool setScale, Vector3 position, Vector3 rotation, Vector3 scale, bool worldSpace = true)
        {
            if (setPosition)
            {
                if (worldSpace)
                    transform.position = position;
                else
                    transform.localPosition = position;
            }
            if (setRotation)
            {
                if (worldSpace)
                    transform.eulerAngles = rotation;
                else
                    transform.localEulerAngles = rotation;
            }
            Root.TeleportRoot(transform.position, Quaternion.Euler(transform.eulerAngles));
        }
        protected override void SetActive(bool active)
        {
            //gameObject.SetActive(active);
        }
        public override void SetParent(int parentID, string parentName)
        {
            if (Attrs.TryGetValue(parentID, out BaseAttr attr))
            {
                Transform parent = attr.transform.FindChlid("ChildPoint", true);
                if (parent == null) return;
                transform.SetParent(parent);
                SetTransform(true, true, false, Vector3.zero, Vector3.zero, Vector3.one, false);
            }

        }
        private void SetJointPosition(IncomingMessage msg)
        {
            //Debug.Log("SetJointPosition");
            List<float> jointPositions = msg.ReadFloatList().ToList();
            List<float> speedScales = msg.ReadFloatList().ToList();
            if (moveableJoints.Count != jointPositions.Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointPositions.Count, moveableJoints.Count));
                return;
            }
            SetJointPosition(jointPositions, ControlMode.Target, speedScales);
        }
        private void SetJointPositionDirectly(IncomingMessage msg)
        {
            //Debug.Log("SetJointPositionDirectly");
            List<float> jointPositions = msg.ReadFloatList().ToList();
            if (moveableJoints.Count != jointPositions.Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointPositions.Count, moveableJoints.Count));
                return;
            }
            SetJointPosition(jointPositions, ControlMode.Direct);
        }
        private void SetIndexJointPosition(IncomingMessage msg)
        {
            int inedx = msg.ReadInt32();
            if (moveableJoints.Count! > inedx)
            {
                Debug.LogError($"The index of target joint positions is {inedx}, but the valid number of joints in robot arm is {moveableJoints.Count}");
                return;
            }
            float jointPosition = msg.ReadFloat32();
            SetIndexJointPosition(inedx, jointPosition, ControlMode.Target);
        }
        private void SetIndexJointPositionDirectly(IncomingMessage msg)
        {
            int inedx = msg.ReadInt32();
            if (moveableJoints.Count! > inedx)
            {
                Debug.LogError($"The index of target joint positions is {inedx}, but the valid number of joints in robot arm is {moveableJoints.Count}");
                return;
            }
            float jointPosition = msg.ReadFloat32();
            SetIndexJointPosition(inedx, jointPosition, ControlMode.Direct);
        }

        private void SetJointPositionContinue(IncomingMessage msg)
        {
            float startTime = Time.time * 1000;
            int timeCount = msg.ReadInt32();
            int interval = msg.ReadInt32();
            List<List<float>> jointPositions = new List<List<float>>();
            for (int i = 0; i < timeCount; i++)
            {
                jointPositions.Add(msg.ReadFloatList().ToList());
            }
            StartCoroutine(SetJointPositionContinue(interval, jointPositions));
        }
        public IEnumerator SetJointPositionContinue(int interval, List<List<float>> jointPositions)
        {
            Debug.Log("SetJointPositionContinue");
            int timeCount = jointPositions.Count;
            int jointCount = jointPositions[0].Count;
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
            float startTime = Time.time * 1000;
            for (int i = 0; i < timeCount; i++)
            {
                while ((Time.time * 1000 - startTime) < interval * i)
                {
                    yield return 0;
                }
                while ((Time.time * 1000 - startTime) > interval * i + 1)
                {
                    i++;
                }
                if (i >= jointPositions.Count)
                    i = jointPositions.Count - 1;
                SetJointPosition(jointPositions[i], ControlMode.Target);
            }
        }
        private void SetJointVelocity(IncomingMessage msg)
        {
            Debug.Log("SetJointVelocity");

            List<float> jointTargetVelocitys = msg.ReadFloatList().ToList();
            if (moveableJoints.Count != jointTargetVelocitys.Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointTargetVelocitys.Count, moveableJoints.Count));
                return;
            }
            SetJointVelocity(jointTargetVelocitys);
        }
        private void SetIndexJointVelocity(IncomingMessage msg)
        {
            Debug.Log("SetIndexJointVelocity");
            int index = msg.ReadInt32();
            float jointTargetVelocity = msg.ReadFloat32();
            SetIndexJointVelocity(index, jointTargetVelocity);
        }
        private void SetJointVelocity(List<float> jointTargetVelocitys)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().SetJointTargetVelocity(jointTargetVelocitys[i]);
            }
        }
        private void SetIndexJointVelocity(int index, float jointTargetVelocity)
        {
            moveableJoints[index].GetUnit().SetJointTargetVelocity(jointTargetVelocity);
        }
        public void SetJointPosition(List<float> jointTargetPositions, ControlMode mode = ControlMode.Target, List<float> speedScales = null)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                float speedScale;
                if (speedScales != null && speedScales.Count > i)
                    speedScale = speedScales[i];
                else
                    speedScale = 1;
                moveableJoints[i].GetUnit().SetJointTargetPosition(jointTargetPositions[i], mode, speedScale);
            }
        }
        public void SetIndexJointPosition(int index, float jointPosition, ControlMode mode = ControlMode.Target)
        {
            moveableJoints[index].GetUnit().SetJointTargetPosition(jointPosition, mode);
        }
        private void AddJointForce(List<Vector3> jointForces)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().AddJointForce(jointForces[i]);
            }
        }
        private void AddJointForceAtPosition(List<Vector3> jointForces, List<Vector3> forcesPosition)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().AddJointForceAtPosition(jointForces[i], forcesPosition[i]);
            }
        }
        private void AddJointTorque(List<Vector3> jointForces)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().AddJointTorque(jointForces[i]);
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
            GUILayout.Space(10);
            GUILayout.Label("Editor Tool:");
            // if (GUILayout.Button("Get Articulation Datas"))
            // {
            //     script.GetArticulationDatas();
            //     EditorUtility.SetDirty(script);
            // }
            if (GUILayout.Button("Get Joint Parameters"))
            {
                script.GetJointParameters();
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
