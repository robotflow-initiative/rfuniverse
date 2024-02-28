# pyrfuniverse tutorial : test_urdf_parameter

## 1 基本功能

- 展示关节目标位置设置面板

<img src="../Image/urdf_parameter.png">

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv()
```

### 2.2 初始化机械臂

```python
robot = env.LoadURDF(path=os.path.abspath("../URDF/Franka/panda.urdf"), axis="z")
robot.SetTransform(position=[0, 0, 0])
robot.EnabledNativeIK(False)
```

- `LoadURDF`：从 URDF 文件中导入模型
- `EnabledNativeIK`：`native_ik` 是一个插件，当设置为 `false` 时就无法使用 `IKTargetDoMove` 等接口，只能手动设置每个 joint 的 position（使用 `setJointPosition`）。反之亦然，设置为 `true` 时，不能通过 `setJointPosition` 手动设置。

### 2.3 展示关节目标位置设置面板

```python
env.ShowArticulationParameter(robot.id)
```
