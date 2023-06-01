using UnityEngine;

public class TriggerProcess : MonoBehaviour
{
    public OmplManagerAttr manager;
    public ArticulationBody body;

    private void OnTriggerEnter(Collider col)
    {
        var other = col.GetComponentInParent<ArticulationBody>();
        if (other)
        {
            var p1 = body.GetComponentsInParent<ArticulationBody>();
            var p2 = other.GetComponentsInParent<ArticulationBody>();
            if (p1.Length > 1 && p1[1] == other) return;
            if (p2.Length > 1 && p2[1] == body) return;
        }
        manager?.TriggerHandle();
    }
}
