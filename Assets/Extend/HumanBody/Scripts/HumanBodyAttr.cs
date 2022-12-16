using System.IO;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Attributes;
using UnityEditor;
using DG.Tweening;

[Serializable]
public struct Bones
{
    public Transform Pelvis;
    public Transform Spine1;
    public Transform Spine2;
    public Transform Spine3;
    public Transform LeftShoulder;
    public Transform LeftUpperArm;
    public Transform LeftLowerArm;
    public Transform LeftHand;
    public Transform RightShoulder;
    public Transform RightUpperArm;
    public Transform RightLowerArm;
    public Transform RightHand;
    public Transform LeftUpperLeg;
    public Transform LeftLowerLeg;
    public Transform LeftFoot;
    public Transform LeftToes;
    public Transform RightUpperLeg;
    public Transform RightLowerLeg;
    public Transform RightFoot;
    public Transform RightToes;
    public Transform Neck;
    public Transform Head;
    public Transform LeftEye;
    public Transform RightEye;
    public Transform Jaw;
    public Transform LeftThumb1;
    public Transform LeftThumb2;
    public Transform LeftThumb3;
    public Transform LeftIndex1;
    public Transform LeftIndex2;
    public Transform LeftIndex3;
    public Transform LeftMiddle1;
    public Transform LeftMiddle2;
    public Transform LeftMiddle3;
    public Transform LeftRing1;
    public Transform LeftRing2;
    public Transform LeftRing3;
    public Transform LeftPinky1;
    public Transform LeftPinky2;
    public Transform LeftPinky3;
    public Transform RightThumb1;
    public Transform RightThumb2;
    public Transform RightThumb3;
    public Transform RightIndex1;
    public Transform RightIndex2;
    public Transform RightIndex3;
    public Transform RightMiddle1;
    public Transform RightMiddle2;
    public Transform RightMiddle3;
    public Transform RightRing1;
    public Transform RightRing2;
    public Transform RightRing3;
    public Transform RightPinky1;
    public Transform RightPinky2;
    public Transform RightPinky3;
}

public class HumanBodyAttr : BaseAttr
{
    public SkinnedMeshRenderer skin;
    public Transform root;
    public Bones bones;

    public override string Type
    {
        get { return "HumanBody"; }
    }
    [HideInInspector]
    public List<Transform> ikTargets = new List<Transform>();
    public override void Init()
    {
        base.Init();
    }

