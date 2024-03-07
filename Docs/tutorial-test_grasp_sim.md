# pyrfuniverse tutorial : test_grasp_sim

## 1 基本功能

- 展示 Franka 二指夹爪抓取测试
- 基本思路：随机初始化物体的位置，若出现物体与夹爪之间出现穿模的现象，则不抓取；若物体的生成位置合理，则夹爪正常抓取。由于大部分时候会出现穿模现象，因此在场景中一次同时初始化大量的夹爪和物体作平行同步测试。

<img src="../Image/grasp_sim.gif">

## 2 实现流程

### 2.1 数据预处理

```python
mesh_path = "../Mesh/drink1/drink1.obj"

points, normals = get_grasp_pose(mesh_path, 100)
points = points.reshape(-1).tolist()
normals = normals.reshape(-1).tolist()
```

- 导入所需要的 mesh 和 pose 数据，并作预处理

### 2.2 初始化环境

```python
env = RFUniverseBaseEnv(assets=["GraspSim"], ext_attr=[GraspSimAttr])
```

### 2.3 展示 Franka 二指夹爪抓取测试

```python
grasp_sim = env.InstanceObject(id=123123, name="GraspSim", attr_type=GraspSimAttr)
grasp_sim.StartGraspSim(
    mesh=os.path.abspath(mesh_path),
    gripper="franka_hand",
    points=points,
    normals=normals,
    depth_range_min=-0.05,
    depth_range_max=0,
    depth_lerp_count=5,
    angle_lerp_count=5,
    parallel_count=100,
)

env.step()
while not grasp_sim.data['done']:
    env.step()
```

- 调用 `StartGraspSim` 开始夹爪测试，并等待测试完毕

```python
points = grasp_sim.data["points"]
points = np.array(points).reshape([-1, 3])
quaternions = grasp_sim.data["quaternions"]
quaternions = np.array(quaternions).reshape([-1, 4])
width = grasp_sim.data["width"]
width = np.array(width).reshape([-1, 1])

env.close()

data = np.concatenate((points, quaternions, width), axis=1)
csv = pd.DataFrame(data, columns=["x", "y", "z", "qx", "qy", "qz", "qw", "width"])

csv_path = os.path.join(os.path.dirname(mesh_path), "grasps_rfu.csv")
csv.to_csv(csv_path, index=True, header=True)

env.Pend()
env.close()
```

- 记录测试结果并保存