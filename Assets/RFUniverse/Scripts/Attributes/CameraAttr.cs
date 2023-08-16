using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace RFUniverse.Attributes
{
    public class CameraAttr : BaseCameraAttr
    {
        public static Shader cameraDepthShader = null;
        public static Shader cameraNormalShader = null;
        public static Shader cameraIDShader = null;
        public static Shader cameraMotionVectorShader = null;
        public override void Init()
        {
            base.Init();

            if (cameraDepthShader == null)
                cameraDepthShader = Shader.Find("RFUniverse/CameraDepth");
            if (cameraNormalShader == null)
                cameraNormalShader = Shader.Find("RFUniverse/CameraNormal");
            if (cameraIDShader == null)
                cameraIDShader = Shader.Find("RFUniverse/CameraID");
            if (cameraMotionVectorShader == null)
                cameraMotionVectorShader = Shader.Find("RFUniverse/CameraMotionVector");
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
            tex.Reinitialize(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
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
            tex.Reinitialize(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
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
            return tex;
        }
        public override Texture2D GetAmodalMask(int id, int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("GetAmodalMask");
            if (!ActiveAttrs.ContainsKey(id)) return null;
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            int originCameraLayers = Camera.cullingMask;
            Camera.cullingMask = 1 << PlayerMain.Instance.tempLayer;
            List<int> originLayers = SetTempLayer(ActiveAttrs[id]);
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            Camera.RenderWithShader(cameraIDShader, "");
            RevertLayer(ActiveAttrs[id], originLayers);
            Camera.cullingMask = originCameraLayers;
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            return tex;
        }
        public override Texture2D GetMotionVector(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("GetMotionVector");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            Camera.RenderWithShader(cameraMotionVectorShader, "");
            RenderTexture.active = Camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            return tex;
        }
    }



}
