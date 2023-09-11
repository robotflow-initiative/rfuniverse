using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float rotateSpeed = 90f;

    private Camera m_Camera = null;

    private Vector3 startPosition = Vector3.zero;
    private Vector3 startRotation = Vector3.zero;
    void Start()
    {

        m_Camera = GetComponent<Camera>();
        startPosition = m_Camera.transform.position;
        startRotation = m_Camera.transform.eulerAngles;
    }

    void Update()
    {
        if (m_Camera == null) return;
        if (Input.GetKey(KeyCode.W))
            m_Camera.transform.position += m_Camera.transform.forward * moveSpeed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift)?5f:1f) * (Input.GetKey(KeyCode.LeftAlt)?0.2f:1f);
        if (Input.GetKey(KeyCode.S))
            m_Camera.transform.position += m_Camera.transform.forward * -1 * moveSpeed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift)?5f:1f) * (Input.GetKey(KeyCode.LeftAlt)?0.2f:1f);
        if (Input.GetKey(KeyCode.A))
            m_Camera.transform.position += m_Camera.transform.right * -1 * moveSpeed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift)?5f:1f) * (Input.GetKey(KeyCode.LeftAlt)?0.2f:1f);
        if (Input.GetKey(KeyCode.D))
            m_Camera.transform.position += m_Camera.transform.right * moveSpeed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift)?5f:1f) * (Input.GetKey(KeyCode.LeftAlt)?0.2f:1f);
        if (Input.GetKey(KeyCode.Space))
            m_Camera.transform.position += m_Camera.transform.up * moveSpeed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift)?5f:1f) * (Input.GetKey(KeyCode.LeftAlt)?0.2f:1f);
        if (Input.GetKey(KeyCode.LeftControl))
            m_Camera.transform.position += m_Camera.transform.up * -1 * moveSpeed * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift)?5f:1f) * (Input.GetKey(KeyCode.LeftAlt)?0.2f:1f);
        if (Input.GetMouseButton(1))
        {
            m_Camera.transform.Rotate(Vector3.up * rotateSpeed * Input.GetAxis("Mouse X") * Time.deltaTime,Space.World);
            m_Camera.transform.Rotate( -Vector3.right * rotateSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime,Space.Self);
            //m_Camera.transform.localEulerAngles = new Vector3(Mathf.Clamp(m_Camera.transform.localEulerAngles.x, -90, 90), m_Camera.transform.localEulerAngles.y, m_Camera.transform.localEulerAngles.z);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            m_Camera.transform.position = startPosition;
            m_Camera.transform.eulerAngles = startRotation;
        }
    }
}
