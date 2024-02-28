# pyrfuniverse tutorial : test_articulation_ik

## 1 基本功能

- 调用原生的 IK 算法实现机械臂的移动和旋转

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(scene_file="ArticulationIK.json")
```

- 从预先配置好的 `ArticulationIK.json` 文件中导入场景

### 2.2 调用 IK 算法移动机械臂

```python
for id in ids:
    current_robot = env.GetAttr(id)
    current_robot.IKTargetDoMove(position=[0, 0, -0.5], duration=0.1, relative=True)
    env.step()
    while not current_robot.data["move_done"]:
        env.step()
    current_robot.IKTargetDoMove(position=[0, -0.5, 0], duration=0.1, relative=True)
    env.step()
    while not current_robot.data["move_done"]:
        env.step()
    current_robot.IKTargetDoMove(position=[0, 0.5, 0.5], duration=0.1, relative=True)
    current_robot.IKTargetDoRotateQuaternion(
        quaternion=utility.UnityEularToQuaternion([90, 0, 0]),
        duration=30,
        relative=True,
    )
    env.step()
    while not current_robot.data["move_done"] or not current_robot.data["rotate_done"]:
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

