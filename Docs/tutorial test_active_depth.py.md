# pyrfuniverse tutorial:test_active_depth.py
## 1.基本功能：
![](https://notes.sjtu.edu.cn/uploads/upload_32bac8767a1a8dbbac2e91991131c521.png)
### 通过对图片进行RGB解码，在已知相机内外参的情况下，得到物体的红外深度，并进行可视化
## 2.实现流程：
``` python=
import cv2
import numpy as np
try:
    import open3d as o3d
except ImportError:
    print('This feature requires open3d, please install with `pip install open3d`')
    raise
import pyrfuniverse.utils.rfuniverse_utility as utility
import pyrfuniverse.utils.depth_processor as dp
from pyrfuniverse.envs.base_env import RFUniverseBaseEnv  
```
### o3d.geometry.TriangleMesh.create_coordinate_frame()

### o3d.visualization.draw_geometries()
### 多个几何图形一起可视化（点云和网格一起被可视化）
### TriangleMesh 3d三角网格的数据结构

``` python=
env = RFUniverseBaseEnv(
    scene_file='ActiveDepth.json'
)

main_intrinsic_matrix = [600, 0, 0, 0, 600, 0, 240, 240, 1]
ir_intrinsic_matrix = [480, 0, 0, 0, 480, 0, 240, 240, 1]

nd_main_intrinsic_matrix = np.reshape(main_intrinsic_matrix, [3, 3]).T
```
### 读取json文件，初始化相机内参数矩阵（包括焦距参数，主点偏移和轴倾斜）
### 这里fx，fy给值相同，应该是理想情况下的针眼相机

```python=
env._step()
image_byte = env.instance_channel.data[789789]['rgb']
image_rgb = np.frombuffer(image_byte, dtype=np.uint8)
image_rgb = cv2.imdecode(image_rgb, cv2.IMREAD_COLOR)
# cv2.imshow("show", image_rgb)
# cv2.waitKey(0)
image_rgb = cv2.cvtColor(image_rgb, cv2.COLOR_BGR2RGB)
image_rgb = np.transpose(image_rgb, [1, 0, 2])
env.instance_channel.set_action(
    'GetRGB',
    id=789789,
    intrinsic_matrix=main_intrinsic_matrix
)
env._step()
image_byte = env.instance_channel.data[789789]['rgb']
image_rgb = np.frombuffer(image_byte, dtype=np.uint8)
image_rgb = cv2.imdecode(image_rgb, cv2.IMREAD_COLOR)
# cv2.imshow("show", image_rgb)
# cv2.waitKey(0)
image_rgb = cv2.cvtColor(image_rgb, cv2.COLOR_BGR2RGB)
image_rgb = np.transpose(image_rgb, [1, 0, 2])
```
### 从缓冲器读取原始图像，过opencv分为RGB三个不同通道
```python=
env.instance_channel.set_action(
    'GetDepthEXR',
    id=789789,
    intrinsic_matrix=main_intrinsic_matrix,
)
env._step()
image_depth_exr = env.instance_channel.data[789789]['depth_exr']

env.instance_channel.set_action(
    'GetActiveDepth',
    id=789789,
    main_intrinsic_matrix=main_intrinsic_matrix,
    ir_intrinsic_matrix=ir_intrinsic_matrix
)
env._step()
image_active_depth = env.instance_channel.data[789789]['active_depth']
image_active_depth = np.transpose(image_active_depth, [1, 0])
```
### 定义深度信息
```python=
local_to_world_matrix = env.instance_channel.data[789789]['local_to_world_matrix']
local_to_world_matrix = np.reshape(local_to_world_matrix, [4, 4]).T
```
### 转置，仿射变换得到世界坐标系下的矩阵(根据相机外参提供的旋转矩阵和平移向量)
```python=
# point = dp.image_array_to_point_cloud(image_rgb, image_active_depth, 45, local_to_world_matrix)

color = utility.EncodeIDAsColor(568451)[0:3]
real_point_cloud1 = dp.image_bytes_to_point_cloud_intrinsic_matrix(image_byte, image_depth_exr, nd_main_intrinsic_matrix, local_to_world_matrix)
real_point_cloud1 = dp.mask_point_cloud_with_id_color(real_point_cloud1, image_id, color)
active_point_cloud1 = dp.image_array_to_point_cloud_intrinsic_matrix(image_rgb, image_active_depth, nd_main_intrinsic_matrix, local_to_world_matrix)
active_point_cloud1 = dp.mask_point_cloud_with_id_color(active_point_cloud1, image_id, color)
filtered_point_cloud1 = dp.filter_active_depth_point_cloud_with_exact_depth_point_cloud(active_point_cloud1, real_point_cloud1)
```
### 动态规划点云进行滤波去噪？
```python=
env.instance_channel.set_action(
    'GetRGB',
    id=123123,
    intrinsic_matrix=main_intrinsic_matrix
)
env._step()
image_byte = env.instance_channel.data[123123]['rgb']
image_rgb = np.frombuffer(image_byte, dtype=np.uint8)
image_rgb = cv2.imdecode(image_rgb, cv2.IMREAD_COLOR)
# cv2.imshow("show", image_rgb)
# cv2.waitKey(0)
image_rgb = cv2.cvtColor(image_rgb, cv2.COLOR_BGR2RGB)
image_rgb = np.transpose(image_rgb, [1, 0, 2])

env.instance_channel.set_action(
    'GetID',
    id=123123,
    intrinsic_matrix=main_intrinsic_matrix
)
env._step()
image_id = env.instance_channel.data[123123]['id_map']
image_id = np.frombuffer(image_id, dtype=np.uint8)
image_id = cv2.imdecode(image_id, cv2.IMREAD_COLOR)
# cv2.imshow("show", image_id)
# cv2.waitKey(0)
image_id = cv2.cvtColor(image_id, cv2.COLOR_BGR2RGB)
# image_id = np.transpose(image_id, [1, 0, 2])
env.instance_channel.set_action(
    'GetDepthEXR',
    id=123123,
    intrinsic_matrix=main_intrinsic_matrix,
)
env._step()
image_depth_exr = env.instance_channel.data[123123]['depth_exr']
env.instance_channel.set_action(
    'GetActiveDepth',
    id=123123,
    main_intrinsic_matrix=main_intrinsic_matrix,
    ir_intrinsic_matrix=ir_intrinsic_matrix
)
env._step()
image_active_depth = env.instance_channel.data[123123]['active_depth']
image_active_depth = np.transpose(image_active_depth, [1, 0])

local_to_world_matrix = env.instance_channel.data[123123]['local_to_world_matrix']
local_to_world_matrix = np.reshape(local_to_world_matrix, [4, 4]).T
env.close()
```
### 本地增加任意点云？
```python=
# point = dp.image_array_to_point_cloud(image_rgb, image_active_depth, 45, local_to_world_matrix)
real_point_cloud2 = dp.image_bytes_to_point_cloud_intrinsic_matrix(image_byte, image_depth_exr, nd_main_intrinsic_matrix, local_to_world_matrix)
real_point_cloud2 = dp.mask_point_cloud_with_id_color(real_point_cloud2, image_id, color)
active_point_cloud2 = dp.image_array_to_point_cloud_intrinsic_matrix(image_rgb, image_active_depth, nd_main_intrinsic_matrix, local_to_world_matrix)
active_point_cloud2 = dp.mask_point_cloud_with_id_color(active_point_cloud2, image_id, color)
filtered_point_cloud2 = dp.filter_active_depth_point_cloud_with_exact_depth_point_cloud(active_point_cloud2, real_point_cloud2)

# unity space to open3d space and show
real_point_cloud1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
real_point_cloud2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
active_point_cloud1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
active_point_cloud2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
filtered_point_cloud1.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
filtered_point_cloud2.transform([[-1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]])
coorninate = o3d.geometry.TriangleMesh.create_coordinate_frame()

o3d.visualization.draw_geometries([real_point_cloud1, real_point_cloud2, coorninate])
o3d.visualization.draw_geometries([active_point_cloud1, active_point_cloud2, coorninate])
o3d.visualization.draw_geometries([filtered_point_cloud1, filtered_point_cloud2, coorninate])
```
### 使用open3d进行可视化

