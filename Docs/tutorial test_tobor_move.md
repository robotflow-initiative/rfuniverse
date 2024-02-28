# pyrfuniverse tutorial : test_tobor_move

## 1 基本功能

- 展示 tobor 车轮驱动移动

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(assets=["tobor_r300_ag95_ag95"])
```

### 2.2 初始化 tobor

```python
torbor = env.InstanceObject(name="tobor_r300_ag95_ag95", attr_type=attr.ControllerAttr)
torbor.SetTransform(position=[0, 0.05, 0])
torbor.SetImmovable(False)
env.step()
```

- `SetImmovable`：设置基关节的可移动性

### 2.3 驱动 tobor 移动

```python
while 1:
    torbor.MoveForward(1, 0.2)
    env.step(300)
    torbor.TurnLeft(90, 30)
    env.step(300)
    torbor.MoveForward(1, 0.2)
    env.step(300)
    torbor.TurnLeft(90, 30)
    env.step(300)
    torbor.MoveForward(1, 0.2)
    env.step(300)
    torbor.TurnRight(90, 30)
    env.step(300)
    torbor.MoveBack(1, 0.2)
    env.step(300)
    torbor.TurnRight(90, 30)
    env.step(300)

```

- `MoveForward`：前进，第一个参数表示距离，第二个参数表示速度
- `MoveBack`：后退，第一个参数表示距离，第二个参数表示速度
- `TurnLeft`：左转，第一个参数表示角度，第二个参数表示速度
- `TurnRight`：右转，第一个参数表示角度，第二个参数表示速度