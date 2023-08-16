using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HeatMap;
using System.Drawing;
using static UnityEngine.GraphicsBuffer;
using UnityEditor;
using System.IO;

namespace RFUniverse.Attributes
{
    [RequireComponent(typeof(Camera))]
    public abstract class BaseCameraAttr : BaseAttr
    {
        new protected Camera camera = null;
        public Camera Camera
        {
            get
            {
                if (camera == null)
                    camera = GetComponent<Camera>();
                return camera;
            }
        }

        protected Texture2D tex = null;

        protected string rgbBase64String = null;
        protected string normalBase64String = null;
        protected string idBase64String = null;
        protected string depthBase64String = null;
        protected string depthEXRBase64String = null;
        protected string amodalMaskBase64String = null;
        protected string motionVectorBase64String = null;
        protected string heatMapBase64String = null;

        public override void Init()
        {
            base.Init();
            tex = new Texture2D(1, 1);

            Camera.enabled = false;
            Camera.depth = -100;
            Camera.allowMSAA = true;
            Camera.allowHDR = false;
            Camera.depthTextureMode |= DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
            Camera.cullingMask &= ~(1 << PlayerMain.Instance.axisLayer);
            Camera.cullingMask &= ~(1 << PlayerMain.Instance.tempLayer);
        }

        private void Awake()
        {
            if (transform.Find("CameraView(Clone)") == null)
            {
                GameObject cameraView = GameObject.Instantiate(Resources.Load<GameObject>("CameraView"));
                cameraView.transform.parent = transform;
                cameraView.transform.localPosition = Vector3.zero;
                cameraView.transform.localRotation = Quaternion.identity;
            }
        }
        public override Dictionary<string, object> CollectData()
        {
            Dictionary<string, object> data = base.CollectData();
            data.Add("width", Camera.pixelWidth);
            data.Add("height", Camera.pixelHeight);
            data.Add("fov", Camera.fieldOfView);
            if (rgbBase64String != null)
            {
                data.Add("rgb", rgbBase64String);
                rgbBase64String = null;
            }
            if (normalBase64String != null)
            {
                data.Add("normal", normalBase64String);
                normalBase64String = null;
            }
            if (idBase64String != null)
            {
                data.Add("id_map", idBase64String);
                idBase64String = null;
            }
            if (depthBase64String != null)
            {
                data.Add("depth", depthBase64String);
                depthBase64String = null;
            }
            if (depthEXRBase64String != null)
            {
                data.Add("depth_exr", depthEXRBase64String);
                depthEXRBase64String = null;
            }
            if (amodalMaskBase64String != null)
            {
                data.Add("amodal_mask", amodalMaskBase64String);
                amodalMaskBase64String = null;
            }
            if (heatMapBase64String != null)
            {
                data.Add("heat_map", heatMapBase64String);
                heatMapBase64String = null;
            }
            if (ddBBOX != null)
            {
                data.Add("2d_bounding_box", ddBBOX);
                ddBBOX = null;
            }
            if (dddBBOX != null)
            {
                data.Add("3d_bounding_box", dddBBOX);
                dddBBOX = null;
            }
            return data;
        }

