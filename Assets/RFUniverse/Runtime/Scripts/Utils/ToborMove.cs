using UnityEngine;
namespace RFUniverse
{

    public class ToborMove : MonoBehaviour, ICustomMove
    {
        public ArticulationBody left;
        public ArticulationBody right;
        public float r = 0.077f;
        public float l = 0.2565f;
        float targetVelocity;
        int mod;
        float time = 0;

        public void Forward(float dis, float speed)
        {
            dis = Mathf.Abs(dis);
            speed = Mathf.Abs(speed);
            mod = 1;
            targetVelocity = speed / (2 * r * Mathf.PI) * 360;
            time = dis / speed;
            time += Time.time;
        }
        public void Back(float dis, float speed)
        {
            dis = Mathf.Abs(dis);
            speed = Mathf.Abs(speed);
            mod = 2;
            targetVelocity = speed / (2 * r * Mathf.PI) * 360;
            time = dis / speed;
            time += Time.time;
        }
        public void Left(float angle, float speed)
        {
            angle = Mathf.Abs(angle);
            speed = Mathf.Abs(speed);
            mod = 3;
            targetVelocity = speed * l / r;
            time = angle / speed;
            time += Time.time;
        }
        public void Right(float angle, float speed)
        {
            angle = Mathf.Abs(angle);
            speed = Mathf.Abs(speed);
            mod = 4;
            targetVelocity = speed * l / r;
            time = angle / speed;
            time += Time.time;
        }

        private void FixedUpdate()
        {
            if (Time.time > time)
            {
                SetDriveTargetVelocities(left, 0);
                SetDriveTargetVelocities(right, 0);
            }
            else
            {
                switch (mod)
                {
                    case 1:
                        SetDriveTargetVelocities(left, targetVelocity);
                        SetDriveTargetVelocities(right, targetVelocity);
                        break;
                    case 2:
                        SetDriveTargetVelocities(left, -targetVelocity);
                        SetDriveTargetVelocities(right, -targetVelocity);
                        break;
                    case 3:
                        SetDriveTargetVelocities(left, -targetVelocity);
                        SetDriveTargetVelocities(right, targetVelocity);
                        break;
                    case 4:
                        SetDriveTargetVelocities(left, targetVelocity);
                        SetDriveTargetVelocities(right, -targetVelocity);
                        break;
                }
            }
        }

        void SetDriveTargetVelocities(ArticulationBody joint, float target)
        {
            ArticulationDrive tempDrive = joint.xDrive;
            tempDrive.targetVelocity = target;
            joint.xDrive = tempDrive;
        }
    }

}
