# 常见问题 Q&A

**Q: 按照 README 上的指引 clone 完 pyrfuniverse 后尝试运行 Test 目录下的测试脚本发现报错**

A: 是否忘记了查看 **pyrfuniverse 目录下的 README**，请按照其上的指引安装其他依赖

---

**Q: 如何更新 rfuniverse？**

A: 下载新版本的 rfuniverse，**并手动运行一次场景**，将 pyrfuniverse 的仓库克隆到项目文件夹下，然后在终端中运行

```
pip install pyrfuniverse --upgrade
```

以更新 pyrfuniverse 库

---

**Q: 运行测试脚本时出现 `ModuleNotFoundError: No module named 'extend'` 报错**

A: 尝试在测试脚本开头加上如下代码段

```python
import sys
sys.path.append("/{path_to_rfuniverse}/pyrfuniverse")
```

（把其中的 `{path_to_rfuniverse}` 替换为项目根目录的绝对路径）

---

**Q: 在运行测试脚本时出现类似如下报错**

```
Unity Env Log Type:Exception
Condition:FileNotFoundException: File not found
StackTrace:System.Reflection.RuntimeMethodInfo.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at <00000000000000000000000000000000>:0)
...
```

A: 尝试把终端的工作目录切换到 `/{path_to_rfuniverse}/pyrfuniverse/Test`，而非 `/{path_to_rfuniverse}/pyrfuniverse`

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
