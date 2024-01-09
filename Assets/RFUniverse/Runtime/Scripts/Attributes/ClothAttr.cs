#if OBI
using Obi;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse.Attributes
{
    public class ClothAttr : BaseAttr
    {

        ObiCloth obiCloth = null;

        public override void Init()
        {
            base.Init();
            obiCloth = GetComponentInChildren<ObiCloth>();
        }

        [RFUAPI]
        public void AddAttach(int id, float maxDis = 0.03f)
        {
            if (Attrs.TryGetValue(id, out BaseAttr value))
                AddAttach(value.transform, maxDis);
        }
        [RFUAPI]
        public void RemoveAttach(int id)
        {
            if (Attrs.TryGetValue(id, out BaseAttr value))
                RemoveAttach(value.transform);
        }

        ObiConstraints<ObiPinConstraintsBatch> pinConstraints = null;
        Dictionary<Transform, ObiPinConstraintsBatch> currentPinConstraints = new Dictionary<Transform, ObiPinConstraintsBatch>();
        public void AddAttach(Transform point, float maxDis = 0.03f)
        {
            if (currentPinConstraints.ContainsKey(point)) return;

            if (pinConstraints == null)
            {
                pinConstraints = obiCloth.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
            }
            ObiCollider collider = point.GetComponent<ObiCollider>();
            if (!collider)
            {
                point.gameObject.AddComponent<MeshCollider>();
                collider = point.gameObject.AddComponent<ObiCollider>();
            }

            Dictionary<int, float> distances = new Dictionary<int, float>();
            for (int i = 0; i < obiCloth.particleCount; i++)
            {
                i = obiCloth.GetParticleRuntimeIndex(i);
                float dis = Vector3.Distance(obiCloth.GetParticlePosition(i), point.position);
                if (dis < maxDis)
                    distances.Add(i, dis);
            }
            var batch = new ObiPinConstraintsBatch();
            foreach (var particleIndex in distances)
            {
                Vector3 particlePosition = obiCloth.GetParticlePosition(particleIndex.Key);
                Vector3 colliderPosition = collider.transform.InverseTransformPoint(particlePosition);
                Quaternion particleRotation = obiCloth.GetParticleOrientation(particleIndex.Key);
                Quaternion colliderRotation = Quaternion.Inverse(collider.transform.rotation) * particleRotation;
                batch.AddConstraint(particleIndex.Key, collider, colliderPosition, colliderRotation, 0, 0, float.PositiveInfinity);
                batch.activeConstraintCount++;
            }
            pinConstraints.AddBatch(batch);
            obiCloth.SetConstraintsDirty(Oni.ConstraintType.Pin);

            currentPinConstraints.Add(point, batch);
        }

        public void RemoveAttach(Transform point)
        {
            if (currentPinConstraints.TryGetValue(point, out var obiPinConstraintsBatch))
            {
                pinConstraints.RemoveBatch(obiPinConstraintsBatch);
                obiCloth.SetConstraintsDirty(Oni.ConstraintType.Pin);
                currentPinConstraints.Remove(point);
            }
        }


        [RFUAPI]
        void GetParticles()
        {
            List<Vector3> particles = new List<Vector3>();
            for (int i = 0; i < obiCloth.particleCount; i++)
            {
                i = obiCloth.GetParticleRuntimeIndex(i);
                particles.Add(obiCloth.GetParticlePosition(i));
            }
            CollectData.AddDataNextStep("particles", particles);
        }
    }
}
#endif
