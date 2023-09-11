using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicJoint : MonoBehaviour
{
    public MimicJoint Parent; // Null means this is a root node 
    public float multiplier = 1.0f;
    public float offset = 0.0f;
    public bool sync = true;
}
