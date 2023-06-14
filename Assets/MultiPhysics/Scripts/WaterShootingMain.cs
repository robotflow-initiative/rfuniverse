using System;
#if UNITY_STANDALONE_WIN
using com.zibra.liquid.Manipulators;
#endif
using RFUniverse.Manager;
using Robotflow.RFUniverse.SideChannels;
using UnityEngine;

public class WaterShootingMain : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    private void Start()
    {
        AssetManager.Instance.AddListener("SetZibraLiquid", new Action<IncomingMessage>(SetZibraLiquid));
        AssetManager.Instance.AddListener("SetZibraLiquidEmitter", new Action<IncomingMessage>(SetZibraLiquidEmitter));
    }
    private void SetZibraLiquid(IncomingMessage msg)
    {
        bool active = msg.ReadBoolean(false);
        this.zibraLiquid.SetActive(active);
    }

    private void SetZibraLiquidEmitter(IncomingMessage msg)
    {
        float volumePerSimTime = msg.ReadFloat32();
        float x = msg.ReadFloat32();
        float y = msg.ReadFloat32();
        float z = msg.ReadFloat32();
        this.emitter.VolumePerSimTime = volumePerSimTime;
        this.emitter.InitialVelocity = new Vector3(x, y, z);
    }

    public GameObject zibraLiquid;

    public ZibraLiquidEmitter emitter;
#endif
}
