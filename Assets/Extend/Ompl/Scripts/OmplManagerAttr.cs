using System.Collections.Generic;
using UnityEngine;

namespace RFUniverse.Attributes
{

    public class OmplManagerAttr : BaseAttr
    {
        bool isCollide = false;
        ControllerAttr robot;

        public override void Init()
        {
            base.Init();
        }

        public override void AddPermanentData(Dictionary<string, object> data)
        {
            base.AddPermanentData(data);
            data["is_collide"] = isCollide;
        }

        [RFUAPI]
        void ModifyRobot(int id)
        {
            if (!Attrs.ContainsKey(id)) return;
            robot = (ControllerAttr)Attrs[id];
            foreach (var collider in robot.GetComponentsInChildren<Collider>())
            {
                collider.isTrigger = true;
            }
            foreach (var body in robot.GetComponentsInChildren<ArticulationBody>())
            {
                TriggerProcess trigger = body.gameObject.AddComponent<TriggerProcess>();
                trigger.manager = this;
                trigger.body = body;
            }
        }
        [RFUAPI]
        void SetJointState(List<float> jointPositions)
        {
            if (robot == null) return;
            robot.SetJointPosition(jointPositions, ControlMode.Direct);
            isCollide = false;
        }
        [RFUAPI]
        void RestoreRobot()
        {
            if (robot == null) return;
            foreach (var collider in robot.GetComponentsInChildren<Collider>())
            {
                collider.isTrigger = false;
            }
            foreach (var trigger in robot.GetComponentsInChildren<TriggerProcess>())
            {
                Destroy(trigger);
            }
        }

        public void TriggerHandle()
        {
            isCollide = true;
        }
    }
}
