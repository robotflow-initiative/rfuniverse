# pyrfuniverse tutorial : test_save_obj

## 1 基本功能

- 将场景中的多个物体保存为 obj 模型

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(scene_file="SimpleYCBModel.json")
```

### 2.2 保存物体

```python
model = []

for i in env.attrs:
    if type(env.attrs[i]) is attr.RigidbodyAttr:
        model.append(i)
```

- 把所有刚体记录在 `model` 这个列表中

```python
env.ExportOBJ(model, os.path.abspath("../Mesh/scene_mesh.obj"))

env.Pend()
env.close()
```

- `ExportOBJ`：把列表中所有的对象保存为 obj 格式的文件