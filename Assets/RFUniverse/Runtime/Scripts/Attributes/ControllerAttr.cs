using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using DG.Tweening;
using UnityEngine.AddressableAssets;

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
        List<ArticulationBody> joints;
        public List<ArticulationBody> Joints
        {
            get
            {
                if (joints == null)
                    joints = jointParameters.Where(s => s.body != null).Select(s => s.body).ToList();
                return joints;
            }
        }
        [NonSerialized]
        List<ArticulationBody> moveableJoints;
        public List<ArticulationBody> MoveableJoints
        {
            get
            {
                if (moveableJoints == null)
                    moveableJoints = jointParameters.Where(s => s.body != null && s.moveable).Select(s => s.body).ToList();
                for (int i = 0; i < moveableJoints.Count; i++)
                {
                    moveableJoints[i].GetUnit().indexOfMoveableJoints = i;
                }
                return moveableJoints;
            }
        }

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
            if (graspPoint == null) graspPoint = Joints.LastOrDefault()?.transform;
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
                jointParameters.Add(new ArticulationParameter
                {
                    body = item,
                    moveable = item.jointType != ArticulationJointType.FixedJoint && item.GetUnit().mimicParent == null && !item.isRoot,
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
            bioIK.SetGenerations(10);
            bioIK.SetPopulationSize(50);
            bioIK.SetElites(1);
            bioIK.Smoothing = 0;
            foreach (var item in MoveableJoints)
            {
                BioIK.BioJoint joint = bioIK.FindSegment(item.transform).AddJoint();
                iKCopy[item.transform] = item;
                joint.SetAnchor(item.anchorPosition);
                joint.SetOrientation(item.anchorRotation.eulerAngles);
                joint.SetDefaultFrame(item.transform.localPosition, item.transform.localRotation);
                switch (item.jointType)
                {
                    case ArticulationJointType.RevoluteJoint:
                        joint.JointType = BioIK.JointType.Rotational;
                        joint.X.Enabled = true;
                        switch (item.twistLock)
                        {
                            case ArticulationDofLock.FreeMotion:
                                joint.X.Constrained = false;
                                break;
                            case ArticulationDofLock.LimitedMotion:
                                joint.X.Constrained = true;
                                joint.X.SetUpperLimit(item.xDrive.upperLimit);
                                joint.X.SetLowerLimit(item.xDrive.lowerLimit);
                                joint.X.SetTargetValue(item.xDrive.target);
                                break;
                        }
                        joint.Y.Enabled = false;
                        joint.Z.Enabled = false;
                        break;
                    case ArticulationJointType.PrismaticJoint:
                        joint.JointType = BioIK.JointType.Translational;
                        switch (item.linearLockX)
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
                                joint.X.SetUpperLimit(item.xDrive.upperLimit);
                                joint.X.SetLowerLimit(item.xDrive.lowerLimit);
                                joint.X.SetTargetValue(item.xDrive.target);
                                break;
                        }
                        switch (item.linearLockY)
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
                                joint.Y.SetUpperLimit(item.yDrive.upperLimit);
                                joint.Y.SetLowerLimit(item.yDrive.lowerLimit);
                                joint.Y.SetTargetValue(item.yDrive.target);
                                break;
                        }
                        switch (item.linearLockZ)
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
                                joint.Z.SetUpperLimit(item.zDrive.upperLimit);
                                joint.Z.SetLowerLimit(item.zDrive.lowerLimit);
                                joint.Z.SetTargetValue(item.zDrive.target);
                                break;
                        }
                        break;
                    case ArticulationJointType.SphericalJoint:
                        joint.JointType = BioIK.JointType.Rotational;
                        switch (item.twistLock)
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
                                joint.X.SetUpperLimit(item.xDrive.upperLimit);
                                joint.X.SetLowerLimit(item.xDrive.lowerLimit);
                                joint.X.SetTargetValue(item.xDrive.target);
                                break;
                        }
                        switch (item.swingYLock)
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
                                joint.Y.SetUpperLimit(item.yDrive.upperLimit);
                                joint.Y.SetLowerLimit(item.yDrive.lowerLimit);
                                joint.Y.SetTargetValue(item.yDrive.target);
                                break;
                        }
                        switch (item.swingZLock)
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
                                joint.Z.SetUpperLimit(item.zDrive.upperLimit);
                                joint.Z.SetLowerLimit(item.zDrive.lowerLimit);
                                joint.Z.SetTargetValue(item.zDrive.target);
                                break;
                        }
                        break;
                }
            }

            if (iKFollow != null)
            {
                BioIK.BioSegment segment = bioIK.FindSegment(iKFollow);
                segment.Objectives = new BioIK.BioObjective[] { };
                BioIK.BioObjective positionObjective = segment.AddObjective(BioIK.ObjectiveType.Position);
                ((BioIK.Position)positionObjective).SetTargetTransform(iKTarget);
                if (iKTargetOrientation)
                {
                    BioIK.BioObjective orientationObjective = segment.AddObjective(BioIK.ObjectiveType.Orientation);
                    ((BioIK.Orientation)orientationObjective).SetTargetTransform(iKTarget);
                }
            }
            bioIK.Refresh();
