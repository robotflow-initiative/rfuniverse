# pyrfuniverse tutorial : test_image_stream

## 1 基本功能

- 测试图片流的显示
- 基本思路：虚拟环境中的相机不断地对物体拍摄，同时另一个线程负责把得到的图片展示在屏幕上。

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(assets=["Camera", "GameObject_Box"])

camera = env.InstanceObject(name="Camera", id=123456, attr_type=attr.CameraAttr)
camera.SetTransform(position=[0, 0.25, 0], rotation=[30, 0, 0])
box = env.InstanceObject(name="GameObject_Box", attr_type=attr.GameObjectAttr)
box.SetTransform(position=[0, 0.05, 0.5], scale=[0.1, 0.1, 0.1])
box.SetColor([1, 0, 0, 1])
```

- 在场景中创建相机和物体，并将它们放置在合适的位置

### 2.2 拍摄并展示图片流

```python
img = None

class ImageThread(threading.Thread):
    def __init__(self):
        threading.Thread.__init__(self)

    def run(self):
        while True:
            global img
            if img is not None:
                cv2.imshow("image", img)
                cv2.waitKey(10)

thread = ImageThread()
thread.start()
while True:
    camera.GetRGB(width=512, height=512)
    box.Rotate([0, 1, 0], False)
    env.step()
    image = np.frombuffer(camera.data["rgb"], dtype=np.uint8)
    img = cv2.imdecode(image, cv2.IMREAD_COLOR)
```

- `ImageThread` 类创建了一个线程用于不断的把名为 `img` 的全局变量（即虚拟环境中拍摄得到的图片）显示到屏幕上
- `GetRGB` 用于控制相机拍摄 RGB 图像
- `Rotate` 旋转虚拟环境中的物体，以让图片流产生动态效果