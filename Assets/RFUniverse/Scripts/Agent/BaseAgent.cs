using RFUniverse.Attributes;
using RFUniverse.Manager;
using Robotflow.RFUniverse;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAgent : Agent
{
    protected EnvironmentParameters environmentParameters;

    private static BaseAgent instance = null;
    public static BaseAgent Instance => instance;

    [SerializeField]
    float fixedDeltaTime = 0.02f;

    public float FixedDeltaTime
    {
        get
        {
            return fixedDeltaTime;
        }
        set
        {
            fixedDeltaTime = value;
            Time.fixedDeltaTime = fixedDeltaTime;
        }
    }
    [SerializeField]
    float timeScale = 1;
    public float TimeScale
    {
        get
        {
            return timeScale;
        }
        set
        {
            timeScale = value;
            Time.timeScale = timeScale;
        }
    }

    public AssetManager am;
    public InstanceManager im;
    public DebugManager dm;

    public List<BaseAttr> sceneAttr = new List<BaseAttr>();
    private void Awake()
    {
        instance = this;
        Init();
    }

    private void Start()
    {
        environmentParameters = Academy.Instance.EnvironmentParameters;
        foreach (var item in sceneAttr)
        {
            if (item != null)
                item.Instance();
        }
    }
    protected virtual void Init()
    {
        FixedDeltaTime = fixedDeltaTime;
        TimeScale = timeScale;
        am = AssetManager.Instance;
        im = InstanceManager.Instance;
        dm = DebugManager.Instance;
    }

    public Action OnStepAction;
    protected virtual void FixedUpdate()
    {
        OnStepAction?.Invoke();
    }

    // public Action OnEpisodeBeginAction;
    // public override void OnEpisodeBegin()
    // {
    //     OnEpisodeBeginAction?.Invoke();
    // }
}
