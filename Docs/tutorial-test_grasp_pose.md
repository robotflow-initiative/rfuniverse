# pyrfuniverse tutorial : test_grasp_pose

## 1 基本功能

- 展示 Franka 二指夹爪抓形预览

## 2 实现流程

### 2.1 数据预处理

```python
mesh_path = "../Mesh/drink1/drink1.obj"
pose_path = "../Mesh/drink1/grasps_rfu.csv"

data = pd.read_csv(pose_path, usecols=["x", "y", "z", "qx", "qy", "qz", "qw"])
data = data.to_numpy()
positions = data[:, 0:3].reshape(-1).tolist()
quaternions = data[:, 3:7].reshape(-1).tolist()
```

- 导入所需要的 mesh 和 pose 数据，并作预处理

### 2.2 初始化环境

```python
env = RFUniverseBaseEnv(ext_attr=[GraspSimAttr])
```

### 2.3 展示 Franka 二指夹爪抓形预览

```python
grasp_sim = env.InstanceObject(id=123123, name="GraspSim", attr_type=GraspSimAttr)
grasp_sim.ShowGraspPose(
    mesh=os.path.abspath(mesh_path),
    gripper="SimpleFrankaGripper",
    positions=positions,
    quaternions=quaternions,
)

env.Pend()
env.close()
```

- `InstanceObject` 实例化 GraspSim 对象
- `ShowGraspPose` 展示抓形预览，其中
    - `mesh` 参数为 `.obj` 文件的绝对路径
    - `gripper` 参数为夹爪的名称
    - `positions` 参数为抓取点坐标的列表
    - `quaternions` 参数为用于表示旋转的四元数
