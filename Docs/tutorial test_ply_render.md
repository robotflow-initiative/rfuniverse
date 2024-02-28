# pyrfuniverse tutorial : test_ply_render

## 1 基本功能

- 展示点云显示功能

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv()
env.SetViewBackGround([0.0, 0.0, 0.0])
```

- `SetViewBackGround` 设置背景为黑色

### 2.2 显示点云

```python
point_cloud = env.InstanceObject(
    name="PointCloud", id=123456, attr_type=attr.PointCloudAttr
)
point_cloud.ShowPointCloud(ply_path=os.path.abspath("../Mesh/000000_000673513312.ply"))
point_cloud.SetTransform(rotation=[-90, 0, 0])
point_cloud.SetRadius(radius=0.001)

env.Pend()
env.close()
```

- `ShowPointCloud` 导入一个点云文件并显示
- `SetRadius` 设置点云中每个点显示的半径大小
