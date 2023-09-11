using System;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum ControlMode
{
    Direct = 0,
    Target = 1,
    Speed = 2
};

[RequireComponent(typeof(ArticulationBody))]
[DisallowMultipleComponent]
public class ArticulationUnit : MonoBehaviour
{
    public ArticulationBody mimicParent = null;
    public float mimicMultiplier = 1.0f;
    public float mimicOffset = 0.0f;
    public event Action OnSetJointPositionDirectly = null;
    private ArticulationBody articulationBody;
    private int dofCount => articulationBody.dofCount;

    void Awake()
    {
        articulationBody = gameObject.GetComponent<ArticulationBody>();
        if (articulationBody == null)
        {
            Debug.LogError($"No ArticulationBody component in {gameObject.name}");
        }
        //controlMode = ControlMode.Target;
        if (dofCount > 0)
        {
            articulationBody.jointVelocity = new ArticulationReducedSpace(0);
        }

        if (mimicParent)
        {
            ArticulationDrive drive = articulationBody.xDrive;
            drive.driveType = ArticulationDriveType.Target;
            articulationBody.xDrive = drive;

            drive = articulationBody.yDrive;
            drive.driveType = ArticulationDriveType.Target;
            articulationBody.yDrive = drive;

            drive = articulationBody.zDrive;
            drive.driveType = ArticulationDriveType.Target;
            articulationBody.zDrive = drive;

            mimicParent.GetComponent<ArticulationUnit>().OnSetJointPositionDirectly += MimicDirectly;
        }
    }

    enum ForceMode
    {
        None,
        Force,
        ForceAtPosition,
        Torque
    }
    private ForceMode forceMode = ForceMode.None;
    private Vector3 force = Vector3.zero;
    private Vector3 position = Vector3.zero;
    void FixedUpdate()
    {
        switch (forceMode)
        {
            case ForceMode.Force:
                articulationBody.AddForce(force);
                break;
            case ForceMode.ForceAtPosition:
                articulationBody.AddForceAtPosition(force, position);
                break;
            case ForceMode.Torque:
                articulationBody.AddTorque(force);
                break;
        }
        forceMode = ForceMode.None;
        Mimic();
    }
    private void Mimic()
    {
        if (mimicParent == null) return;
        ArticulationDrive currentDrive = articulationBody.xDrive;
        switch (mimicParent.jointType)
        {
            case ArticulationJointType.FixedJoint:
                break;
            case ArticulationJointType.PrismaticJoint:
                currentDrive.target = mimicParent.jointPosition[0] * mimicMultiplier + mimicOffset;
                break;
            case ArticulationJointType.RevoluteJoint:
                currentDrive.target = mimicParent.jointPosition[0] * Mathf.Rad2Deg * mimicMultiplier + mimicOffset;
                break;
            case ArticulationJointType.SphericalJoint:
                break;
            default:
                break;
        }
        articulationBody.xDrive = currentDrive;
    }
    private void MimicDirectly()
    {
        SetJointPositionDirectly(mimicParent.xDrive.target * mimicMultiplier + mimicOffset);
    }

    public float CalculateCurrentJointPosition()
    {
        if (articulationBody == null) return 0;
        if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
            return articulationBody.jointPosition[0];
        else if (articulationBody.jointType == ArticulationJointType.RevoluteJoint)
            return articulationBody.jointPosition[0] * Mathf.Rad2Deg;
        return 0;
    }
    public float CalculateCurrentJointVelocity()
    {
        if (articulationBody == null) return 0;
        if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
            return articulationBody.jointVelocity[0];
        else if (articulationBody.jointType == ArticulationJointType.RevoluteJoint)
            return articulationBody.jointVelocity[0] * Mathf.Rad2Deg;
        return 0;
    }
    public float CalculateCurrentJointAcceleration()
    {
        if (articulationBody == null) return 0;
        if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
            return articulationBody.jointAcceleration[0];
        else if (articulationBody.jointType == ArticulationJointType.RevoluteJoint)
            return articulationBody.jointAcceleration[0] * Mathf.Rad2Deg;
        return 0;
    }
    public float CalculateCurrentJointForce()
    {
        if (articulationBody == null) return 0;
        if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
            return articulationBody.jointForce[0];
        else if (articulationBody.jointType == ArticulationJointType.RevoluteJoint)
            return articulationBody.jointForce[0] * Mathf.Rad2Deg;
        return 0;
    }

