using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class GetBoneWeight : MonoBehaviour
{
    public GameObject source;
    [Range(0, 1)]
    public float weight = 0.6f;
    public Dictionary<Transform, List<Vector3>> boneVertexPositions = new();
    public int bone = 0;
    public string boneName = "";

    private void OnDrawGizmos()
    {
        if (source == null) return;
        SkinnedMeshRenderer skin = source.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skin == null) return;
        Transform[] bones = skin.rootBone.GetComponentsInChildren<Transform>();
        foreach (var item in boneVertexPositions)
        {
            if (item.Key != bones[bone]) continue;
            boneName = bones[bone].name;
            foreach (var vecter in item.Value)
            {
                Gizmos.DrawSphere(vecter, 0.005f);
            }
        }

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GetBoneWeight))]
public class BoneWeightEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GetBoneWeight script = target as GetBoneWeight;
        if (GUILayout.Button("GetBoneWeight"))
        {
            SkinnedMeshRenderer skin = script.source.GetComponentInChildren<SkinnedMeshRenderer>();

            Dictionary<int, List<int>> boneVertex = new();
            BoneWeight1[] boneWeights = skin.sharedMesh.GetAllBoneWeights().ToArray();
            Debug.Log(boneWeights.Length);
            for (int i = 0; i < boneWeights.Length; i++)
            {
                Debug.Log("------------" + i);
                BoneWeight1 item = boneWeights[i];
                int index = 0;
                if (!boneVertex.ContainsKey(index))
                    boneVertex.Add(index, new List<int>());
                boneVertex[index].Add(i);
            }
            Debug.Log(boneVertex.Count);

            script.boneVertexPositions.Clear();
            Transform[] bones = skin.rootBone.GetComponentsInChildren<Transform>();
            foreach (var item in boneVertex)
            {
                List<Vector3> positions = new List<Vector3>();
                foreach (var index in item.Value)
                {
                    Vector3 worldPoint = script.source.transform.TransformPoint(skin.sharedMesh.vertices[index]);
                    positions.Add(worldPoint);
                }
                script.boneVertexPositions.Add(bones[item.Key], positions);
            }
            Debug.Log(script.boneVertexPositions.Count);

        }
    }
}
#endif
