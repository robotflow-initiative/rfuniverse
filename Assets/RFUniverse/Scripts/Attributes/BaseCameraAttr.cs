using System;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;


namespace RFUniverse.Attributes
{
    public abstract class BaseCameraAttr : BaseAttr
    {
        protected Camera camera = null;
        protected Texture2D tex = null;

        protected string rgbBase64String = null;
        protected string normalBase64String = null;
        protected string idBase64String = null;
        protected string depthBase64String = null;
        protected string depthEXRBase64String = null;
        protected string amodalMaskBase64String = null;

        protected int width = 512;
        protected int height = 512;

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
            base.Init();
            tex = new Texture2D(1, 1);
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
            //camera.cullingMask = PlayerMain.Instance.simulationLayer;
        }
        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
            // FOV (angle in degree)
            msg.WriteFloat32(camera.fieldOfView);
            List<float> matrix = new List<float>();
            for (int i = 0; i < 16; i++)
            {
                matrix.Add(camera.projectionMatrix[i]);
            }
            msg.WriteFloatList(matrix);
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
            if (amodalMaskBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(amodalMaskBase64String);
                amodalMaskBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (ddBBOX != null)
            {
                msg.WriteBoolean(true);
                msg.WriteInt32(ddBBOX.Count);
                foreach (var item in ddBBOX)
                {
                    msg.WriteFloat32(item.x);
                    msg.WriteFloat32(item.y);
                    msg.WriteFloat32(item.width);
                    msg.WriteFloat32(item.height);
                }
                ddBBOX = null;
            }
            else
                msg.WriteBoolean(false);
            if (dddBBOX != null)
            {
                msg.WriteBoolean(true);
                msg.WriteInt32(dddBBOX.Count);
                foreach (var item in dddBBOX)
                {
                    msg.WriteFloat32(item.Item1.x);
                    msg.WriteFloat32(item.Item1.y);
                    msg.WriteFloat32(item.Item1.z);
                    msg.WriteFloat32(item.Item2.x);
                    msg.WriteFloat32(item.Item2.y);
                    msg.WriteFloat32(item.Item2.z);
                    msg.WriteFloat32(item.Item3.x);
                    msg.WriteFloat32(item.Item3.y);
                    msg.WriteFloat32(item.Item3.z);
                }
                dddBBOX = null;
            }
            else
                msg.WriteBoolean(false);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "AlignView":
                    AlignView();
                    return;
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
                case "GetAmodalMask":
                    GetAmodalMask(msg);
                    return;
                case "Get2DBBOX":
                    Get2DBBOX();
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        void AlignView()
        {
            transform.position = PlayerMain.Instance.mainCamera.transform.position;
            transform.rotation = PlayerMain.Instance.mainCamera.transform.rotation;
        }
        void GetRGB(IncomingMessage msg)
        {
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            GetRGB(width, height);
        }
        public abstract void GetRGB(int width, int height);
        void GetNormal(IncomingMessage msg)
        {
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            GetNormal(width, height);
        }
        public abstract void GetNormal(int width, int height);
        void GetID(IncomingMessage msg)
        {
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            GetID(width, height);
        }
        public abstract void GetID(int width, int height);
        void GetDepth(IncomingMessage msg)
        {
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            float near = msg.ReadFloat32();
            float far = msg.ReadFloat32();
            GetDepth(width, height, near, far);
        }
        public abstract void GetDepth(int width, int height, float near, float far);
        void GetDepthEXR(IncomingMessage msg)
        {
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            GetDepthEXR(width, height);
        }
        public abstract void GetDepthEXR(int width, int height);
        void GetAmodalMask(IncomingMessage msg)
        {
            width = msg.ReadInt32();
            height = msg.ReadInt32();
            GetAmodalMask(width, height);
        }
        public abstract void GetAmodalMask(int width, int height);

        List<int> originLayers = new List<int>();
        protected void SetTempLayer(BaseAttr target)
        {
            originLayers = new List<int>();
            foreach (var item in target.GetChildComponentFilter<Renderer>())
            {
                if ((PlayerMain.Instance.simulationLayer.value & item.gameObject.layer) > 0)
                    item.gameObject.layer = PlayerMain.Instance.tempLayer;
            }
            camera.cullingMask = 1 << PlayerMain.Instance.tempLayer;
        }
        protected void RevertLayer(BaseAttr target)
        {
            List<Renderer> trans = target.GetChildComponentFilter<Renderer>();
            for (int i = 0; i < trans.Count; i++)
            {
                if ((PlayerMain.Instance.simulationLayer.value & trans[i].gameObject.layer) > 0)
                    trans[i].gameObject.layer = originLayers[i];
            }
            camera.cullingMask = PlayerMain.Instance.simulationLayer;
        }

        List<Rect> ddBBOX = null;
        void Get2DBBOX()
        {
            ddBBOX = new List<Rect>();
            foreach (var item in BaseAttr.Attrs.Values)
            {
                Rect rect = Get2DBBOX(item);
                if (rect.max.x > 0 && rect.max.y > 0 && rect.min.x < camera.pixelWidth && rect.min.y < camera.pixelHeight)
                    ddBBOX.Add(rect);
            }
        }
        Rect Get2DBBOX(BaseAttr attr)
        {
            float maxX = float.MinValue;
            float minX = float.MaxValue;
            float maxY = float.MinValue;
            float minY = float.MaxValue;
            foreach (var render in attr.GetChildComponentFilter<MeshFilter>())
            {
                List<Vector3> vertices = new List<Vector3>();
                render.mesh.GetVertices(vertices);

                foreach (var item in vertices)
                {
                    Vector3 point = render.transform.TransformPoint(item);
                    point = camera.WorldToScreenPoint(point);
                    if (point.x > maxX) maxX = point.x;
                    if (point.x < minX) minX = point.x;
                    if (point.y > maxY) maxY = point.y;
                    if (point.y < minY) minY = point.y;
                }
            }
            return new Rect((maxX + minX) / 2, (maxY + minY) / 2, maxX - minX, maxY - minY);
        }
        List<Tuple<Vector3, Vector3, Vector3>> dddBBOX = null;
        void Get3DBBOX()
        {
            dddBBOX = new List<Tuple<Vector3, Vector3, Vector3>>();
            foreach (var item in BaseAttr.Attrs.Values)
            {
                Tuple<Vector3, Vector3, Vector3> box = Get3DBBOX(item);
                Vector3 center = camera.WorldToScreenPoint(box.Item1);
                if (center.x > 0 && center.y > 0 && center.x < camera.pixelWidth && center.y < camera.pixelHeight)
                    dddBBOX.Add(box);
            }
        }
        Tuple<Vector3, Vector3, Vector3> Get3DBBOX(BaseAttr attr)
        {
            float maxX = float.MinValue;
            float minX = float.MaxValue;
            float maxY = float.MinValue;
            float minY = float.MaxValue;
            float maxZ = float.MinValue;
            float minZ = float.MaxValue;
            foreach (var render in attr.GetChildComponentFilter<MeshFilter>())
            {
                List<Vector3> vertices = new List<Vector3>();
                render.mesh.GetVertices(vertices);
                foreach (var item in vertices)
                {
                    Vector3 point = render.transform.TransformPoint(item);
                    point = attr.transform.InverseTransformPoint(point);
                    if (point.x > maxX) maxX = point.x;
                    if (point.x < minX) minX = point.x;
                    if (point.y > maxY) maxY = point.y;
                    if (point.y < minY) minY = point.y;
                    if (point.z > maxZ) maxZ = point.z;
                    if (point.z < minZ) minZ = point.z;
                }
            }
            Vector3 position = attr.transform.TransformPoint(new Vector3((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2));
            Vector3 rotation = attr.transform.eulerAngles;
            Vector3 size = new Vector3((maxX - minX) * attr.transform.lossyScale.x, (maxY - minY) * attr.transform.lossyScale.y, (maxZ - minZ) * attr.transform.lossyScale.z);

            return new Tuple<Vector3, Vector3, Vector3>(position, rotation, size);
        }
    }
}
