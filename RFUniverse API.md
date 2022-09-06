# RFUnivese Player 接口文档

Unity Player在初始状态下是不包含任何物体空白场景，物体通过python端发送消息实现创建和驱动。创建的物体必须分配一个全局唯一的ID，后续驱动物体的操作通过该ID来进行区分。

---

实例化仿真环境后将会打开RFUniverse窗口，随后调用所有接口:

```
from pyrfuniverse.envs import RFUniverseBaseEnv
env = RFUniverseBaseEnv
(
    executable_file='<rfuniverse_path>',
    scene_file='<scenedata_file_name>',
    assets=assets_name_list,
)
```

* (str)executable_file:RFUniverse可执行文件路径，不使用此参数则默认为Unity Editor
* (str)scene_file:需要加载的场景Json文件名
* (list<str>)assets:需要预加载的物体名列表，预加载过的物体在InstanceObject时会立即生成，否则异步生成可能造成错误

---

### AssetChannel:

负责资源加载创建以及环境中的通用功能

* SendMessage:发送一个字符串消息
  
  ```
  env.asset_channel.SendMessage
  (
      'msgString'
      ...
  )
  ```
  
  * (必要) (str)msg:消息标识
  * *args:任意string,int,float,bool,list<float>类型参数
  
  ---

* IgnoreLayerCollision:设置物体层间碰撞的开启或关闭
  
  ```
  env.asset_channel.set_action
  (
      'IgnoreLayerCollision',
      layer1=1,
      layer2=2,
      ignore=True
  )
  ```
  
  * (必要) (int) layer1:层1
  * (必要) (int) layer2:层2
  * (必要) (bool) ignore:忽略
  
  ---

* GetCurrentCollisionPairs:获取当前的碰撞对
  
  ```
  env.asset_channel.set_action
  (
      'GetCurrentCollisionPairs'
  )
  env._step()
  result = env.asset_channel.data['collision_pairs']
  ```
  
  ---

* GetRFMoveColliders:获取碰撞范围
  
  ```
  env.asset_channel.set_action
  (
      'GetRFMoveColliders'
  )
  env._step()
  result = env.asset_channel.data['colliders']
  ```
  
  ---

* SetGravity:设置环境重力
  
  ```
  env.asset_channel.set_action
  (
      'SetGravity',
      x=0,
      y=-0.98,
      z=0
  )
  ```
  
  * (必要) (float) x
  * (必要) (float) y
  * (必要) (float) z
  
  ---

* SetGroundPhysicMaterial:设置地面物理材质
  
  ```
  env.asset_channel.set_action
  (
      'SetGroundPhysicMaterial',
      bounciness=0,
      dynamic_friction=1,
      static_friction=1
      friction_combine=0
      bounce_combine=0
  )
  ```
  
  * (必要) (float) bounciness:弹力
  * (必要) (float) dynamic_friction:动摩擦力
  * (必要) (float) static_friction:静摩擦力
  * (必要) (int) friction_combine:摩擦力混合方式
  * (必要) (int) bounce_combine:弹力混合方式
  
  ---

* SetTimeStep:设置step时间间隔
  
  ```
  env.asset_channel.set_action
  (
      'SetTimeStep',
      delta_time=0.02,
  )
  ```
  
  * (必要) (float) delta_time:每step时间间隔
  
  ---

* SetTimeScale:设置时间缩放
  
  ```
  env.asset_channel.set_action
  (
      'SetTimeScale',
      time_scale=1,
  )
  ```
  
  * (必要) (float) time_scale:时间缩放倍数
  
  ---