#endif
        }

        void ResetIKTarget()
        {
            if (iKFollow != null && iKTarget != null)
            {
                iKTarget.position = iKFollow.position;
                iKTarget.rotation = iKFollow.rotation;
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

        public override void AddPermanentData(Dictionary<string, object> data)
        {
            base.AddPermanentData(data);
            // Number of Articulation Joints
            data["number_of_joints"] = Joints.Count;

            data["names"] = Joints.Select(s => s.GetUnit().jointName).ToList();
            // Each part's joint type
            data["types"] = Joints.Select(s => s.jointType.ToString()).ToList();
            // Position
            data["positions"] = Joints.Select(s => s.transform.position).ToList();
            // Rotation
            data["rotations"] = Joints.Select(s => s.transform.eulerAngles).ToList();
            // Quaternion
            data["quaternions"] = Joints.Select(s => s.transform.rotation).ToList();
            // LocalPosition
            data["local_positions"] = Joints.Select(s => s.transform.localPosition).ToList();
            // LocalRotation
            data["local_rotations"] = Joints.Select(s => s.transform.localEulerAngles).ToList();
            // LocalQuaternion
            data["local_quaternions"] = Joints.Select(s => s.transform.localRotation).ToList();
            // Velocity
            data["velocities"] = Joints.Select(s => s.velocity).ToList();
            // AngularVelocity
            data["angular_velocities"] = Joints.Select(s => s.angularVelocity).ToList();

            // Number of Articulation Moveable Joints
            data["number_of_moveable_joints"] = MoveableJoints.Count;
            // Each part's joint position
            data["joint_positions"] = MoveableJoints.Select(s => s.GetUnit().CalculateCurrentJointPosition()).ToList();
            // Each part's joint velocity
            data["joint_velocities"] = MoveableJoints.Select(s => s.GetUnit().CalculateCurrentJointVelocity()).ToList();
            // Each part's joint acceleration
            data["joint_accelerations"] = MoveableJoints.Select(s => s.GetUnit().CalculateCurrentJointAcceleration()).ToList();
            // Each part's joint force
            data["joint_force"] = MoveableJoints.Select(s => s.jointForce[0]).ToList();
            // Each part's joint lower limit
            data["joint_lower_limit"] = MoveableJoints.Select(s => s.xDrive.lowerLimit).ToList();
            // Each part's joint upper limit
            data["joint_upper_limit"] = MoveableJoints.Select(s => s.xDrive.upperLimit).ToList();
            // Each part's joint upper limit
            data["joint_stiffness"] = MoveableJoints.Select(s => s.xDrive.stiffness).ToList();
            // Each part's joint upper limit
            data["joint_damping"] = MoveableJoints.Select(s => s.xDrive.damping).ToList();
        }

        public List<float> GetJointPositions()
        {
            List<float> jointPositions = new List<float>();
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                jointPositions.Add(MoveableJoints[i].GetUnit().CalculateCurrentJointPosition());
            }
            return jointPositions;
        }



        [RFUAPI]
        public void SetImmovable(bool immovable)
        {
            ArticulationBody first = GetComponentInChildren<ArticulationBody>();
            if (first.isRoot)
                first.immovable = immovable;
            else
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, Is not root ArticulationBody");
        }
        [RFUAPI]
        public void EnabledNativeIK(bool enabled)
        {
#if BIOIK
            Debug.Log($"EnabledNativeIK: ID: {ID} {enabled}");
            if (bioIK == null)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, Dont have IK compenent");
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

        [RFUAPI]
        public void IKTargetDoMove(List<float> position, float duration, bool isSpeedBased, bool isRelative)
        {
#if BIOIK
            if (bioIK == null)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, Dont have IK compenent");
                return;
            }
            if (!bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need to open NativeIK");
                return;
            }
#endif
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
            {
                iKTarget.DOMove(pos, duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
                {
                    moveDone = true;
                };
            }
        }

        [RFUAPI]
        public void IKTargetDoRotate(List<float> eulerAngle, float duration, bool isSpeedBased, bool isRelative)
        {
            if (iKTarget == null) return;
            Quaternion target = Quaternion.Euler(eulerAngle[0], eulerAngle[1], eulerAngle[2]);
            IKTargetDoRotateQuaternion(target, duration, isSpeedBased, isRelative);
        }
        [RFUAPI]
        public void IKTargetDoRotateQuaternion(List<float> quaternion, float duration, bool isSpeedBased, bool isRelative)
        {
            if (iKTarget == null) return;
            Quaternion target = new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]);
            IKTargetDoRotateQuaternion(target, duration, isSpeedBased, isRelative);
        }
        private void IKTargetDoRotateQuaternion(Quaternion target, float duration, bool isSpeedBased = true, bool isRelative = false)
        {
#if BIOIK
            if (bioIK == null)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, Dont have IK compenent");
                return;
            }
            if (!bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need open NativeIK");
                return;
            }
