# pyrfuniverse tutorial : test_custom_message

## 1 基本功能

- 发送自定义消息与动态消息

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(assets=["CustomAttr"])
```

- 定义一个具有 `CustomAttr` assets 的场景

### 2.2 发送自定义消息

```python
custom = env.InstanceObject(name="CustomAttr", id=123456, attr_type=attr.CustomAttr)
custom.CustomMessage(message="this is instance channel custom message")
env.step()
print(custom.data["custom_message"])
```

- 调用 `InstanceObject` 方法，实例化一个用于发送自定义消息的对象

- 调用 `CustomMessage` 方法，来传递要发送的自定义消息

### 2.3 发送动态消息

```python
# dynamic object
def dynamic_object_callback(args):
    print(args[0])
    print(args[1])
    print(args[2])
    print(args[3])
    print(args[4])
    print(args[5])
    print(args[6])
    print(args[7])
    print(args[8])
    print(args[9])
    print(args[10])
    print(args[11])
    print(args[12])
    print(args[13])

env.AddListenerObject("DynamicObject", dynamic_object_callback)
env.SendObject(
    "DynamicObject",
    "string:",
    "this is dynamic object",
    "int:",
    123456,
    "bool:",
    True,
    "float:",
    4849.6564,
    "list:",
    [616445.085, 9489984.0, 65419596.0, 9849849.0],
    "dict:",
    {"1": 1, "2": 2, "3": 3},
    "tuple:",
    ("1", 1, 0.562),
)
env.step()
```

- 调用 `AddListenerObject` 方法，第一个参数为消息头，第二个参数为收到消息后调用的回调函数

- 调用 `SendObject` 方法，第一个参数为消息头，后续的参数为消息的内容，支持字符串、布尔值、整数、浮点数以及包含浮点数的列表等多种类型