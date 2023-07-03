using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using RFUniverse.Attributes;
using System.Linq;
using RFUniverse;

public class OmplManagerAttr : BaseAttr
{
    bool isCollide = false;
    ControllerAttr robot;

    public override void Init()
    {
        base.Init();
    }
    public override Dictionary<string, object> CollectData()
    {
        Dictionary<string, object> data = base.CollectData();
        data.Add("is_collide", isCollide);
        return data;
    }

    public override void AnalysisData(string type, object[] data)
    {
        switch (type)
        {
            case "ModifyRobot":
                ModifyRobot((int)data[0]);
                return;
            case "SetJointState":
                SetJointState(RFUniverseUtility.ConvertType<List<float>>(data[0]));
                return;
            case "RestoreRobot":
                RestoreRobot();
                return;
        }
        base.AnalysisData(type, data);
    }
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

    void SetJointState(List<float> jointPositions)
    {
        if (robot == null) return;
        robot.SetJointPosition(jointPositions, ControlMode.Direct);
        isCollide = false;
    }
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
