using UnityEngine;
using RFUniverse.Attributes;
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter.Control;
using Unity.Robotics.UrdfImporter;

namespace RFUniverse
{
    public static class RFUniverseUtility
    {
        public static Color EncodeIDAsColor(int instanceId)
        {
            long r = (instanceId * (long)16807 + 187) % 256;
            long g = (instanceId * (long)48271 + 79) % 256;
            long b = (instanceId * (long)95849 + 233) % 256;
            return new Color32((byte)r, (byte)g, (byte)b, 255);
        }

        public static List<T> GetChildComponentFilter<T>(this BaseAttr attr) where T : Component
        {
            List<T> components = new List<T>();
            foreach (var item in attr.GetComponentsInChildren<T>())
            {
                if (item.GetComponentInParent<BaseAttr>() == attr)
                    components.Add(item);
            }
            return components;
        }

        public static Transform FindChlid(this Transform parent, string targetName, bool includeSelf = true)
        {
            if (includeSelf && parent.name == targetName)
                return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = FindChlid(parent.GetChild(i), targetName, true);
                if (child == null)
                    continue;
                else
                    return child;
            }
            return null;
        }

        public static ControllerAttr NormalizeRFUniverseArticulation(GameObject root)
        {
            ControllerAttr attr = root.GetComponent<ControllerAttr>() ?? root.AddComponent<ControllerAttr>();
            // Add basic script for root node
            IgnoreSelfCollision ign = root.GetComponent<IgnoreSelfCollision>() ?? root.AddComponent<IgnoreSelfCollision>();
            if (root.transform.GetChild(1).GetComponent<ArticulationBody>() == null)
                root.transform.GetChild(1).gameObject.AddComponent<ArticulationBody>();
            // Remove URDFImporter Scripts
            Controller controller = root.GetComponentInChildren<Controller>();
            Destroy(controller);
            UrdfRobot urdfRobot = root.GetComponentInChildren<UrdfRobot>();
            Destroy(urdfRobot);
            UrdfPlugins urdfPlugins = root.GetComponentInChildren<UrdfPlugins>();
            Destroy(urdfPlugins.gameObject);
            UrdfLink[] urdfLinks = root.GetComponentsInChildren<UrdfLink>();
            foreach (var urdfLink in urdfLinks)
            {
                Destroy(urdfLink);
            }
            UrdfInertial[] urdfInertials = root.GetComponentsInChildren<UrdfInertial>();
            foreach (var urdfInertial in urdfInertials)
            {
                Destroy(urdfInertial);
            }
            UrdfJoint[] urdfJoints = root.GetComponentsInChildren<UrdfJoint>();
            foreach (var urdfJoint in urdfJoints)
            {
                Destroy(urdfJoint);
            }
            UrdfVisuals[] urdfVisuals = root.GetComponentsInChildren<UrdfVisuals>();
            foreach (var urdfVisual in urdfVisuals)
            {
                Destroy(urdfVisual);
            }
            UrdfVisual[] urdfVisuals1 = root.GetComponentsInChildren<UrdfVisual>();
            foreach (var urdfVisual1 in urdfVisuals1)
            {
                Destroy(urdfVisual1);
            }
            UrdfCollisions[] urdfCollisions = root.GetComponentsInChildren<UrdfCollisions>();
            foreach (var urdfCollision in urdfCollisions)
            {
                Destroy(urdfCollision);
            }
            UrdfCollision[] urdfCollisions1 = root.GetComponentsInChildren<UrdfCollision>();
            foreach (var urdfCollision1 in urdfCollisions1)
            {
                Destroy(urdfCollision1);
            }

            // Add RFUniverse scripts
            ArticulationBody[] articulationBodies = root.GetComponentsInChildren<ArticulationBody>();
            for (int i = 0; i < articulationBodies.Length; ++i)
            {
                //articulationBodies[i].useGravity = false;

                if (articulationBodies[i].gameObject.GetComponent<ArticulationUnit>() == null)
                {
                    articulationBodies[i].gameObject.AddComponent<ArticulationUnit>();
                }

                if (articulationBodies[i].isRoot)
                {
                    articulationBodies[i].immovable = true;
                }

                var xDrive = articulationBodies[i].xDrive;
                xDrive.stiffness = 100000;
                xDrive.damping = 9000;
                xDrive.forceLimit = float.MaxValue;
                articulationBodies[i].xDrive = xDrive;

                var yDrive = articulationBodies[i].yDrive;
                yDrive.stiffness = 100000;
                yDrive.damping = 9000;
                yDrive.forceLimit = float.MaxValue;
                articulationBodies[i].yDrive = yDrive;

                var zDrive = articulationBodies[i].zDrive;
                zDrive.stiffness = 100000;
                zDrive.damping = 9000;
                zDrive.forceLimit = float.MaxValue;
                articulationBodies[i].zDrive = zDrive;
            }
            return attr;
        }

        public static void Destroy(Object obj)
        {
            if (Application.isEditor)
                GameObject.DestroyImmediate(obj);
            else
                GameObject.Destroy(obj);
        }

        public static List<BaseAttrData> SortByParent(List<BaseAttrData> datas)
        {
            List<int> headID = new List<int>();
            int i = 0;
            while (i < datas.Count)
            {
                if (datas[i].parentID > 0 && !headID.Contains(datas[i].parentID))
                {
                    datas.Remove(datas[i]);
                    datas.Add(datas[i]);
                }
                else
                {
                    headID.Add(datas[i].id);
                    i++;
                }
            }
            return datas;
        }
    }
}
