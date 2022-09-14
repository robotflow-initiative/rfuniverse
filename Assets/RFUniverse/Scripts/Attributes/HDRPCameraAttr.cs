using System;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;


namespace RFUniverse.Attributes
{
    public class HDRPCameraAttr : BaseCameraAttr
    {
        public static Material cameraDepthMaterial = null;
        public static Material cameraNormalMaterial = null;
        public static Material cameraIDMaterial = null;
        HDAdditionalCameraData cameraHD;
        CustomPassVolume volume = null;
        DrawRenderersCustomPass pass = null;
        protected override void Init()
        {
            base.Init();
            if (cameraDepthMaterial == null)
                cameraDepthMaterial = Resources.Load<Material>("HDRPCameraDepth");
            if (cameraNormalMaterial == null)
                cameraNormalMaterial = Resources.Load<Material>("HDRPCameraNormal");
            if (cameraIDMaterial == null)
                cameraIDMaterial = Resources.Load<Material>("HDRPCameraID");


            cameraHD = GetComponent<HDAdditionalCameraData>() ?? gameObject.AddComponent<HDAdditionalCameraData>();
            var mask = new FrameSettingsOverrideMask();
            mask.mask = ~(new BitArray128());
            cameraHD.customRenderingSettings = true;
            cameraHD.renderingPathCustomFrameSettingsOverrideMask = mask;
            cameraHD.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, true);
            cameraHD.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Postprocess, false);
            cameraHD.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            volume = GetComponent<CustomPassVolume>();
            if (volume == null)
                volume = gameObject.AddComponent<CustomPassVolume>();
            volume.isGlobal = false;
            volume.targetCamera = camera;
            volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
            pass = (DrawRenderersCustomPass)volume.AddPassOfType<DrawRenderersCustomPass>();
            pass.targetColorBuffer = CustomPass.TargetBuffer.Camera;
            pass.targetDepthBuffer = CustomPass.TargetBuffer.Camera;
            pass.clearFlags = ClearFlag.All;
            pass.renderQueueType = CustomPass.RenderQueueType.All;
            pass.layerMask = camera.cullingMask;
            pass.overrideMaterialPassName = "ForwardOnly";
            pass.overrideDepthState = true;
            pass.depthCompareFunction = CompareFunction.LessEqual;
            pass.depthWrite = true;
            pass.sortingCriteria = SortingCriteria.RenderQueue;
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
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 8);
            cameraHD.customRenderingSettings = false;
            pass.enabled = false;
            camera.Render();
            RenderTexture.active = camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            rgbBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public override void GetNormal(int width, int height)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 8);
            cameraHD.customRenderingSettings = true;
            pass.enabled = true;
            pass.overrideMaterial = cameraNormalMaterial;
            //pass.overrideMaterialPassName = "ForwardOnly";
            camera.Render();

            RenderTexture.active = camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            normalBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }

        public override void GetID(int width, int height)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            cameraHD.customRenderingSettings = true;
            pass.enabled = true;
            pass.overrideMaterial = cameraIDMaterial;
            //pass.overrideMaterialPassName = "ForwardOnly";
            camera.Render();

            RenderTexture.active = camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            idBase64String = Convert.ToBase64String(tex.EncodeToPNG());

        }
        public override void GetDepth(int width, int height, float near, float far)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, GraphicsFormat.R8G8B8A8_SRGB, 1);
            cameraHD.customRenderingSettings = true;
            pass.enabled = true;
            pass.overrideMaterial = cameraDepthMaterial;
            //pass.overrideMaterialPassName = "ForwardOnly";
            cameraDepthMaterial.SetFloat("_CameraZeroDis", near);
            cameraDepthMaterial.SetFloat("_CameraOneDis", far);
            camera.Render();

            RenderTexture.active = camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.R8, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            depthBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }

        public override void GetDepthEXR(int width, int height)
        {
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear, 1);
            cameraHD.customRenderingSettings = true;
            pass.enabled = true;
            pass.overrideMaterial = cameraDepthMaterial;
            //pass.overrideMaterialPassName = "ForwardOnly";
            cameraDepthMaterial.SetFloat("_CameraZeroDis", 0);
            cameraDepthMaterial.SetFloat("_CameraOneDis", 1);
            camera.Render();

            RenderTexture.active = camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RFloat, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            depthEXRBase64String = Convert.ToBase64String(tex.EncodeToEXR(Texture2D.EXRFlags.CompressRLE));
        }

        public override void GetAmodalMask(int width, int height)
        {
            SetTempLayer(this);
            camera.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
            pass.enabled = true;
            pass.overrideMaterial = cameraIDMaterial;
            camera.Render();

            RevertLayer(this);
            RenderTexture.active = camera.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.ReleaseTemporary(camera.targetTexture);
            amodalMaskBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
    }
}
