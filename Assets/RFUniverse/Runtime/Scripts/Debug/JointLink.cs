using RFUniverse.Attributes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using RFUniverse.Manager;
using UnityEngine.Pool;
using System.Linq;

namespace RFUniverse.DebugTool
{

    public class JointLink : MonoBehaviour
    {
        public GameObject center;
        public LineRenderer jointTextSource;
        ObjectPool<LineRenderer> jointTextPool;
        Dictionary<ArticulationBody, LineRenderer> jointTexts = new ();
        private void Awake()
        {
            jointTextPool = new ObjectPool<LineRenderer>(()=>Instantiate(jointTextSource, center.transform),s=>s.gameObject.SetActive(true),s=>s.gameObject.SetActive(false),s=>Destroy(s.gameObject));
        }

        ControllerAttr target;
        public ControllerAttr Target
        {
            get { return target; }
            set
            {
                target = value;
                if (target != null)
                {
                    foreach (var item in jointTexts)
                    {
                        jointTextPool.Release(item.Value);
                    }
                    jointTexts.Clear();
                    foreach (var item in target.MoveableJoints)
                    {
                        jointTexts.Add(item, jointTextPool.Get());
                    }
                }
            }
        }
        void FixedUpdate()
        {
            if (DebugManager.Instance.IsDebugJointLink && Target && Target.gameObject.activeInHierarchy)
            {
                center.gameObject.SetActive(true);

                foreach (var item in jointTexts)
                {
                    if (item.Key.GetComponentsInParent<ArticulationBody>().Count() < 2)
                        item.Value.enabled = false;
                    else
                    {
                        item.Value.enabled = true;
                        item.Value.SetPosition(0, item.Key.GetComponentsInParent<ArticulationBody>()[1].transform.position);
                        item.Value.SetPosition(1, item.Key.transform.position);
                    }
                    item.Value.transform.position = item.Key.transform.position;
                    item.Value.GetComponentInChildren<Canvas>().transform.rotation = Camera.main.transform.rotation;
                    item.Value.GetComponentInChildren<TextMeshProUGUI>().text = item.Key.GetUnit().GetJointPosition().ToString("f2");
                }
            }
            else
            {
                center.gameObject.SetActive(false);
            }
        }
    }
}
