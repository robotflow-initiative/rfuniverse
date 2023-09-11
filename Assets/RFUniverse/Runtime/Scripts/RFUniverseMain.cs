using UnityEngine;

namespace RFUniverse
{
    public class ConfigData
    {
        public string assets_path;
        public string executable_file;
    }
    public class RFUniverseMain : MonoBehaviour
    {
        [SerializeField]
        private Camera mainCamera;
        public Camera MainCamera => mainCamera;

        [SerializeField]
        private Camera axisCamera;

        [SerializeField]
        private GameObject ground;
        public GameObject Ground => ground;

        public bool GroundActive
        {
            get
            {
                if (Ground == null) return false;
                return Ground.activeSelf;
            }
            set
            {
                Ground.SetActive(value);
            }
        }

        [SerializeField]
        private Light sun;
        public Light Sun
        {
            get
            {
                return sun;
            }
        }
        public LayerMask simulationLayer = 1 << 0;//常规显示层
        public int axisLayer = 6;//debug显示层
        public int tempLayer = 21;//相机渲染临时层


        protected virtual void Awake()
        {
            //Application.targetFrameRate = 60;
            axisCamera.cullingMask = 1 << axisLayer;
        }
    }
}
