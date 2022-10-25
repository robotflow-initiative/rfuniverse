# RFUniverse

RFUniverse是基于Unity开发的用于机器人仿真和强化学习的平台，主要有三个功能模块：

[Python接口](https://github.com/mvig-robotflow/rfuniverse/blob/main/RFUniverse%20API.md)：Python封装的通信接口

Unity端Player：接收python端消息并执行仿真

[Unity端Editor](https://github.com/mvig-robotflow/rfuniverse/blob/main/RFUniverse%20Editor%20User%20Manual.pdf)：用于搭建或编辑仿真场景

---

按照以下步骤配置并通过发布版运行示例场景

1. 下载最新的RFUniverse可执行程序并解压：
   
   [RFUniverse Releases](https://github.com/mvig-robotflow/rfuniverse/releases)

2. Clone pyrfuniverse仓库，切换到与Release相同的Tag，正确配置环境：
   
   <https://github.com/mvig-robotflow/pyrfuniverse>
   
   安装时请使用editable模式，以保证后续对源码的修改正确生效
   
   ```
   pip install -e .
   ```

3. 在pyrfuniverse/envs/tobor_robotiq85_manipulation_env.py 第18行
   
   修改路径参数为第一步中解压后相对应的路径
   
   Linux:
   
   ```
   executable_file='~/RFUniverse/RFUniverse.x86_64'
   ```
   
   Windows:
   
   ```
   executable_file='~/RFUniverse/RFUniverse.exe'
   ```

4. 运行`pyrfuniverse/AtomicActions/`下任意python脚本 

---

##### 进入Edit模式

启动RFUniverse时添加参数<-edit>以进入Edit模式

Linux:

```
RFUniverse.x86_64 -edit
```

Windows:

```
RFUniverse.exe -edit
```

---

## Unity源工程与SDK使用说明

*以下步骤说明将默认你对UnityEditor有一定了解*

如果你想要在RFUniverse中加入自己的定制资源或功能，你可以在RFUniverse原工程的基础上添加

或者你想要为自己的项目添加RFUniverse功能，可以导入[RFUniverse Core SDK](https://github.com/mvig-robotflow/rfuniverse/releases)

如果你选择向自己的工程中导入RFUniverse Core SDK，还需要添加一些额外操作

在ProjectSettings-Player-OtherSettings中勾选`Allow ‘unsafe’ Code`

在PackageManager中导入

`https://github.com/mvig-robotflow/rfuniverse_base.git?path=/com.robotflow.rfuniverse`

`https://github.com/Unity-Technologies/URDF-Importer.git?path=/com.unity.robotics.urdf-importer#v0.5.2`

`Addressables`

package的导入方法可以参照[GitHub - Unity-Technologies/URDF-Importer: URDF importer](https://github.com/Unity-Technologies/URDF-Importer)

---

##### 插件与资源补足

打开RFUniverse工程或导入RFUniverse Core SDK后需要自行补充第三方插件和资源

**请将插件放入Plugins目录**

免费：

(必须)[DoTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)：DoTween，补间动画插件

付费：

- (可选：删除EditMode目录以消除报错)[Modern UI Pack](https://assetstore.unity.com/packages/tools/gui/modern-ui-pack-201717)：Edit模式的UI插件，提供了便捷美观的UI控件
- (可选)[Obi](https://assetstore.unity.com/publishers/5170)：Softbody，Cloth，Fluid等物理仿真插件
- (可选：删除Plugins/BioIK目录以消除报错)[BioIK](https://assetstore.unity.com/packages/tools/animation/bio-ik-67819)：IK解算插件，**请注意导入该插件时不要覆盖工程中现有的文件BioIK.cs脚本**

按需自行下载的模型资源文件

- (可选)[iGibson扫描场景](https://svl.stanford.edu/igibson/)

- (可选)[YCB数据集](http://ycb-benchmarks.s3-website-us-east-1.amazonaws.com/)

---

##### Assets目录结构

* AddressableAssetsData：Unity可寻址资源系统固定目录，管理资源地址和资源打包配置
* Assets：资源目录，包含所有动态加载资源，**如果不需要使用内置的模型和机器人资源，可以将其删除**
  * Model：模型/贴图/材质等资源
  * PhysicalMaterials：物理材质
  * Prefab：预制体，分配Addressable地址用于资源加载
* EditMode： Editor相关场景/资源/脚本，独立于核心模块，**如果不使用EditMode，可以将其删除以消除报错**
* RFUniverse：RFUniverse Core 核心功能资源及脚本
* StreamingAssets：配置文件保存目录
  * SceneData：场景Json文件的保存目录
* TextMesh Pro：TMP UI 资源

---

##### Scene场景

* RFUniverse/First.unity：发布程序运行的首个场景，在该场景接收命令行参数后跳转至其他场景
* RFUniverse/Empty.unity：Player模式场景
* EditMode/Edit.unity：Editor模式场景

---

##### 工程下的更多示例场景

pyrfuniverse/Test目录下有更多单项功能的示例

运行其中的某个python脚本后，在UnityEditor中运行Empty场景即可

---

##### 核心功能

###### Agent

是Python与Unity进行通信的基础，场景中必须有一个Agent才能与python建立通信

###### Attributes

Attr是RFUniverse中物体的基本单元，所有的物体都是基于BaseAttr派生而来，如GameObjectAttr，RigidbodyAttr，ControllerAttr等

###### Manager

Manager负责接受和发送不同类型的数据，每一个Manager有独立的channel与python保持通信，在运行过程中通过channel接受或发送数据。RFUniverser中有两个重要的Manager：

- AssetManager：负责环境中通用的接口和数据的发送

- InstanceManager：负责分发和收集面向不同Attr的接口和数据

---

##### 搭建一个简单场景

在RFUniverse中，可以将物体按照规则配置成Prefab并在运行时通过python接口动态加载物体，也可以提前搭建好固定的场景来与python通信，两种方式的选择在于你是否需要在一个发布版中运行不同的环境，大部分情况下在UnityEditor中提前搭建场景更简单快捷。

###### 搭建场景的基本流程：

1. 复制一份Empty场景，在此基础上添加自己的物体或者将RFUniverse/Assets/Prefab/RFUniverse导入现有场景，同时移除场景中原本的MainCamera

2. 为需要通信的物体添加BaseAttr脚本，手动设置不同的ID

3. 将BaseAttr脚本添加到RFUniverse/Agent的BaseAgent脚本SceneAttr列表中

4. 参照example编写python脚本，通过ID来读取物体上的信息并调用物体上的接口

---

##### 添加定制化接口

RFUniverse还在不断开发升级维护中，更新比较频繁，为了保证核心代码的更新不影响二次开发，添加了接口和数据通信的扩展模块。如果你需要在RFUniverse中添加自己的功能，请遵循以下步骤和限制，以保证RFUnivers后续更新不会与你添加的代码冲突

接口分为全局接口(AssetManager)和面向对象接口(InstanceManager)

* ###### 扩展Attr面向对象接口/属性
  
  在pyrfuniverse工程中，参照pyrfuniverse/attributes/custom_attr.py，在同目录下新建脚本，添加新增的消息读取代码和新接口，传入参数类型为`dict`，返回值类型为`OutgoingMessage`。并在pyrfuniverse/attributes/\_\_init\_\_.py中添加`import`，并加入`__all__`
  
  在Unity工程中，参照RFUniverse/Scripts/Attributes/CustomAttr.py脚本，新建脚本，继承`BaseAttr`或其他派生类，重写Type属性值与python脚本_attr前半段命名相同，重写`CollectData`在其中写入新增的数据，重写`AnalysisMsg`添加接口实现函数。

* ###### 扩展全局接口/属性
  
  在pyrfuniverse工程中，修改pyrfuniverse/rfuniverse_channer/asset_channer_ext.py脚本，参照现有代码，添加新增的消息读取代码和新接口，同样传入类型为`dict`，返回类型为`OutgoingMessage`
  
  在Unity工程中，修改AssetManagerExt.cs脚本，在`AnalysisMsg`方法的`switch`块中添加分支，并添加接口接收函数。数据发送可以在任意位置调用`AssetManager.Instance.channel.SendMetaDataToPython(sendMsg);`

定制化接口的具体添加示例请看pyrfuniverse/Test/test_custom_message.py

---

##### 动态消息接口

除了固定参数的接口外，AssetManager还支持发送动态消息进行双向数据通信，更加灵活方便

* **Python->Unity**
  
  Unity工程
  
  `AssetManger.Instance.AddListener(string message, Action<IncomingMessage> action);`
  
  传入消息名称和消息接收函数开启监听，接受函数的传入参数类型为`IncomingMessage`
  
  python端
  
  `env.asset_channel.SendMessage(self, message: str, *args)`
  
  传入消息名称和任意数量的数据进行发送

* **Unity->Python**
  
  python端
  
  `env.asset_channel.AddListener(self, message: str, fun)`
  
  传入消息名称和消息接收函数开启监听，接受函数的传入参数类型为`IncomingMessage`
  
  Unity工程
  
  `AssetManger.Instance.SendMessage(string message, params object[] objects);`
  
  传入消息名称和任意数量的数据进行发送

*请注意，动态消息必须保证接收函数中从`IncomingMessage`读取数据的类型和顺序与发送消息时传入的类型和顺序相同，否则程序会报错*

动态消息接口的具体使用示例请看pyrfuniverse/Test/test_custom_message.py