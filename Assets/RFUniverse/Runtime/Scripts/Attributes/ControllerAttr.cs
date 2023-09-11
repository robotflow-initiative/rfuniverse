using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public Transform graspPoint;
        public bool initBioIK = false;
        public bool iKTargetOrientation = true;
        [NonSerialized]
        public Transform iKFollow;
        [NonSerialized]
        public Transform iKTarget;
        public override void Init()
        {
            base.Init();
            IsRFMoveCollider = false;
            joints = jointParameters.Select(s => s.body).ToList();
            moveableJoints = jointParameters.Where(s => s.moveable).Select(s => s.body).ToList();
            if (graspPoint == null) graspPoint = joints.LastOrDefault()?.transform;
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
                    moveable = item.jointType != ArticulationJointType.FixedJoint && item.GetUnit().mimicParent == null && !item.isRoot,
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

            Transform first = GetComponentInChildren<ArticulationBody>().transform;
            iKFollow = new GameObject("iKFollowPoint").transform;
            iKFollow.SetParent(graspPoint);
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
                        iKCopy[item.Key].GetUnit().SetJointPosition(item.Value);
                    }
                }
            }
#endif
        }

        public override Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = base.CollectData();
            // Number of Articulation Joints
            data.Add("number_of_joints", joints.Count);
            // Position
            data.Add("positions", joints.Select(s => s.transform.position).ToList());
            // Rotation
            data.Add("rotations", joints.Select(s => s.transform.eulerAngles).ToList());
            // Quaternion
            data.Add("quaternions", joints.Select(s => s.transform.rotation).ToList());
            // LocalPosition
            data.Add("local_positions", joints.Select(s => s.transform.localPosition).ToList());
            // LocalRotation
            data.Add("local_rotations", joints.Select(s => s.transform.localEulerAngles).ToList());
            // LocalQuaternion
            data.Add("local_quaternions", joints.Select(s => s.transform.localRotation).ToList());
            // Velocity
            data.Add("velocities", joints.Select(s => s.velocity).ToList());
            // Number of Articulation Moveable Joints
            data.Add("number_of_moveable_joints", moveableJoints.Count);

            // Each part's joint position
            data.Add("joint_positions", moveableJoints.Select(s => s.GetUnit().CalculateCurrentJointPosition()).ToList());
            // Each part's joint velocity
            data.Add("joint_velocities", moveableJoints.Select(s => s.GetUnit().CalculateCurrentJointVelocity()).ToList());
            // Each part's joint acceleration
            data.Add("joint_accelerations", moveableJoints.Select(s => s.GetUnit().CalculateCurrentJointAcceleration()).ToList());
            // Each part's joint force
            data.Add("joint_force", moveableJoints.Select(s => s.GetUnit().CalculateCurrentJointForce()).ToList());
            // Each part's joint type
            data.Add("joint_types", moveableJoints.Select(s => s.jointType.ToString()).ToList());
            // Each part's joint lower limit
            data.Add("joint_lower_limit", moveableJoints.Select(s => s.xDrive.lowerLimit).ToList());
            // Each part's joint upper limit
            data.Add("joint_upper_limit", moveableJoints.Select(s => s.xDrive.upperLimit).ToList());
            // Whether all parts are stable
            data.Add("all_stable", AllStable());
            data.Add("move_done", moveDone);
            data.Add("rotate_done", rotateDone);
#if UNITY_2022_1_OR_NEWER
            if (sendJointInverseDynamicsForce)
            {
                List<float> gravityForces = new List<float>();
                Root.GetJointGravityForces(gravityForces);
                data.Add("gravity_forces", gravityForces);
                List<float> coriolisCentrifugalForces = new List<float>();
                Root.GetJointCoriolisCentrifugalForces(coriolisCentrifugalForces);
                data.Add("coriolis_centrifugal_forces", coriolisCentrifugalForces);
                List<float> driveForces = new List<float>();
                Root.GetDriveForces(driveForces);
                data.Add("drive_forces", driveForces);

                sendJointInverseDynamicsForce = false;
            }
