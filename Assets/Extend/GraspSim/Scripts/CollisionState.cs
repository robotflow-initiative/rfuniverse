using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CollisionState : MonoBehaviour
{
    public bool collision = false;
    // void OnTriggerStay(Collider other)
    // {
    //     print(1);
    //     collision = true;
    // }

    void OnTriggerStay(Collider other)
    {
        //print(0);
        collision = true;
    }
    // void OnTriggerExit(Collider other)
    // {
    //     print(2);
    // }
    //void OnCollisionEnter(Collision col)
    //{
    //    collision = true;
    //}
}
