using RFUniverse.Attributes;
using RFUniverse.EditMode;
using UnityEngine;
using RFUniverse;

namespace RFUniverse.EditMode
{
    public class ColliderView : MonoBehaviour
    {
        public GameObject box;
        public GameObject sphere;
        public MeshRenderer capsule;
        public MeshFilter mesh;
        public void SetColliderData(ColliderAttr attr, ColliderData colliderData)
        {
            if (attr == null || colliderData == null)
            {
                transform.parent = null;
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            Transform trans = attr.transform.FindChildIndexQueue(colliderData.renderIndexQueue);
            transform.SetParent(trans);
            transform.localPosition = new Vector3(colliderData.position[0], colliderData.position[1], colliderData.position[2]);
            transform.localEulerAngles = new Vector3(colliderData.rotation[0], colliderData.rotation[1], colliderData.rotation[2]);
            transform.localScale = new Vector3(colliderData.scale[0], colliderData.scale[1], colliderData.scale[2]);
            //transform.SetParent(null);
            box.gameObject.SetActive(false);
            sphere.gameObject.SetActive(false);
            capsule.gameObject.SetActive(false);
            mesh.gameObject.SetActive(false);
            switch (colliderData.type)
            {
                case ColliderType.None:
                    break;
                case ColliderType.Box:
                    box.SetActive(true);
                    break;
                case ColliderType.Sphere:
                    sphere.SetActive(true);
                    sphere.transform.localScale = Vector3.one * colliderData.radius * 2;
                    break;
                case ColliderType.Capsule:
                    capsule.gameObject.SetActive(true);
                    capsule.material.SetFloat("_Radius", colliderData.radius);
                    capsule.material.SetFloat("_Height", colliderData.height);
                    switch (colliderData.direction)
                    {
                        case 0:
                            capsule.transform.localEulerAngles = new Vector3(0, 0, 90);
                            break;
                        case 1:
                            capsule.transform.localEulerAngles = Vector3.zero;
                            break;
                        case 2:
                            capsule.transform.localEulerAngles = new Vector3(90, 0, 0);
                            break;
                    }
                    break;
                case ColliderType.Mesh:
                    mesh.gameObject.SetActive(true);
                    mesh.sharedMesh = trans.GetComponent<MeshFilter>().sharedMesh;
                    break;
                case ColliderType.Original:
                    mesh.gameObject.SetActive(true);
                    Transform child = trans.Find("Collider");
                    if (child == null) return;
                    MeshCollider col = child.GetComponent<MeshCollider>();
                    if (col == null) return;
                    mesh.sharedMesh = col.sharedMesh;
                    break;
            }
        }
    }
}