using RFUniverse.Manager;
using UnityEngine;

namespace RFUniverse.DebugTool
{
    public class ColliderBound : MonoBehaviour
    {
        public GameObject center;

        public GameObject box;
        public GameObject sphere;
        public MeshRenderer capsule;
        public MeshFilter mesh;

        private new Collider collider;
        public Collider Collider
        {
            get
            {
                return collider;
            }
            set
            {
                collider = value;
                if (collider)
                {
                    gameObject.SetActive(true);
                }
                else
                {
                    gameObject.SetActive(false);
                    return;
                }
                box.SetActive(false);
                sphere.SetActive(false);
                capsule.gameObject.SetActive(false);
                mesh.gameObject.SetActive(false);
                if (collider is BoxCollider)
                {
                    box.SetActive(true);
                    BoxCollider boxCollider = collider as BoxCollider;
                    box.transform.localPosition = boxCollider.center;
                    box.transform.localScale = boxCollider.size;
                }
                else if (collider is SphereCollider)
                {
                    sphere.SetActive(true);
                    SphereCollider sphereCollider = collider as SphereCollider;
                    sphere.transform.localScale = Vector3.one * sphereCollider.radius * 2;
                }
                else if (collider is CapsuleCollider)
                {
                    capsule.gameObject.SetActive(true);
                    CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                    capsule.transform.localPosition = capsuleCollider.center;
                    capsule.material.SetFloat("_Radius", capsuleCollider.radius);
                    capsule.material.SetFloat("_Height", capsuleCollider.height);
                    switch (capsuleCollider.direction)
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
                }
                else if (collider is MeshCollider)
                {
                    mesh.gameObject.SetActive(true);
                    MeshCollider meshCollider = collider as MeshCollider;
                    mesh.sharedMesh = meshCollider.sharedMesh;
                }
            }
        }
        void FixedUpdate()
        {
            if (DebugManager.Instance.IsDebugColliderBound && Collider && Collider.gameObject.activeInHierarchy)
            {
                center.SetActive(true);
                transform.position = Collider.transform.position;
                transform.rotation = Collider.transform.rotation;
                transform.localScale = Collider.transform.lossyScale;
            }
            else
                center.SetActive(false);

        }
    }
}
