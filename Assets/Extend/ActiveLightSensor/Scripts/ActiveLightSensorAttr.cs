using System;
using UnityEngine;

namespace RFUniverse.Attributes
{
    public class ActiveLightSensorAttr : CameraAttr
    {
        public Light irLight;
        public Camera leftCamera;
        public Camera rightCamera;

        public override string Name
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

        [RFUAPI]
        void GetActiveDepth(float[,] intrinsicMatrix)
        {
            CollectData.AddDataNextStep("ir_left", Convert.ToBase64String(GetCameraIR(intrinsicMatrix, true).EncodeToPNG()));
            CollectData.AddDataNextStep("ir_right", Convert.ToBase64String(GetCameraIR(intrinsicMatrix, false).EncodeToPNG()));
        }
        public Texture2D GetCameraIR(float[,] intrinsicMatrix, bool leftOrRight)
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
}