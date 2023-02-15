using System;
using UnityEngine;
public enum MovingDirection
{
    None = 0,
    Positive = 1,
    Negative = -1,
};

// Direct: Move joint position directly through articulationBody.jointPosition.

// Target: Move joint position through articulationDrive.target, when velocity is smaller 
//     than a given threshold, stop it.

// TargetWithJointPosition: Move joint position through articulationDrive.target, when 
//     velocity is smaller than a given threshold, move joint position directly through 
//     articulationBody.jointPosition.

// Speed: Move 
public enum ControlMode
{
    Direct = 0,
    Target = 1,
    TargetWithJointPosition = 2,
    Speed = 3
};

[RequireComponent(typeof(ArticulationBody))]
[DisallowMultipleComponent]
public class ArticulationUnit : MonoBehaviour
{
    public ArticulationBody mimicParent = null;
    public float mimicMultiplier = 1.0f;
    public float mimicOffset = 0.0f;
    public bool mimicSync = false;
    public event Action OnSetJointPositionDirectly = null;
    private ControlMode controlMode;
    private ArticulationBody articulationBody;
    private int dofCount;
    private float speed = 100.0f;
    private float speedScale = 1.0f;
    private float jointArrivingTolerance = 0.3f;
    private float targetJointPosition = 0;
    private float jointVelocityThres = 0.1f;
    private MovingDirection direction = MovingDirection.None;

    private int checkStableFrequency = 5;
    private int checkStableFrequencyCounter = 0;

    void Awake()
    {
        articulationBody = gameObject.GetComponent<ArticulationBody>();
        if (articulationBody == null)
        {
            Debug.LogError($"No ArticulationBody component in {gameObject.name}");
        }
        dofCount = articulationBody.jointPosition.dofCount;
        controlMode = ControlMode.Target;
        if (dofCount > 0)
        {
            articulationBody.jointVelocity = new ArticulationReducedSpace(0);
        }

        if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
        {
            speed = 0.1f;
            jointArrivingTolerance = 0.001f;
            jointVelocityThres = 0.3f;
        }
        if (mimicParent)
            mimicParent.GetComponent<ArticulationUnit>().OnSetJointPositionDirectly += MimicDirectly;
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
        var currentDrive = articulationBody.xDrive;
        if (mimicSync)
        {
            currentDrive.target = mimicParent.xDrive.target * mimicMultiplier + mimicOffset;
        }
        else if (mimicParent.jointType != ArticulationJointType.FixedJoint)
        {
            currentDrive.target = mimicParent.jointPosition[0] * Mathf.Rad2Deg * mimicMultiplier + mimicOffset;
        }
        articulationBody.xDrive = currentDrive;
    }
    private void MimicDirectly()
    {
        SetJointPositionDirectly(mimicParent.xDrive.target * mimicMultiplier + mimicOffset);
    }
    void AttrUpdate()
    {
        // No freedom, i.e. Fixed joint
        if (dofCount == 0)
        {
            return;
        }

        if (direction == MovingDirection.None)
        {
            return;
        }

        float currentTargetJointPosition = 0.0f;
        if (controlMode == ControlMode.Speed)
        {
            currentTargetJointPosition = CalculateCurrentTargetJointPosition();
        }
        else
        {
            currentTargetJointPosition = targetJointPosition;
        }

        // Currently, we ignore spherical joint, and suppose each joint has 1DoF at most.
        // If support sherical joint in the future, the following code needs modification.
        ArticulationDrive drive = articulationBody.xDrive;
        drive.target = currentTargetJointPosition;
        articulationBody.xDrive = drive;
        // For prismatic joint, we cannot know which drive controls it. So we traverse every drive and promise the movement happens.
        if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
        {
            drive = articulationBody.yDrive;
            drive.target = currentTargetJointPosition;
            articulationBody.yDrive = drive;

            drive = articulationBody.zDrive;
            drive.target = currentTargetJointPosition;
            articulationBody.zDrive = drive;
        }

        if (controlMode == ControlMode.Direct)
        {
            SetJointPositionDirectly(targetJointPosition);
        }
        else if (controlMode == ControlMode.Target || controlMode == ControlMode.Speed)
        {
            if (IsStable())
            {
                direction = MovingDirection.None;
            }
        }
        else if (controlMode == ControlMode.TargetWithJointPosition)
        {
            if (IsStable())
            {
                SetJointPositionDirectly(targetJointPosition);
            }
        }
    }

    public float CalculateCurrentJointPosition()
    {
        if (articulationBody == null)
        {
            return 0;
        }

        if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
        {
            return articulationBody.jointPosition[0];
        }
        else if (articulationBody.jointType == ArticulationJointType.RevoluteJoint)
        {
            // In Articulation Body, joint position for revolute joint is radius, so it need to be converted to degree.
            return articulationBody.jointPosition[0] * Mathf.Rad2Deg;
        }
        else
        {
            return 0;
        }
    }
    public void SetJointTargetVelocity(float jointTargetVelocity)
    {
        ArticulationDrive drive = articulationBody.xDrive;
        drive.targetVelocity = jointTargetVelocity;
        articulationBody.xDrive = drive;
        drive = articulationBody.yDrive;
        drive.targetVelocity = jointTargetVelocity;
        articulationBody.yDrive = drive;
        drive = articulationBody.zDrive;
        drive.targetVelocity = jointTargetVelocity;
        articulationBody.zDrive = drive;
        articulationBody.jointVelocity = new ArticulationReducedSpace(jointTargetVelocity);
    }
    public void SetJointTargetPosition(float jointPosition, ControlMode controlMode, float speedScale = 1f)
    {
        this.controlMode = controlMode;
        this.speedScale = speedScale;
        targetJointPosition = jointPosition;
        float currentJointPosition = CalculateCurrentJointPosition();
        if (targetJointPosition < currentJointPosition)
        {
            direction = MovingDirection.Negative;
        }
        else
        {
            direction = MovingDirection.Positive;
        }
        AttrUpdate();
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
    public MovingDirection GetMovingDirection()
    {
        return direction;
    }

    private float CalculateCurrentTargetJointPosition()
    {
        float currentJointPosition = CalculateCurrentJointPosition();

        // Check whether the joint has reached the target
        if (Mathf.Abs(currentJointPosition - targetJointPosition) < jointArrivingTolerance)
        {
            // Debug.Log(string.Format("{0} Stop.", name));
            direction = MovingDirection.None;
        }

        float currentTargetJointPosition = currentJointPosition + (float)direction * speedScale * speed * Time.fixedDeltaTime;

        return currentTargetJointPosition;
    }

    public void SetJointPositionDirectly(float target)
    {
        ArticulationDrive drive = articulationBody.xDrive;
        drive.target = target;
        articulationBody.xDrive = drive;

        if (articulationBody.jointType == ArticulationJointType.RevoluteJoint)
        {
            articulationBody.jointPosition = new ArticulationReducedSpace(target * Mathf.Deg2Rad);
        }
        else if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
        {
            articulationBody.jointPosition = new ArticulationReducedSpace(target);
        }
        articulationBody.jointVelocity = new ArticulationReducedSpace(0);
        direction = MovingDirection.None;
        OnSetJointPositionDirectly?.Invoke();
    }

    private bool IsStable()
    {
        if (checkStableFrequencyCounter < checkStableFrequency)
        {
            checkStableFrequencyCounter += 1;
            return false;
        }

        checkStableFrequencyCounter = 0;
        if (dofCount == 0 || direction == MovingDirection.None)
        {
            return true;
        }

        return articulationBody.jointVelocity[0] < jointVelocityThres;
    }
}
