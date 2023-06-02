using RFUniverse.Manager;
using Robotflow.RFUniverse;
using System;
using UnityEngine;

public class BaseAgent : Agent
{
    private static BaseAgent instance = null;
    public static BaseAgent Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject agentObj = Instantiate((GameObject)Resources.Load("Agent"));
                DontDestroyOnLoad(agentObj);
                instance = agentObj.GetComponent<BaseAgent>();
                AssetManager am = AssetManager.Instance;
                InstanceManager im = InstanceManager.Instance;
                DebugManager dm = DebugManager.Instance;
            }
            return instance;
        }
    }

    public Action OnStepAction;
    void FixedUpdate()
    {
        OnStepAction?.Invoke();
    }
}
