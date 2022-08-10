## RFUniverse

RFUniverse是基于Unity开发的用于机器人仿真和强化学习的平台，主要有三个功能模块：

[Python接口](RFUniverse%20Player%20%E6%8E%A5%E5%8F%A3%E6%96%87%E6%A1%A3.md?fileId=9061)：Python封装的通信接口

Unity端Player：接收python端消息并执行仿真

[Unity端Editor](RFUniverse%20Editor%20%E4%BD%BF%E7%94%A8%E6%8C%87%E5%8D%97.md?fileId=8886)：用于搭建或编辑仿真场景

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