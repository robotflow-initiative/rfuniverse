using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using RFUniverse.Attributes;
using Robotflow.RFUniverse.SideChannels;
using UnityEditor;

//Digit脚本
//Config参数结构
public class DigitConfig
{
    public CameraConfig cameraConfig = new CameraConfig();
    public GelConfig gelConfig = new GelConfig();
    public LightConfig lightConfig = new LightConfig();
    public ForceConfig forceConfig = new ForceConfig();
}
//相机参数结构
public class CameraConfig
{
    public Vector3 position = new Vector3(0, 0, 0.015f);//位置
    public Vector3 rotation = new Vector3(-90, 0, 0);//旋转
    public float fov = 60;//FOV
    public float nearPlane = 0.01f;//近裁减平面

}
//Gel参数结构
public class GelConfig
{
    public Vector3 position = new Vector3(0, 0.022f, 0.015f);//位置
    public float width = 0.02f;//宽
    public float height = 0.03f;//高
    public bool curvature = true;//应用球面
    public float curvatureMax = 0.005f;//最大偏移
    public float r = 0.1f;//球面半径
    public int meshVerticesCountW = 100;//生成网格宽度上的顶点数
    public int meshVerticesCountH => (int)(meshVerticesCountW * height / width);
    public int resolutionW = 100;//输出图像宽度上的像素
    public int resolutionH => (int)(resolutionW * height / width);
}
//灯光参数结构
public class LightConfig
{
    public Vector3 centerPosition = new Vector3(0, 0.005f, 0.015f);//中心位置
    public bool spot = true;//方向光
    public float[] spotAngle = new float[3] { 60f, 60f, 60f };//spotAngle
    public bool polar = true;//应用极坐标
    public Vector3[] cartesianPosition = new Vector3[3] { new Vector3(-0.01732f, 0, -0.01f), new Vector3(0.01732f, 0, -0.01f), new Vector3(0, 0, 0.02f) };//三维坐标位置
    public Vector3[] cartesianRotaiton = new Vector3[3] { new Vector3(-0.01732f, 0, -0.01f), new Vector3(0.01732f, 0, -0.01f), new Vector3(0, 0, 0.02f) };//三维坐标旋转
    public float[] polarDistance = new float[3] { 0.02f, 0.02f, 0.02f };//极坐标距离
    public float[] polarAngle = new float[3] { 210, 330, 90 };//极坐标角度
    public Color[] color = new Color[3] { Color.red, Color.green, Color.blue };//灯光颜色
    public float[] intensity = new float[3] { 1, 1, 1 };//灯光强度
}
//力参数结构
public class ForceConfig
{
    public bool enable = true;//应用受力变形
    public float minForce = 0;//最小变形受力
    public float maxForce = 0.01f;//最大变形受力
    public float maxDeformation = 0.005f;//最大变形

}
public class DigitAttr : BaseAttr
{
    public const int startLayer = 22;//22层开始都是digit预留层
    public Camera cameraLight;//光照相机
    public Camera cameraDepth;//深度相机
    //private RenderTexture lightTex;//光照RT
    //private RenderTexture depthTex;//深度RT
    public Shader depthShader;//深度Shdaer
    public MeshRenderer gel;//Gel
    public Transform lightCenter;//灯光中心
    public Light light0;//灯光1
    public Light light1;//灯光2
    public Light light2;//灯光3

    public bool showImage = true;

    public RawImage lightImage;//灯光UI
    public RawImage depthImage;//深度UI

    public Transform proxy;//光照相机
    public Camera cameraLightProxy;//光照相机
    public MeshRenderer gelProxy;//Gel
    public Transform lightCenterProxy;//灯光中心
    public Light light0Proxy;//灯光1
    public Light light1Proxy;//灯光2
    public Light light2Proxy;//灯光3 
    List<Color> defultDepth = new List<Color>();
    Texture2D tex;

    private static List<DigitAttr> Digits => BaseAttr.Attrs.Where(s => (s.Value is DigitAttr)).Select(s => (DigitAttr)s.Value).ToList();
    public int index => Digits.IndexOf(this);

