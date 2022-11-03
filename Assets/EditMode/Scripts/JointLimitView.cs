using RFUniverse.Attributes;
using UnityEngine;

namespace RFUniverse.EditMode
{
    public class JointLimitView : MonoBehaviour
    {
        public MeshRenderer revoluteX;
        public MeshRenderer revoluteY;
        public MeshRenderer revoluteZ;
        public GameObject prismaticY;
        public GameObject prismaticX;
        public GameObject prismaticZ;

        public void SetArticulationData(ControllerAttr attr, ArticulationData articulationData)
        {
            if (attr == null || articulationData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            Transform trans = attr.transform.FindChildIndexQueue(articulationData.artIndexQueue);
            transform.SetParent(trans);
            transform.localPosition = new Vector3(articulationData.anchorPosition[0], articulationData.anchorPosition[1], articulationData.anchorPosition[2]);
            transform.localRotation = new Quaternion(articulationData.anchorRotation[0], articulationData.anchorRotation[1], articulationData.anchorRotation[2], articulationData.anchorRotation[3]);
            transform.SetParent(null);
            switch (articulationData.JointType)
            {
                case ArticulationJointType.FixedJoint:
                    revoluteX.gameObject.SetActive(false);
                    revoluteY.gameObject.SetActive(false);
                    revoluteZ.gameObject.SetActive(false);
                    prismaticX.SetActive(false);
                    prismaticY.SetActive(false);
                    prismaticZ.SetActive(false);
                    break;
                case ArticulationJointType.PrismaticJoint:
                    revoluteX.gameObject.SetActive(false);
                    revoluteY.gameObject.SetActive(false);
                    revoluteZ.gameObject.SetActive(false);
                    prismaticX.SetActive(false);
                    prismaticY.SetActive(false);
                    prismaticZ.SetActive(false);
                    if (articulationData.linearLockX == ArticulationDofLock.LimitedMotion)
                    {
                        prismaticX.SetActive(true);
                        prismaticX.transform.Find("upper").localPosition = Vector3.up * articulationData.xDrive.upperLimit;
                        prismaticX.transform.Find("lower").localPosition = Vector3.up * articulationData.xDrive.lowerLimit;
                        prismaticX.transform.GetComponentInChildren<LineRenderer>().SetPositions(new Vector3[] { Vector3.up * articulationData.xDrive.upperLimit, Vector3.up * articulationData.xDrive.lowerLimit });
                    }
                    if (articulationData.linearLockY == ArticulationDofLock.LimitedMotion)
                    {
                        prismaticY.SetActive(true);
                        prismaticY.transform.Find("upper").localPosition = Vector3.up * articulationData.yDrive.upperLimit;
                        prismaticY.transform.Find("lower").localPosition = Vector3.up * articulationData.yDrive.lowerLimit;
                        prismaticY.transform.GetComponentInChildren<LineRenderer>().SetPositions(new Vector3[] { Vector3.up * articulationData.yDrive.upperLimit, Vector3.up * articulationData.yDrive.lowerLimit });
                    }
                    if (articulationData.linearLockZ == ArticulationDofLock.LimitedMotion)
                    {
                        prismaticZ.SetActive(true);
                        prismaticZ.transform.Find("upper").localPosition = Vector3.up * articulationData.zDrive.upperLimit;
                        prismaticZ.transform.Find("lower").localPosition = Vector3.up * articulationData.zDrive.lowerLimit;
                        prismaticZ.transform.GetComponentInChildren<LineRenderer>().SetPositions(new Vector3[] { Vector3.up * articulationData.zDrive.upperLimit, Vector3.up * articulationData.zDrive.lowerLimit });
                    }
                    break;
                case ArticulationJointType.RevoluteJoint:
                    revoluteX.gameObject.SetActive(false);
                    revoluteY.gameObject.SetActive(false);
                    revoluteZ.gameObject.SetActive(false);
                    prismaticX.SetActive(false);
                    prismaticY.SetActive(false);
                    prismaticZ.SetActive(false);
                    if (articulationData.twistLock == ArticulationDofLock.LimitedMotion)
                    {
                        revoluteX.gameObject.SetActive(true);
                        revoluteX.transform.Find("upper").localEulerAngles = new Vector3(180, 0, articulationData.xDrive.upperLimit);
                        revoluteX.transform.Find("lower").localEulerAngles = new Vector3(180, 0, articulationData.xDrive.lowerLimit);
                        revoluteX.material.SetFloat("_upper", articulationData.xDrive.upperLimit);
                        revoluteX.material.SetFloat("_lower", articulationData.xDrive.lowerLimit);
                    }
                    break;
                case ArticulationJointType.SphericalJoint:
                    revoluteX.gameObject.SetActive(false);
                    revoluteY.gameObject.SetActive(false);
                    revoluteZ.gameObject.SetActive(false);
                    prismaticX.SetActive(false);
                    prismaticY.SetActive(false);
                    prismaticZ.SetActive(false);
                    if (articulationData.twistLock == ArticulationDofLock.LimitedMotion)
                    {
                        revoluteX.gameObject.SetActive(true);
                        revoluteX.transform.Find("upper").localEulerAngles = new Vector3(180, 0, articulationData.xDrive.upperLimit);
                        revoluteX.transform.Find("lower").localEulerAngles = new Vector3(180, 0, articulationData.xDrive.lowerLimit);
                        revoluteX.material.SetFloat("_upper", articulationData.xDrive.upperLimit);
                        revoluteX.material.SetFloat("_lower", articulationData.xDrive.lowerLimit);
                    }
                    if (articulationData.swingYLock == ArticulationDofLock.LimitedMotion)
                    {
                        revoluteY.gameObject.SetActive(true);
                        revoluteY.transform.Find("upper").localEulerAngles = new Vector3(180, 0, articulationData.yDrive.upperLimit);
                        revoluteY.transform.Find("lower").localEulerAngles = new Vector3(180, 0, articulationData.yDrive.lowerLimit);
                        revoluteY.material.SetFloat("_upper", articulationData.yDrive.upperLimit);
                        revoluteY.material.SetFloat("_lower", articulationData.yDrive.lowerLimit);
                    }
                    if (articulationData.swingZLock == ArticulationDofLock.LimitedMotion)
                    {
                        revoluteZ.gameObject.SetActive(true);
                        revoluteZ.transform.Find("upper").localEulerAngles = new Vector3(180, 0, articulationData.zDrive.upperLimit);
                        revoluteZ.transform.Find("lower").localEulerAngles = new Vector3(180, 0, articulationData.zDrive.lowerLimit);
                        revoluteZ.material.SetFloat("_upper", articulationData.zDrive.upperLimit);
                        revoluteZ.material.SetFloat("_lower", articulationData.zDrive.lowerLimit);
                    }
                    break;
            }
        }
    }
}
