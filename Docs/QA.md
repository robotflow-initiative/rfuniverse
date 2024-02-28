# 常见问题 Q&A

Q: 在 Linux 运行 `test_heat_map.py` 文件，生成热力图时出现如下报错：
```bash
Unity Env Log Type:Exception
Condition:DllNotFoundException: Unable to load DLL 'gdiplus'. Tried the load the following dynamic libraries: Unable to load dynamic library 'gdiplus' because of 'Failed to open the requested dynamic library (0x06000000) dlerror() = gdiplus: cannot open shared object file: No such file or directory
```
A: 尝试在终端运行以下代码以安装缺失的库文件：
```bash
sudo apt install libc6-dev
sudo apt install libgdiplus
```