    static string humanBodyBioIKLimitPath = $"{UnityEngine.Application.streamingAssetsPath}/HumanBodyBioIKLimit.json";
#if BIOIK
    public BioIK.BioIK bioIK;
    public class BioIKMotion
    {
        public bool Enabled;
        public bool Constrained;
        public double UpperLimit;
        public double LowerLimit;
        public BioIKMotion() { }
        public BioIKMotion(BioIK.BioJoint.Motion motion)
        {
            Enabled = motion.Enabled;
            Constrained = motion.Constrained;
            UpperLimit = motion.UpperLimit;
            LowerLimit = motion.LowerLimit;
        }
    }
    public void InitBioIK()
    {
        bioIK = root.GetComponent<BioIK.BioIK>() ?? root.gameObject.AddComponent<BioIK.BioIK>();
        bioIK.SetGenerations(3);
        bioIK.SetPopulationSize(50);
        bioIK.SetElites(1);
        bioIK.Smoothing = 0.99f;
        bioIK.Refresh();
        if (!File.Exists(humanBodyBioIKLimitPath)) return;
        var boneLimits = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tuple<string, BioIKMotion, BioIKMotion, BioIKMotion>>>(File.ReadAllText(humanBodyBioIKLimitPath));
        foreach (var item in boneLimits)
        {
            Transform trans = (Transform)bones.GetType().GetField(item.Item1).GetValue(bones);
            if (trans == null)
            {
                Debug.Log($" Dont have Bone:{item.Item1}");
                continue;
            }
            BioIK.BioJoint joint = bioIK.FindSegment(trans).Joint;
            if (joint == null)
                joint = bioIK.FindSegment(trans).AddJoint();
            joint.X.Enabled = item.Item2.Enabled;
            joint.X.Constrained = item.Item2.Constrained;
            joint.X.UpperLimit = item.Item2.UpperLimit;
            joint.X.LowerLimit = item.Item2.LowerLimit;
            joint.Y.Enabled = item.Item3.Enabled;
            joint.Y.Constrained = item.Item3.Constrained;
            joint.Y.UpperLimit = item.Item3.UpperLimit;
            joint.Y.LowerLimit = item.Item3.LowerLimit;
            joint.Z.Enabled = item.Item4.Enabled;
            joint.Z.Constrained = item.Item4.Constrained;
            joint.Z.UpperLimit = item.Item4.UpperLimit;
            joint.Z.LowerLimit = item.Item4.LowerLimit;
        }
        ikTargets.Clear();
        if (bones.LeftHand != null)
            ikTargets.Add(InitIKTarget(bones.LeftHand));
        if (bones.RightHand != null)
            ikTargets.Add(InitIKTarget(bones.RightHand));
        if (bones.LeftFoot != null)
            ikTargets.Add(InitIKTarget(bones.LeftFoot));
        if (bones.RightFoot != null)
            ikTargets.Add(InitIKTarget(bones.RightFoot));
        if (bones.Head != null)
            ikTargets.Add(InitIKTarget(bones.Head));
        // if (bones.Pelvis != null)
        //     InitIKTarget(bones.Pelvis);
        bioIK.Refresh();
    }
    Transform InitIKTarget(Transform end)
    {
        BioIK.BioSegment segment = bioIK.FindSegment(end);
        segment.Objectives = new BioIK.BioObjective[] { };
        Transform iKTarget = new GameObject("IKTarget").transform;
        iKTarget.parent = root.parent.transform;
        iKTarget.position = end.position;
        iKTarget.rotation = end.rotation;
        BioIK.BioObjective positionObjective = segment.AddObjective(BioIK.ObjectiveType.Position);
        ((BioIK.Position)positionObjective).SetTargetTransform(iKTarget);
        BioIK.BioObjective orientationObjective = segment.AddObjective(BioIK.ObjectiveType.Orientation);
        ((BioIK.Orientation)orientationObjective).SetTargetTransform(iKTarget);
        return iKTarget;
    }
    public void SaveBioIK()
    {
        BioIK.BioIK bioIK = GetComponentInChildren<BioIK.BioIK>();
        if (bioIK == null) return;
        var boneLimits = new List<Tuple<string, BioIKMotion, BioIKMotion, BioIKMotion>>();
        foreach (FieldInfo fieldInfo in bones.GetType().GetFields())
        {
            if (fieldInfo.GetValue(bones) is Transform)
            {
                Transform trans = (Transform)fieldInfo.GetValue(bones);
                if (trans == null) continue;
                BioIK.BioJoint joint = bioIK.FindSegment((Transform)fieldInfo.GetValue(bones)).Joint;
                if (joint == null)
                {
                    Debug.Log($"Bone:{fieldInfo.Name} dont have joint");
                    continue;
                }
                boneLimits.Add(new Tuple<string, BioIKMotion, BioIKMotion, BioIKMotion>(fieldInfo.Name, new BioIKMotion(joint.X), new BioIKMotion(joint.Y), new BioIKMotion(joint.Z)));
            }
        }
        File.WriteAllText($"{UnityEngine.Application.streamingAssetsPath}/HumanBodyBioIKLimit.json", Newtonsoft.Json.JsonConvert.SerializeObject(boneLimits));
    }
#endif

