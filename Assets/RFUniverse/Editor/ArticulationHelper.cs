using UnityEngine;
using UnityEditor;

namespace RFUniverse
{
    public class ArticulationHelper : Editor
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
    }
}
