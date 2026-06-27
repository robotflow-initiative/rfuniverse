# URDF描述文件

整身URDF描述文件，包含底盘和身体。

## 产品型号

`x7` : 19dof, 底盘(3dof) + 躯干(3dof) + 双臂(12dof) + 头部(1dof)

`x7_pro` :  23dof, 底盘(3dof) + 躯干(4dof) + 双臂(14dof) + 头部(2dof)

## 启动测试

```bash
ros2 launch daxbot_description view_robot.launch.py model:=x7       # 启动x7
ros2 launch daxbot_description view_robot.launch.py model:=x7_pro   # 启动x7_pro
```