* InstanceObject:创建物体。
  
  ```
  env.asset_channel.set_action
  (
      'InstanceObject',
      name='kinova_gen3_robotiq85',
      id=123
  )
  ```
  
  * (必要) (stirng) name:资源名称 
  
  * (必要) (int) id:物体创建后赋予实例ID
    
    #### RfUniverseRelease中已有的资源类型及名称:
  
  * GameObjcet:静态模型
    
    ###### Base:
    
    GameObject_Box
    
    GameObject_Cylinder
    
    GameObject_Sphere
    
    GameObject_Quad
    
    Collider_Box
    
    ###### IGbison环境:
    
    Hainesburg_mesh_texture
    
    Halfway_mesh_texture
    
    Hallettsville_mesh_texture
    
    Hambleton_mesh_texture
    
    Hammon_mesh_texture
    
    Hatfield_mesh_texture
    
    Haxtun_mesh_texture
    
    Haymarket_mesh_texture
    
    Hendrix_mesh_texture
    
    Hercules_mesh_texture
    
    Highspire_mesh_texture
    
    Hitchland_mesh_texture
  
  * Rigidbody:刚体
    
    ###### Base:
    
    Rigidbody_Box
    
    Rigidbody_Cylinder
    
    Rigidbody_Sphere
    
    ###### 77个YCB数据集模型:
    
    详见[The YCB Object and Model Set](http://ycb-benchmarks.s3-website-us-east-1.amazonaws.com/)
  
  * Controller:机械臂及关节体
    
    ###### Robots:
    
    kinova_gen3_robotiq85
    
    ur5_robotiq85
    
    franka_panda
    
    tobor_robotiq85_robotiq85
    
    ###### Other:
    
    pen_and_pencap
    
    handled_box,handled_drawer
    
    Drawer1,Drawer2,Drawer3,Drawer4,Drawer5,Drawer6,Drawer7,Drawer8,Drawer9,Drawer10
  
  * Camera:相机

---

### InstanceChannel

针对创建后每个实例物体的接口

#### 所有物体可用:

* SetTransform:设置物体Position, Rotation, Scale
  
  ```
  env.instance_channel.set_action
  (
      'SetTransform',
      id=123,
      position=[0,0,0],
      rotation=[0,0,0],
      scale=[0,0,0]
  )
  ```
  
  * (必要) (int) id:物体实例ID
  * (list<float>[3]) position:局部坐标位置
  * (list<float>[3]) rotation:局部坐标旋转
  * (list<float>[3]) scale:局部坐标缩放

---

* SetRotationQuaternion:设置物体Quaternion
  
  ```
  env.instance_channel.set_action
  (
      'SetRotationQuaternion',
      id=123,
      quaternion=[0,0,0]
  )
  ```
  
  * (必要) (int) id:物体实例ID
  * (list<float>[4]) quaternion:局部坐标四元数旋转

---

* SetActive:设置物体激活状态
  
  ```
  env.instance_channel.set_action
  (
      'SetActive',
      id=123,
      active=Fales
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (bool)active:是否激活

---

* SetParent:设置父物体
  
  ```
  env.instance_channel.set_action
  (
      'SetParent',
      id=123,
      parent_id=456,
      name='parent'
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (int)parent_id:父物体所在实例的ID，-1则不设父物体
  * (必要) (string)name:父物体名，为空则防止在根节点

---

* SetLayer:设置物体层
  
  ```
  env.instance_channel.set_action
  (
      'SetLayer',
      id=123,
      layer=1
  )  
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (int)layer:层编号

---

* Destroy:销毁物体
  
  ```
  env.instance_channel.set_action
  (
      'Destroy',
      id=123
  )
  ```
  
  * (必要) (int)id:物体实例ID

---

* GetLoaclPointFromWorld:世界空间转局部空间位置
  
  ```
  env.instance_channel.set_action
  (
      'GetLoaclPointFromWorld',
      id=123,
      point=[1, 1, 1]
  )
  env._step()
  result = env.instance_channel.data[123]['result_local_point']
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[3])point:世界空间位置

---

* GetWorldPointFromLocal:局部空间转世界空间位置
  
  ```
  env.instance_channel.set_action
  (
      'GetWorldPointFromLocal',
      id=123,
      point=[1, 1, 1]
  )
  env._step()
  result = env.instance_channel.data[123]['result_world_point']
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[3])point:局部空间位置

---

#### GameObject类型可用:

* Translate:增量移动物体
  
  ```
  env.instance_channel.set_action
  (
      'Translate',
      id=123,
      translation=[0,0,0]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[3])translation:增量移动值

---

* Rotate:增量旋转物体
  
  ```
  env.instance_channel.set_action
  (
      'Rotate',
      id=123,
      rotate=[0,0,0]
  )  
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[3])translation:增量旋转值

---

* SetColor:修改物体基础颜色
  
  ```
  env.instance_channel.set_action
  (
      'SetColor',
      id=123,
      color=[0,0,0,0]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[4])color:颜色rgba值

---

#### Rigidbody类型可用:

* AddForce:为Rigidbody施加持续的力
  
  ```
  env.instance_channel.set_action
  (
      'AddForce',
      id=123,
      force=[0,0,0]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[3])force:施加力值

---

* SetVelocity:修改Rigidbody速度
  
  ```
  env.instance_channel.set_action
  (
      'SetVelocity',
      id=123,
      velocity=[0,0,0]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[3])velocity:速度值

---

#### Articulation类型可用:

* SetJointPosition:修改关节体内每个关节的目标位置
  
  ```
  env.instance_channel.set_action
  (
      'SetJointPosition',
      id=123,
      joint_positions=[],
      speed_scales=[]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[n])joint_positions:关节位置
  * (list<float>[n])speed_scales:关节速度倍率

---

* SetJointPositionDirectly:直接修改关节体内每个关节的位置
  
  ```
  env.instance_channel.set_action
  (
      'SetJointPositionDirectly',
      id=123,
      joint_positions=[]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[n])joint_positions:关节位置

---

* SetJointPositionContinue:持续设置每个关节位置
  
  ```
  env.instance_channel.set_action
  (
      'SetJointPositionContinue',
      id=123,
      interval=20,
      time_joint_positions=[]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (int)interval:间隔，单位毫秒
  * (必要) (list<list<float>[n]>[t])time_joint_positions:关节位置

---

* SetJointVelocity:设置每个关节速度
  
  ```
  env.instance_channel.set_action
  (
      'SetJointVelocity',
      id=123,
      joint_velocitys=[]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[n])joint_velocitys:关节速度

---

* AddJointForce:为每个关节施加力
  
  ```
  env.instance_channel.set_action
  (
      'SetJointForce',
      id=123,
      joint_forces=[]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[n])joint_forces:力

---

* AddJointForceAtPosition:为每个关节特定位置施加力
  
  ```
  env.instance_channel.set_action
  (
      'AddJointForceAtPosition',
      id=123,
      joint_forces=[]
      forces_position=[]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[n])joint_forces:力
  * (必要) (list<float>[n])forces_position:位置施加力的世界空间位置

---

* AddJointTorque:为每个关节施加扭矩
  
  ```
  env.instance_channel.set_action
  (
      'AddJointTorque',
      id=123,
      joint_torque=[]
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[n])joint_torque:扭矩

---

* GetJointInverseDynamicsForce:获取关节的反向动力学力
  
  ```
  env.instance_channel.set_action
  (
      'GetJointInverseDynamicsForce',
      id=123,
  )
  env._step()
  gravity_forces = env.instance_channer.data[123]['gravity_forces']
  coriolis_centrifugal_forces = env.instance_channer.data[123]['coriolis_centrifugal_forces']
  drive_forces = env.instance_channer.data[123]['drive_forces']
  ```
  
  * (必要) (int)id:物体实例ID

---

* SetImmovable:设置关节体是否可移动
  
  ```
  env.instance_channel.set_action
  (
      'SetImmovable',
      id=123,
      immovable=True
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (bool)immovable:可移动

---

* MoveForward:关节体前进，只有包含定制化移动脚本脚本的物体可用
  
  ```
  env.instance_channel.set_action
  (
      'MoveForward',
      id=123,
      distance=1,
      speed=0.2,
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (float)distance:移动距离
  * (必要) (float)speed:移动速度

---

* MoveBack:关节体后退，只有包含定制化移动脚本脚本的物体可用
  
  ```
  env.instance_channel.set_action
  (
      'MoveBack',
      id=123,
      distance=1,
      speed=0.2,
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (float)distance:移动距离
  * (必要) (float)speed:移动速度

---

* TurnLeft:关节体左转，只有包含定制化移动脚本脚本的物体可用
  
  ```
  env.instance_channel.set_action
  (
      'TurnLeft',
      id=123,
      angle=1,
      speed=0.2,
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (float)angle:转向角度
  * (必要) (float)speed:转向速度

---

* TurnRight:关节体左转，只有包含定制化移动脚本脚本的物体可用
  
  ```
  env.instance_channel.set_action
  (
      'TurnRight',
      id=123,
      angle=1,
      speed=0.2,
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (float)angle:转向角度
  * (必要) (float)speed:转向速度

---

* EnabledNativeIK:启用或禁用关节体原生IK
  
  ```
  env.instance_channel.set_action
  (
      'EnabledNativeIK',
      id=123,
      enabled=True
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (bool)enabled:是否启用原生IK

---

* IKTargetDoMove:线性移动IK目标
  
  ```
  env.instance_channel.set_action
  (
      'IKTargetDoMove',
      id=123,
      position=[1, 1, 1],
      speed=0.5,
      relative=True
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[3])position:目标位置
  * (必要) (bool)speed:移动速度
  * (bool)relative:是否是相对位置

---

* IKTargetDoRotateQuaternion:线性旋转IK目标
  
  ```
  env.instance_channel.set_action
  (
      'IKTargetDoRotateQuaternion',
      id=123,
      quaternion=[1, 1, 1],
      speed=0.5,
      relative=True
  )
  ```
  
  * (必要) (int)id:物体实例ID
  * (必要) (list<float>[4])quaternion:目标旋转
  * (必要) (bool)speed:旋转速度
  * (bool)relative:是否是相对位置

---

* IKTargetDoComplete:立即完成之前的IK目标运动
  
  ```
  env.instance_channel.set_action
  (
      'IKTargetDoComplete',
      id=123,
  )
  ```
  
  * (必要) (int)id:物体实例ID

---

* IKTargetDoKill:定制运动IK目标
  
  ```
  env.instance_channel.set_action
  (
      'IKTargetDoKill',
      id=123,
  )
  ```
  
  * (必要) (int)id:物体实例ID

---

#### Camera类型可用:

* GetRGB:获取相机RGB图像
  
  ```
  env.instance_channel.set_action
  (
      'GetRGB',
      id=123,
      width=512,
      height=512
  )
  env._step()
  result = env.instance_channer.data[123]['rgb']
  ```
  
  * (必要) (int)id:相机实例ID
  * (必要) (int)width:图像宽度
  * (必要) (int)height:图像高度

---

* GetNormal:获取相机世界空间法线图像
  
  ```
  env.instance_channel.set_action
  (
      'GetNormal',
      id=123,
      width=512,
      height=512
  )
  env._step()
  result = env.instance_channer.data[123]['normal']
  ```
  
  * (必要) (int)id:相机实例ID
  * (必要) (int)width:图像宽度
  * (必要) (int)height:图像高度

---

* GetID:获取相机世界空间法线图像
  
  ```
  env.instance_channel.set_action
  (
      'GetID',
      id=123,
      width=512,
      height=512
  )
  env._step()
  result = env.instance_channer.data[123]['id_map']
  ```
  
  * (必要) (int)id:相机实例ID
  * (必要) (int)width:图像宽度
  * (必要) (int)height:图像高度

---

* GetDepth:获取相机8位深度图像，图像将进行remap以提高精度
  
  ```
  env.instance_channel.set_action
  (
      'GetDepth',
      id=123,
      width=512,
      height=512,
      zero_dis=0,
      one_dis=5,
  )
  env._step()
  result = env.instance_channer.data[123]['depth']
  ```
  
  * (必要) (int)id:相机实例ID
  * (必要) (int)width:图像宽度
  * (必要) (int)height:图像高度
  * (必要) (float)zero_dis:黑色像素点的实际深度
  * (必要) (float)one_dis:白色像素点的实际深度

---

* GetDepthEXR:获取相机16位深度图像，图像存储真实距离
  
  ```
  env.instance_channel.set_action
  (
      'GetDepthEXR',
      id=123,
      width=512,
      height=512,
  )
  env._step()
  result = env.instance_channer.data[123]['depth_exr']
  ```
  
  * (必要) (int)id:相机实例ID
  * (必要) (int)width:图像宽度
  * (必要) (int)height:图像高度

---