    public static string GetVarName(System.Linq.Expressions.Expression<Func<string, string>> exp)
    {
        return ((System.Linq.Expressions.MemberExpression)exp.Body).Member.Name;
    }
    public override void CollectData(OutgoingMessage msg)
    {
        base.CollectData(msg);
        msg.WriteBoolean(moveDone);
        msg.WriteBoolean(rotateDone);
    }
    public override void AnalysisMsg(IncomingMessage msg, string type)
    {
        switch (type)
        {
            case "HumanIKTargetDoMove":
                HumanIKTargetDoMove(msg);
                return;
            case "HumanIKTargetDoRotate":
                HumanIKTargetDoRotate(msg);
                return;
            case "HumanIKTargetDoRotateQuaternion":
                HumanIKTargetDoRotateQuaternion(msg);
                return;
            case "HumanIKTargetDoComplete":
                HumanIKTargetDoComplete(msg);
                return;
            case "HumanIKTargetDoKill":
                HumanIKTargetDoKill(msg);
                return;
        }
        base.AnalysisMsg(msg, type);
    }
    bool moveDone;
    private void HumanIKTargetDoMove(IncomingMessage msg)
    {
        Debug.Log("HumanIKTargetDoMove");
        int index = msg.ReadInt32();
        if (ikTargets.Count <= index) return;
        Transform iKTarget = ikTargets[index];
        moveDone = false;
        float x = msg.ReadFloat32();
        float y = msg.ReadFloat32();
        float z = msg.ReadFloat32();
        float duration = msg.ReadFloat32();
        bool isSpeedBased = msg.ReadBoolean();
        bool isRelative = msg.ReadBoolean();
        Debug.Log(iKTarget.name);
        iKTarget.DOMove(new Vector3(x, y, z), duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
        {
            moveDone = true;
        };
    }
    bool rotateDone;
    private void HumanIKTargetDoRotate(IncomingMessage msg)
    {
        Debug.Log("HumanIKTargetDoRotate");
        int index = msg.ReadInt32();
        if (ikTargets.Count <= index) return;
        Transform iKTarget = ikTargets[index];
        rotateDone = false;
        float x = msg.ReadFloat32();
        float y = msg.ReadFloat32();
        float z = msg.ReadFloat32();
        float duration = msg.ReadFloat32();
        bool isSpeedBased = msg.ReadBoolean();
        bool isRelative = msg.ReadBoolean();
        iKTarget.DORotate(new Vector3(x, y, z), duration).SetSpeedBased(isSpeedBased).SetEase(Ease.Linear).SetRelative(isRelative).onComplete += () =>
        {
            rotateDone = true;
        };
    }
    private void HumanIKTargetDoRotateQuaternion(IncomingMessage msg)
    {
        Debug.Log("HumanIKTargetDoRotateQuaternion");
        int index = msg.ReadInt32();
        if (ikTargets.Count <= index) return;
        Transform iKTarget = ikTargets[index];
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
    private void HumanIKTargetDoComplete(IncomingMessage msg)
    {
        int index = msg.ReadInt32();
        if (ikTargets.Count <= index) return;
        Transform iKTarget = ikTargets[index];
        iKTarget.DOComplete();
    }
    private void HumanIKTargetDoKill(IncomingMessage msg)
    {
        int index = msg.ReadInt32();
        if (ikTargets.Count <= index) return;
        Transform iKTarget = ikTargets[index];
        iKTarget.DOKill();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HumanBodyAttr), true)]
public class HumanBodyAttrEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        HumanBodyAttr script = target as HumanBodyAttr;
        if (GUILayout.Button("Refresh All Bones"))
        {
            if (script.root == null)
            {
                Debug.LogWarning("root is null");
                return;
            }
            //Body
            Transform[] bones = script.root.GetComponentsInChildren<Transform>();
            script.bones.Pelvis = bones.FirstOrDefault((s) => s.name.Contains("pelvis", StringComparison.OrdinalIgnoreCase));
            script.bones.Spine1 = bones.FirstOrDefault((s) => s.name.Contains("spine", StringComparison.OrdinalIgnoreCase));
            script.bones.Spine2 = bones.FirstOrDefault((s) => s != script.bones.Spine1 && s.name.Contains("spine", StringComparison.OrdinalIgnoreCase));
            script.bones.Spine3 = bones.FirstOrDefault((s) => s != script.bones.Spine1 && script.bones.Spine2 && s.name.Contains("spine", StringComparison.OrdinalIgnoreCase));
            script.bones.Neck = bones.FirstOrDefault((s) => s.name.Contains("neck", StringComparison.OrdinalIgnoreCase));
            script.bones.Head = bones.FirstOrDefault((s) => s.name.Contains("head", StringComparison.OrdinalIgnoreCase));
            script.bones.LeftEye = bones.FirstOrDefault((s) => s.name.Contains("left", StringComparison.OrdinalIgnoreCase) && s.name.Contains("eye", StringComparison.OrdinalIgnoreCase));
            script.bones.RightEye = bones.FirstOrDefault((s) => s.name.Contains("right", StringComparison.OrdinalIgnoreCase) && s.name.Contains("eye", StringComparison.OrdinalIgnoreCase));
            script.bones.Jaw = bones.FirstOrDefault((s) => s.name.Contains("jaw", StringComparison.OrdinalIgnoreCase));
            //Left
            script.bones.LeftHand = bones.FirstOrDefault((s) => s.name.Contains("left", StringComparison.OrdinalIgnoreCase) && (s.name.Contains("hand", StringComparison.OrdinalIgnoreCase) || s.name.Contains("wrist", StringComparison.OrdinalIgnoreCase)));
            script.bones.LeftLowerArm = script.bones.LeftHand?.parent;
            script.bones.LeftUpperArm = script.bones.LeftLowerArm?.parent;
            script.bones.LeftShoulder = script.bones.LeftUpperArm?.parent;

            script.bones.LeftThumb1 = bones.FirstOrDefault((s) => s.name.Contains("thumb", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.LeftHand);
            script.bones.LeftThumb2 = script.bones.LeftThumb1?.childCount > 0 ? script.bones.LeftThumb1?.GetChild(0) : null;
            script.bones.LeftThumb3 = script.bones.LeftThumb2?.childCount > 0 ? script.bones.LeftThumb2?.GetChild(0) : null;
            script.bones.LeftIndex1 = bones.FirstOrDefault((s) => s.name.Contains("index", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.LeftHand);
            script.bones.LeftIndex2 = script.bones.LeftIndex1?.childCount > 0 ? script.bones.LeftIndex1?.GetChild(0) : null;
            script.bones.LeftIndex3 = script.bones.LeftIndex2?.childCount > 0 ? script.bones.LeftIndex2?.GetChild(0) : null;
            script.bones.LeftMiddle1 = bones.FirstOrDefault((s) => s.name.Contains("middle", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.LeftHand);
            script.bones.LeftMiddle2 = script.bones.LeftMiddle1?.childCount > 0 ? script.bones.LeftMiddle1?.GetChild(0) : null;
            script.bones.LeftMiddle3 = script.bones.LeftMiddle2?.childCount > 0 ? script.bones.LeftMiddle2?.GetChild(0) : null;
            script.bones.LeftRing1 = bones.FirstOrDefault((s) => s.name.Contains("ring", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.LeftHand);
            script.bones.LeftRing2 = script.bones.LeftRing1?.childCount > 0 ? script.bones.LeftRing1?.GetChild(0) : null;
            script.bones.LeftRing3 = script.bones.LeftRing2?.childCount > 0 ? script.bones.LeftRing2?.GetChild(0) : null;
            script.bones.LeftPinky1 = bones.FirstOrDefault((s) => s.name.Contains("pinky", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.LeftHand);
            script.bones.LeftPinky2 = script.bones.LeftPinky1?.childCount > 0 ? script.bones.LeftPinky1?.GetChild(0) : null;
            script.bones.LeftPinky3 = script.bones.LeftPinky2?.childCount > 0 ? script.bones.LeftPinky2?.GetChild(0) : null;

            script.bones.LeftUpperLeg = bones.FirstOrDefault((s) => s.name.Contains("left", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.Pelvis);
            script.bones.LeftLowerLeg = script.bones.LeftUpperLeg?.childCount > 0 ? script.bones.LeftUpperLeg?.GetChild(0) : null;
            script.bones.LeftFoot = script.bones.LeftLowerLeg?.childCount > 0 ? script.bones.LeftLowerLeg?.GetChild(0) : null;
            script.bones.LeftToes = script.bones.LeftFoot?.childCount > 0 ? script.bones.LeftFoot?.GetChild(0) : null;
            //Right
            script.bones.RightHand = bones.FirstOrDefault((s) => s.name.Contains("right", StringComparison.OrdinalIgnoreCase) && (s.name.Contains("hand", StringComparison.OrdinalIgnoreCase) || s.name.Contains("wrist", StringComparison.OrdinalIgnoreCase)));
            script.bones.RightLowerArm = script.bones.RightHand?.parent;
            script.bones.RightUpperArm = script.bones.RightLowerArm?.parent;
            script.bones.RightShoulder = script.bones.RightUpperArm?.parent;

            script.bones.RightThumb1 = bones.FirstOrDefault((s) => s.name.Contains("thumb", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.RightHand);
            script.bones.RightThumb2 = script.bones.RightThumb1?.childCount > 0 ? script.bones.RightThumb1?.GetChild(0) : null;
            script.bones.RightThumb3 = script.bones.RightThumb2?.childCount > 0 ? script.bones.RightThumb2?.GetChild(0) : null;
            script.bones.RightIndex1 = bones.FirstOrDefault((s) => s.name.Contains("index", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.RightHand);
            script.bones.RightIndex2 = script.bones.RightIndex1?.childCount > 0 ? script.bones.RightIndex1?.GetChild(0) : null;
            script.bones.RightIndex3 = script.bones.RightIndex2?.childCount > 0 ? script.bones.RightIndex2?.GetChild(0) : null;
            script.bones.RightMiddle1 = bones.FirstOrDefault((s) => s.name.Contains("middle", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.RightHand);
            script.bones.RightMiddle2 = script.bones.RightMiddle1?.childCount > 0 ? script.bones.RightMiddle1?.GetChild(0) : null;
            script.bones.RightMiddle3 = script.bones.RightMiddle2?.childCount > 0 ? script.bones.RightMiddle2?.GetChild(0) : null;
            script.bones.RightRing1 = bones.FirstOrDefault((s) => s.name.Contains("ring", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.RightHand);
            script.bones.RightRing2 = script.bones.RightRing1?.childCount > 0 ? script.bones.RightRing1?.GetChild(0) : null;
            script.bones.RightRing3 = script.bones.RightRing2?.childCount > 0 ? script.bones.RightRing2?.GetChild(0) : null;
            script.bones.RightPinky1 = bones.FirstOrDefault((s) => s.name.Contains("pinky", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.RightHand);
            script.bones.RightPinky2 = script.bones.RightPinky1?.childCount > 0 ? script.bones.RightPinky1?.GetChild(0) : null;
            script.bones.RightPinky3 = script.bones.RightPinky2?.childCount > 0 ? script.bones.RightPinky2?.GetChild(0) : null;

            script.bones.RightUpperLeg = bones.FirstOrDefault((s) => s.name.Contains("right", StringComparison.OrdinalIgnoreCase) && s.parent == script.bones.Pelvis);
            script.bones.RightLowerLeg = script.bones.RightUpperLeg?.childCount > 0 ? script.bones.RightUpperLeg?.GetChild(0) : null;
            script.bones.RightFoot = script.bones.RightLowerLeg?.childCount > 0 ? script.bones.RightLowerLeg?.GetChild(0) : null;
            script.bones.RightToes = script.bones.RightFoot?.childCount > 0 ? script.bones.RightFoot?.GetChild(0) : null;
            EditorUtility.SetDirty(script);
        }
#if BIOIK
        if (GUILayout.Button("Add BioIK"))
        {
            script.InitBioIK();
        }
        if (GUILayout.Button("Save BioIK"))
        {
            script.SaveBioIK();
        }
#endif
    }
}
#endif