# pyrfuniverse tutorial : test_digit

## 1 基本功能

- 展示交互式 Digit 触觉传感器仿真

<img src="../Image/digit.png">

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(ext_attr=[DigitAttr])
```

### 2.2 展示交互式 Digit 触觉传感器仿真

```python
digit = env.InstanceObject(name="Digit", attr_type=DigitAttr)
digit.SetTransform(position=[0, 0.015, 0])
target = env.InstanceObject(name="DigitTarget")
target.SetTransform(position=[0, 0.05, 0.015])
env.SetViewTransform(position=[-0.1, 0.033, 0.014], rotation=[0, 90, 0])
env.Pend()
env.close()
```

- `InstanceObject` 实例化 Digit 传感器
- `SetTransform` 设定传感器和物体于合适的位置和姿态
- `SetViewTransform` 设定视角于合适的位置和姿态
