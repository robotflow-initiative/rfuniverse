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
            SetJointPositionDirectly();
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
                SetJointPositionDirectly();
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
    public void SetJointTargetPosition(float jointPosition, ControlMode controlMode, float speedScale = 1.0f)
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
        float currentTargetJointPosition = 0.0f;

        // Check whether the joint has reached the target
        if (Mathf.Abs(currentJointPosition - targetJointPosition) < jointArrivingTolerance)
        {
            // Debug.Log(string.Format("{0} Stop.", name));
            direction = MovingDirection.None;
        }

        currentTargetJointPosition = currentJointPosition + (float)direction * speedScale * speed * Time.fixedDeltaTime;

        return currentTargetJointPosition;
    }

    private void SetJointPositionDirectly()
    {
        if (articulationBody.jointType == ArticulationJointType.RevoluteJoint)
        {
            articulationBody.jointPosition = new ArticulationReducedSpace(targetJointPosition * Mathf.Deg2Rad);
        }
        else if (articulationBody.jointType == ArticulationJointType.PrismaticJoint)
        {
            articulationBody.jointPosition = new ArticulationReducedSpace(targetJointPosition);
        }
        articulationBody.jointVelocity = new ArticulationReducedSpace(0);
        direction = MovingDirection.None;
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
