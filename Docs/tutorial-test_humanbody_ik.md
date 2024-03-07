# pyrfuniverse tutorial : test_humanbody_ik

## 1 基本功能

- 展示人体 IK 接口

## 2 实现流程

### 2.1 初始化环境

```python
env = RFUniverseBaseEnv(scene_file="HumanBodyIK.json", ext_attr=[HumanbodyAttr])
env.step()
```

### 2.2 展示人体 IK 接口

```python
human = env.GetAttr(168242)
for index in range(5):
    human.HumanIKTargetDoMove(
        index=index, position=[0, 0, 0.5], duration=1, speed_based=False, relative=True
    )
    human.WaitDo()
    human.HumanIKTargetDoMove(
        index=index, position=[0, 0.5, 0], duration=1, speed_based=False, relative=True
    )
    human.WaitDo()
    human.HumanIKTargetDoMove(
        index=index, position=[0, 0, -0.5], duration=1, speed_based=False, relative=True
    )
    human.WaitDo()
    human.HumanIKTargetDoMove(
        index=index, position=[0, -0.5, 0], duration=1, speed_based=False, relative=True
    )
    human.WaitDo()

env.Pend()
env.close()
```

- `HumanIKTargetDoMove` 可以控制人体的四肢移动到指定的位置
    - `index`：移动的目标
        - 0 ：左手
        - 1 ：右手
        - 2 ：左脚
        - 3 ：右脚
        - 4 ：头
    - `position`：移动的终点位置
    - `duration`：如果 `speed_based` 参数被设置为 True，则该参数表示移动所需的时间；如果 `speed_based` 参数被设置为 False，则该参数表示移动的速度
    - `speed_based`：含义如上，默认为 True
    - `relative`：如果为 True，则为相对坐标；否则为世界绝对坐标