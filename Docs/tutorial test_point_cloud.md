# pyrfuniverse tutorial : test_point_cloud

## 1 基本功能

- 使用图像宽高和 fov 获取深度图并转换为点云

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(scene_file="PointCloud.json")
```

- 从预先配置好的 `PointCloud.json` 文件中导入场景

### 2.2 获取图像

```python
camera1 = env.GetAttr(698548)
camera1.GetDepthEXR(width=1920, height=1080)
camera1.GetRGB(width=1920, height=1080)
camera1.GetID(width=1920, height=1080)
env.step()
```

### 2.3 转换为点云

```python
image_rgb = camera1.data["rgb"]
image_depth_exr = camera1.data["depth_exr"]
fov = 60
local_to_world_matrix = camera1.data["local_to_world_matrix"]
point1 = dp.image_bytes_to_point_cloud(
    image_rgb, image_depth_exr, fov, local_to_world_matrix
)
```

- `fov`：视野（Field of View），默认为 60
- `image_bytes_to_point_cloud`：通过 RGB 图像、深度图、`fov`、`local_to_world_matrix`，生成场景的点云

### 2.4 可视化点云

```python
# unity space to open3d space and show
point1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
point2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
coorninate = o3d.geometry.TriangleMesh.create_coordinate_frame()
o3d.visualization.draw_geometries([point1, point2, coorninate])
```