        public override void AnalysisData(string type, object[] data)
        {
            switch (type)
            {
                case "AlignView":
                    AlignView();
                    return;
                case "GetRGB":
                    GetRGB(data[0].ConvertType<float[,]>(), (int)data[1], (int)data[2], (float)data[3]);
                    return;
                case "GetNormal":
                    GetNormal((float[,])data[0], (int)data[1], (int)data[2], (float)data[3]);
                    return;
                case "GetID":
                    GetID((float[,])data[0], (int)data[1], (int)data[2], (float)data[3]);
                    return;
                case "GetDepth":
                    GetDepth((float)data[0], (float)data[1], (float[,])data[2], (int)data[3], (int)data[4], (float)data[5]);
                    return;
                case "GetDepthEXR":
                    GetDepthEXR(data[0].ConvertType<float[,]>(), (int)data[1], (int)data[2], (float)data[3]);
                    return;
                case "GetAmodalMask":
                    GetAmodalMask((int)data[0], data[1].ConvertType<float[,]>(), (int)data[2], (int)data[3], (float)data[4]);
                    return;
                case "GetMotionVector":
                    GetMotionVector(data[0].ConvertType<float[,]>(), (int)data[1], (int)data[2], (float)data[3]);
                    return;
                case "Get2DBBox":
                    Get2DBBox(data[0].ConvertType<float[,]>(), (int)data[1], (int)data[2], (float)data[3]);
                    return;
                case "Get3DBBox":
                    Get3DBBox();
                    return;
                case "StartHeatMapRecord":
                    StartHeatMapRecord(data[0].ConvertType<List<int>>());
                    return;
                case "EndHeatMapRecord":
                    EndHeatMapRecord();
                    return;
                case "GetHeatMap":
                    GetHeatMap((int)data[0], (float[,])data[1], (int)data[2], (int)data[3], (float)data[4]);
                    return;
            }
            base.AnalysisData(type, data);
        }
        void AlignView()
        {
            transform.position = PlayerMain.Instance.MainCamera.transform.position;
            transform.rotation = PlayerMain.Instance.MainCamera.transform.rotation;
        }
        void GetRGB(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetRGB(intrinsicMatrix);
            else
                GetRGB(width, height, fov);
            rgbBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public Texture2D GetRGB(float[,] intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetRGB(size.x, size.y);
        }
        public abstract Texture2D GetRGB(int width, int height, float? fov = null);
        void GetNormal(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetNormal(intrinsicMatrix);
            else
                GetNormal(width, height, fov);
            normalBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public Texture2D GetNormal(float[,] intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetNormal(size.x, size.y);
        }
        public abstract Texture2D GetNormal(int width, int height, float? unPhysicalFov = null);
        void GetID(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetID(intrinsicMatrix);
            else
                GetID(width, height, fov);
            idBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public Texture2D GetID(float[,] intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetID(size.x, size.y);
        }
        public abstract Texture2D GetID(int width, int height, float? unPhysicalFov = null);
        public Texture2D GetIDSingleChannel(float[,] intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetIDSingleChannel(size.x, size.y);
        }
        public abstract Texture2D GetIDSingleChannel(int width, int height, float? unPhysicalFov = null);
        void GetDepth(float near, float far, float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetDepth(intrinsicMatrix, near, far);
            else
                GetDepth(width, height, near, far, fov);
            depthBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public Texture2D GetDepth(float[,] intrinsicMatrix, float near, float far)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetDepth(size.x, size.y, near, far);
        }
        public abstract Texture2D GetDepth(int width, int height, float near, float far, float? unPhysicalFov = null);
        void GetDepthEXR(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetDepthEXR(intrinsicMatrix);
            else
                GetDepthEXR(width, height, fov);
            depthEXRBase64String = Convert.ToBase64String(tex.EncodeToEXR(Texture2D.EXRFlags.CompressRLE));
        }
        public Texture2D GetDepthEXR(float[,] intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetDepthEXR(size.x, size.y);
        }
        public abstract Texture2D GetDepthEXR(int width, int height, float? unPhysicalFov = null);
        void GetAmodalMask(int id, float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetAmodalMask(id, intrinsicMatrix);
            else
                GetAmodalMask(id, width, height, fov);
            amodalMaskBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public Texture2D GetAmodalMask(int id, float[,] intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetAmodalMask(size.x, size.y, id);
        }
        public abstract Texture2D GetAmodalMask(int id, int width, int height, float? unPhysicalFov = null);

        void GetMotionVector(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetMotionVector(intrinsicMatrix);
            else
                GetMotionVector(width, height, fov);
            motionVectorBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public Texture2D GetMotionVector(float[,] intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetMotionVector(size.x, size.y);
        }
        public abstract Texture2D GetMotionVector(int width, int height, float? unPhysicalFov = null);

        public Vector2Int SetCameraIntrinsicMatrix(Camera set_camera, float[,] intrinsicMatrix)
        {
            set_camera.usePhysicalProperties = true;
            float focal = 35;
            float ax, ay, sizeX, sizeY;
            float x0, y0, shiftX, shiftY;
            ax = intrinsicMatrix[0, 0];
            ay = intrinsicMatrix[1, 1];
            x0 = intrinsicMatrix[0, 2];
            y0 = intrinsicMatrix[1, 2];
            int width = (int)x0 * 2;
            int height = (int)y0 * 2;
            sizeX = focal * width / ax;
            sizeY = focal * height / ay;
            shiftX = -(x0 - width / 2.0f) / width;
            shiftY = (y0 - height / 2.0f) / height;
            set_camera.sensorSize = new Vector2(sizeX, sizeY);
            set_camera.focalLength = focal;
            set_camera.lensShift = new Vector2(shiftX, shiftY);
            return new Vector2Int(width, height);
        }

        protected List<int> SetTempLayer(BaseAttr target)
        {
            List<int> originLayers = new List<int>();
            foreach (var item in target.GetChildComponentFilter<Renderer>())
            {
                //if ((PlayerMain.Instance.simulationLayer.value & item.gameObject.layer) > 0)
                //{
                originLayers.Add(item.gameObject.layer);
                item.gameObject.layer = PlayerMain.Instance.tempLayer;
                //}
            }
            return originLayers;
        }
        protected void RevertLayer(BaseAttr target, List<int> originLayers)
        {
            List<Renderer> trans = target.GetChildComponentFilter<Renderer>();
            for (int i = 0; i < trans.Count; i++)
            {
                //if ((PlayerMain.Instance.simulationLayer.value & trans[i].gameObject.layer) > 0)
                trans[i].gameObject.layer = originLayers[i];
            }
        }


        Dictionary<int, Rect> ddBBOX = null;

        void Get2DBBox(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                Get2DBBox(intrinsicMatrix);
            else
                Get2DBBox(width, height, fov);
        }
        public void Get2DBBox(float[,] intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            Get2DBBox(size.x, size.y);
        }

        void Get2DBBox(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("Get2DBBox");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height);
            ddBBOX = new Dictionary<int, Rect>();
            foreach (var item in ActiveAttrs)
            {
                if (item.Value is not GameObjectAttr) continue;
                Rect rect = (item.Value as GameObjectAttr).Get2DBBox(Camera);
                if (rect.max.x > 0 && rect.max.y > 0 && rect.min.x < Camera.pixelWidth && rect.min.y < Camera.pixelHeight)
                    ddBBOX.Add(item.Key, rect);
            }
        }


        Dictionary<int, Tuple<Vector3, Vector3, Vector3>> dddBBOX = null;
        void Get3DBBox()
        {
            dddBBOX = new Dictionary<int, Tuple<Vector3, Vector3, Vector3>>();
            foreach (var item in ActiveAttrs)
            {
                if (item.Value is not GameObjectAttr) continue;
                Tuple<Vector3, Vector3, Vector3> box = (item.Value as GameObjectAttr).Get3DBBox(false);
                Vector3 center = Camera.WorldToScreenPoint(box.Item1);
                if (center.x > 0 && center.y > 0 && center.x < Camera.pixelWidth && center.y < Camera.pixelHeight)
                    dddBBOX.Add(item.Key, box);
            }
        }



        List<BaseAttr> heatMapTarget = new List<BaseAttr>();
        List<Vector3> heatMapPositions = new List<Vector3>();
        public void StartHeatMapRecord(List<int> ids)
        {
            StartHeatMapRecord(Attrs.Where((s) => ids.Contains(s.Key)).Select((s) => s.Value).ToList());
        }
        public void StartHeatMapRecord(List<BaseAttr> targets)
        {
            heatMapTarget.AddRange(targets);
            PlayerMain.Instance.OnStepAction += RecordFrame;
        }
        void RecordFrame()
        {
            foreach (var item in heatMapTarget)
            {
                heatMapPositions.Add(item.transform.position);
            }
        }
        public void EndHeatMapRecord()
        {
            heatMapTarget.Clear();
            PlayerMain.Instance.OnStepAction -= RecordFrame;
        }
        void GetHeatMap(int radius, float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetHeatMap(intrinsicMatrix, radius);
            else
                GetHeatMap(width, height, radius, fov);
            heatMapBase64String = Convert.ToBase64String(tex.EncodeToPNG());
        }
        public void GetHeatMap(float[,] intrinsicMatrix, int radius)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            GetHeatMap(size.x, size.y, radius);
        }
        public Texture2D GetHeatMap(int width, int height, int radius = 50, float? unPhysicalFov = null)
        {
            Debug.Log("GetHeatMap");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height);
            HeatMapMaker heatMap = new HeatMapMaker
            {
                Width = width,
                Height = height,
                Radius = radius,
                Opacity = 1
            };
            List<HeatPoint> heatPoints = new List<HeatPoint>();
            foreach (var item in heatMapPositions)
            {
                Vector3 screenPosition = Camera.WorldToScreenPoint(item);
                heatPoints.Add(new HeatPoint
                {
                    X = screenPosition.x,
                    Y = screenPosition.y,
                    W = 1
                });
            }
            heatMapPositions.Clear();
            heatMap.HeatPoints = heatPoints;
            tex = BitmapToTexture2D(heatMap.MakeHeatMap());
            RenderTexture.ReleaseTemporary(Camera.targetTexture);
            return tex;
        }

        //Bitmap转Texture2D
        public Texture2D BitmapToTexture2D(Bitmap bitmap)
        {
            Texture2D texture = new Texture2D(bitmap.Width, bitmap.Height, TextureFormat.RGB24, false);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    System.Drawing.Color color = bitmap.GetPixel(i, j);
                    texture.SetPixel(i, j, new Color32(color.R, color.G, color.B, color.A));
                }
            }
            texture.Apply();
            return texture;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CameraAttr), true)]
    public class CameraAttrEditor : Editor
    {
        Vector2Int size = new Vector2Int(EditorPrefs.GetInt("CAMERA_TEX_W", 512), EditorPrefs.GetInt("CAMERA_TEX_H", 512));
        float fov = EditorPrefs.GetFloat("CAMERA_TEX_FOV", 60);
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CameraAttr script = target as CameraAttr;
            GUILayout.Space(10);
            GUILayout.Label("Editor Tool:");
            size = EditorGUILayout.Vector2IntField("Size:", size);
            fov = EditorGUILayout.FloatField("Fov:", fov);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("GetRGB"))
            {
                Texture2D tex = script.GetRGB(size.x, size.y, fov);
                if (!Directory.Exists($"{Application.streamingAssetsPath}/ImageEditor"))
                    Directory.CreateDirectory($"{Application.streamingAssetsPath}/ImageEditor");
                File.WriteAllBytes($"{Application.streamingAssetsPath}/ImageEditor/{script.ID}_RGB.png", tex.EncodeToPNG());
                SaveEditorPrefs();
            }
            if (GUILayout.Button("GetNormal"))
            {
                Texture2D tex = script.GetNormal(size.x, size.y, fov);
                if (!Directory.Exists($"{Application.streamingAssetsPath}/ImageEditor"))
                    Directory.CreateDirectory($"{Application.streamingAssetsPath}/ImageEditor");
                File.WriteAllBytes($"{Application.streamingAssetsPath}/ImageEditor/{script.ID}_Normal.png", tex.EncodeToPNG());
                SaveEditorPrefs();
            }
            if (GUILayout.Button("GetDepthEXR"))
            {
                Texture2D tex = script.GetDepthEXR(size.x, size.y, fov);
                if (!Directory.Exists($"{Application.streamingAssetsPath}/ImageEditor"))
                    Directory.CreateDirectory($"{Application.streamingAssetsPath}/ImageEditor");
                File.WriteAllBytes($"{Application.streamingAssetsPath}/ImageEditor/{script.ID}_DepthEXR.exr", tex.EncodeToEXR(Texture2D.EXRFlags.CompressRLE));
                SaveEditorPrefs();
            }
            GUILayout.EndHorizontal();
        }

        void SaveEditorPrefs()
        {
            EditorPrefs.SetInt("CAMERA_TEX_W", size.x);
            EditorPrefs.SetInt("CAMERA_TEX_H", size.y);
            EditorPrefs.SetFloat("CAMERA_TEX_FOV", fov);
        }
    }
#endif
}