#endif
            return data;
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

        internal bool AllStable()
        {
            //foreach (ArticulationBody joint in joints)
            //{
            //    if (joint.TryGetComponent(out ArticulationUnit unit))
            //        if (unit.GetMovingDirection() != MovingDirection.None)
            //            return false;
            //}
            return true;
        }

        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "SetJointPosition":
                    SetJointPosition(data[0].ConvertType<List<float>>(), data[1].ConvertType<List<float>>());
                    return;
                case "SetJointPositionDirectly":
                    SetJointPositionDirectly(data[0].ConvertType<List<float>>());
                    return;
                case "SetIndexJointPosition":
                    SetIndexJointPosition((int)data[0], (float)data[1]);
                    return;
                case "SetIndexJointPositionDirectly":
                    SetIndexJointPositionDirectly((int)data[0], (float)data[1]);
                    return;
                case "SetJointPositionContinue":
                    StartCoroutine(SetJointPositionContinue((int)data[0], data[1].ConvertType<List<List<float>>>()));
                    return;
                case "SetJointVelocity":
                    SetJointVelocity(data[0].ConvertType<List<float>>());
                    return;
                case "SetIndexJointVelocity":
                    SetIndexJointVelocity((int)data[0], (float)data[1]);
                    return;
                case "AddJointForce":
                    AddJointForce(data[0].ConvertType<List<List<float>>>());
                    return;
                case "AddJointForceAtPosition":
                    AddJointForceAtPosition(data[0].ConvertType<List<List<float>>>(), data[1].ConvertType<List<List<float>>>());
                    return;
                case "AddJointTorque":
                    AddJointTorque(data[0].ConvertType<List<List<float>>>());
                    return;
                case "SetImmovable":
                    SetImmovable((bool)data[0]);
                    return;
                case "GetJointInverseDynamicsForce":
                    GetJointInverseDynamicsForce();
                    return;
                case "MoveForward":
                    MoveForward((float)data[0], (float)data[1]);
                    return;
                case "MoveBack":
                    MoveBack((float)data[0], (float)data[1]);
                    return;
                case "TurnLeft":
                    TurnLeft((float)data[0], (float)data[1]);
                    return;
                case "TurnRight":
                    TurnRight((float)data[0], (float)data[1]);
                    return;
                case "GripperOpen":
                    GripperOpen();
                    return;
                case "GripperClose":
                    GripperClose();
                    return;
                case "EnabledNativeIK":
                    EnabledNativeIK((bool)data[0]);
                    return;
                case "IKTargetDoMove":
                    IKTargetDoMove(data[0].ConvertType<List<float>>(), (float)data[1], (bool)data[2], (bool)data[3]);
                    return;
                case "IKTargetDoRotate":
                    IKTargetDoRotate(data[0].ConvertType<List<float>>(), (float)data[1], (bool)data[2], (bool)data[3]);
                    return;
                case "IKTargetDoRotateQuaternion":
                    IKTargetDoRotateQuaternion(data[0].ConvertType<List<float>>(), (float)data[1], (bool)data[2], (bool)data[3]);
                    return;
                case "IKTargetDoComplete":
                    IKTargetDoComplete();
                    return;
                case "IKTargetDoKill":
                    IKTargetDoKill();
                    return;
                case "SetIKTargetOffset":
                    SetIKTargetOffset(data[0].ConvertType<List<float>>(), data[1].ConvertType<List<float>>(), data[2].ConvertType<List<float>>());
                    return;
            }
            base.AnalysisData(type, data);
        }
        private void SetImmovable(bool immovable)
        {
            ArticulationBody first = GetComponentInChildren<ArticulationBody>();
            if (first.isRoot)
                first.immovable = immovable;
            else
                Debug.LogWarning($"Controller ID:{ID},Name:{Name},is not root ArticulationBody");
        }
        public void EnabledNativeIK(bool enabled)
        {
#if BIOIK
            Debug.Log($"EnabledNativeIK: ID: {ID} {enabled}");
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
        private void IKTargetDoMove(List<float> position, float duration, bool isSpeedBased, bool isRelative)
        {
            Debug.Log("IKTargetDoMove");
            if (iKTarget == null) return;
            moveDone = false;
            Vector3 pos = new Vector3(position[0], position[1], position[2]);
            if (duration == 0)
            {
                if (isRelative)
                    tempIKTargetPosition = iKTarget.transform.position + pos;
                else
                    tempIKTargetPosition = pos;
            }
            else
                iKTarget.DOMove(pos, duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
                {
                    moveDone = true;
                };
        }

        private void IKTargetDoRotate(List<float> eulerAngle, float duration, bool isSpeedBased, bool isRelative)
        {
            if (iKTarget == null) return;
            Quaternion target = Quaternion.Euler(eulerAngle[0], eulerAngle[1], eulerAngle[2]);
            IKTargetDoRotateQuaternion(target, duration, isSpeedBased, isRelative);
        }
        private void IKTargetDoRotateQuaternion(List<float> quaternion, float duration, bool isSpeedBased, bool isRelative)
        {
            if (iKTarget == null) return;
            Quaternion target = new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]);
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
        private void SetIKTargetOffset(List<float> position, List<float> rotation, List<float> quaternion)
        {
            if (iKFollow == null) return;
            iKFollow.localPosition = new Vector3(position[0], position[1], position[2]);
            if (quaternion != null)
                iKFollow.localRotation = new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]);
            else
                iKFollow.localEulerAngles = new Vector3(rotation[0], rotation[1], rotation[2]);
            ResetIKTarget();
        }
        bool sendJointInverseDynamicsForce = false;
        private void GetJointInverseDynamicsForce()
        {
            sendJointInverseDynamicsForce = true;
#if !UNITY_2022_1_OR_NEWER
            Debug.LogWarning($"Controller ID:{ID},Name:{Name}, Current Unity Version dont support GetJointInverseDynamicsForce API");
#endif
        }

        private void MoveForward(float distance, float speed)
        {
            GetComponent<ICustomMove>()?.Forward(distance, speed);
        }
        private void MoveBack(float distance, float speed)
        {
            GetComponent<ICustomMove>()?.Back(distance, speed);
        }
        private void TurnLeft(float angle, float speed)
        {
            GetComponent<ICustomMove>()?.Left(angle, speed);
        }
        private void TurnRight(float angle, float speed)
        {
            GetComponent<ICustomMove>()?.Right(angle, speed);
        }
        public void GripperOpen()
        {
            Debug.Log("GripperOpen");
            GetComponent<ICustomGripper>()?.Open();
        }
        public void GripperClose()
        {
            Debug.Log("GripperClose");
            GetComponent<ICustomGripper>()?.Close();
        }
        public override void SetPosition(Vector3 position, bool worldSpace = true)
        {
            if (worldSpace)
                transform.position = position;
            else
                transform.localPosition = position;
            Root.TeleportRoot(transform.position, transform.rotation);
        }
        public override void SetRotation(Vector3 rotation, bool worldSpace = true)
        {
            if (worldSpace)
                transform.eulerAngles = rotation;
            else
                transform.localEulerAngles = rotation;
            Root.TeleportRoot(transform.position, transform.rotation);
        }

        public override void SetRotationQuaternion(List<float> quaternion, bool worldSpace = true)
        {
            SetRotation(new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]).eulerAngles, worldSpace);
        }
        public override void SetScale(Vector3 scale)
        {
            return;
        }

        public override void Translate(List<float> translate, bool worldSpace)
        {
            return;
        }
        public override void Rotate(List<float> translate, bool worldSpace)
        {
            return;
        }
        public override void LookAt(List<float> target, List<float> worldUp)
        {
            return;
        }
        protected override void SetActive(bool active)
        {
            return;
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
        private void SetJointPosition(List<float> jointPositions, List<float> speedScales)
        {
            if (moveableJoints.Count != jointPositions.Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointPositions.Count, moveableJoints.Count));
                return;
            }
            SetJointPosition(jointPositions, ControlMode.Target, speedScales);
        }
        private void SetJointPositionDirectly(List<float> jointPositions)
        {
            Debug.Log("SetJointPositionDirectly");
            if (moveableJoints.Count != jointPositions.Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointPositions.Count, moveableJoints.Count));
                return;
            }
            SetJointPosition(jointPositions, ControlMode.Direct);
        }
        public void SetIndexJointPosition(int index, float jointPosition)
        {
            if (moveableJoints.Count! > index)
            {
                Debug.LogError($"The index of target joint positions is {index}, but the valid number of joints in robot arm is {moveableJoints.Count}");
                return;
            }
            SetIndexJointPosition(index, jointPosition, ControlMode.Target);
        }
        public void SetIndexJointPositionDirectly(int index, float jointPosition)
        {
            if (moveableJoints.Count! > index)
            {
                Debug.LogError($"The index of target joint positions is {index}, but the valid number of joints in robot arm is {moveableJoints.Count}");
                return;
            }
            SetIndexJointPosition(index, jointPosition, ControlMode.Direct);
        }

        public IEnumerator SetJointPositionContinue(int interval, List<List<float>> jointPositions)
        {
            Debug.Log("SetJointPositionContinue");
            if (jointPositions.Count == 0)
            {
                Debug.LogError("JointPositions Length is 0");
                yield break;
            }
            if (moveableJoints.Count != jointPositions[0].Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointPositions.Count, moveableJoints.Count));
                yield break;
            }
            float startTime = Time.time * 1000;
            for (int i = 0; i < jointPositions.Count; i++)
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
        private void SetJointVelocity(List<float> jointTargetVelocitys)
        {
            Debug.Log("SetJointVelocity");
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().SetJointTargetVelocity(jointTargetVelocitys[i]);
            }
        }
        private void SetIndexJointVelocity(int index, float jointTargetVelocity)
        {
            Debug.Log("SetIndexJointVelocity");
            moveableJoints[index].GetUnit().SetJointTargetVelocity(jointTargetVelocity);
        }
        public void SetJointPosition(List<float> jointTargetPositions, ControlMode mode = ControlMode.Target, List<float> speedScales = null)
        {
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().SetJointTarget(jointTargetPositions[i], mode);
            }
        }
        private void SetIndexJointPosition(int index, float jointPosition, ControlMode mode = ControlMode.Target)
        {
            moveableJoints[index].GetUnit().SetJointTarget(jointPosition, mode);
        }
        private void AddJointForce(List<List<float>> jointForces)
        {
            if (moveableJoints.Count != jointForces.Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointForces.Count, moveableJoints.Count));
                return;
            }
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().AddJointForce(new Vector3(jointForces[i][0], jointForces[i][1], jointForces[i][2]));
            }
        }
        private void AddJointForceAtPosition(List<List<float>> jointForces, List<List<float>> forcesPosition)
        {
            if (moveableJoints.Count != jointForces.Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointForces.Count, moveableJoints.Count));
                return;
            }
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().AddJointForceAtPosition(new Vector3(jointForces[i][0], jointForces[i][1], jointForces[i][2]), new Vector3(forcesPosition[i][0], forcesPosition[i][1], forcesPosition[i][2]));
            }
        }
        private void AddJointTorque(List<List<float>> jointTorques)
        {
            if (moveableJoints.Count != jointTorques.Count)
            {
                Debug.LogError(string.Format("The number of target joint positions is {0}, but the valid number of joints in robot arm is {1}", jointTorques.Count, moveableJoints.Count));
                return;
            }
            for (int i = 0; i < moveableJoints.Count; i++)
            {
                moveableJoints[i].GetUnit().AddJointTorque(new Vector3(jointTorques[i][0], jointTorques[i][1], jointTorques[i][2]));
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
