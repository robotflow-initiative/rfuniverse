# pyrfuniverse tutorial : test_load_urdf

## 1 基本功能

- urdf 文件导入
- 效果
  - 导入 3 个 urdf 文件
  - 实现第一个刚体部件的移动与旋转

## 2 实现流程

### 2.1 初始化环境并导入 urdf 文件

```python
env = RFUniverseBaseEnv()

ur5 = env.LoadURDF(id=639787, path=os.path.abspath('../URDF/UR5/ur5_robot.urdf'), native_ik=True)
ur5.SetTransform(position=[1, 0, 0])
yumi = env.LoadURDF(id=358136, path=os.path.abspath('../URDF/yumi_description/urdf/yumi.urdf'), native_ik=False)
yumi.SetTransform(position=[2, 0, 0])
kinova = env.LoadURDF(id=985135, path=os.path.abspath('../URDF/kinova_gen3/GEN3_URDF_V12.urdf'), native_ik=False)
kinova.SetTransform(position=[3, 0, 0])
```

- 依次导入 3 个 urdf 文件，同时指定三个刚体的初始位置
- `native_ik` 是一个插件，当设置为 `false` 时就无法使用 [2.2](#2.2) 中 `IKTargetDoMove` 等接口，只能手动设置每个 joint 的 position（使用 `setJointPosition`）。反之亦然，设置为 `true` 时，不能通过 `setJointPosition` 手动设置。

### 2.2 设置第一个刚体的动作

```python
ur5.IKTargetDoMove(position=[0, 0.5, 0], duration=0.1, relative=True)
env.step()
ur5.WaitDo()
```

- `IKTargetDoMove` 中参数的含义：
  - `position` : 刚体末端的目标位置
  - `duration` : 从当前位置到目标位置的持续时间
  - `relative` : `true` 时为相对当前位置，`false` 为移动到世界坐标的绝对位置

```python
ur5.IKTargetDoMove(position=[0, 0, -0.5], duration=0.1, relative=True)
env.step()
ur5.WaitDo()
ur5.IKTargetDoMove(position=[0, -0.2, 0.3], duration=0.1, relative=True)
ur5.IKTargetDoRotateQuaternion(quaternion=utility.UnityEularToQuaternion([0, 90, 0]), duration=30, relative=True)
env.step()
ur5.WaitDo()

while 1:
    env.step()
```

- 由于第一个刚体在初始设置 `native_ik` 为 `true` 所以只需要设定末端位置，由 ik 算法反算各个 joint 的角度
