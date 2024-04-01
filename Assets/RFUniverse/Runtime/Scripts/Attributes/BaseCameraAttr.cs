using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HeatMap;
using System.Drawing;
using UnityEditor;
using System.IO;
using UnityEngine.AddressableAssets;

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
                GameObject cameraView = Instantiate(Addressables.LoadAssetAsync<GameObject>("CameraView").WaitForCompletion());
                cameraView.transform.parent = transform;
                cameraView.transform.localPosition = Vector3.zero;
                cameraView.transform.localRotation = Quaternion.identity;
                cameraView.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        [RFUAPI]
        public void AlignView()
        {
            transform.position = PlayerMain.Instance.MainCamera.transform.position;
            transform.rotation = PlayerMain.Instance.MainCamera.transform.rotation;
        }
        [RFUAPI]
        public void GetRGB(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetRGB(intrinsicMatrix, width, height);
            else
                GetRGB(width, height, fov);
            CollectData.AddDataNextStep("rgb", tex.EncodeToPNG());
        }
        public Texture2D GetRGB(float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetRGB(size.x, size.y);
        }
        public abstract Texture2D GetRGB(int width, int height, float? fov = null);

        [RFUAPI]
        public void GetNormal(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetNormal(intrinsicMatrix, width, height);
            else
                GetNormal(width, height, fov);
            CollectData.AddDataNextStep("normal", tex.EncodeToPNG());
        }
        public Texture2D GetNormal(float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetNormal(size.x, size.y);
        }
        public abstract Texture2D GetNormal(int width, int height, float? unPhysicalFov = null);

        [RFUAPI]
        public void GetID(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetID(intrinsicMatrix, width, height);
            else
                GetID(width, height, fov);
            CollectData.AddDataNextStep("id_map", tex.EncodeToPNG());
        }
        public Texture2D GetID(float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetID(size.x, size.y);
        }
        public abstract Texture2D GetID(int width, int height, float? unPhysicalFov = null);

        public Texture2D GetIDSingleChannel(float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetIDSingleChannel(size.x, size.y);
        }
        public abstract Texture2D GetIDSingleChannel(int width, int height, float? unPhysicalFov = null);

        [RFUAPI]
        public void GetDepth(float near, float far, float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetDepth(near, far, intrinsicMatrix, width, height);
            else
                GetDepth(near, far, width, height, fov);
            CollectData.AddDataNextStep("depth", tex.EncodeToPNG());
        }
        public Texture2D GetDepth(float near, float far, float[,] intrinsicMatrix, int width, int height)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetDepth(near, far, size.x, size.y);
        }
        public abstract Texture2D GetDepth(float near, float far, int width, int height, float? unPhysicalFov = null);


        [RFUAPI]
        public void GetDepth16Bit(float near, float far, float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetDepth16Bit(near, far, intrinsicMatrix, width, height);
            else
                GetDepth16Bit(near, far, width, height, fov);
            CollectData.AddDataNextStep("depth", tex.EncodeToPNG());
        }

        public Texture2D GetDepth16Bit(float near, float far, float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetDepth16Bit(near, far, size.x, size.y);
        }
        public abstract Texture2D GetDepth16Bit(float near, float far, int width, int height, float? unPhysicalFov = null);

        [RFUAPI]
        public void GetDepthEXR(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetDepthEXR(intrinsicMatrix, width, height);
            else
                GetDepthEXR(width, height, fov);
            CollectData.AddDataNextStep("depth_exr", tex.EncodeToEXR(Texture2D.EXRFlags.CompressRLE));
        }
        public Texture2D GetDepthEXR(float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetDepthEXR(size.x, size.y);
        }
        public abstract Texture2D GetDepthEXR(int width, int height, float? unPhysicalFov = null);

        [RFUAPI]
        public void GetAmodalMask(int id, float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetAmodalMask(id, intrinsicMatrix, width, height);
            else
                GetAmodalMask(id, width, height, fov);
            CollectData.AddDataNextStep("amodal_mask", tex.EncodeToPNG());
        }
        public Texture2D GetAmodalMask(int id, float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetAmodalMask(size.x, size.y, id);
        }
        public abstract Texture2D GetAmodalMask(int id, int width, int height, float? unPhysicalFov = null);


        [RFUAPI]
        public void GetMotionVector(float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetMotionVector(intrinsicMatrix, width, height);
            else
                GetMotionVector(width, height, fov);
            CollectData.AddDataNextStep("motion_vector", tex.EncodeToPNG());
        }
        public Texture2D GetMotionVector(float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix, width, height);
            return GetMotionVector(size.x, size.y);
        }
        public abstract Texture2D GetMotionVector(int width, int height, float? unPhysicalFov = null);

        public static Vector2Int SetCameraIntrinsicMatrix(Camera set_camera, float[,] intrinsicMatrix, int width = -1, int height = -1)
        {
            set_camera.usePhysicalProperties = true;
            float focal = 35;
            float ax, ay, sizeX, sizeY;
            float x0, y0, shiftX, shiftY;
            ax = intrinsicMatrix[0, 0];
            ay = intrinsicMatrix[1, 1];
            x0 = intrinsicMatrix[0, 2];
            y0 = intrinsicMatrix[1, 2];
            if (width <= 0)
                width = (int)x0 * 2;
            if (height <= 0)
                height = (int)y0 * 2;
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

        [RFUAPI]
        public void Get2DBBox(float[,] intrinsicMatrix, int width, int height, float fov)
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
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height);
            Dictionary<int, Rect> ddBBOX = new Dictionary<int, Rect>();
            foreach (var item in ActiveAttrs)
            {
                if (item.Value is not GameObjectAttr) continue;
                Rect rect = (item.Value as GameObjectAttr).Get2DBBox(Camera);
                if (rect.max.x > 0 && rect.max.y > 0 && rect.min.x < Camera.pixelWidth && rect.min.y < Camera.pixelHeight)
                    ddBBOX.Add(item.Key, rect);
            }
            CollectData.AddDataNextStep("2d_bounding_box", ddBBOX);
        }

        [RFUAPI]
        public void Get3DBBox()
        {
            Dictionary<int, Tuple<Vector3, Vector3, Vector3>> dddBBOX = new Dictionary<int, Tuple<Vector3, Vector3, Vector3>>();
            foreach (var item in ActiveAttrs)
            {
                if (item.Value is not GameObjectAttr) continue;
                Tuple<Vector3, Vector3, Vector3> box = (item.Value as GameObjectAttr).Get3DBBox(false);
                Vector3 center = Camera.WorldToScreenPoint(box.Item1);
                if (center.x > 0 && center.y > 0 && center.x < Camera.pixelWidth && center.y < Camera.pixelHeight)
                    dddBBOX.Add(item.Key, box);
            }
            CollectData.AddDataNextStep("3d_bounding_box", dddBBOX);
        }



        List<BaseAttr> heatMapTarget = new List<BaseAttr>();
        List<Vector3> heatMapPositions = new List<Vector3>();
        [RFUAPI]
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
        [RFUAPI]
        public void EndHeatMapRecord()
        {
            heatMapTarget.Clear();
            PlayerMain.Instance.OnStepAction -= RecordFrame;
        }
        [RFUAPI]
        public void GetHeatMap(int radius, float[,] intrinsicMatrix, int width, int height, float fov)
        {
            if (intrinsicMatrix != null)
                GetHeatMap(intrinsicMatrix, radius);
            else
                GetHeatMap(width, height, radius, fov);
            CollectData.AddDataNextStep("heat_map", tex.EncodeToPNG());
        }
        public void GetHeatMap(float[,] intrinsicMatrix, int radius)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            GetHeatMap(size.x, size.y, radius);
        }
        public Texture2D GetHeatMap(int width, int height, int radius = 50, float? unPhysicalFov = null)
        {
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
        Vector2Int size;
        float fov;
        private void OnEnable()
        {
            size = new Vector2Int(EditorPrefs.GetInt("CAMERA_TEX_W", 512), EditorPrefs.GetInt("CAMERA_TEX_H", 512));
            fov = EditorPrefs.GetFloat("CAMERA_TEX_FOV", 60);
        }

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
