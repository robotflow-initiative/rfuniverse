using System.IO;
using System;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine.Experimental.Rendering;

// TODO: Add a target display manager.

namespace RFUniverse.Attributes
{
    public class CameraAttr : BaseAttr
    {
        public static Shader cameraDepthShader = null;
        public static Shader cameraNormalShader = null;
        public static Shader cameraIDShader = null;
        Camera camera = null;
        Texture2D tex = null;

        string rgbBase64String = null;
        string normalBase64String = null;
        string depthBase64String = null;
        string idBase64String = null;
        string depthEXRBase64String = null;

        int width = 512;
        int height = 512;
        public override string Name
        {
            get { return "Camera"; }
        }
        public override string Type
        {
            get { return "Camera"; }
        }
        protected override void Init()
        {
            camera = GetComponent<Camera>();
            if (camera == null)
                camera = gameObject.AddComponent<Camera>();

            camera.enabled = false;

            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 1000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(1, 1, 1, 0);
            camera.depth = -100;
            camera.allowMSAA = true;
            camera.allowHDR = false;

            if (cameraDepthShader == null)
                cameraDepthShader = Shader.Find("RFUniverse/CameraDepth");
            if (cameraNormalShader == null)
                cameraNormalShader = Shader.Find("RFUniverse/CameraNormal");
            if (cameraIDShader == null)
                cameraIDShader = Shader.Find("RFUniverse/CameraID");

        }

        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
            // near plane
            msg.WriteFloat32(camera.nearClipPlane);
            // far plane
            msg.WriteFloat32(camera.farClipPlane);
            // FOV (angle in degree)
            msg.WriteFloat32(camera.fieldOfView);
            // target display
            msg.WriteInt32(camera.targetDisplay);
            // target width
            msg.WriteInt32(width);
            // target height
            msg.WriteInt32(height);

            if (rgbBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(rgbBase64String);
                rgbBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (normalBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(normalBase64String);
                normalBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (idBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(idBase64String);
                idBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (depthBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(depthBase64String);
                depthBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (depthEXRBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(depthEXRBase64String);
                depthEXRBase64String = null;
            }
            else
                msg.WriteBoolean(false);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "GetRGB":
                    GetRGB(msg);
                    return;
                case "GetNormal":
                    GetNormal(msg);
                    return;
                case "GetID":
                    GetID(msg);
                    return;
                case "GetDepth":
                    GetDepth(msg);
                    return;
                case "GetDepthEXR":
                    GetDepthEXR(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        void GetRGB(IncomingMessage msg)
        {
            //if (isConstant) return;
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            //string path = msg.ReadString();
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
            camera.RenderWithShader(null, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            rgbBase64String = Convert.ToBase64String(tex.EncodeToPNG());
            //File.WriteAllBytes(path, tex.EncodeToPNG());
        }
        void GetNormal(IncomingMessage msg)
        {
            //if (isConstant) return;
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            //string path = msg.ReadString();
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
            camera.RenderWithShader(cameraNormalShader, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            normalBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        void GetID(IncomingMessage msg)
        {
            //if (isConstant) return;
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            //string path = msg.ReadString();
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            camera.RenderWithShader(cameraIDShader, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            idBase64String = Convert.ToBase64String(tex.EncodeToPNG());

        }
        void GetDepth(IncomingMessage msg)
        {
            //if (isConstant) return;

            width = msg.ReadInt32();
            height = msg.ReadInt32();
            float near = msg.ReadFloat32();
            float far = msg.ReadFloat32();
            //string path = msg.ReadString();
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear, 1);
            Shader.SetGlobalFloat("_CameraZeroDis", near);
            Shader.SetGlobalFloat("_CameraOneDis", far);
            camera.RenderWithShader(cameraDepthShader, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            depthBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        void GetDepthEXR(IncomingMessage msg)
        {
            //if (isConstant) return;

            width = msg.ReadInt32();
            height = msg.ReadInt32();
            //float near = msg.ReadFloat32();
            //float far = msg.ReadFloat32();
            //string path = msg.ReadString();
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1);
            Shader.SetGlobalFloat("_CameraZeroDis", 0);
            Shader.SetGlobalFloat("_CameraOneDis", 1);
            camera.RenderWithShader(cameraDepthShader, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            depthEXRBase64String = Convert.ToBase64String(tex.EncodeToEXR(Texture2D.EXRFlags.CompressRLE));
            //File.WriteAllBytes(path, tex.EncodeToPNG());
        }
    }
}
