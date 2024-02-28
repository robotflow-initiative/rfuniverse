# pyrfuniverse tutorial : test_active_depth.py

## 1 基本功能

<img src="../Image/active_depth/depth.png" width=50%><img src="../Image/active_depth/active_depth.png" width=50%>

<img src="../Image/active_depth/depth_id_select.png" width=50%><img src="../Image/active_depth/active_depth_id_select.png" width=50%><img src="../Image/active_depth/distance_filt.png" width=50%><img src="../Image/active_depth/active_depth_scene.png" width=50%>



- 在已知相机内外参的情况下，得到物体的红外深度图和真实深度图，分别转化为点云，并进行可视化

## 2 实现流程

### 2.1 初始化环境

```python
nd_main_intrinsic_matrix = np.array([[600, 0, 240],
                                     [0, 600, 240],
                                     [0, 0, 1]])
nd_ir_intrinsic_matrix = np.array([[480, 0, 240],
                                   [0, 480, 240],
                                   [0, 0, 1]])

env = RFUniverseBaseEnv(scene_file="ActiveDepth.json", ext_attr=[ActiveLightSensorAttr])
active_light_sensor_1 = env.GetAttr(789789)
```

- 从 json 文件中加载环境
- 初始化相机内参矩阵
- 这里 $f_x, f_y$ 相等，应为理想情况下的针眼相机

### 2.2 获取图像

#### 2.2.1 获取 RGB 图像

```python
active_light_sensor_1.GetRGB(intrinsic_matrix=nd_main_intrinsic_matrix)
env.step()
image_byte = active_light_sensor_1.data["rgb"]
image_rgb = np.frombuffer(image_byte, dtype=np.uint8)
image_rgb = cv2.imdecode(image_rgb, cv2.IMREAD_COLOR)
image_rgb = cv2.cvtColor(image_rgb, cv2.COLOR_BGR2RGB)
```

#### 2.2.2 获取物体语义分割图

```python
active_light_sensor_1.GetID(intrinsic_matrix=nd_main_intrinsic_matrix)
env.step()
image_id = active_light_sensor_1.data["id_map"]
image_id = np.frombuffer(image_id, dtype=np.uint8)
image_id = cv2.imdecode(image_id, cv2.IMREAD_COLOR)
image_id = cv2.cvtColor(image_id, cv2.COLOR_BGR2RGB)
```

#### 2.2.3 获取深度图

```python
active_light_sensor_1.GetDepthEXR(intrinsic_matrix=nd_main_intrinsic_matrix)
env.step()
image_depth_exr = active_light_sensor_1.data["depth_exr"]

active_light_sensor_1.GetActiveDepth(
    main_intrinsic_matrix_local=nd_main_intrinsic_matrix,
    ir_intrinsic_matrix_local=nd_ir_intrinsic_matrix,
)
env.step()
image_active_depth = active_light_sensor_1.data["active_depth"]
```

- `GetDepthEXR` 返回 EXR 格式的深度图，且可以视为没有误差
- `GetActiveDepth` 模拟现实中的红外深度相机，返回带有误差的深度图

### 2.3 点云绘制

```python
local_to_world_matrix = active_light_sensor_1.data["local_to_world_matrix"]

# point = dp.image_array_to_point_cloud(image_rgb, image_active_depth, 45, local_to_world_matrix)

color = utility.EncodeIDAsColor(568451)[0:3]
real_point_cloud1 = dp.image_bytes_to_point_cloud_intrinsic_matrix(
    image_byte, image_depth_exr, nd_main_intrinsic_matrix, local_to_world_matrix
)

active_point_cloud1 = dp.image_array_to_point_cloud_intrinsic_matrix(
    image_rgb, image_active_depth, nd_main_intrinsic_matrix, local_to_world_matrix
)

mask_real_point_cloud1 = dp.mask_point_cloud_with_id_color(
    real_point_cloud1, image_id, color
)
mask_active_point_cloud1 = dp.mask_point_cloud_with_id_color(
    active_point_cloud1, image_id, color
)
filtered_point_cloud1 = dp.filter_active_depth_point_cloud_with_exact_depth_point_cloud(
    mask_active_point_cloud1, mask_real_point_cloud1
)
```

