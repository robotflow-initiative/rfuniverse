# pyrfuniverse tutorial : test_save_scene

## 1 基本功能

- 测试场景的搭建/保存/载入

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(assets=["Collider_Box", "Rigidbody_Sphere"])
```

### 2.2 加载物体

```python
box1 = env.InstanceObject(name="Collider_Box", attr_type=attr.ColliderAttr)
box1.SetTransform(position=[-0.5, 0.5, 0], scale=[0.1, 1, 1])
box2 = env.InstanceObject(name="Collider_Box", attr_type=attr.ColliderAttr)
box2.SetTransform(position=[0.5, 0.5, 0], scale=[0.1, 1, 1])
box3 = env.InstanceObject(name="Collider_Box", attr_type=attr.ColliderAttr)
box3.SetTransform(position=[0, 0.5, 0.5], scale=[1, 1, 0.1])
box4 = env.InstanceObject(name="Collider_Box", attr_type=attr.ColliderAttr)
box4.SetTransform(position=[0, 0.5, -0.5], scale=[1, 1, 0.1])
sphere = env.InstanceObject(name="Rigidbody_Sphere", attr_type=attr.RigidbodyAttr)
sphere.SetTransform(position=[0, 0.5, 0], scale=[0.5, 0.5, 0.5])
env.Pend()
```

- `InstanceObject`：从资产中实例化对象，也就是在虚拟场景中创建物体
- `SetTransform`：设置物体的位置和姿态

### 2.3 保存场景

```python
env.SaveScene("test_scene.json")
env.ClearScene()
env.Pend()
```

- `SaveScene`：把场景保存为 json 格式的文件

### 2.4 加载场景

```python
env.LoadSceneAsync("test_scene.json")
env.Pend()
env.close()
```

- `LoadSceneAsync`：异步加载场景