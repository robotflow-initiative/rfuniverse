using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse.Attributes.Digit
{
    //目标物体挂载该脚本，接收碰撞事件后发送给Digit
    public class DigitTarget : MonoBehaviour
    {
        public GameObject render;
        public Material[] lightMaterial;

        private void OnCollisionStay(Collision other)
        {
            DigitCollider collider = other.collider.GetComponentInParent<DigitCollider>();
            if (collider == null) return;
            GameObject copyRender = GetOrCreateRender(collider.digit);
            float force = collider.digit.transform.InverseTransformVector(other.impulse).y;
            collider.digit.CollisionStay(transform, copyRender, force);//将碰撞事件发送到Digit
        }



        //Render列表，key：Digit，value：复制出的渲染物体
        private Dictionary<DigitAttr, GameObject> targets = new Dictionary<DigitAttr, GameObject>();
        //获取或者生成渲染物体
        GameObject GetOrCreateRender(DigitAttr target)
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
            copyRender.layer = target.index + DigitAttr.startLayer;
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
