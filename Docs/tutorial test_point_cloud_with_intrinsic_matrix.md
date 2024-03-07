# pyrfuniverse tutorial : test_point_cloud_with_intrinsic_matrix

## 1 基本功能

- 使用相机内参获取深度图并转换为点云

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(scene_file="PointCloud.json")
```

- 从预先配置好的 `PointCloud.json` 文件中导入场景

### 2.2 获取相机内参和图像

```python
nd_intrinsic_matrix = np.array([[960, 0, 960], [0, 960, 540], [0, 0, 1]])
camera1 = env.GetAttr(698548)
camera1.GetDepthEXR(intrinsic_matrix=nd_intrinsic_matrix)
camera1.GetRGB(intrinsic_matrix=nd_intrinsic_matrix)
camera1.GetID(intrinsic_matrix=nd_intrinsic_matrix)
env.step()
```

### 2.3 转换为点云

```python
image_rgb = camera1.data["rgb"]
image_depth_exr = camera1.data["depth_exr"]
local_to_world_matrix = camera1.data["local_to_world_matrix"]
point1 = dp.image_bytes_to_point_cloud_intrinsic_matrix(
    image_rgb, image_depth_exr, nd_intrinsic_matrix, local_to_world_matrix
)
```

- `image_bytes_to_point_cloud_intrinsic_matrix`：通过 RGB 图像、深度图、内参矩阵、局部到世界坐标系的转换矩阵，生成场景的点云

### 2.4 可视化点云

```python
# unity space to open3d space and show
point1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
point2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
coorninate = o3d.geometry.TriangleMesh.create_coordinate_frame()
o3d.visualization.draw_geometries([point1, point2, coorninate])
```