using RFUniverse.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RFUniverse
{
    public class CreateReference : IDisposable
    {
        List<BaseAttr> baseAttrs = new List<BaseAttr>();
        List<UnityEngine.Object> references = new List<UnityEngine.Object>();
        public CreateReference(BaseAttr baseAttr)
        {
            baseAttr.GetComponentsInChildren<MeshFilter>().ToList().ForEach(s =>
            {
                references.Add(s.sharedMesh);
            });
            baseAttr.GetComponentsInChildren<MeshCollider>().ToList().ForEach(s =>
            {
                references.Add(s.sharedMesh);
            });
            baseAttr.GetComponentsInChildren<Renderer>().ToList().ForEach(s =>
            {
                s.sharedMaterials.ToList().ForEach(m =>
                {
                    references.Add(m);
                });
            });
            baseAttr.GetComponentsInChildren<Renderer>().ToList().ForEach(s =>
            {
                s.sharedMaterials.ToList().ForEach(m =>
                {
                    m.GetTexturePropertyNames().ToList().ForEach(t =>
                    {
                        references.Add(m.GetTexture(t));
                    });
                });
            });
        }

        public void Add(BaseAttr baseAttr)
        {
            if (baseAttrs.Contains(baseAttr)) return;
            baseAttrs.Add(baseAttr);
        }

        public void Remove(BaseAttr baseAttr)
        {
            if (baseAttrs.Contains(baseAttr))
                baseAttrs.Remove(baseAttr);
            if (baseAttrs.Count == 0)
                Dispose();
        }
        public void Dispose()
        {
            references.ForEach(s =>
            {
                UnityEngine.Object.Destroy(s);
            });
        }
    }
}
