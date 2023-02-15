using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionState : MonoBehaviour
{
    public bool collision = false;
    // void OnTriggerStay(Collider other)
    // {
    //     print(1);
    //     collision = true;
    // }

    void OnTriggerEnter(Collider other)
    {
        //print(0);
        collision = true;
    }
    // void OnTriggerExit(Collider other)
    // {
    //     print(2);
    // }
}
