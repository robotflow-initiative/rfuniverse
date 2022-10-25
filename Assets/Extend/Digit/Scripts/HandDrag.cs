using UnityEngine;
//手掌控制脚本
public class HandDrag : MonoBehaviour
{
    ArticulationBody body = null;
    public float rotateSpeed = 100;

    void Start()
    {
        body = GetComponent<ArticulationBody>();
    }

    float dis = 0;
    bool draging = false;

    void OnMouseDown()
    {
        if (body == null) return;
        //Debug.Log("Mouse Down");
        dis = Vector3.Distance(Camera.main.transform.position, body.transform.position);
        draging = true;
    }
    void OnMouseUp()
    {
        if (body == null) return;
        //body.velocity = Vector3.zero;
        draging = false;
    }
    void FixedUpdate()
    {
        if (body == null) return;
        if (draging)
        {
            //Debug.Log("Mouse Drag");
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            pos = pos - Camera.main.transform.position;
            pos = pos.normalized;
            pos = pos * dis;
            pos = Camera.main.transform.position + pos;
            body.TeleportRoot(pos, body.transform.rotation);
        }

        if (Input.GetKey(KeyCode.I))
        {
            Quaternion rot = Quaternion.AngleAxis(rotateSpeed * Time.fixedDeltaTime, Vector3.up);
            body.TeleportRoot(body.transform.position, body.transform.rotation * rot);
        }
        else if (Input.GetKey(KeyCode.O))
        {
            Quaternion rot = Quaternion.AngleAxis(-rotateSpeed * Time.fixedDeltaTime, Vector3.up);
            body.TeleportRoot(body.transform.position, body.transform.rotation * rot);
        }

        if (Input.GetKey(KeyCode.J))
        {
            Quaternion rot = Quaternion.AngleAxis(rotateSpeed * Time.fixedDeltaTime, Vector3.forward);
            body.TeleportRoot(body.transform.position, body.transform.rotation * rot);
        }
        else if (Input.GetKey(KeyCode.K))
        {
            Quaternion rot = Quaternion.AngleAxis(-rotateSpeed * Time.fixedDeltaTime, Vector3.forward);
            body.TeleportRoot(body.transform.position, body.transform.rotation * rot);
        }

        if (Input.GetKey(KeyCode.N))
        {
            Quaternion rot = Quaternion.AngleAxis(rotateSpeed * Time.fixedDeltaTime, Vector3.left);
            body.TeleportRoot(body.transform.position, body.transform.rotation * rot);
        }
        else if (Input.GetKey(KeyCode.M))
        {
            Quaternion rot = Quaternion.AngleAxis(-rotateSpeed * Time.fixedDeltaTime, Vector3.left);
            body.TeleportRoot(body.transform.position, body.transform.rotation * rot);
        }
    }
}