- `real_point_cloud1` 使用没有误差的深度图绘制
- `active_point_cloud1` 使用带有误差的深度图绘制
- 使用之前生成的物体语义分割图来从点云中切割出目标物体
- `filtered_point_cloud1` 使用真实的深度图去滤波改良基于红外线的带有误差的深度图

```python
active_light_sensor_2 = env.GetAttr(123123)

active_light_sensor_2.GetRGB(intrinsic_matrix=nd_main_intrinsic_matrix)
env.step()
image_byte = active_light_sensor_2.data["rgb"]
image_rgb = np.frombuffer(image_byte, dtype=np.uint8)
image_rgb = cv2.imdecode(image_rgb, cv2.IMREAD_COLOR)
image_rgb = cv2.cvtColor(image_rgb, cv2.COLOR_BGR2RGB)

active_light_sensor_2.GetID(intrinsic_matrix=nd_main_intrinsic_matrix)
env.step()
image_id = active_light_sensor_2.data["id_map"]
image_id = np.frombuffer(image_id, dtype=np.uint8)
image_id = cv2.imdecode(image_id, cv2.IMREAD_COLOR)
image_id = cv2.cvtColor(image_id, cv2.COLOR_BGR2RGB)

active_light_sensor_2.GetDepthEXR(intrinsic_matrix=nd_main_intrinsic_matrix)
env.step()
image_depth_exr = active_light_sensor_2.data["depth_exr"]

active_light_sensor_2.GetActiveDepth(
    main_intrinsic_matrix_local=nd_main_intrinsic_matrix,
    ir_intrinsic_matrix_local=nd_ir_intrinsic_matrix,
)
env.step()
image_active_depth = active_light_sensor_2.data["active_depth"]

local_to_world_matrix = active_light_sensor_2.data["local_to_world_matrix"]
env.Pend()
env.close()

# point = dp.image_array_to_point_cloud(image_rgb, image_active_depth, 45, local_to_world_matrix)
real_point_cloud2 = dp.image_bytes_to_point_cloud_intrinsic_matrix(
    image_byte, image_depth_exr, nd_main_intrinsic_matrix, local_to_world_matrix
)

active_point_cloud2 = dp.image_array_to_point_cloud_intrinsic_matrix(
    image_rgb, image_active_depth, nd_main_intrinsic_matrix, local_to_world_matrix
)

mask_real_point_cloud2 = dp.mask_point_cloud_with_id_color(
    real_point_cloud2, image_id, color
)
mask_active_point_cloud2 = dp.mask_point_cloud_with_id_color(
    active_point_cloud2, image_id, color
)
filtered_point_cloud2 = dp.filter_active_depth_point_cloud_with_exact_depth_point_cloud(
    mask_active_point_cloud2, mask_real_point_cloud2
)
```
- 对另一个视角的相机重复上述流程

### 2.4 可视化呈现

```python
# unity space to open3d space and show
real_point_cloud1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
real_point_cloud2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
active_point_cloud1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
active_point_cloud2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
mask_real_point_cloud1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
mask_real_point_cloud2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
mask_active_point_cloud1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
mask_active_point_cloud2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
filtered_point_cloud1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
filtered_point_cloud2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])

coordinate = o3d.geometry.TriangleMesh.create_coordinate_frame()

o3d.visualization.draw_geometries([real_point_cloud1, real_point_cloud2, coordinate])
o3d.visualization.draw_geometries([active_point_cloud1, active_point_cloud2, coordinate])
o3d.visualization.draw_geometries([mask_real_point_cloud1, mask_real_point_cloud2, coordinate])
o3d.visualization.draw_geometries([mask_active_point_cloud1, mask_active_point_cloud2, coordinate])
o3d.visualization.draw_geometries([filtered_point_cloud1, filtered_point_cloud2, coordinate])
```
