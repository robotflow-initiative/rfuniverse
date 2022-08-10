using RFUniverse.Attributes;
using RFUniverse.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Obi;
using UnityEngine;

namespace RFUniverse.EditMode
{
    public class EditableUnit : MonoBehaviour
    {
        public BaseAttrData data = null;
        public MeshRenderer box;
        public BaseAttr attr;
        // public string name;
        // public int id;
        public Sprite image;
        public Color selected;
        public Color unSelected;
        public void Load(bool allSet = true)
        {
            if (string.IsNullOrEmpty(data.name) || data.id < 0) return;
            image = EditMain.Instance.assetsData.GetImageWithName(data.name);
            AssetManager.Instance.GetGameObject(data.name, (attrObject) =>
            {
                attrObject = Instantiate(attrObject);

                attrObject.name = attrObject.name.Replace("(Clone)", "");

                MeshRenderer[] rendererBound = attrObject.GetComponentsInChildren<MeshRenderer>();
                Collider[] colliderBound = attrObject.GetComponentsInChildren<Collider>();
                List<Bounds> rendererbds = rendererBound.Select(s => s.bounds).ToList();
                List<Bounds> colliderbds = colliderBound.Select(s => s.bounds).ToList();
                rendererbds.AddRange(colliderbds);
                if (rendererbds.Count > 0)
                {
                    Bounds bounds = rendererbds[0];
                    foreach (var item in rendererbds)
                    {
                        bounds.Encapsulate(item);
                    }

                    bounds.Expand(0.03f);

                    transform.position = bounds.center;
                    transform.localScale = bounds.size;
                }

                // MimicJointController[] mimic = attrObject.GetComponentsInChildren<MimicJointController>();
                // foreach (var item in mimic)
                // {
                //     Destroy(item);
                // }
                // ArticulationUnit[] unit = attrObject.GetComponentsInChildren<ArticulationUnit>();
                // foreach (var item in unit)
                // {
                //     Destroy(item);
                // }
                // ArticulationBody[] articulation = attrObject.GetComponentsInChildren<ArticulationBody>();
                // foreach (var item in articulation)
                // {
                //     item.enabled = false;
                // }
                Rigidbody[] rigidbody = attrObject.GetComponentsInChildren<Rigidbody>();
                foreach (var item in rigidbody)
                {
                    Destroy(item);
                }
                ObiFixedUpdater[] obiUpdater = attrObject.GetComponentsInChildren<ObiFixedUpdater>();
                foreach (var item in obiUpdater)
                {
                    item.enabled = false;
                }
                ObiRigidbody[] obiRigidbody = attrObject.GetComponentsInChildren<ObiRigidbody>();
                foreach (var item in obiRigidbody)
                {
                    item.enabled = false;
                }
                Collider[] collider = attrObject.GetComponentsInChildren<Collider>();
                foreach (var item in collider)
                {
                    item.enabled = false;
                }

                transform.parent = attrObject.transform;

                attr = attrObject.GetComponent<BaseAttr>();

                if (allSet)
                    attr.SetAttrData(data);
                else
                {
                    attr.Name = data.name;
                    attr.ID = data.id;
                    attr.SetParent(data.parentID, data.parentName);
                    attr.SetTransform(true, true, true, new Vector3(data.position[0], data.position[1], data.position[2]), new Vector3(data.rotation[0], data.rotation[1], data.rotation[2]), new Vector3(data.scale[0], data.scale[1], data.scale[2]));
                }


                BaseAttr.AddAttr(attr);

                EditMain.Instance.CurrentSelectedUnit = this;
                EditMain.Instance.CurrentEditMode = EditMode.Select;
            });

            EditMain.Instance.OnModeChange += ModeChange;
            ModeChange(EditMain.Instance.CurrentEditMode, EditMain.Instance.CurrentSelectedUnit);
        }

        void ModeChange(EditMode editMode, EditableUnit unit)
        {
            switch (editMode)
            {
                case EditMode.Select:
                case EditMode.Parent:
                    box.gameObject.SetActive(true);
                    break;
                default:
                    box.gameObject.SetActive(false);
                    break;
            }
        }
        public void OnClick()
        {
            EditMain.Instance.SelectUnit(this);
        }
        public void SetSelect(bool b)
        {
            box.material.SetColor("_EmissionColor", b ? selected : unSelected);
        }
        private void OnDestroy()
        {
            EditMain.Instance.OnModeChange -= ModeChange;
        }
    }
}
