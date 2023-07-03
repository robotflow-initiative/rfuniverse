using RFUniverse;
using RFUniverse.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LightAttrData : BaseAttrData
{
    public LightData lightData;
    public LightAttrData() : base()
    {
        type = "Light";
    }
    public LightAttrData(BaseAttrData b) : base(b)
    {
        if (b is LightAttrData)
            lightData = (b as LightAttrData).lightData;
        type = "Light";
    }

    public override void SetAttrData(BaseAttr attr)
    {
        base.SetAttrData(attr);
        ((LightAttr)attr).SetLightData(lightData);
    }
}
[Serializable]
public class LightData
{
    public Color color = Color.white;
    public LightType type = LightType.Point;
    public LightShadows shadow = LightShadows.Hard;
    public float intensity = 1;
    public float range = 10;
    public float spotAngle = 30;
}

[RequireComponent(typeof(Light))]
public class LightAttr : BaseAttr
{
    Transform lightView;
    private void Awake()
    {
        lightView = transform.Find("LightView(Clone)");
        if (lightView == null)
        {
            lightView = GameObject.Instantiate(Resources.Load<GameObject>("LightView")).transform;
            lightView.parent = transform;
            lightView.localPosition = Vector3.zero;
            lightView.localRotation = Quaternion.identity;
        }
    }

    new protected Light light = null;
    public Light Light
    {
        get
        {
            if (light == null)
                light = GetComponent<Light>();
            return light;
        }
    }
    public LightType Type
    {
        get
        {
            return Light.type;
        }
        set
        {
            Light.type = value;
            lightView?.Find("Point").gameObject.SetActive(false);
            lightView?.Find("Spot").gameObject.SetActive(false);
            lightView?.Find("Directional").gameObject.SetActive(false);
            switch (Light.type)
            {
                case LightType.Point:
                    lightView?.Find("Point").gameObject.SetActive(true);
                    break;
                case LightType.Spot:
                    lightView?.Find("Spot").gameObject.SetActive(true);
                    break;
                case LightType.Directional:
                    lightView?.Find("Directional").gameObject.SetActive(true);
                    break;
            }
        }
    }

    private LightData lightData = new LightData();

    [EditAttr("Rigidbody", "RFUniverse.EditMode.LightAttrUI")]
    public LightData LightData
    {
        get
        {
            if (lightData == null)
                lightData = GetLightData();
            return lightData;
        }
        set
        {
            lightData = value;
        }
    }
    public LightData GetLightData()
    {
        LightData data = new LightData();
        data.color = Light.color;
        data.type = Type;
        data.shadow = Light.shadows;
        data.intensity = Light.intensity;
        data.range = Light.range;
        data.spotAngle = Light.spotAngle;
        return data;
    }
    public void SetLightData(LightData data)
    {
        Light.color = data.color;
        Type = data.type;
        Light.shadows = data.shadow;
        Light.intensity = data.intensity;
        Light.range = data.range;
        Light.spotAngle = data.spotAngle;
    }
    public override void Init()
    {
        base.Init();
        Light.cullingMask &= ~(1 << PlayerMain.Instance.axisLayer);
        Light.cullingMask &= ~(1 << PlayerMain.Instance.tempLayer);
        Type = Light.type;
    }
    public override BaseAttrData GetAttrData()
    {
        LightAttrData data = new LightAttrData(base.GetAttrData());
        data.lightData = GetLightData();
        return data;
    }
    public override void AnalysisData(string type, object[] data)
    {
        switch (type)
        {
            case "SetColor":
                SetColor(RFUniverseUtility.ConvertType<List<float>>(data[0]));
                return;
            case "SetIntensity":
                SetIntensity((float)data[0]);
                return;
            case "SetRange":
                SetRange((float)data[0]);
                return;
            case "SetType":
                SetType((int)data[0]);
                return;
            case "SetShadow":
                SetShadow((int)data[0]);
                return;
            case "SetSpotAngle":
                SetSpotAngle((float)data[0]);
                return;
        }
        base.AnalysisData(type, data);
    }

    private void SetSpotAngle(float spotAngle)
    {
        Light.spotAngle = spotAngle;
    }

    private void SetShadow(int shadows)
    {
        Light.shadows = (LightShadows)shadows;
    }

    private void SetType(int type)
    {
        Type = (LightType)type;
    }

    private void SetRange(float range)
    {
        Light.range = range;
    }

    private void SetIntensity(float intensity)
    {
        Light.intensity = intensity;
    }

    private void SetColor(List<float> color)
    {
        Light.color = new Color(color[0], color[1], color[2]);
    }


}
