using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RFUniverse.Attributes;
using Unity.Robotics.UrdfImporter.Control;
using Unity.Robotics.UrdfImporter;
using System.Text;

public class ArticulationHelper : MonoBehaviour
{
    [MenuItem("RFUniverse/Articulation Helper/Normalize RFUniverse Articulation")]
    static void NormalizeRFUniverseArticulation()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("No gameobject selected");
            return;
        }
        GameObject articulationRoot = Selection.gameObjects[0];

        // Add basic script for root node
        if (articulationRoot.GetComponent<GameObjectAttr>() == null)
            articulationRoot.AddComponent<GameObjectAttr>();
        if (articulationRoot.GetComponent<IgnoreSelfCollision>() == null)
            articulationRoot.AddComponent<IgnoreSelfCollision>();

        // Remove URDFImporter Scripts
        Controller controller = articulationRoot.GetComponentInChildren<Controller>();
        DestroyImmediate(controller);
        UrdfRobot urdfRobot = articulationRoot.GetComponentInChildren<UrdfRobot>();
        DestroyImmediate(urdfRobot);
        UrdfPlugins[] urdfPlugins = articulationRoot.GetComponentsInChildren<UrdfPlugins>();
        foreach (var urdfPlugin in urdfPlugins)
        {
            DestroyImmediate(urdfPlugin);
        }
        UrdfLink[] urdfLinks = articulationRoot.GetComponentsInChildren<UrdfLink>();
        foreach (var urdfLink in urdfLinks)
        {
            DestroyImmediate(urdfLink);
        }
        UrdfInertial[] urdfInertials = articulationRoot.GetComponentsInChildren<UrdfInertial>();
        foreach (var urdfInertial in urdfInertials)
        {
            DestroyImmediate(urdfInertial);
        }
        UrdfJoint[] urdfJoints = articulationRoot.GetComponentsInChildren<UrdfJoint>();
        foreach (var urdfJoint in urdfJoints)
        {
            DestroyImmediate(urdfJoint);
        }
        UrdfVisuals[] urdfVisuals = articulationRoot.GetComponentsInChildren<UrdfVisuals>();
        foreach (var urdfVisual in urdfVisuals)
        {
            DestroyImmediate(urdfVisual);
        }
        UrdfVisual[] urdfVisuals1 = articulationRoot.GetComponentsInChildren<UrdfVisual>();
        foreach (var urdfVisual1 in urdfVisuals1)
        {
            DestroyImmediate(urdfVisual1);
        }
        UrdfCollisions[] urdfCollisions = articulationRoot.GetComponentsInChildren<UrdfCollisions>();
        foreach (var urdfCollision in urdfCollisions)
        {
            DestroyImmediate(urdfCollision);
        }
        UrdfCollision[] urdfCollisions1 = articulationRoot.GetComponentsInChildren<UrdfCollision>();
        foreach (var urdfCollision1 in urdfCollisions1)
        {
            DestroyImmediate(urdfCollision1);
        }

        // Add RFUniverse scripts
        ArticulationBody[] articulationBodies = articulationRoot.GetComponentsInChildren<ArticulationBody>();
        for (int i = 0; i < articulationBodies.Length; ++i)
        {
            articulationBodies[i].useGravity = false;

            if (articulationBodies[i].gameObject.GetComponent<ArticulationUnit>() == null)
            {
                articulationBodies[i].gameObject.AddComponent<ArticulationUnit>();
            }
            
            if (articulationBodies[i].isRoot)
            {
                articulationBodies[i].immovable = true;
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.RevoluteJoint)
            {
                var drive = articulationBodies[i].xDrive;
                drive.stiffness = 100000;
                drive.damping = 9000;
                drive.forceLimit = 10000;
                articulationBodies[i].xDrive = drive;
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.PrismaticJoint)
            {
                var xDrive = articulationBodies[i].xDrive;
                xDrive.stiffness = 100000;
                xDrive.damping = 9000;
                xDrive.forceLimit = 10000;
                articulationBodies[i].xDrive = xDrive;

                var yDrive = articulationBodies[i].yDrive;
                yDrive.stiffness = 100000;
                yDrive.damping = 9000;
                yDrive.forceLimit = 10000;
                articulationBodies[i].yDrive = yDrive;

                var zDrive = articulationBodies[i].zDrive;
                zDrive.stiffness = 100000;
                zDrive.damping = 9000;
                zDrive.forceLimit = 10000;
                articulationBodies[i].zDrive = zDrive;
            }
        }
    }

    [MenuItem("RFUniverse/Articulation Helper/Print all joint and index")]
    static void PrintJointAndIndex()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("No gameobject selected");
            return;
        }
        GameObject articulationRoot = Selection.gameObjects[0];
        ArticulationBody[] articulationBodies = articulationRoot.GetComponentsInChildren<ArticulationBody>();

        for (int i = 0; i < articulationBodies.Length; ++i)
        {            
            string jointType = "";
            if (articulationBodies[i].isRoot)
            {
                jointType = "Root";
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.FixedJoint)
            {
                jointType = "Fixed";
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.RevoluteJoint)
            {
                jointType = "Revolute";
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.PrismaticJoint)
            {
                jointType = "Prismatic";
            }

            Debug.Log(string.Format("{0}: {1} {2}", articulationBodies[i].name, i.ToString(), jointType));
        }
    }

    [MenuItem("RFUniverse/Articulation Helper/Print moveable joint and index")]
    static void PrintMoveableJointAndIndex()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("No gameobject selected");
            return;
        }
        GameObject articulationRoot = Selection.gameObjects[0];
        ArticulationBody[] articulationBodies = articulationRoot.GetComponentsInChildren<ArticulationBody>();

        for (int i = 0; i < articulationBodies.Length; ++i)
        {
            if (articulationBodies[i].jointType == ArticulationJointType.RevoluteJoint)
            {
                Debug.Log(string.Format("{0}: {1} {2}", articulationBodies[i].name, i.ToString(), "Revolute"));
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.PrismaticJoint)
            {
                Debug.Log(string.Format("{0}: {1} {2}", articulationBodies[i].name, i.ToString(), "Prismatic"));
            }
        }
    }

    [MenuItem("RFUniverse/Articulation Helper/Print joint and index in format")]
    static void PrintJointIndexFormat()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("No gameobject selected");
            return;
        }
        GameObject articulationRoot = Selection.gameObjects[0];
        ArticulationBody[] articulationBodies = articulationRoot.GetComponentsInChildren<ArticulationBody>();

        StringBuilder sb = new StringBuilder();
        sb.Append("/**\n");
        sb.AppendLine(" * Links:\n");
        
        for (int i = 0; i < articulationBodies.Length; ++i)
        {            
            string jointType = "";
            if (articulationBodies[i].isRoot)
            {
                jointType = "Root";
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.FixedJoint)
            {
                jointType = "Fixed";
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.RevoluteJoint)
            {
                jointType = "Revolute";
            }
            else if (articulationBodies[i].jointType == ArticulationJointType.PrismaticJoint)
            {
                jointType = "Prismatic";
            }

            string line = string.Format(" * {0}: {1}\t{2}\n", i.ToString(), articulationBodies[i].name, jointType);
            sb.Append(line);
        }

        sb.Append("*/");
        Debug.Log(sb.ToString());
    }
}
