# pyrfuniverse tutorial : test_heat_map

## 1 基本功能

- 展示交互式 heatmap 热力图

<img src="../Image/heatmap.png">

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv()

camera = env.InstanceObject(name="Camera", attr_type=attr.CameraAttr)
camera.SetTransform(position=[-0.1, 0.033, 0.014], rotation=[0, 90, 0])
target = env.InstanceObject(name="Rigidbody_Sphere", attr_type=attr.RigidbodyAttr)
target.SetDrag(2)
target.EnabledMouseDrag(True)
target.SetUseGravity(False)
target.SetTransform(position=[0, 0.05, 0.015], scale=[0.01, 0.01, 0.01])
env.step()
env.AlignCamera(camera.id)
```

- 首先初始化相机和物体
- `SetDrag` 通过传入的 `drag` 参数设置拖动的阻力大小
- `EnabledMouseDrag(True)` 启用鼠标左键拖动物体的功能
- `SetUseGravity(False)` 关闭该物体的重力作用
- `AlignCamera` 将 GUI 的视角与给定的相机对齐

### 2.2 展示交互式 heatmap 热力图

```python
env.SendLog("Click End Pend button to start heat map record")
env.Pend()
camera.StartHeatMapRecord([target.id])
env.SendLog("Drag the sphere to generate heat map")
env.SendLog("Click End Pend button to end heat map record")
env.Pend()
camera.EndHeatMapRecord()
camera.GetHeatMap()
env.step()
print(camera.data["heat_map"])
image_np = np.frombuffer(camera.data["heat_map"], dtype=np.uint8)
image_np = cv2.imdecode(image_np, cv2.IMREAD_COLOR)
print(image_np.shape)
env.close()
cv2.imshow("heatmap", image_np)
cv2.waitKey(0)
```

- 单击 End Pend 按钮开始记录热力图，此时你可以自由拖动小球
- 再次单击 End Pend 按钮停止记录热力图，之后，程序会通过 cv2 把生成的热力图显示在屏幕上