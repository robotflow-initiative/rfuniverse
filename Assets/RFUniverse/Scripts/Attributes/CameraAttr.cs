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
        public override void Init()
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
        public override Texture2D GetRGB(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("GetRGB");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
            Camera.RenderWithShader(null, "");
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            rgbBase64String = Convert.ToBase64String(tex.EncodeToPNG());
            return tex;
        }
        public override Texture2D GetNormal(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("GetNormal");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
            Camera.RenderWithShader(cameraNormalShader, "");
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            normalBase64String = Convert.ToBase64String(tex.EncodeToPNG());
            return tex;
        }
        public override Texture2D GetID(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("GetID");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            Camera.RenderWithShader(cameraIDShader, "");
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            idBase64String = Convert.ToBase64String(tex.EncodeToPNG());
            return tex;
        }
        public override Texture2D GetIDSingleChannel(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("GetID");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            Camera.RenderWithShader(cameraIDShader, "");
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.R8, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            idBase64String = Convert.ToBase64String(tex.EncodeToPNG());
            return tex;
        }
        public override Texture2D GetDepth(int width, int height, float near, float far, float? unPhysicalFov = null)
        {
            Debug.Log("GetDepth");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
            Shader.SetGlobalFloat("_CameraZeroDis", near);
            Shader.SetGlobalFloat("_CameraOneDis", far);
            Camera.RenderWithShader(cameraDepthShader, "");
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.R8, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            depthBase64String = Convert.ToBase64String(tex.EncodeToPNG());
            return tex;
        }
        public override Texture2D GetDepthEXR(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("GetDepthEXR");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear, 1);
            Shader.SetGlobalFloat("_CameraZeroDis", 0);
            Shader.SetGlobalFloat("_CameraOneDis", 1);
            Camera.RenderWithShader(cameraDepthShader, "");
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RFloat, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            depthEXRBase64String = Convert.ToBase64String(tex.EncodeToEXR(Texture2D.EXRFlags.CompressRLE));
            return tex;
        }
        public override Texture2D GetAmodalMask(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("GetAmodalMask");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            SetTempLayer(this);
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            Camera.RenderWithShader(cameraIDShader, "");
            RevertLayer(this);
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            amodalMaskBase64String = Convert.ToBase64String(tex.EncodeToPNG());
            return tex;
        }
    }
}
