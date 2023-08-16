using UnityEngine;
using RFUniverse.Attributes;
using System;
using RFUniverse.Manager;
using System.Threading.Tasks;

namespace RFUniverse.DebugTool
{
    public class DDDBBox : MonoBehaviour
    {
        public Renderer render;
        public GameObjectAttr target;
        int frame = 0;
        public static int total = 50;
        int index;
        private void Awake()
        {
            index = UnityEngine.Random.Range(0, total);
        }
        //Task t;
        //private void OnEnable()
        //{
        //    t = Task.Run(() =>
        //    {
        //        while (true)
        //        {
        //            if (target && target.gameObject.activeInHierarchy)
        //            {
        //                gameObject.SetActive(true);
        //                render.material.SetColor("_Color", RFUniverseUtility.EncodeIDAsColor(target.ID));

        //                render.bounds = target.GetAppendBounds();
        //                Tuple<Vector3, Vector3, Vector3> bound = target.Get3DBBox();
        //                transform.position = bound.Item1;
        //                transform.eulerAngles = bound.Item2;
        //                render.material.SetVector("_Size", bound.Item3);
        //            }
        //            else
        //            {
        //                gameObject.SetActive(false);
        //            }
        //        }
        //    });
        //}
        //private void OnDisable()
        //{
        //    t.Dispose();
        //}
        void FixedUpdate()
        {
            if ((frame++ % total) != index) return;
            if (target && target.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                render.material.SetColor("_Color", RFUniverseUtility.EncodeIDAsColor(target.ID));

                render.bounds = target.GetAppendBounds();
                Tuple<Vector3, Vector3, Vector3> bound = target.Get3DBBox(false);
                transform.position = bound.Item1;
                transform.eulerAngles = bound.Item2;
                render.material.SetVector("_Size", bound.Item3);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
