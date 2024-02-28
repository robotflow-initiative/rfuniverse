# pyrfuniverse tutorial : test_pick_and_place

## 1 基本功能

- 展示机械臂的基础接口和原生 IK 驱动的抓取

<img src="../Image/pick_place.gif">

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(assets=["franka_panda"])
env.SetTimeStep(0.005)
robot = env.InstanceObject(
    name="franka_panda", id=123456, attr_type=attr.ControllerAttr
)
robot.SetIKTargetOffset(position=[0, 0.105, 0])
env.step()
gripper = env.GetAttr(1234560)
gripper.GripperOpen()
robot.IKTargetDoMove(position=[0, 0.5, 0.5], duration=0, speed_based=False)
robot.IKTargetDoRotate(rotation=[0, 45, 180], duration=0, speed_based=False)
robot.WaitDo()
```

- `SetTimeStep`：设置在对应的 Unity 环境中，一个时间步的时长
- `SetIKTargetOffset`：设置机械臂运动的偏移量，此处的偏移量向上，这样后续移动机械臂时，只需把目标设置为物体的位置即可，机械臂便会自动移动到物体上方的位置进行抓取

### 2.2 循环抓取物体

```python
while 1:
    box1 = env.InstanceObject(
        name="Rigidbody_Box", id=111111, attr_type=attr.RigidbodyAttr
    )
    box1.SetTransform(
        position=[random.uniform(-0.5, -0.3), 0.03, random.uniform(0.3, 0.5)],
        scale=[0.06, 0.06, 0.06],
    )
    box2 = env.InstanceObject(
        name="Rigidbody_Box", id=222222, attr_type=attr.RigidbodyAttr
    )
    box2.SetTransform(
        position=[random.uniform(0.3, 0.5), 0.03, random.uniform(0.3, 0.5)],
        scale=[0.06, 0.06, 0.06],
    )
    env.step(100)

    position1 = box1.data["position"]
    position2 = box2.data["position"]

    robot.IKTargetDoMove(
        position=[position1[0], position1[1] + 0.5, position1[2]],
        duration=2,
        speed_based=False,
    )
    robot.WaitDo()
    robot.IKTargetDoMove(
        position=[position1[0], position1[1], position1[2]],
        duration=2,
        speed_based=False,
    )
    robot.WaitDo()
    gripper.GripperClose()
    env.step(50)
    robot.IKTargetDoMove(
        position=[0, 0.5, 0], duration=2, speed_based=False, relative=True
    )
    robot.WaitDo()
    robot.IKTargetDoMove(
        position=[position2[0], position2[1] + 0.5, position2[2]],
        duration=4,
        speed_based=False,
    )
    robot.WaitDo()
    robot.IKTargetDoMove(
        position=[position2[0], position2[1] + 0.06, position2[2]],
        duration=2,
        speed_based=False,
    )
    robot.WaitDo()
    gripper.GripperOpen()
    env.step(50)
    robot.IKTargetDoMove(
        position=[0, 0.5, 0], duration=2, speed_based=False, relative=True
    )
    robot.WaitDo()
    robot.IKTargetDoMove(position=[0, 0.5, 0.5], duration=2, speed_based=False)
    robot.WaitDo()
    box1.Destroy()
    box2.Destroy()
    env.step()
```

- `IKTargetDoMove` 中参数的含义：
  - `position`：用于表示旋转的四元数
  - `duration`：从当前位置到目标位置的持续时间
  - `relative`：`true` 表示相对当前位置，`false` 表示世界坐标的绝对位置

- `IKTargetDoRotateQuaternion` 中参数的含义：
  - `quaternion`：刚体末端的目标位置
  - `duration`：从当前位置到目标位置的持续时间
  - `relative`：`true` 表示相对当前位置，`false` 表示世界坐标的绝对位置

- `WaitDo`：等待机械臂的运动结束，再运行后续代码