# PrinterManager

**本README由AI生成**

**Windows 打印机与驱动程序管理工具**

一个基于 .NET Framework 4.0 / WinForms 的 Windows 桌面应用程序，提供直观的图形界面来管理本地网络打印机和打印机驱动程序。**需要管理员权限运行。**

---

## 功能概览

### 🖨️ 打印机管理
- **枚举打印机** —— 列出所有本地和网络连接的打印机，显示名称、类型、状态、驱动程序、端口等详细信息
- **删除打印机** —— 移除选中的打印机设备
- **设置默认打印机** —— 将选中打印机设为系统默认
- **共享设置** —— 一键开启/取消打印机的网络共享
- **添加网络打印机** —— 支持通过 UNC 路径（`\\Server\ShareName`）添加网络共享打印机
- **网络浏览** —— 调用 Windows 资源管理器浏览网络，自动获取 UNC 路径
- **网络扫描** —— 扫描指定 IP 网段，自动发现网络中的共享打印机（基于 SMB 协议），支持随时停止
- **批量安装** —— 可同时勾选多台扫描到的网络打印机一键安装

### 🔧 驱动程序管理
- **枚举驱动** —— 列出系统中所有已安装的打印驱动程序（支持 V3/V4 版本识别）
- **安装驱动** —— 通过 INF 文件安装打印机驱动（自动解析驱动名称）
- **增强卸载** —— 深度卸载打印机驱动，执行完整的清理流程：
  1. PowerShell 删除所有使用该驱动的打印机对象
  2. 获取驱动 INF 路径
  3. 解析 INF 确定驱动版本
  4. 停止 Print Spooler 服务
  5. 清空 `spool\PRINTERS` 队列残留文件
  6. 终止占用驱动文件的进程
  7. Win32 API `DeletePrinterDriverEx` 卸载驱动
  8. 删除 Driver Store 目录（自动 takeown + icacls 提权）
  9. 清理注册表残留
  10. 恢复 Print Spooler 服务

### ⚙️ 系统工具
- **后台执行 + 转圈等待提示** —— 删除/安装打印机、共享设置、服务重启、清空任务、驱动卸载等耗时操作均在后台线程执行，同时弹出转圈等待框并实时显示进度文字，界面不再卡顿
- **重启打印服务**（Print Spooler）—— 一键重启，解决打印卡顿/队列阻塞问题
- **Point and Print** 注册表自动配置 —— 添加网络打印机时自动设置注册表项，允许从网络服务器安装驱动程序
- **日志面板** —— 彩色日志输出，实时记录所有操作结果（成功/告警/错误）；在"打印机列表"和"驱动程序"页面底部及"操作日志"标签页同步显示

---

## 项目结构

```
PrinterManager/
├── PrinterManager.sln                     # 解决方案文件 (VS2022)
├── LICENSE                                # GPL v3 许可证
├── PrinterManager/
│   ├── Program.cs                         # 应用程序入口
│   ├── app.manifest                       # 管理员权限清单
│   ├── PrinterManager.csproj              # 项目文件 (.NET 4.0)
│   ├── Core/
│   │   ├── PrinterApiWrapper.cs           # Windows API P/Invoke 封装 (winspool.drv)
│   │   ├── PrinterOperations.cs           # 打印机 CRUD 操作
│   │   ├── DriverOperations.cs            # 驱动程序安装/增强卸载
│   │   └── NetworkScanner.cs              # 网络打印机发现 (NetShareEnum + Ping)
│   ├── Models/
│   │   ├── PrinterInfo.cs                 # 打印机信息模型
│   │   ├── DriverInfo.cs                  # 驱动程序信息模型
│   │   └── ScanProgress.cs                # 扫描进度模型
│   ├── Helpers/
│   │   └── Progress.cs                    # IProgress 跨线程同步辅助
│   └── UI/
│       ├── MainForm.cs                    # 主窗口（含 Designer + resx）
│       ├── AddNetworkPrinterForm.cs       # 添加网络打印机对话框
│       └── ScanPrinterForm.cs             # 网络扫描对话框
```

---

## 技术栈

| 范畴 | 技术 |
|------|------|
| 运行时 | .NET Framework 4.0 |
| 界面 | Windows Forms (WinForms) |
| 语言 | C# |
| Windows API | P/Invoke → `winspool.drv`（EnumPrinters / AddPrinter / DeletePrinter / SetPrinter 等）|
| 网络发现 | `NetShareEnum` (SMB) + `System.Net.Ping` |
| 驱动卸载 | PowerShell + Win32 API + 注册表操作 + 文件系统操作 |
| IDE | Visual Studio 2022 |

---

## 系统要求

- **操作系统**: Windows 7 / 8 / 10 / 11（含 Server 版本）
- **运行时**: .NET Framework 4.0 或更高版本
- **权限**: **管理员权限**（安装/卸载打印机和驱动必须）

> 应用程序清单 (`app.manifest`) 已配置 `requireAdministrator`，启动时会自动请求 UAC 提权。

---

## 构建与运行

### 使用 Visual Studio
1. 打开 `PrinterManager.sln`
2. 选择 **Debug** | **Any CPU** 配置
3. 按 `F5` 构建并运行

### 使用 MSBuild 命令行
```bash
msbuild PrinterManager.sln /p:Configuration=Release /p:Platform="Any CPU"
```

编译产物位于 `PrinterManager\bin\Release\`。

---

## 使用指南

### 打印机列表
主界面默认显示 "打印机管理" 标签页，列出所有本地和网络连接的打印机。选中打印机后可使用工具栏按钮执行操作。页面底部内嵌操作日志面板，与"驱动程序"页面、"操作日志"标签页同步显示所有操作结果。

执行耗时操作时会弹出转圈等待框并实时显示进度，操作完成自动关闭。

### 添加网络打印机
- **方式一（手动输入）**: 点击 "添加网络打印机" → 输入 UNC 路径（如 `\\192.168.1.100\HP-LaserJet`）
- **方式二（网络浏览）**: 点击 "浏览网络" 打开资源管理器查找打印机
- **方式三（扫描发现）**: 点击 "扫描网络" → 输入目标 IP 或网段 → 扫描 → 勾选打印机 → 一键安装

### 卸载驱动程序
切换到 "驱动程序管理" 标签页 → 选中驱动 → 点击 "卸载驱动"。程序会自动执行 10 步深度清理流程。

---

## 许可证

本项目基于 **GNU General Public License v3.0** 开源协议发布。详见 [LICENSE](./LICENSE) 文件。
