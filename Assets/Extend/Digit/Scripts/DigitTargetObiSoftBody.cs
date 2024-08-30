#if OBI
using UnityEngine;
using Obi;
using System.Collections.Generic;

namespace RFUniverse.Attributes.Digit
{
    //需要被Digit抓取的ObiSoftbody物体挂此脚本
    //仿真Digit传感器的基本原理：
    //为Digit接触到的物体生成一个copy，这个copy只能被该Digit相机看到，该Digit相机也只能看到这个copy
    //获取物体与Gel面碰撞的Impulse，根据Impulse对copy相对相机进行偏移，以此来模拟压力不同成像的变化
    //这么做可以保证多个Digit同时成像，相互之间不产生影响
    public class DigitTargetObiSoftBody : MonoBehaviour
    {
        public Material[] lightMaterial;
        ObiSoftbody softbody;//该Softbody实例
        SkinnedMeshRenderer skinnedMeshRenderer;//softbody的Renderer
        Mesh mesh;//生成copy的mesh
        void Start()
        {
            //初始化
            mesh = new Mesh();
            softbody = GetComponent<ObiSoftbody>();//从自身获取Softbody组件
            skinnedMeshRenderer = softbody.GetComponentInChildren<SkinnedMeshRenderer>();
            //！！！！！！！！注册ObiSoftbody的碰撞委托事件
            softbody.solver.OnCollision += ParticleCollision;

        }
        //Obi的碰撞事件，参数中会包含每个Particle的碰撞信息
        void ParticleCollision(ObiSolver solver, ObiNativeContactList contacts)
        {
            //保存每个Collider碰撞impulse的字典，key为ObiSolver中Collider的ID，value为impulse
            Dictionary<int, float> impulse = new Dictionary<int, float>();
            //遍历碰撞信息
            foreach (var item in contacts)
            {
                //当该Particle的normalImpulse>0，即为有碰撞，以碰撞到Collider的ID为key，normalImpulse为value写入字典
                if (item.normalImpulse > 0)
                    //当字典中存在该collider，表示多个Particle碰撞到同一个Collider，此时累加该Collider的Impulse
                    if (impulse.ContainsKey(item.bodyB))
                        impulse[item.bodyB] += item.normalImpulse;
                    //不存在则直接写入即可
                    else
                        impulse.Add(item.bodyB, item.normalImpulse);
            }
            //
            //！！！以上遍历结束后Dictionary<int, float> impulse中存储的就是每个Collider对ObiSoftbody的法向冲量normalImpulse
            //Obi还支持tangentImpulse/bitangentImpulse/stickImpulse/rollingFrictionImpulse
            //
            //然后根据Impulse做自己要做的事情
            if (impulse.Count > 0)
            {
                skinnedMeshRenderer.BakeMesh(mesh);
                foreach (var item in impulse)
                {
                    ObiColliderWorld world = ObiColliderWorld.GetInstance();
                    DigitCollider collider = world.colliderHandles[item.Key].owner.GetComponent<DigitCollider>();
                    if (collider == null) continue;
                    GameObject copyRender = GetOrCreateRender(collider.digit);
                    collider.digit.CollisionStay(transform, copyRender, item.Value);//将碰撞事件发送到Digit
                }
            }
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
            copyRender = new GameObject("render", typeof(MeshFilter), typeof(MeshRenderer));
            copyRender.GetComponent<MeshFilter>().mesh = mesh;
            copyRender.GetComponent<MeshRenderer>().materials = lightMaterial;
            transform.localScale = skinnedMeshRenderer.transform.lossyScale;
            //改变渲染层
            copyRender.layer = target.index + DigitAttr.startLayer;
            //添加进Render列表
            targets.Add(target, copyRender);
            return copyRender;
        }
    }
}
#endif