#endif
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
            {
                iKTarget.DORotateQuaternion(target, duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
                {
                    rotateDone = true;
                };
            }
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
            //foreach (var item in MoveableJoints)
            //{
            //    BioIK.BioJoint joint = bioIK.FindSegment(item.transform).AddJoint();
            //    joint.SetDefaultFrame(item.transform.localPosition, item.transform.localRotation);
            //}
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
        [RFUAPI]
        private void IKTargetDoComplete()
        {
            if (iKTarget == null) return;
            iKTarget.DOComplete();
            moveDone = true;
            rotateDone = true;
        }
        [RFUAPI]
        private void IKTargetDoKill()
        {
            if (iKTarget == null) return;
            iKTarget.DOKill();
            moveDone = true;
            rotateDone = true;
        }

        [RFUAPI]
        public void GetIKTargetJointPosition(List<float> position = null, List<float> eulerAngle = null, List<float> quaternion = null, int iterate = 100)
        {
#if BIOIK
            if (bioIK == null)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, Dont have IK compenent");
                return;
            }
            if (!bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need open NativeIK");
                return;
            }
            if (iKTarget == null) return;
            Vector3 sourcePosition = iKTarget.transform.position;
            Quaternion sourceQuaternion = iKTarget.transform.rotation;
            if (position != null && position.Count == 3)
                iKTarget.transform.position = RFUniverseUtility.ListFloatToVector3(position);
            if (eulerAngle != null && eulerAngle.Count == 3)
                iKTarget.transform.eulerAngles = RFUniverseUtility.ListFloatToVector3(position);
            if (quaternion != null && quaternion.Count == 4)
                iKTarget.transform.rotation = RFUniverseUtility.ListFloatToQuaternion(quaternion);
            for (int i = 0; i < iterate; i++)
                bioIK.FixedUpdate1();
            List<float> result = MoveableJoints.Select(s => bioIK.targets[s.transform]).ToList();
            CollectData.AddDataNextStep("result_joint_position", result);

            iKTarget.transform.position = sourcePosition;
            iKTarget.transform.rotation = sourceQuaternion;
#endif
        }

        protected override void DoMove(List<float> position, float duration, bool isSpeedBased, bool isRelative)
        {
            return;
        }

        protected override void DoRotate(List<float> eulerAngle, float duration, bool isSpeedBased, bool isRelative)
        {
            return;
        }
        protected override void DoRotateQuaternion(List<float> quaternion, float duration, bool isSpeedBased, bool isRelative)
        {
            return;
        }
        protected override void DoRotateQuaternion(Quaternion target, float duration, bool isSpeedBased = true, bool isRelative = false)
        {
            return;
        }
        protected override void DoComplete()
        {
            return;
        }
        protected override void DoKill()
        {
            return;
        }
        [RFUAPI]
        public void SetIKTargetOffset(List<float> position = null, List<float> eulerAngle = null, List<float> quaternion = null)
        {
            if (iKFollow == null) return;
            if (position != null && position.Count == 3)
                iKFollow.localPosition = RFUniverseUtility.ListFloatToVector3(position);
            if (eulerAngle != null && eulerAngle.Count == 3)
                iKFollow.localEulerAngles = RFUniverseUtility.ListFloatToVector3(eulerAngle);
            if (quaternion != null && quaternion.Count == 4)
                iKFollow.localRotation = RFUniverseUtility.ListFloatToQuaternion(quaternion);
            ResetIKTarget();
        }
        [RFUAPI]
        private void GetJointInverseDynamicsForce()
        {
#if UNITY_2022_1_OR_NEWER
            List<float> gravityForces = new List<float>();
            Root.GetJointGravityForces(gravityForces);
            CollectData.AddDataNextStep("gravity_forces", gravityForces);
            List<float> coriolisCentrifugalForces = new List<float>();
            Root.GetJointCoriolisCentrifugalForces(coriolisCentrifugalForces);
            CollectData.AddDataNextStep("coriolis_centrifugal_forces", coriolisCentrifugalForces);
            List<float> driveForces = new List<float>();
            Root.GetDriveForces(driveForces);
            CollectData.AddDataNextStep("drive_forces", driveForces);

#else
        Debug.LogWarning($"Controller ID:{ID},Name:{Name}, Current Unity Version dont support GetJointInverseDynamicsForce API");
#endif
        }
        [RFUAPI]
        private void MoveForward(float distance, float speed)
        {
            GetComponent<ICustomMove>()?.Forward(distance, speed);
        }
        [RFUAPI]
        private void MoveBack(float distance, float speed)
        {
            GetComponent<ICustomMove>()?.Back(distance, speed);
        }
        [RFUAPI]
        private void TurnLeft(float angle, float speed)
        {
            GetComponent<ICustomMove>()?.Left(angle, speed);
        }
        [RFUAPI]
        private void TurnRight(float angle, float speed)
        {
            GetComponent<ICustomMove>()?.Right(angle, speed);
        }
        [RFUAPI]
        public void GripperOpen()
        {
            GetComponent<ICustomGripper>()?.Open();
        }
        [RFUAPI]
        public void GripperClose()
        {
            GetComponent<ICustomGripper>()?.Close();
        }
        [RFUAPI]
        public void GripperOpenDirectly()
        {
            GetComponent<ICustomGripper>()?.OpenDirectly();
        }
        [RFUAPI]
        public void GripperCloseDirectly()
        {
            GetComponent<ICustomGripper>()?.CloseDirectly();
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
        [RFUAPI]
        public void SetJointPosition(List<float> jointPositions)
        {
#if BIOIK
            if (bioIK != null && bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need to close NativeIK");
                return;
            }
#endif
            if (MoveableJoints.Count != jointPositions.Count)
            {
                Debug.LogError($"The number of target joint is {jointPositions.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            SetJointPosition(jointPositions, ControlMode.Target);
        }
        [RFUAPI]
        public void SetJointPositionDirectly(List<float> jointPositions)
        {
#if BIOIK
            if (bioIK != null && bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need to close NativeIK");
                return;
            }
#endif
            if (MoveableJoints.Count != jointPositions.Count)
            {
                Debug.LogError($"The number of target joint is {jointPositions.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            SetJointPosition(jointPositions, ControlMode.Direct);
        }
        [RFUAPI]
        public void SetIndexJointPosition(int index, float jointPosition)
        {
#if BIOIK
            if (bioIK != null && bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need to close NativeIK");
                return;
            }
#endif
            if (index >= MoveableJoints.Count)
            {
                Debug.LogError($"The index of target joint is {index}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            SetIndexJointPosition(index, jointPosition, ControlMode.Target);
        }
        [RFUAPI]
        public void SetIndexJointPositionDirectly(int index, float jointPosition)
        {
#if BIOIK
            if (bioIK != null && bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need to close NativeIK");
                return;
            }
#endif
            if (index >= MoveableJoints.Count)
            {
                Debug.LogError($"The index of target joint is {index}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            SetIndexJointPosition(index, jointPosition, ControlMode.Direct);
        }
        [RFUAPI]
        public void SetJointPositionContinue(int interval, List<List<float>> jointPositions)
        {
            StartCoroutine(StartJointPositionContinue(interval, jointPositions));
        }
        public IEnumerator StartJointPositionContinue(int interval, List<List<float>> jointPositions)
        {
#if BIOIK
            if (bioIK != null && bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need to close NativeIK");
                yield break;
            }
#endif
            if (jointPositions.Count == 0)
            {
                Debug.LogError("JointPositions Length is 0");
                yield break;
            }
            if (MoveableJoints.Count != jointPositions[0].Count)
            {
                Debug.LogError($"The number of target joint is {jointPositions.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
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


        [RFUAPI]
        public void SetJointDriveForce(List<float> jointDriveForce)
        {
            if (MoveableJoints.Count != jointDriveForce.Count)
            {
                Debug.LogError($"The number of target joint is {jointDriveForce.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].GetUnit().SetJointForce(jointDriveForce[i]);
            }
        }

        [RFUAPI]
        public void SetJointDamping(List<float> jointDamping)
        {
            if (MoveableJoints.Count != jointDamping.Count)
            {
                Debug.LogError($"The number of target joint is {jointDamping.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].GetUnit().SetJointDamping(jointDamping[i]);
            }
        }


        [RFUAPI]
        public void SetJointStiffness(List<float> jointStiffness)
        {
            if (MoveableJoints.Count != jointStiffness.Count)
            {
                Debug.LogError($"The number of target joint is {jointStiffness.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].GetUnit().SetJointStiffness(jointStiffness[i]);
            }
        }
        [RFUAPI]
        public void SetJointLimit(List<float> upper, List<float> lower)
        {
            if (MoveableJoints.Count != upper.Count)
            {
                Debug.LogError($"The number of target joint is {upper.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            if (MoveableJoints.Count != lower.Count)
            {
                Debug.LogError($"The number of target joint is {lower.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].GetUnit().SetJointLimit(upper[i], lower[i]);
            }
        }
        [RFUAPI]
        public void SetJointVelocity(List<float> jointTargetVelocitys)
        {
#if BIOIK
            if (bioIK != null && bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need to close NativeIK");
                return;
            }
#endif
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].GetUnit().SetJointTargetVelocity(jointTargetVelocitys[i]);
            }
        }
        [RFUAPI]
        public void SetIndexJointVelocity(int index, float jointTargetVelocity)
        {
#if BIOIK
            if (bioIK != null && bioIK.enabled)
            {
                Debug.LogWarning($"Controller ID: {ID},Name: {Name}, You need to close NativeIK");
                return;
            }
#endif
            MoveableJoints[index].GetUnit().SetJointTargetVelocity(jointTargetVelocity);
        }
        public void SetJointPosition(List<float> jointTargetPositions, ControlMode mode = ControlMode.Target)
        {
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].GetUnit().SetJointTarget(jointTargetPositions[i], mode);
            }
        }
        private void SetIndexJointPosition(int index, float jointPosition, ControlMode mode = ControlMode.Target)
        {
            MoveableJoints[index].GetUnit().SetJointTarget(jointPosition, mode);
        }
        [RFUAPI]
        public void SetJointUseGravity(bool useGravity)
        {
            foreach (var item in Joints)
            {
                item.useGravity = useGravity;
            }
        }
        [RFUAPI]
        public void AddJointForce(List<List<float>> jointForces)
        {
            if (MoveableJoints.Count != jointForces.Count)
            {
                Debug.LogError($"The number of target joint is {jointForces.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].AddForce(new Vector3(jointForces[i][0], jointForces[i][1], jointForces[i][2]));
            }
        }
        [RFUAPI]
        public void AddJointForceAtPosition(List<List<float>> jointForces, List<List<float>> forcesPosition)
        {
            if (MoveableJoints.Count != jointForces.Count)
            {
                Debug.LogError($"The number of target joint is {jointForces.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].AddForceAtPosition(new Vector3(jointForces[i][0], jointForces[i][1], jointForces[i][2]), new Vector3(forcesPosition[i][0], forcesPosition[i][1], forcesPosition[i][2]));
            }
        }
        [RFUAPI]
        public void AddJointTorque(List<List<float>> jointTorques)
        {
            if (MoveableJoints.Count != jointTorques.Count)
            {
                Debug.LogError($"The number of target joint is {jointTorques.Count}, but the valid number of joints in robot arm is {MoveableJoints.Count}");
                return;
            }
            for (int i = 0; i < MoveableJoints.Count; i++)
            {
                MoveableJoints[i].AddTorque(new Vector3(jointTorques[i][0], jointTorques[i][1], jointTorques[i][2]));
            }
        }
        [RFUAPI]
        public void GetJointLocalPointFromWorld(int jointIndex, List<float> world)
        {
            if (jointIndex >= joints.Count)
            {
                Debug.LogError($"The index of target joint is {jointIndex}, but the valid number of joints in robot arm is {joints.Count}");
                return;
            }
            CollectData.AddDataNextStep("result_joint_local_point", joints[jointIndex].transform.InverseTransformPoint(RFUniverseUtility.ListFloatToVector3(world)));
        }

        [RFUAPI]
        public void GetJointWorldPointFromLocal(int jointIndex, List<float> local)
        {
            if (jointIndex >= joints.Count)
            {
                Debug.LogError($"The index of target joint is {jointIndex}, but the valid number of joints in robot arm is {joints.Count}");
                return;
            }
            CollectData.AddDataNextStep("result_joint_world_point", joints[jointIndex].transform.TransformPoint(RFUniverseUtility.ListFloatToVector3(local)));
        }

        [RFUAPI]
        public void AddRoot6DOF(int newID)
        {
            ControllerAttr root = Addressables.LoadAssetAsync<GameObject>("Root6DOF").WaitForCompletion().GetComponent<ControllerAttr>();
            root = Instantiate(root);
            root.SetPosition(transform.position);
            root.SetRotation(transform.eulerAngles);
            root.ID = newID;
            root.Instance();
            transform.SetParent(root.joints.Last().transform);
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
            if (GUILayout.Button(script.Root.useGravity ? "Disable Gravity" : "Enable Gravity"))
            {
                script.SetJointUseGravity(!script.Root.useGravity);
            }

            // if (GUILayout.Button("Add BioIK"))
            // {
            //     script.InitBioIK();
            // }
        }
    }
#endif
}