    public override string Type
    {
        get { return "Digit"; }
    }
    public override void Init()
    {
        tex = new Texture2D(1, 1);
        lightImage.canvas.gameObject.SetActive(showImage);
        //读取config
        ReadConfig();

        //设置深度相机材质
        cameraDepth.SetReplacementShader(depthShader, "");
        //将RT赋予相应UI
        lightImage.texture = cameraLight.targetTexture;
        depthImage.texture = cameraDepth.targetTexture;

        tex = GetDepth();
        foreach (var item in tex.GetPixels())
        {
            defultDepth.Add(item);
        }
    }
    protected override void AfterRigister()
    {
        //根据index改变UI位置
        //lightImage.rectTransform.sizeDelta = new Vector2(Digits[0].lightImage.rectTransform.sizeDelta.x, Digits[0].lightImage.rectTransform.sizeDelta.y);
        //depthImage.rectTransform.sizeDelta = lightImage.rectTransform.sizeDelta;
        //lightImage.rectTransform.anchoredPosition = new Vector2(lightImage.rectTransform.sizeDelta.x * index, lightImage.rectTransform.sizeDelta.y);
        //depthImage.rectTransform.anchoredPosition = new Vector2(depthImage.rectTransform.sizeDelta.x * index, depthImage.rectTransform.sizeDelta.y);

        //根据index设置gel的层，灯光照射等层，相机渲染的层
        gel.gameObject.layer = index + startLayer;
        light0.cullingMask = 1 << (index + startLayer);
        light1.cullingMask = 1 << (index + startLayer);
        light2.cullingMask = 1 << (index + startLayer);
        cameraLight.cullingMask = 1 << (index + startLayer);
        cameraDepth.cullingMask = 1 << (index + startLayer);

        SetProxy();
    }