    public void SetJointTarget(float jointPosition, ControlMode controlMode)
    {
        switch (controlMode)
        {
            case ControlMode.Direct:
                SetJointPositionDirectly(jointPosition);
                break;
            case ControlMode.Target:
                SetJointPosition(jointPosition);
                break;
            case ControlMode.Speed:
                SetJointTargetVelocity(jointPosition);
                break;
            default:
                break;
        }
    }
    public void SetJointPosition(float target)
    {
        articulationBody.SetDriveTarget(ArticulationDriveAxis.X, target);
        articulationBody.SetDriveTarget(ArticulationDriveAxis.Y, target);
        articulationBody.SetDriveTarget(ArticulationDriveAxis.Z, target);
    }

    public void SetJointPositionDirectly(float target)
    {
        SetJointPosition(target);
        target = articulationBody.jointType == ArticulationJointType.PrismaticJoint ? target : target * Mathf.Deg2Rad;
        articulationBody.jointPosition = new ArticulationReducedSpace(target, target, target);
        OnSetJointPositionDirectly?.Invoke();
    }
    public void SetJointTargetVelocity(float jointTargetVelocity)
    {
        //articulationBody.SetJointVelocities(new float[] { jointTargetVelocity, jointTargetVelocity, jointTargetVelocity }.ToList());
        ArticulationDrive drive = articulationBody.xDrive;
        drive.driveType = ArticulationDriveType.Velocity;
        drive.targetVelocity = jointTargetVelocity;
        articulationBody.xDrive = drive;

        drive = articulationBody.yDrive;
        drive.driveType = ArticulationDriveType.Velocity;
        drive.targetVelocity = jointTargetVelocity;
        articulationBody.yDrive = drive;

        drive = articulationBody.zDrive;
        drive.driveType = ArticulationDriveType.Velocity;
        drive.targetVelocity = jointTargetVelocity;
        articulationBody.zDrive = drive;
    }

    public float GetJointPosition()
    {
        switch (articulationBody.jointType)
        {
            case ArticulationJointType.FixedJoint:
            default:
                return 0;
            case ArticulationJointType.PrismaticJoint:
                return articulationBody.jointPosition[0];
            case ArticulationJointType.RevoluteJoint:
            case ArticulationJointType.SphericalJoint:
                return articulationBody.jointPosition[0] * Mathf.Rad2Deg;
        }
    }
    public void AddJointForce(Vector3 jointForce)
    {
        forceMode = ForceMode.Force;
        force = jointForce;
    }
    public void AddJointForceAtPosition(Vector3 jointForce, Vector3 forcesPosition)
    {
        forceMode = ForceMode.ForceAtPosition;
        force = jointForce;
        position = forcesPosition;
    }
    public void AddJointTorque(Vector3 jointForce)
    {
        forceMode = ForceMode.Torque;
        force = jointForce;
    }

    //public MovingDirection GetMovingDirection()
    //{
    //    return direction;
    //}

    //private float CalculateCurrentTargetJointPosition()
    //{
    //    float currentJointPosition = CalculateCurrentJointPosition();

    //    // Check whether the joint has reached the target
    //    if (Mathf.Abs(currentJointPosition - targetJointPosition) < jointArrivingTolerance)
    //    {
    //        // Debug.Log(string.Format("{0} Stop.", name));
    //        direction = MovingDirection.None;
    //    }

    //    float currentTargetJointPosition = currentJointPosition + (float)direction * speedScale * speed * Time.fixedDeltaTime;

    //    return currentTargetJointPosition;
    //}



    //private bool IsStable()
    //{
    //    if (checkStableFrequencyCounter < checkStableFrequency)
    //    {
    //        checkStableFrequencyCounter += 1;
    //        return false;
    //    }

    //    checkStableFrequencyCounter = 0;
    //    if (dofCount == 0 || direction == MovingDirection.None)
    //    {
    //        return true;
    //    }

    //    return articulationBody.jointVelocity[0] < jointVelocityThres;
    //}
}
