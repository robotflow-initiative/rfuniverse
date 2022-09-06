using System;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;


namespace RFUniverse.Attributes
{
    public class CameraAttr : BaseCameraAttr
    {
        public static Shader cameraDepthShader = null;
        public static Shader cameraNormalShader = null;
        public static Shader cameraIDShader = null;
        protected override void Init()
        {
            base.Init();

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
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            base.AnalysisMsg(msg, type);
        }
        public override void GetRGB(int width, int height)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
            camera.RenderWithShader(null, "");
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            rgbBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public override void GetNormal(int width, int height)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
            camera.RenderWithShader(cameraNormalShader, "");
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            normalBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public override void GetID(int width, int height)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            camera.RenderWithShader(cameraIDShader, "");
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            idBase64String = Convert.ToBase64String(tex.EncodeToPNG());

        }
        public override void GetDepth(int width, int height, float near, float far)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear, 1);
            Shader.SetGlobalFloat("_CameraZeroDis", near);
            Shader.SetGlobalFloat("_CameraOneDis", far);
            camera.RenderWithShader(cameraDepthShader, "");
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            depthBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public override void GetDepthEXR(int width, int height)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1);
            Shader.SetGlobalFloat("_CameraZeroDis", 0);
            Shader.SetGlobalFloat("_CameraOneDis", 1);
            camera.RenderWithShader(cameraDepthShader, "");
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            depthEXRBase64String = Convert.ToBase64String(tex.EncodeToEXR(Texture2D.EXRFlags.CompressRLE));
        }
        public override void GetAmodalMask(int width, int height)
        {
            SetTempLayer(this);
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            camera.RenderWithShader(cameraIDShader, "");
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            RevertLayer(this);
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            amodalMaskBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
    }
}
