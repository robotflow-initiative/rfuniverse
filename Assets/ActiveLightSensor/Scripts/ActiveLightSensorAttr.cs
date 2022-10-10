using System;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;
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
        public override string Type
        {
            get { return "ActiveLightSensor"; }
        }
        protected override void Init()
        {
            base.Init();
            irLight.enabled = false;
            leftCamera.enabled = false;
            rightCamera.enabled = false;
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
            Vector2Int size = SetCameraIntrinsicMatrix(leftCamera, intrinsicMatrix);
            SetCameraIntrinsicMatrix(rightCamera, intrinsicMatrix);
            leftLRBase64String = GetCameraIR(size.x, size.y, leftCamera);
            rightLRBase64String = GetCameraIR(size.x, size.y, rightCamera);

        }
        public string GetCameraIR(int width, int height, Camera cam)
        {
            irLight.enabled = true;
            PlayerMain.Instance.sun.enabled = false;
            RenderSettings.ambientLight = Color.black;
            cam.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 8);
            cam.Render();
            RenderTexture.active = cam.targetTexture;
            tex.Reinitialize(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            RenderTexture.ReleaseTemporary(cam.targetTexture);
            irLight.enabled = false;
            PlayerMain.Instance.sun.enabled = true;
            RenderSettings.ambientLight = Color.gray;
            return Convert.ToBase64String(tex.EncodeToPNG());
        }
    }
}