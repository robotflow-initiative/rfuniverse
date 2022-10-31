using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicJointController : MonoBehaviour
{

    private ArticulationBody[] articulationChain;
    private MimicJoint[] mimicJoints;
    private List<MimicJoint> mimicRootJoints;
    private Dictionary<MimicJoint, TreeNode<MimicJoint>> mimicToNode;

    // Start is called before the first frame update
    void Start()
    {
        articulationChain = GetComponentsInChildren<ArticulationBody>();
        mimicToNode = new Dictionary<MimicJoint, TreeNode<MimicJoint>>();
        mimicRootJoints = new List<MimicJoint>();
        mimicJoints = GetComponentsInChildren<MimicJoint>();

        foreach (var mimic in mimicJoints)
        {
            mimicToNode.Add(mimic, new TreeNode<MimicJoint>(mimic));
        }

        foreach (var mimic in mimicJoints)
        {
            if (mimic.Parent == null)
            {
                mimicRootJoints.Add(mimic);
            }
            else
            {
                var parentNode = mimicToNode[mimic.Parent];
                var currentNode = mimicToNode[mimic];
                parentNode.AddChild(currentNode);
            }
        }
    }

    private void MimicVisitor(MimicJoint parent, MimicJoint child)
    {
        var childJoint = child.GetComponent<ArticulationBody>();
        var parentJoint = parent.GetComponent<ArticulationBody>();
        var currentDrive = childJoint.xDrive;
        if (child.sync)
        {
            currentDrive.target = parentJoint.xDrive.target * child.multiplier + child.offset;
        }
        else if (parentJoint.jointType != ArticulationJointType.FixedJoint)
        {
            currentDrive.target = parentJoint.jointPosition[0] * Mathf.Rad2Deg * child.multiplier + child.offset;
        }

        childJoint.xDrive = currentDrive;
    }

    public void SamplePose(ArticulationBody joint)
    {
        ArticulationDrive currentDrive = joint.xDrive;
        float upper = currentDrive.upperLimit;
        float lower = currentDrive.lowerLimit;
        currentDrive.target = Random.Range(lower, upper);
        joint.xDrive = currentDrive;
    }

    public void SamplePose()
    {
        foreach (ArticulationBody joint in articulationChain)
        {
            // for now, sample for revolute and prismatic joints only
            if (joint.jointType != ArticulationJointType.RevoluteJoint &&
            joint.jointType != ArticulationJointType.PrismaticJoint)
                continue;
            if (joint.transform.GetComponent<MimicJoint>() != null)
                continue;
            SamplePose(joint);
        }

        foreach (var mimic in mimicRootJoints)
        {
            SamplePose(mimic.GetComponent<ArticulationBody>());
            mimicToNode[mimic].Traverse(MimicVisitor);
        }
    }

    // For Debugging
    private void FixedUpdate()
    {
        foreach (var mimic in mimicRootJoints)
        {
            mimicToNode[mimic].Traverse(MimicVisitor);
        }
    }
}
