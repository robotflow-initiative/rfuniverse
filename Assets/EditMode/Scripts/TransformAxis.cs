using RFUniverse.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RFUniverse.EditMode
{
    public class TransformAxis : MonoBehaviour
    {
        public float moveSpeed = 0.2f;
        public float rotateSpeed = 30;
        public float scaleSpeed = 10;

        public float moveAdsorb = 0.1f;
        public float rotateAdsorb = 5;
        public float scaleAdsorb = 1;

        public GameObject move;
        public GameObject rotate;
        public GameObject scale;

        BaseAttr controlledAttr;

        bool pressCtrlKey = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                pressCtrlKey = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                pressCtrlKey = false;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                switch (EditMain.Instance.CurrentEditMode)
                {
                    case EditMode.Move:
                        transform.position = Vector3.zero;
                        controlledAttr.SetTransform(true, false, false, transform.position, Vector3.zero, Vector3.one, true);
                        EditMain.Instance.ui.TransformChange(controlledAttr.transform.localPosition);
                        break;
                    case EditMode.Rotate:
                        transform.eulerAngles = Vector3.zero;
                        controlledAttr.SetTransform(false, true, false, Vector3.zero, transform.eulerAngles, Vector3.one, true);
                        EditMain.Instance.ui.TransformChange(controlledAttr.transform.localEulerAngles);
                        break;
                    case EditMode.Scale:
                        transform.localScale = Vector3.one;
                        controlledAttr.SetTransform(false, false, true, Vector3.zero, Vector3.zero, transform.localScale, true);
                        EditMain.Instance.ui.TransformChange(controlledAttr.transform.localScale);
                        break;
                    default:
                        break;
                }
                ReSet(controlledAttr);
            }
        }
        public void ReSet(BaseAttr attr)
        {
            controlledAttr = attr;
            if (controlledAttr == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                transform.position = controlledAttr.transform.position;
                transform.eulerAngles = controlledAttr.transform.eulerAngles;
                transform.localScale = Vector3.one;
            }
        }
        public void ModeChange(EditMode editMode, EditableUnit unit)
        {
            move.SetActive(false);
            rotate.SetActive(false);
            scale.SetActive(false);
            ReSet(controlledAttr);
            switch (editMode)
            {
                case EditMode.Move:
                    move.SetActive(true);
                    EditMain.Instance.ui.TransformChange(controlledAttr.transform.localPosition);
                    break;
                case EditMode.Rotate:
                    rotate.SetActive(true);
                    EditMain.Instance.ui.TransformChange(controlledAttr.transform.localEulerAngles);
                    break;
                case EditMode.Scale:
                    scale.SetActive(true);
                    EditMain.Instance.ui.TransformChange(controlledAttr.transform.localScale);
                    break;
            }
        }
        Vector3 realVector3;
        public void MoveX(bool start)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 x = Camera.main.WorldToScreenPoint(transform.position + transform.right * 0.001f);
            Vector2 dir = x - center;
            dir = dir.normalized;
            float d = Vector2.Dot(dir, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

            if (!start)
                transform.localPosition = realVector3;

            transform.Translate(Vector3.right * center.z * d * moveSpeed);
            realVector3 = transform.localPosition;

            if (pressCtrlKey)
            {
                transform.localPosition = new Vector3(Mathf.RoundToInt(transform.localPosition.x / moveAdsorb) * moveAdsorb, transform.localPosition.y, transform.localPosition.z);
            }
            //EditMain.Instance.ui.SetTips($"Position X : {controlled.localPosition.x}");
            controlledAttr.SetTransform(true, false, false, transform.position, Vector3.zero, Vector3.one, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localPosition);
        }

        public void MoveY(bool start)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 y = Camera.main.WorldToScreenPoint(transform.position + transform.up * 0.001f);
            Vector2 dir = y - center;
            dir = dir.normalized;
            float d = Vector2.Dot(dir, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

            if (!start)
                transform.localPosition = realVector3;

            transform.Translate(Vector3.up * center.z * d * moveSpeed);
            realVector3 = transform.localPosition;

            if (pressCtrlKey)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, Mathf.RoundToInt(transform.localPosition.y / moveAdsorb) * moveAdsorb, transform.localPosition.z);
            }
            //EditMain.Instance.ui.SetTips($"Position Y : {controlled.localPosition.y}");
            controlledAttr.SetTransform(true, false, false, transform.position, Vector3.zero, Vector3.one, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localPosition);
        }

        public void MoveZ(bool start)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 z = Camera.main.WorldToScreenPoint(transform.position + transform.forward * 0.001f);
            Vector2 dir = z - center;
            dir = dir.normalized;
            float d = Vector2.Dot(dir, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

            if (!start)
                transform.localPosition = realVector3;

            transform.Translate(Vector3.forward * center.z * d * moveSpeed);
            realVector3 = transform.localPosition;

            if (pressCtrlKey)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, Mathf.RoundToInt(transform.localPosition.z / moveAdsorb) * moveAdsorb);
            }
            //EditMain.Instance.ui.SetTips($"Position Z : {controlled.localPosition.z}");
            controlledAttr.SetTransform(true, false, false, transform.position, Vector3.zero, Vector3.one, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localPosition);
        }
        public void RotateXPositive(bool start)
        {
            RotateX(start, 1);
        }
        public void RotateXNegative(bool start)
        {
            RotateX(start, -1);
        }
        void RotateX(bool start, int sign)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);

            Vector2 dir = Input.mousePosition - center;
            Vector2 dir2 = dir + new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            float a = Vector2.SignedAngle(dir, dir2);

            if (!start)
                transform.localEulerAngles = realVector3;

            transform.Rotate(Vector3.right * a * sign * rotateSpeed);
            realVector3 = transform.localEulerAngles;

            if (pressCtrlKey)
            {
                transform.localEulerAngles = new Vector3(Mathf.RoundToInt(transform.localEulerAngles.x / rotateAdsorb) * rotateAdsorb, transform.localEulerAngles.y, transform.localEulerAngles.z);
            }
            //EditMain.Instance.ui.SetTips($"Rotation X : {controlled.localEulerAngles.x}");
            controlledAttr.SetTransform(false, true, false, Vector3.zero, transform.eulerAngles, Vector3.one, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localEulerAngles);
        }
        public void RotateYPositive(bool start)
        {
            RotateX(start, 1);
        }
        public void RotateYNegative(bool start)
        {
            RotateY(start, -1);
        }
        public void RotateY(bool start, int sign)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);

            Vector2 dir = Input.mousePosition - center;
            Vector2 dir2 = dir + new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            float a = Vector2.SignedAngle(dir, dir2);

            if (!start)
                transform.localEulerAngles = realVector3;

            transform.Rotate(Vector3.up * a * sign * rotateSpeed);
            realVector3 = transform.localEulerAngles;

            if (pressCtrlKey)
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.RoundToInt(transform.localEulerAngles.y / rotateAdsorb) * rotateAdsorb, transform.localEulerAngles.z);
            }
            //EditMain.Instance.ui.SetTips($"Rotation Y : {controlled.localEulerAngles.y}");
            controlledAttr.SetTransform(false, true, false, Vector3.zero, transform.eulerAngles, Vector3.one, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localEulerAngles);
        }
        public void RotateZPositive(bool start)
        {
            RotateZ(start, 1);
        }
        public void RotateZNegative(bool start)
        {
            RotateX(start, -1);
        }
        public void RotateZ(bool start, int sign)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);

            Vector2 dir = Input.mousePosition - center;
            Vector2 dir2 = dir + new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            float a = Vector2.SignedAngle(dir, dir2);

            if (!start)
                transform.localEulerAngles = realVector3;

            transform.Rotate(Vector3.forward * a * sign * rotateSpeed);
            realVector3 = transform.localEulerAngles;

            if (pressCtrlKey)
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.RoundToInt(transform.localEulerAngles.z / rotateAdsorb) * rotateAdsorb);
            }
            //EditMain.Instance.ui.SetTips($"Rotation Z : {controlled.localEulerAngles.z}");
            controlledAttr.SetTransform(false, true, false, Vector3.zero, transform.eulerAngles, Vector3.one, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localEulerAngles);
        }
        public void ScaleXPositive(bool start)
        {
            ScaleX(start, 1);
        }
        public void ScaleXNegative(bool start)
        {
            ScaleX(start, -1);
        }
        public void ScaleX(bool start, int sign)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 x = Camera.main.WorldToScreenPoint(transform.position + transform.right * 0.001f);
            Vector2 dir = x - center;
            dir = dir.normalized;
            float d = Vector2.Dot(dir, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

            if (!start)
                transform.localScale = realVector3;

            Vector3 scale = transform.localScale;
            scale += Vector3.right * center.z * d * scaleSpeed * sign;
            realVector3 = scale;
            if (pressCtrlKey)
            {
                scale = new Vector3(Mathf.RoundToInt(scale.x / scaleAdsorb) * scaleAdsorb, scale.y, scale.z);
            }
            scale = new Vector3(Mathf.Max(0.01f, scale.x), Mathf.Max(0.01f, scale.y), Mathf.Max(0.01f, scale.z));


            transform.localScale = scale;
            //EditMain.Instance.ui.SetTips($"Scale X : {controlled.localScale.x}");
            controlledAttr.SetTransform(false, false, true, Vector3.zero, Vector3.zero, transform.localScale, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localScale);
        }
        public void ScaleYPositive(bool start)
        {
            ScaleY(start, 1);
        }
        public void ScaleYNegative(bool start)
        {
            ScaleY(start, -1);
        }
        public void ScaleY(bool start, int sign)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 y = Camera.main.WorldToScreenPoint(transform.position + transform.up * 0.001f);
            Vector2 dir = y - center;
            dir = dir.normalized;
            float d = Vector2.Dot(dir, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

            if (!start)
                transform.localScale = realVector3;

            Vector3 scale = transform.localScale;
            scale += Vector3.up * center.z * d * scaleSpeed * sign;
            realVector3 = scale;
            if (pressCtrlKey)
            {
                scale = new Vector3(scale.x, Mathf.RoundToInt(scale.y / scaleAdsorb) * scaleAdsorb, scale.z);
            }
            scale = new Vector3(Mathf.Max(0.01f, scale.x), Mathf.Max(0.01f, scale.y), Mathf.Max(0.01f, scale.z));

            transform.localScale = scale;
            //EditMain.Instance.ui.SetTips($"Scale Y : {controlled.localScale.y}");
            controlledAttr.SetTransform(false, false, true, Vector3.zero, Vector3.zero, transform.localScale, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localScale);
        }
        public void ScaleZPositive(bool start)
        {
            ScaleZ(start, 1);
        }
        public void ScaleZNegative(bool start)
        {
            ScaleZ(start, -1);
        }
        public void ScaleZ(bool start, int sign)
        {
            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 z = Camera.main.WorldToScreenPoint(transform.position + transform.forward * 0.001f);
            Vector2 dir = z - center;
            dir = dir.normalized;
            float d = Vector2.Dot(dir, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

            if (!start)
                transform.localScale = realVector3;

            Vector3 scale = transform.localScale;
            scale += Vector3.forward * center.z * d * scaleSpeed * sign;
            realVector3 = scale;
            if (pressCtrlKey)
            {
                scale = new Vector3(scale.x, scale.y, Mathf.RoundToInt(scale.z / scaleAdsorb) * scaleAdsorb);
            }
            scale = new Vector3(Mathf.Max(0.01f, scale.x), Mathf.Max(0.01f, scale.y), Mathf.Max(0.01f, scale.z));

            transform.localScale = scale;
            //EditMain.Instance.ui.SetTips($"Scale Z : {controlled.localScale.z}");

            controlledAttr.SetTransform(false, false, true, Vector3.zero, Vector3.zero, transform.localScale, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localScale);
        }

        Vector3 startVector3;
        public void ScaleA(bool start)
        {

            Vector3 center = Camera.main.WorldToScreenPoint(transform.position);
            float d = Vector2.Dot(Vector2.one, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
            float s = center.z * d * scaleSpeed;


            Vector3 scale = transform.localScale;


            if (start)
                startVector3 = scale;
            else
                scale = realVector3;

            scale *= 1 + s;

            realVector3 = scale;
            if (pressCtrlKey)
            {
                scale = Mathf.RoundToInt(scale.magnitude / startVector3.magnitude / scaleAdsorb) * scaleAdsorb * startVector3;
            }

            scale = new Vector3(Mathf.Max(0.01f, scale.x), Mathf.Max(0.01f, scale.y), Mathf.Max(0.01f, scale.z));
            transform.localScale = scale;
            //EditMain.Instance.ui.SetTips($"Scale XYZ : {controlled.localScale.x.ToString("f3")},{controlled.localScale.y.ToString("f3")},{controlled.localScale.z.ToString("f3")}");
            controlledAttr.SetTransform(false, false, true, Vector3.zero, Vector3.zero, transform.localScale, true);
            EditMain.Instance.ui.TransformChange(controlledAttr.transform.localScale);
        }
    }
}
