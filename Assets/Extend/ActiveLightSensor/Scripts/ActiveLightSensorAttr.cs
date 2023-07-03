using System;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse.Attributes
{
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
        public override void Init()
        {
            base.Init();
            irLight.enabled = false;
            leftCamera.enabled = false;
            rightCamera.enabled = false;
            //leftCamera.cullingMask = PlayerMain.Instance.simulationLayer;
            //rightCamera.cullingMask = PlayerMain.Instance.simulationLayer;
        }
        public override Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = base.CollectData();
            if (leftLRBase64String != null && rightLRBase64String != null)
            {
                data.Add("ir_left", leftLRBase64String);
                leftLRBase64String = null;
                data.Add("ir_right", rightLRBase64String);
                rightLRBase64String = null;
            }
            return data;
        }

        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "GetActiveDepth":
                    GetActiveDepth((float[,])data[0]);
                    return;
            }
            base.AnalysisData(type, data);
        }
        void GetActiveDepth(float[,] intrinsicMatrix)
        {
            Debug.Log("GetActiveDepth");
            leftLRBase64String = Convert.ToBase64String(GetCameraIR(intrinsicMatrix, true).EncodeToPNG());
            rightLRBase64String = Convert.ToBase64String(GetCameraIR(intrinsicMatrix, false).EncodeToPNG());
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