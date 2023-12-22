using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CollisionState : MonoBehaviour
{
    public bool collision = false;
    void OnTriggerEnter(Collider _)
    {
        collision = true;
    }
    void OnTriggerExit(Collider _)
    {
        collision = false;
    }
    void OnCollisionEnter(Collision _)
    {
        collision = true;
    }
    void OnCollisionExit(Collision _)
    {
        collision = false;
    }
}
