# pyrfuniverse tutorial : test_cloth_attach

## 1 基本功能

- 展示布料在虚拟环境中的表现

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv()
env.DebugObjectPose()
env.EnabledGroundObiCollider(True)
```

- `DebugObjectPose` 开启显示物体基坐标的功能
- `EnabledGroundObiCollider` 启动 Unity 中的 Obi 碰撞插件

### 2.2 导入布料物体

```python
mesh = env.LoadCloth(
    path=os.path.abspath("../Mesh/Tshirt.obj")
)
mesh.SetTransform(position=[0, 1, 0])
env.step(200)
mesh.GetParticles()
env.step()
```

- 通过把布料网格化，构造一个由布料微粒（Cloth Particle）组成的布料网格（Cloth Mesh），来处理布料与一般物体的碰撞

### 2.3 模拟抓取布料移动

```python
position1 = mesh.data['particles'][500]
position2 = mesh.data['particles'][200]
point1 = env.InstanceObject("Empty")
point1.SetTransform(position=position1)
mesh.AddAttach(point1.id)
point2 = env.InstanceObject("Empty")
point2.SetTransform(position=position2)
mesh.AddAttach(point2.id)
env.step()

point1.DoMove([-0.25, 1, 0], 2, speed_based=False)
point2.DoMove([0.25, 1, 0], 2, speed_based=False)
point2.WaitDo()

while True:
    point1.DoMove([-0.25, 1, -0.5], 1)
    point2.DoMove([0.25, 1, -0.5], 1)
    point2.WaitDo()

    point1.DoMove([-0.25, 1, 0.5], 1)
    point2.DoMove([0.25, 1, 0.5], 1)
    point2.WaitDo()

env.Pend()
```