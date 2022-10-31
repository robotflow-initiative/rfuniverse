using System;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;
using RFUniverse;
using RFUniverse.Attributes;

public class ActiveLightSensorAttr : CameraAttr
{
    public Light irLight;
    public Camera leftCamera;
    public Camera rightCamera;

    protected string leftLRBase64String = null;
    protected string rightLRBase64String = null;
    public override string Name
    {
        get { return "ActiveLightSensor"; }
    }
    public override string Type
    {
        get { return "ActiveLightSensor"; }
    }
    public override void Init()
    {
        base.Init();
        irLight.enabled = false;
        leftCamera.enabled = false;
        rightCamera.enabled = false;
        //leftCamera.cullingMask = PlayerMain.Instance.simulationLayer;
        //rightCamera.cullingMask = PlayerMain.Instance.simulationLayer;
    }
    public override void CollectData(OutgoingMessage msg)
    {
        base.CollectData(msg);
        if (leftLRBase64String != null && rightLRBase64String != null)
        {
            msg.WriteBoolean(true);

            msg.WriteString(leftLRBase64String);
            // List<float> matrix = new List<float>();
            // for (int i = 0; i < 16; i++)
            // {
            //     matrix.Add(leftCamera.cameraToWorldMatrix[i]);
            // }
            // msg.WriteFloatList(matrix);
            // matrix.Clear();
            // for (int i = 0; i < 16; i++)
            // {
            //     matrix.Add(leftCamera.projectionMatrix[i]);
            // }
            // msg.WriteFloatList(matrix);
            leftLRBase64String = null;

            msg.WriteString(rightLRBase64String);
            // for (int i = 0; i < 16; i++)
            // {
            //     matrix.Add(rightCamera.cameraToWorldMatrix[i]);
            // }
            // msg.WriteFloatList(matrix);
            // matrix.Clear();
            // for (int i = 0; i < 16; i++)
            // {
            //     matrix.Add(rightCamera.projectionMatrix[i]);
            // }
            // msg.WriteFloatList(matrix);
            rightLRBase64String = null;
        }
        else
            msg.WriteBoolean(false);
    }

    public override void AnalysisMsg(IncomingMessage msg, string type)
    {
        switch (type)
        {
            case "GetActiveDepth":
                GetActiveDepth(msg);
                return;
        }
        base.AnalysisMsg(msg, type);
    }
    void GetActiveDepth(IncomingMessage msg)
    {
        Debug.Log("GetActiveDepth");
        List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
        leftLRBase64String = Convert.ToBase64String(GetCameraIR(intrinsicMatrix, true).EncodeToPNG());
        rightLRBase64String = Convert.ToBase64String(GetCameraIR(intrinsicMatrix, false).EncodeToPNG());
    }
    public Texture2D GetCameraIR(List<float> intrinsicMatrix, bool leftOrRight)
    {
        Vector2Int size = SetCameraIntrinsicMatrix(leftOrRight ? leftCamera : rightCamera, intrinsicMatrix);
        return GetCameraIR(size.x, size.y, leftOrRight ? leftCamera : rightCamera);
    }
    public Texture2D GetCameraIR(int width, int height, Camera cam)
    {
        irLight.enabled = true;
        PlayerMain.Instance.Sun.enabled = false;
        UnityEngine.Rendering.AmbientMode tempMode = RenderSettings.ambientMode;
        Color tempColor = RenderSettings.ambientLight;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;

        cam.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 8);
        cam.Render();
        RenderTexture.active = cam.targetTexture;
        tex.Reinitialize(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        RenderTexture.ReleaseTemporary(cam.targetTexture);
        irLight.enabled = false;
        PlayerMain.Instance.Sun.enabled = true;
        RenderSettings.ambientMode = tempMode;
        RenderSettings.ambientLight = tempColor;
        return tex;
    }
}