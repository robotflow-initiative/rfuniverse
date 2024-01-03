using UnityEngine;
//物体拖拽脚本
namespace RFUniverse
{
    [DisallowMultipleComponent]
    public class MouseDrag : MonoBehaviour
    {
        public float force = 10;
        new Rigidbody rigidbody = null;
        Vector3 sourcePos = Vector3.zero;
        void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            sourcePos = rigidbody.transform.position;
        }

        // Update is called once per frame
        float dis = 0;
        bool draging = false;
        void OnMouseDown()
        {
            if (rigidbody == null) return;
            //Debug.Log("Mouse Down");
            dis = Vector3.Distance(Camera.main.transform.position, rigidbody.transform.position);
            draging = true;
        }
        void OnMouseUp()
        {
            if (rigidbody == null) return;
            //rigidbody.position = sourcePos;
            rigidbody.velocity = Vector3.zero;
            draging = false;
        }
        void FixedUpdate()
        {
            if (!draging) return;
            if (rigidbody == null) return;
            //Debug.Log("Mouse Drag");
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            pos = pos - Camera.main.transform.position;
            pos = pos.normalized;
            pos = pos * dis;
            pos = Camera.main.transform.position + pos;
            pos = pos - rigidbody.position;
            pos = pos * force;
            rigidbody.AddForce(pos, ForceMode.VelocityChange);
        }
    }
}
