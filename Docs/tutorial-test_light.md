# pyrfuniverse tutorial : test_light

## 1 基本功能

- 展示灯光功能

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(scene_file="LightScene.json")
```

### 2.2 展示灯光功能

```python
light = env.GetAttr(885275)
env.SetShadowDistance(50)
while 1:
    env.step(50)
    light.SetColor(color=[1.0, 0.0, 0.0])
    env.step(50)
    light.SetRange(30.0)
    env.step(50)
    light.SetType(LightType.Directional)
    env.step(50)
    light.SetIntensity(5.0)
    env.step(50)
    light.SetType(LightType.Spot)
    env.step(50)
    light.SetSpotAngle(60.0)
    env.step(50)
    light.SetType(LightType.Point)
    env.step(50)
    light.SetRange(10.0)
    light.SetIntensity(1.0)
    light.SetSpotAngle(30.0)
```

- `SetShadowDistance`：设置阴影的距离，单位为米
- `SetColor`：设置灯光颜色
- `SetRange`：设置灯光范围（只有当灯光的类型为 Spot 或 Point 时才有效）
- `SetType`：设置灯光的类型，与 Unity 里一样，共有 Spot, Directional, Point, Rectangle, Disc 五种
- `SetSpotAngle`：设置灯光的角度（只有当灯光的类型为 Spot 时才有效）
- `SetIntensity`：设置灯光的强度