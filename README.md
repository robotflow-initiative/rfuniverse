## RFUniverse

RFUniverse是基于Unity开发的用于机器人仿真和强化学习的平台，主要有三个功能模块：

[Python接口](https://github.com/mvig-robotflow/rfuniverse/blob/main/RFUniverse%20API.md)：Python封装的通信接口

Unity端Player：接收python端消息并执行仿真

Unity端Editor：用于搭建或编辑仿真场景

---

按照以下步骤配置并通过发布版运行示例场景

1. 下载最新的RFUniverse可执行程序并解压：

   [RFUniverse](https://github.com/mvig-robotflow/rfuniverse/releases)
2. Clone pyrfuniverse仓库，并正确配置环境：
   
   <https://github.com/mvig-robotflow/pyrfuniverse>

   安装时请使用editable模式，以保证后续对源码的修改正确生效
   ```
   ~$ pip install -e .
   ```
3. 修改路径参数为第一步中解压后相对应的路径

   pyrfuniverse/envs/tobor_robotiq85_manipulation_env.py 第18行
   ```
   executable_file='*/RFUniverse/Player.x86_64'
   ```
4. 运行`pyrfuniverse/AtomicActions/`下任意python脚本 

---

##### 进入Editor模式

启动Player时添加参数<-edit>以进入Editor模式
```
~$ ./Player.x86_64 -edit
```

---

### Unity工程说明

##### Assets目录结构

* AddressableAssetsData：Unity可寻址资源系统固定目录，管理资源地址和资源打包配置
* Assets：资源目录，包含所有动态加载资源
  * Model：模型/贴图/材质等资源
  * PhysicalMaterials：物理材质
  * Prefab：预制体，分配Addressable地址用于资源加载
* EditMode： Editor相关场景/资源/代码，独立于核心代码模块
* Plugins：第三方插件目录
  * [Modern UI Pack](https://assetstore.unity.com/packages/tools/gui/modern-ui-pack-201717)：Editor场景的UI插件，提供了便捷美观的UI控件
  * [Obi](https://assetstore.unity.com/publishers/5170)：Softbody，Cloth，Fluid等物理仿真插件
  * [BioIK](https://assetstore.unity.com/packages/tools/animation/bio-ik-67819)：关节IK解算插件
  * [Demigiant](https://assetstore.unity.com/packages/tools/visual-scripting/dotween-pro-32416)：Dotween，补间动画插件
  * [RootMotion](https://assetstore.unity.com/packages/tools/animation/final-ik-14290)：FinalIK，人体IK解算插件
* RFUniverse：核心功能代码
* StreamingAssets：
  * SceneData：场景Json文件的保存目录
* TextMesh Pro：UI文字模块资源
---
##### Scene场景

* RFUniverse/First.unity：程序运行的首个场景，在该场景接收命令行参数后跳转至其他场景
* RFUniverse/Empty.unity：Player模式场景
* EditMode/Edit.unity：Editro模式场景
* EditMode/Image.unity：将新配置的预制体放入该场景的Camera下，在运行状态下可生成截图
---
##### 核心功能

###### Agent

是Python与Unity进行通信的基础，场景中必须有一个Agent才能与python建立通信

###### Attributes

Attr是RFUniverse中物体的基本单元，所有的物体都是基于BaseAttr派生而来，如GameObjectAttr，RigidbodyAttr，ControllerAttr等。

###### Manager

不同的Manager负责接受和发送不同类型的数据，每一个Manager有独立的channel与python保持通信，在运行过程中通过channel接受或发送数据。