    static DigitConfig config = null;//全局config实例
    //读取Config
    void ReadConfig()
    {
        //已读取后不再读取
        if (config == null)
        {
            string path = $"{Application.streamingAssetsPath}/DigitConfig.xml";
            string xml = File.ReadAllText(path);
            config = XMLHelper.XMLToObject<DigitConfig>(xml);
            File.WriteAllText(path, XMLHelper.ObjectToXML(config));
        }
        //应用相应参数
        cameraLight.transform.localPosition = config.cameraConfig.position;
        cameraLight.transform.localEulerAngles = config.cameraConfig.rotation;
        cameraLight.fieldOfView = config.cameraConfig.fov;
        cameraLight.nearClipPlane = config.cameraConfig.nearPlane;

        cameraDepth.transform.localPosition = config.cameraConfig.position;
        cameraDepth.transform.localEulerAngles = config.cameraConfig.rotation;
        cameraDepth.fieldOfView = config.cameraConfig.fov;
        cameraDepth.nearClipPlane = config.cameraConfig.nearPlane;

        //设置相机RT
        cameraLight.targetTexture = RenderTexture.GetTemporary(config.gelConfig.resolutionW, config.gelConfig.resolutionH, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
        cameraDepth.targetTexture = RenderTexture.GetTemporary(config.gelConfig.resolutionW, config.gelConfig.resolutionH, 24, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);


        Shader.SetGlobalFloat("_CameraZeroDis", gel.transform.localPosition.y * transform.lossyScale.x);
        Shader.SetGlobalFloat("_CameraOneDis", (gel.transform.localPosition.y - config.gelConfig.curvatureMax) * transform.lossyScale.x);

        gel.transform.localPosition = config.gelConfig.position;
        // gel.sharedMaterial.SetFloat("r", config.gelConfig.r);
        // gel.sharedMaterial.SetFloat("max", config.gelConfig.curvatureMax);
        if (m == null)
            m = GenerateGelMesh(config.gelConfig.width, config.gelConfig.height, config.gelConfig.meshVerticesCountW, config.gelConfig.meshVerticesCountH, config.gelConfig.r, config.gelConfig.curvatureMax, config.gelConfig.curvature, gel.transform, cameraDepth);
        gel.GetComponent<MeshFilter>().mesh = m;

        lightCenter.localPosition = config.lightConfig.centerPosition;
        if (config.lightConfig.spot)
        {
            light0.type = LightType.Spot;
            light1.type = LightType.Spot;
            light2.type = LightType.Spot;
            light0.spotAngle = config.lightConfig.spotAngle[0];
            light1.spotAngle = config.lightConfig.spotAngle[1];
            light2.spotAngle = config.lightConfig.spotAngle[2];
        }
        else
        {
            light0.type = LightType.Point;
            light1.type = LightType.Point;
            light2.type = LightType.Point;
            light0.cookie = null;
            light1.cookie = null;
            light2.cookie = null;
        }
        light0.color = config.lightConfig.color[0];
        light1.color = config.lightConfig.color[1];
        light2.color = config.lightConfig.color[2];
        light0.intensity = config.lightConfig.intensity[0];
        light1.intensity = config.lightConfig.intensity[1];
        light2.intensity = config.lightConfig.intensity[2];
        if (config.lightConfig.polar)
        {
            float angel;
            float ydx;
            angel = config.lightConfig.polarAngle[0];
            ydx = Mathf.Tan(Mathf.Deg2Rad * angel);
            light0.transform.localPosition = (new Vector3(Mathf.Cos(Mathf.Deg2Rad * angel), 0, Mathf.Sin(Mathf.Deg2Rad * angel))) * config.lightConfig.polarDistance[0];

            angel = config.lightConfig.polarAngle[1];
            ydx = Mathf.Tan(Mathf.Deg2Rad * angel);
            light1.transform.localPosition = (new Vector3(Mathf.Cos(Mathf.Deg2Rad * angel), 0, Mathf.Sin(Mathf.Deg2Rad * angel))) * config.lightConfig.polarDistance[1];

            angel = config.lightConfig.polarAngle[2];
            ydx = Mathf.Tan(Mathf.Deg2Rad * angel);
            light2.transform.localPosition = (new Vector3(Mathf.Cos(Mathf.Deg2Rad * angel), 0, Mathf.Sin(Mathf.Deg2Rad * angel))) * config.lightConfig.polarDistance[2];
        }
        else
        {
            light0.transform.localPosition = config.lightConfig.cartesianPosition[0];
            light1.transform.localPosition = config.lightConfig.cartesianPosition[1];
            light2.transform.localPosition = config.lightConfig.cartesianPosition[2];
            light0.transform.localEulerAngles = config.lightConfig.cartesianRotaiton[0];
            light1.transform.localEulerAngles = config.lightConfig.cartesianRotaiton[1];
            light2.transform.localEulerAngles = config.lightConfig.cartesianRotaiton[2];
        }

    }
    void SetProxy()
    {
        proxy.SetParent(null);
        proxy.position = Vector3.up * 100;
        //应用相应参数
        cameraLightProxy.transform.localPosition = cameraLight.transform.localPosition;
        cameraLightProxy.transform.localEulerAngles = cameraLight.transform.localEulerAngles;
        cameraLightProxy.fieldOfView = cameraLight.fieldOfView;
        cameraLightProxy.nearClipPlane = cameraLight.nearClipPlane;

        RenderTexture lightTexProxy = RenderTexture.GetTemporary(config.gelConfig.resolutionW, config.gelConfig.resolutionH, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
        //将RT赋予相应相机
        cameraLightProxy.targetTexture = lightTexProxy;

        gelProxy.transform.localPosition = gel.transform.localPosition;
        gelProxy.GetComponent<MeshFilter>().mesh = m;

        lightCenterProxy.localPosition = lightCenter.localPosition;
        light0Proxy.type = light0.type;
        light1Proxy.type = light1.type;
        light2Proxy.type = light2.type;
        light0Proxy.spotAngle = light0.spotAngle;
        light1Proxy.spotAngle = light1.spotAngle;
        light2Proxy.spotAngle = light2.spotAngle;
        light0Proxy.cookie = light0.cookie;
        light1Proxy.cookie = light1.cookie;
        light2Proxy.cookie = light2.cookie;
        light0Proxy.color = light0.color;
        light1Proxy.color = light1.color;
        light2Proxy.color = light2.color;
        light0Proxy.intensity = light0.intensity;
        light1Proxy.intensity = light1.intensity;
        light2Proxy.intensity = light2.intensity;
        light0Proxy.transform.localPosition = light0.transform.localPosition;
        light1Proxy.transform.localPosition = light1.transform.localPosition;
        light2Proxy.transform.localPosition = light2.transform.localPosition;
        light0Proxy.transform.localEulerAngles = light0.transform.localEulerAngles;
        light1Proxy.transform.localEulerAngles = light1.transform.localEulerAngles;
        light2Proxy.transform.localEulerAngles = light2.transform.localEulerAngles;

        gelProxy.gameObject.layer = gel.gameObject.layer;
        light0Proxy.cullingMask = light0.cullingMask;
        light1Proxy.cullingMask = light1.cullingMask;
        light2Proxy.cullingMask = light2.cullingMask;
        cameraLightProxy.cullingMask = cameraLight.cullingMask;
    }

    static Mesh m = null;
    //生成Gel网格
    public static Mesh GenerateGelMesh(float width, float height, int countW, int countH, float r, float max, bool curvature, Transform gel, Camera camera)
    {
        if (!curvature)
        {
            countW = 2;
            countH = 2;
            max = 0;
        }
        Mesh m = new Mesh();
        List<Vector3> v = new List<Vector3>();
        List<Vector2> u = new List<Vector2>();
        float w = -width / 2;
        float dMax = r - Mathf.Sqrt(r * r - width * width / 4 - height * height / 4);
        for (int i = 0; i < countW; i++)
        {
            float h = -height / 2;
            for (int j = 0; j < countH; j++)
            {
                float d = r - Mathf.Sqrt(r * r - w * w - h * h);
                d = d / dMax * max;
                Vector3 point = new Vector3(w, h, d);
                v.Add(point);
                point = camera.WorldToViewportPoint(gel.TransformPoint(point));
                u.Add(point);
                h += height / (countH - 1);
            }
            w += width / (countW - 1);
        }
        m.vertices = v.ToArray();
        m.uv = u.ToArray();
        List<int> t = new List<int>();
        for (int i = 0; i < m.vertices.Length; i++)
        {
            if (i % countH < countH - 1 && i + countH < m.vertices.Length)
            {
                t.Add(i);
                t.Add(i + countH);
                t.Add(i + countH + 1);
                t.Add(i);
                t.Add(i + countH + 1);
                t.Add(i + 1);
            }
        }
        m.triangles = t.ToArray();
        return m;
    }
    public override void CollectData(OutgoingMessage msg)
    {
        base.CollectData(msg);
        if (lightBase64String != null)
        {
            msg.WriteBoolean(true);
            msg.WriteString(lightBase64String);
            lightBase64String = null;
            msg.WriteString(depthBase64String);
            depthBase64String = null;
        }
        else
            msg.WriteBoolean(false);
    }
    public override void AnalysisMsg(IncomingMessage msg, string type)
    {
        switch (type)
        {
            case "GetData":
                GetData();
                return;
        }
        base.AnalysisMsg(msg, type);
    }
    string lightBase64String = null;
    string depthBase64String = null;

    public void GetData()
    {
        tex = GetDepth();
        List<Color> depth = tex.GetPixels().ToList();

        for (int i = 0; i < depth.Count; i++)
        {
            depth[i] -= defultDepth[i];
        }
        float sourceMax = depth.Max(s => s.grayscale);
        GaussianBlur gauss = new GaussianBlur(5);
        gauss.SetSourceImage(depth, tex.width, tex.height);
        Color[] blurDepth = gauss.GetBlurImage();
        float blurMax = blurDepth.Max(s => s.grayscale);
        float scale = sourceMax / blurMax;

        float zero = Shader.GetGlobalFloat("_CameraZeroDis");
        float one = Shader.GetGlobalFloat("_CameraOneDis");

        for (int i = 0; i < depth.Count; i++)
        {
            blurDepth[i] *= scale;
        }
        tex.SetPixels(blurDepth.ToArray());
        //tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        gelProxy.material.SetTexture("_Offset", tex);
        gelProxy.material.SetFloat("_Scale", Mathf.Abs(one - zero));

        cameraLightProxy.Render();
        RenderTexture.active = cameraLightProxy.targetTexture;
        tex.Reinitialize(cameraLightProxy.targetTexture.width, cameraLightProxy.targetTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        lightBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        //File.WriteAllBytes("/home/yanbing/rgb.png", tex.EncodeToPNG());

        for (int i = 0; i < depth.Count; i++)
        {
            blurDepth[i] += defultDepth[i];
        }
        tex.Reinitialize(cameraDepth.targetTexture.width, cameraDepth.targetTexture.height, TextureFormat.R8, false);
        tex.SetPixels(blurDepth.ToArray());
        tex.Apply();
        depthBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        //File.WriteAllBytes("/home/yanbing/depth.png", tex.EncodeToPNG());
    }

    public Texture2D GetLight()
    {
        cameraLight.Render();
        RenderTexture.active = cameraLight.targetTexture;
        tex.Reinitialize(cameraLight.targetTexture.width, cameraLight.targetTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        return tex;
    }
    public Texture2D GetDepth()
    {
        cameraDepth.Render();
        RenderTexture.active = cameraDepth.targetTexture;
        tex.Reinitialize(cameraDepth.targetTexture.width, cameraDepth.targetTexture.height, TextureFormat.R8, false);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //        GetData();
    //}

    public List<GameObject> renders = new();
    //碰撞时接收到CollisionEvent发送来的消息
    public void CollisionStay(Transform target, GameObject render, float force)
    {
        if (!renders.Contains(render))
            renders.Add(render);
        //变形
        float deformation = 0;
        //如果启用变形，则根据压力改变Render位置
        if (config.forceConfig.enable)
        {
            force = force < config.forceConfig.minForce ? 0 : force;
            deformation = Mathf.Min(force / config.forceConfig.maxForce, 1) * config.forceConfig.maxDeformation;
        }
        //应用Render位置和旋转
        render.transform.position = target.transform.position - transform.up * deformation;
        render.transform.rotation = target.transform.rotation;
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(DigitAttr), true)]
    public class DigitAttrEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DigitAttr script = target as DigitAttr;
            if (GUILayout.Button("GetLight"))
            {
                Texture2D tex = script.GetLight();
                File.WriteAllBytes($"{Application.streamingAssetsPath}/ImageRuntime/DigitLight.png", tex.EncodeToPNG());
            }
            if (GUILayout.Button("GetDepth"))
            {
                Texture2D tex = script.GetDepth();
                File.WriteAllBytes($"{Application.streamingAssetsPath}/ImageRuntime/DigitDepth.png", tex.EncodeToPNG());
            }
        }
    }
#endif
}

