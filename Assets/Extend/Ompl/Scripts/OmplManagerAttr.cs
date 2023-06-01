using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Attributes;
using System.Linq;

public class OmplManagerAttr : BaseAttr
{
    bool isCollide = false;
    ControllerAttr robot;

    public override void Init()
    {
        base.Init();
    }
    public override void CollectData(OutgoingMessage msg)
    {
        base.CollectData(msg);
        msg.WriteBoolean(isCollide);
    }

    public override void AnalysisMsg(IncomingMessage msg, string type)
    {
        switch (type)
        {
            case "ModifyRobot":
                ModifyRobot(msg);
                return;
            case "SetJointState":
                SetJointState(msg);
                return;
            case "RestoreRobot":
                RestoreRobot(msg);
                return;
        }
        base.AnalysisMsg(msg, type);
    }
    void ModifyRobot(IncomingMessage msg)
    {
        int id = msg.ReadInt32();
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

    void SetJointState(IncomingMessage msg)
    {
        if (robot == null) return;
        List<float> joint_positions = msg.ReadFloatList().ToList();
        robot.SetJointPosition(joint_positions, ControlMode.Direct);
        isCollide = false;
    }
    void RestoreRobot(IncomingMessage msg)
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
