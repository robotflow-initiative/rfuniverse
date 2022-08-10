using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseEvent : MonoBehaviour
{
    public UnityEvent MouseClickEvent;
    public UnityEvent<bool> MouseDragEvent;
    float downTime = -1;
    bool newDrag = false;
    private void OnMouseDown()
    {
        downTime = Time.time;
        newDrag = true;
    }
    private void OnMouseUp()
    {
        if (Time.time - downTime < 0.5f)
            MouseClickEvent.Invoke();
    }
    private void OnMouseDrag()
    {
        if (newDrag)
        {
            MouseDragEvent.Invoke(true);
            newDrag = false;
        }
        else
            MouseDragEvent.Invoke(false);
    }
}
