using System;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.IO;

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

        int width = 512;
        int height = 512;
        public override string Name
        {
            get
            {
                return "Camera";
            }
        }
        protected override void Init()
        {
            camera = GetComponent<Camera>();
            if (camera == null)
                camera = gameObject.AddComponent<Camera>();

            camera.enabled = false;

            camera.nearClipPlane = 0.01f;
            camera.nearClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(1, 1, 1, 0);

            if (cameraDepthShader == null)
                cameraDepthShader = Shader.Find("RFUniverse/CameraDepth");
            if (cameraNormalShader == null)
                cameraNormalShader = Shader.Find("RFUniverse/CameraNormal");
            if (cameraIDShader == null)
                cameraIDShader = Shader.Find("RFUniverse/CameraID");

        }

        public override void CollectData(OutgoingMessage msg)
        {
            msg.WriteString("Camera");
            // id
            msg.WriteInt32(ID);
            // position
            Vector3 position = camera.transform.position;
            msg.WriteFloat32(position.x);
            msg.WriteFloat32(position.y);
            msg.WriteFloat32(position.z);
            // rotation
            Vector3 rotation = camera.transform.eulerAngles;
            msg.WriteFloat32(rotation.x);
            msg.WriteFloat32(rotation.y);
            msg.WriteFloat32(rotation.z);
            // rendering mode
            //msg.WriteInt32((int)Mode);
            // near plane
            msg.WriteFloat32(camera.nearClipPlane);
            // far plane
            msg.WriteFloat32(camera.farClipPlane);
            // FOV (angle in degree)
            msg.WriteFloat32(camera.fieldOfView);
            // target display
            msg.WriteInt32(camera.targetDisplay);

            msg.WriteInt32(width);

            msg.WriteInt32(height);

            if (isConstant)
            {
                msg.WriteBoolean(true);
                msg.WriteString(Convert.ToBase64String(tex.EncodeToPNG()));
            }
            else if (tex != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(Convert.ToBase64String(tex.EncodeToPNG()));
                tex = null;
            }
            else
            {
                msg.WriteBoolean(false);
            }
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "GetNormalConstant":
                    GetNormalConstant(msg);
                    return;
                case "GetDepthConstant":
                    GetDepthConstant(msg);
                    return;
                case "StopConstant":
                    StopConstant();
                    return;
                case "GetNormal":
                    GetNormal(msg);
                    return;
                case "GetDepth":
                    GetDepth(msg);
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        bool isConstant = false;

        private void OnRenderObject()
        {
            if (isConstant)
            {
                camera.Render();
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
            }
        }
        void GetNormalConstant(IncomingMessage msg)
        {
            if (isConstant) return;
            isConstant = true;
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            camera.targetTexture = new RenderTexture(width, height, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
            camera.SetReplacementShader(cameraNormalShader, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height);
        }
        void GetDepthConstant(IncomingMessage msg)
        {
            if (isConstant) return;
            isConstant = true;
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            float near = msg.ReadFloat32();
            float far = msg.ReadFloat32();
            camera.targetTexture = new RenderTexture(width, height, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
            Shader.SetGlobalFloat("_CameraZeroDis", near);
            Shader.SetGlobalFloat("_CameraOneDis", far);
            camera.SetReplacementShader(cameraDepthShader, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height);
        }
        void StopConstant()
        {
            isConstant = false;
        }
        void GetNormal(IncomingMessage msg)
        {
            if (isConstant) return;
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            string path = msg.ReadString();
            camera.targetTexture = new RenderTexture(width, height, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
            camera.RenderWithShader(cameraNormalShader, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            //File.WriteAllBytes(path, tex.EncodeToPNG());
        }
        void GetDepth(IncomingMessage msg)
        {
            if (isConstant) return;

            width = msg.ReadInt32();
            height = msg.ReadInt32();
            float near = msg.ReadFloat32();
            float far = msg.ReadFloat32();
            string path = msg.ReadString();
            camera.targetTexture = new RenderTexture(width, height, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
            Shader.SetGlobalFloat("_CameraZeroDis", near);
            Shader.SetGlobalFloat("_CameraOneDis", far);
            camera.RenderWithShader(cameraDepthShader, "");
            RenderTexture.active = camera.targetTexture;
            tex = new Texture2D(width, height);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            //File.WriteAllBytes(path, tex.EncodeToPNG());
        }
    }
}
