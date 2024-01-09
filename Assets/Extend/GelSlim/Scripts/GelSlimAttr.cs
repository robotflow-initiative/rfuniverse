using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using RFUniverse.Attributes;
using System.Threading.Tasks;
using RFUniverse.Manager;
using RFUniverse;

public class GelSlimAttr : BaseAttr
{
    public Camera cameraLight;//光照相机
    public Camera cameraDepth;//深度相机

    public Shader depthShader;//深度Shdaer

    public MeshRenderer gel;//Gel

    public Light[] lights;

    public Transform renderParent;
    public bool enableForceOffset;
    public float maxForce = 0.05f;
    public float maxDeformation = 0.005f;
    Texture2D tex;
    Mesh sourceMesh;
    public int layer;
    public override void Init()
    {
        tex = new Texture2D(1, 1);

        cameraLight.nearClipPlane = 0.001f;
        cameraDepth.nearClipPlane = 0.001f;

        sourceMesh = gel.GetComponent<MeshFilter>().sharedMesh;
        //设置深度相机材质
        cameraDepth.SetReplacementShader(depthShader, "");

        //根据index设置gel的层，灯光照射等层，相机渲染的层
        layer = LayerManager.Instance.GetLayer();
        gel.gameObject.layer = layer;
        foreach (Light ligjt in lights)
        {
            ligjt.cullingMask = 1 << layer;
        }
        cameraLight.cullingMask = 1 << layer;
        cameraDepth.cullingMask = 1 << layer;
    }

    [RFUAPI]
    public void GetData()
    {
        CollectData.AddDataNextStep("light", Convert.ToBase64String(GetLight().EncodeToPNG()));
        CollectData.AddDataNextStep("depth", Convert.ToBase64String(GetDepth().EncodeToPNG()));
    }

    public Texture2D GetLight()
    {
        cameraLight.targetTexture = RenderTexture.GetTemporary(1600, 1200, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
        cameraLight.Render();
        RenderTexture.active = cameraLight.targetTexture;
        tex.Reinitialize(RenderTexture.active.width, RenderTexture.active.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        RenderTexture.ReleaseTemporary(cameraLight.targetTexture);
        cameraLight.targetTexture = null;
        return tex;
    }
    public Texture2D GetDepth()
    {
        Shader.SetGlobalFloat("_CameraZeroDis", (gel.transform.localPosition.y - cameraDepth.transform.localPosition.y) * transform.lossyScale.x);
        Shader.SetGlobalFloat("_CameraOneDis", (gel.transform.localPosition.y - cameraDepth.transform.localPosition.y - 0.003f) * transform.lossyScale.x);
        cameraDepth.targetTexture = RenderTexture.GetTemporary(1600, 1200, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
        cameraDepth.Render();
        RenderTexture.active = cameraDepth.targetTexture;
        tex.Reinitialize(RenderTexture.active.width, RenderTexture.active.height, TextureFormat.R8, false);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        RenderTexture.ReleaseTemporary(cameraDepth.targetTexture);
        cameraDepth.targetTexture = null;
        return tex;
    }

    [RFUAPI]
    public void BlurGel(int radius = 5, float sigma = 2)
    {
        cameraDepth.orthographic = true;
        Texture2D tex = GetDepth();
        cameraDepth.orthographic = false;
        GaussianBlur gauss = new GaussianBlur(radius, sigma);
        //GaussianBlurGPU gauss = GetComponent<GaussianBlurGPU>();
        //gauss.SetGaussianKernel(20, 10);
        tex = gauss.Blur(tex);
        gel.GetComponent<MeshFilter>().sharedMesh = MoveGelPlane(sourceMesh, tex, 0.003f);
        renderParent.gameObject.SetActive(false);
    }
    [RFUAPI]
    public void RestoreGel()
    {
        renderParent.gameObject.SetActive(true);
        gel.GetComponent<MeshFilter>().sharedMesh = sourceMesh;
    }

    Mesh MoveGelPlane(Mesh sourceMesh, Texture2D depth, float scale)
    {
        Mesh mesh = Instantiate(sourceMesh);
        Vector3[] vertices = sourceMesh.vertices;
        Vector2[] uvs = sourceMesh.uv;
        Color[] colors = depth.GetPixels();
        int height = depth.height;
        int width = depth.width;

        Parallel.For(0, vertices.Length, i =>
        {
            Vector2 uv = uvs[i];
            // 获取灰度值（0到1之间）
            Color displacementColor = colors[Mathf.FloorToInt(uv.x * height) * width + Mathf.FloorToInt(uv.y * width)];

            // 根据灰度值偏移顶点
            float displacementAmount = displacementColor.r * scale;
            Vector3 displacement = Vector3.forward * displacementAmount;

            vertices[i] += displacement;
        });
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        return mesh;
    }


    public List<GameObject> renders = new List<GameObject>();
    //碰撞时接收到CollisionEvent发送来的消息
    public void CollisionStay(Transform target, GameObject render, float force)
    {
        if (!renders.Contains(render))
            renders.Add(render);
        //变形
        float deformation = 0;
        //如果启用变形，则根据压力改变Render位置
        if (enableForceOffset)
        {
            deformation = Mathf.Min(force / maxForce, 1) * maxDeformation;
        }
        //应用Render位置和旋转
        render.transform.position = target.transform.position - transform.up * deformation;
        render.transform.rotation = target.transform.rotation;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(GelSlimAttr), true)]
    public class GelSlimEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GelSlimAttr script = target as GelSlimAttr;
            GUILayout.Space(10);
            GUILayout.Label("Editor Tool:");
            if (GUILayout.Button("GetLight"))
            {
                Texture2D tex = script.GetLight();
                File.WriteAllBytes($"{Application.streamingAssetsPath}/ImageEditor/DigitLight.png", tex.EncodeToPNG());
            }
            if (GUILayout.Button("GetDepth"))
            {
                Texture2D tex = script.GetDepth();
                File.WriteAllBytes($"{Application.streamingAssetsPath}/ImageEditor/DigitDepth.png", tex.EncodeToPNG());
            }
            if (GUILayout.Button("BlurGel"))
            {
                script.BlurGel();
            }
            if (GUILayout.Button("RestoreGel"))
            {
                script.RestoreGel();
            }
        }
    }
#endif
}

