using UnityEngine;

[DisallowMultipleComponent]
public class IgnoreSelfCollision : MonoBehaviour
{
    void OnEnable()
    {
        Ignore();
    }

    public void Ignore()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider1 in colliders)
        {
            foreach (Collider collider2 in colliders)
            {
                Physics.IgnoreCollision(collider1, collider2, true);
            }
        }
    }
}
