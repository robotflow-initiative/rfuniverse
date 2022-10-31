using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RFUniverse;
using RFUniverse.Attributes;
using Unity.Robotics.UrdfImporter.Control;
using Unity.Robotics.UrdfImporter;
using System.Text;

public class ArticulationHelper : MonoBehaviour
{
    [MenuItem("RFUniverse/Articulation Helper/Normalize RFUniverse Articulation")]
    static void NormalizeRFUniverseArticulation()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("No gameobject selected");
            return;
        }
        RFUniverseUtility.NormalizeRFUniverseArticulation(Selection.activeGameObject);
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
