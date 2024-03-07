# pyrfuniverse tutorial : test_save_gripper

## 1 基本功能

- 使用夹爪驱动后将其保存为 obj 格式的模型

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(assets=["allegro_hand_right"])
```

### 2.2 设置夹爪状态

```python
bhand = env.InstanceObject("allegro_hand_right", attr_type=attr.ControllerAttr)
env.step(5)
moveable_joint_count = bhand.data["number_of_moveable_joints"]
print(f"moveable_joint_count:{moveable_joint_count}")
bhand.SetJointPositionDirectly([30 for _ in range(moveable_joint_count)])
env.step(5)
```

- `SetJointPositionDirectly`：为每一个关节设置位置，并直接移动

### 2.3 保存夹爪状态

```python
env.ExportOBJ([bhand.id], os.path.abspath("../Mesh/gripper_mesh.obj"))
```

- `ExportOBJ`：把指定的对象保存为 obj 格式的文件