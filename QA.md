# 常见问题 Q&A

---

**Q: 在 Linux 运行 `test_heat_map.py` 文件，生成热力图时出现如下报错：**

```bash
Unity Env Log Type:Exception
Condition:DllNotFoundException: Unable to load DLL 'gdiplus'. Tried the load the following dynamic libraries: Unable to load dynamic library 'gdiplus' because of 'Failed to open the requested dynamic library (0x06000000) dlerror() = gdiplus: cannot open shared object file: No such file or directory
```

A: 尝试在终端运行以下代码以安装缺失的库文件：

```bash
sudo apt install libc6-dev
sudo apt install libgdiplus
```

---

**Q：pyrfuniverse脚本创建env后没有唤起RFU仿真程序，如何解决？**

A：默认唤起最后运行的仿真程序，如果从来没有运行过仿真程序或者最后是在Unity Editor中运行，就无法自动唤起，双击启动一次Release即可

---

**Q：为什么使用SetTransform接口设置一个物体位置后，从data["position"]没有变化？**

A：调用接口后需要调用env.step()才会执行仿真，使接口生效

---

**Q：如何选择使用正确的版本？**

A：RFU-Release和pyrfu会同步版本号发布，版本号前三位相同时将保证兼容，版本号最后一位表示补丁修复版本，有更新时建议升级。

---

**Q: 安装历史版本时遇到gym安装错误的情况如何处理**

A: 回退setuptools和wheel版本后即可安装

```
pip install setuptools==65.5.0

pip install --user wheel==0.38.0
```

---

**Q: 运行`test_cloth_attach`文件时，产生如下报错**
Condition:NullReferenceException: Object reference not set to an instance of an object
StackTrace:RFUniverse.IDistributeData`1[T].DistributeData (T hand, System.Object[] data) (at Assets/RFUniverse/Runtime/Scripts/Interface/IDistributeData.cs:24)
RFUniverse.Manager.InstanceManager.ReceiveData (System.Object[] data) (at Assets/RFUniverse/Runtime/Scripts/Manager/InstanceManager.cs:45)
...

A: 确保Obi安装正确，而且Obi放在plugin folder底下。然后点击Check Plugins (Fix Error)在Editor的最上方的Tab里。如果看到Obi plugin detected, add obi define symbols在console里就说明Obi安装没有问题。点击Reload在询问是否reload场景的弹窗里。

