#if OBI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class ObiUtility
{
    public static ObiClothBlueprint GenerateClothBlueprints(Mesh mesh)
    {
        float mass = 0.0005f;
        float radius = 0.01f;
        ObiClothBlueprint blueprint = ScriptableObject.CreateInstance<ObiClothBlueprint>();
        blueprint.inputMesh = mesh;
        blueprint.GenerateImmediate();
        for (int i = 0; i < blueprint.invMasses.Length; i++)
        {
            blueprint.invMasses[i] = 1f / mass;
        }
        for (int i = 0; i < blueprint.principalRadii.Length; i++)
        {
            float maxRad = Mathf.Max(blueprint.principalRadii[i].x, blueprint.principalRadii[i].y, blueprint.principalRadii[i].z);
            float mul = radius / maxRad;
            blueprint.principalRadii[i] = blueprint.principalRadii[i] * mul;
        }
        return blueprint;
    }

    public static ObiSoftbodySurfaceBlueprint GenerateSoftbodyBlueprints(Mesh mesh)
    {
        float mass = 0.0005f;
        float radius = 0.01f;
        ObiSoftbodySurfaceBlueprint blueprint = ScriptableObject.CreateInstance<ObiSoftbodySurfaceBlueprint>();
        blueprint.inputMesh = mesh;
        blueprint.surfaceSamplingMode = ObiSoftbodySurfaceBlueprint.SurfaceSamplingMode.Voxels;
        blueprint.surfaceResolution = 16;
        blueprint.volumeSamplingMode = ObiSoftbodySurfaceBlueprint.VolumeSamplingMode.Voxels;
        blueprint.volumeResolution = 16;
        blueprint.GenerateImmediate();
        for (int i = 0; i < blueprint.invMasses.Length; i++)
        {
            blueprint.invMasses[i] = 1f / mass;
        }
        for (int i = 0; i < blueprint.principalRadii.Length; i++)
        {
            float maxRad = Mathf.Max(blueprint.principalRadii[i].x, blueprint.principalRadii[i].y, blueprint.principalRadii[i].z);
            float mul = radius / maxRad;
            blueprint.principalRadii[i] = blueprint.principalRadii[i] * mul;
        }
        return blueprint;
    }

}
#endif
