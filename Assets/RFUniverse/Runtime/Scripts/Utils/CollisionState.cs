using UnityEngine;

[DisallowMultipleComponent]
public class CollisionState : MonoBehaviour
{
    public bool collision = false;
    void OnTriggerStay(Collider _)
    {
        collision = true;
    }
    void OnCollisionStay(Collision c)
    {
        if (c.contactCount > 0)
            collision = true;
    }
}
