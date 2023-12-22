using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse.Attributes.Digit
{
    //目标物体挂载该脚本，接收碰撞事件后发送给GelSlim
    public class GelSlimTarget : MonoBehaviour
    {
        public GameObject render;
        public Material[] lightMaterial;

        private void OnCollisionStay(Collision other)
        {
            if (other.contacts.Length == 0) return;
            GelSlimCollider collider = other.collider.GetComponentInParent<GelSlimCollider>();
            if (collider == null) return;
            GameObject copyRender = GetOrCreateRender(collider.gelSlim);
            float force = collider.gelSlim.transform.InverseTransformVector(other.impulse).y;
            collider.gelSlim.CollisionStay(transform, copyRender, force);//将碰撞事件发送到GelSlim
        }



        //Render列表，key：GelSlim，value：复制出的渲染物体
        private Dictionary<GelSlimAttr, GameObject> targets = new Dictionary<GelSlimAttr, GameObject>();
        //获取或者生成渲染物体
        GameObject GetOrCreateRender(GelSlimAttr target)
        {
            if (targets.TryGetValue(target, out GameObject copyRender))
            {
                copyRender.SetActive(true);
                return copyRender;
            }
            //复制一份Render
            copyRender = Instantiate(render);
            copyRender.transform.localScale = render.transform.lossyScale;
            //改变渲染层
            copyRender.layer = target.layer;
            copyRender.transform.parent = target.renderParent;
            //替换材质
            MeshRenderer[] renderers = copyRender.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer i in renderers)
            {
                i.materials = lightMaterial;
            }
            //添加进Render列表
            targets.Add(target, copyRender);
            return copyRender;
        }
    }
}
