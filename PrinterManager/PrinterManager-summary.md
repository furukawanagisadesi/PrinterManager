# PrinterManager — 项目摘要（供 Agent 使用）

> 用途：把一个 WinForms 的「打印机 / 驱动程序管理工具」的现状、结构、关键逻辑和已做的修改整理成一份可被另一个 Agent 直接消费的说明。
> 生成时点：在完成「主界面新增“打印测试页”按钮（WMI `Win32_Printer.PrintTestPage`）、目标框架由 .NET Framework 4.0 升级到 4.8（期间先用 `Microsoft.NETFramework.ReferenceAssemblies.net40` 过渡，后改为直接使用本机 4.8 目标包并移除该包）、高 DPI（150%）界面“挤在一起”修复（`App.config` 开启 `DpiAwareness=PerMonitorV2` + 三个窗体改为 `AutoScaleMode.Dpi`、`AutoScaleDimensions=(96,96)`、修正扫描窗体表头间距、ListView 列宽按 DPI 换算）、扫描结果可选“用 IP 还是计算机名”连接（`GetServerName` + `SharedPrinterEntry.GetUncPath(useHostName)`，复选框默认勾选计算机名）、修复打印测试页 WMI 返回值解析（`InvokeMethod` 返回 `ManagementBaseObject`，需从其 `["ReturnValue"]` 取值）」之后的状态。

## 1. 项目概览

- **名称**：PrinterManager
- **类型**：Windows 桌面程序（`WinExe`），基于 WinForms + Win32 API P/Invoke
- **目标框架**：`.NET Framework 4.8`（**已从 4.0 升级**）
- **语言**：C#
- **权限**：**必须管理员权限**（`app.manifest` 配 `requireAdministrator`，启动请求 UAC）
- **依赖**：**无 NuGet 包**。仅引用框架程序集：`System`、`System.Core`、`System.Data`、`System.Drawing`、`System.Management`（`PrintTestPage` 用）、`Microsoft.VisualBasic`（`Interaction.InputBox`）、`System.ServiceProcess`、`System.Windows.Forms`、`System.Xml`
- **核心功能**：枚举/删除/设默认/共享 打印机；添加网络共享打印机（手动 UNC / 浏览网络 / 扫描局域网网段）；枚举/安装/增强卸载驱动程序；重启 Print Spooler、清空打印任务、打印测试页；后台执行 + 转圈等待 + 彩色日志
- **构建**：VS2022 或 `msbuild PrinterManager.sln /p:Configuration=Debug`。**需本机安装 .NET Framework 4.8 目标包（targeting pack）**；本机已安装，故无需 NuGet 还原。

## 2. 文件结构

```
PrinterManager/
├── PrinterManager.sln                     # 解决方案 (VS2022)
├── LICENSE                                # GPL v3
├── README.md                              # 项目说明（含功能/构建/使用）
├── PrinterManager-summary.md              # 本文件
└── PrinterManager/
    ├── Program.cs                         # 入口：EnableVisualStyles + Run(MainForm)
    ├── app.manifest                       # requireAdministrator + 兼容性 + DPI 感知
    ├── App.config                         # WinForms 高 DPI 开关（DpiAwareness=PerMonitorV2）
    ├── PrinterManager.csproj              # .NET Framework 4.8，无 NuGet 包
    ├── Core/
    │   ├── PrinterApiWrapper.cs           # winspool.drv / Netapi 等 P/Invoke 与结构体常量
    │   ├── PrinterOperations.cs           # 打印机枚举/删除/网络连接/默认/共享 + 打印测试页
    │   ├── DriverOperations.cs            # 驱动安装 + 10 步增强卸载
    │   ├── DriverEnumerator.cs            # 驱动枚举（DRIVER_INFO_3/4）
    │   ├── InfParser.cs                   # 从 INF 解析驱动名 / 版本(V3/V4)
    │   └── NetworkScanner.cs              # 局域网共享打印机发现 + 计算机名解析
    ├── Models/
    │   ├── PrinterInfo.cs                 # 打印机模型（含 StatusText/TypeText/IsShared 等）
    │   ├── DriverInfo.cs                  # 驱动模型（含 VersionText）
    │   └── ScanProgress.cs                # 扫描进度模型
    ├── Helpers/
    │   ├── ProcessRunner.cs               # 通用进程执行（双 Task 读 stdout/stderr，防死锁）
    │   └── OperationRunner.cs             # 后台执行 + 模态转圈等待框封装
    └── UI/
        ├── MainForm.cs / .Designer.cs     # 主窗口：打印机/驱动/日志三个标签页
        ├── AddNetworkPrinterForm.cs / .Designer.cs  # 添加网络打印机对话框
        ├── ScanPrinterForm.cs / .Designer.cs        # 局域网扫描对话框
        └── WaitForm.cs                    # 转圈等待框（代码构建，无 Designer）
```

> 注意：`Helpers/Progress.cs`（自定义 `IProgress<T>`/`Progress<T>`）**已删除**——它是 4.0 时代的变通实现，升级到 4.8 后与 `System.IProgress<T>`/`System.Progress<T>` 命名冲突；`ScanPrinterForm` 现直接使用框架类型。

## 3. 各文件职责与关键逻辑

### 3.1 `Program.cs`
- `[STAThread] Main()`：`Application.EnableVisualStyles()` → `Application.SetCompatibleTextRenderingDefault(false)` → `Application.Run(new UI.MainForm())`。
- 高 DPI 缩放的实际开关在 `App.config`（见 3.3），此处无需额外调用。

### 3.2 `app.manifest`
- `requestedExecutionLevel level="requireAdministrator"`（安装/卸载驱动必须）。
- `compatibility` 声明支持 Win7~Win10/11（多个 `supportedOS` GUID）。
- `windowsSettings`：`dpiAware=true/PM` + `dpiAwareness=PerMonitorV2, PerMonitor`（进程级 DPI 感知）。
- 与 `App.config` 的值一致，不冲突。

### 3.3 `App.config`（**本次新增**）
- 内容：
  ```xml
  <configuration>
    <System.Windows.Forms.ApplicationConfigurationSection>
      <add key="DpiAwareness" value="PerMonitorV2" />
    </System.Windows.Forms.ApplicationConfigurationSection>
  </configuration>
  ```
- .NET Framework 4.7+ 的 WinForms 高 DPI 支持是**选择性开启**的，必须在 `App.config` 配置；仅靠 manifest 是旧做法且官方不再推荐。
- `PrinterManager.csproj` 中以 `<None Include="App.config" />` 引入，构建时自动生成 `bin\...\PrinterManager.exe.config`。**缺失该文件会导致 150% 缩放下字体变大而布局不缩放，控件重叠。**

### 3.4 `PrinterManager.csproj`
- `OutputType=WinExe`，`TargetFrameworkVersion=v4.8`，`AnyCPU`，`ApplicationManifest=app.manifest`。
- **无 NuGet 包**（升级 4.8 后已删除过渡用的 `Microsoft.NETFramework.ReferenceAssemblies.net40` 与 `RestoreProjectStyle`）。
- 编译项为显式 `<Compile Include=...>` 列表（非 SDK 风格）；**新增/删除 `.cs` 文件必须同步改这里**（`Helpers/Progress.cs` 已移除对应条目）。

### 3.5 `Core/PrinterApiWrapper.cs`
- `internal static`，集中所有 P/Invoke 与常量/结构体。
- 主要 API：`EnumPrinters`、`OpenPrinter`/`ClosePrinter`、`GetPrinter`/`SetPrinter`、`DeletePrinter`、`AddPrinterConnection`/`DeletePrinterConnection`、`EnumPrinterDrivers`、`AddPrinterDriver`、`DeletePrinterDriver(Ex)`、`GetDefaultPrinter`/`SetDefaultPrinter`。
- 结构体：`PRINTER_INFO_2/4`、`DRIVER_INFO_3/4/6`、`PRINTER_DEFAULTS`。
- 常量：`PRINTER_ENUM_*`、`PRINTER_ALL_ACCESS`、`PRINTER_CONTROL_PURGE`、`PRINTER_ATTRIBUTE_SHARED/NETWORK/LOCAL/DEFAULT`、`DPD_DELETE_*` 等。

### 3.6 `Core/PrinterOperations.cs`
- `EnumeratePrinters()`：`EnumPrinters`(LOCAL|CONNECTIONS, level 2) 两次调用取缓冲，映射为 `PrinterInfo`，并标记默认打印机。
- `DeletePrinter(name)`：打开打印机 → `PRINTER_CONTROL_PURGE` 清队列 → `DeletePrinter`。
- `AddNetworkPrinter(uncPath)`：校验 `\\` 前缀 → `SetPointAndPrintRegistry()`（写 HKCU/HKLM 的 Point and Print 策略，允许从网络服务器装驱动）→ `AddPrinterConnection`。
- `RemoveNetworkPrinterConnection`、`SetDefaultPrinter`、`GetDefaultPrinterName`。
- 共享：`GetPrinterInfo2`（打开+取信息2）→ `SetPrinterShare` / `UnsetPrinterShare`（改 `PRINTER_ATTRIBUTE_SHARED` 与 `pShareName` 后 `SetPrinter` level 2）。
- **`PrintTestPage(string printerName)`（本次新增）**：
  - 用 WMI `Win32_Printer.PrintTestPage()` 打印**驱动自带的 Windows 原生测试页**（等同打印机属性里的“打印测试页”）。
  - WQL 名称转义：`\` → `\\`，`'` → `\'`；`SELECT * FROM Win32_Printer WHERE DeviceID='...'`。
  - 用 `ManagementObjectSearcher` 查获 `Win32_Printer` 实例后调用 `InvokeMethod("PrintTestPage", null, null)`。
  - **返回值坑（本次修复）**：该调用绑定到 `ManagementBaseObject` 重载，返回的是 **out 参数对象**，真正的返回值在其 `["ReturnValue"]`（UInt32）属性中；**不能直接 `Convert.ToUInt32(结果)`**（会抛“无法将 `ManagementBaseObject` 强制转换为 `IConvertible`”）。实现里对“`ManagementBaseObject`”与“直接返回值”两种形态都兼容，并 `Dispose` 释放。
  - 返回码非 0 抛 `InvalidOperationException`；找不到打印机抛异常。**依赖 `System.Management`（已引用）。**
  - 另注意：报错发生在 `InvokeMethod` **执行之后**的转换阶段，所以旧实现报失败时测试页其实可能已经送出。

### 3.7 `Core/DriverOperations.cs`（卸载流程最复杂）
- `DeleteDriver(name, env, deleteFiles)`：用 `DeletePrinterDriverEx` + `DPD_DELETE_UNUSED_FILES`（比 `ALL_FILES` 安全）；仅当错误码 `50 (ERROR_NOT_SUPPORTED)` 才回退旧 `DeletePrinterDriver`，`1/87` 直接抛原始错误。
- `InstallDriver(inf, name)`：先查重 → `pnputil /add-driver /install` → `AddDriverViaApi`（`AddPrinterDriver` level 3 注册，忽略 `1795 已安装`）。
- `FindPublishedName(driverName)`：PowerShell 解析 `pnputil /enum-drivers` 的 Original→Published 映射 + `Get-PrinterDriver` 的 INF 文件名，输出 `PUBLISHED_NAME:oemNN.inf`；C# 侧校验必须是 `oem*.inf` 才采用。
- `UninstallDriverEnhanced(driverName)` 返回 `List<string> errors`，流程：
  1. PowerShell 删除使用该驱动的打印机 + 输出 `INF_PATH:`；
  2. C# 读 INF 判断 V3/V4（`[PrinterPackageInstallation]` 或 `Signature` `$CHICAGO$`）；
  3. 停 Spooler（记 `spoolerWasRunning`）；
  4. 清 `spool\PRINTERS` 下 `.SPL/.SHD`；
  5. 结束加载 `spool\drivers` 下 DLL 的进程（排除自己）；
  6. 临时启 Spooler → 查找 Published Name → `DeleteDriver`；
  7. `pnputil /delete-driver oemNN.inf /force`（清 PnP 驱动数据库，否则重装会残留）；
  8. 再停 Spooler → 删 Driver Store 目录（失败则 `takeown` + `icacls` 提权重试）；
  9. 清注册表 `HKLM\SYSTEM\CurrentControlSet\Control\Print\Environments\{arch}\Drivers\{Version-3|4}\{name}`；
  10. `finally` 恢复 Spooler。

### 3.8 `Core/DriverEnumerator.cs`
- `EnumerateDrivers()`：泛型 `EnumDrivers<T>` 统一 Level 3/4 逻辑，映射去重后按名排序。Level 4 在旧系统不支持时静默忽略。

### 3.9 `Core/InfParser.cs`
- `ParseDriverNameFromInf(inf)`：解析 `[Strings]` 与 `[Manufacturer*]`，返回真实驱动名（支持 `%key%` 引用与带引号写法）；失败回退文件名。
- `DetectDriverVersion(inf)`：返回 `"3"`/`"4"`（`[PrinterPackageInstallation]` → 4）。

### 3.10 `Core/NetworkScanner.cs`（**本次重点改动**）
- `SharedPrinterEntry`（公开类）：
  - `Host`(IP 或主机名)、`ShareName`、**`HostName`（计算机名，解析失败为空）**、`Comment`。
  - `UncPath`（旧）= `\\Host\ShareName`（IP 版，保留兼容）。
  - **`GetUncPath(bool useHostName)`**：`useHostName && HostName 非空` → `\\HostName\ShareName`，否则回退 `\\Host\ShareName`。
- `GetSharedPrinters(host)`：`NetShareEnum(level 1)` 取打印机共享（过滤 `STYPE_SPECIAL`），填 `Host/ShareName/Comment`。
- `ScanSubnet(prefix, from, to, progress, ct)`：并行（`MaxDegreeOfParallelism=32`）先 `PingHost` 再 `GetSharedPrintersWithTimeout`（`NetShareEnum` 阻塞，包到后台 Task 限时 3s，超时/取消不拖垮整体）；排序后**调用 `ResolveHostNames(list)`**。
- **`ResolveHostNames(IList<SharedPrinterEntry>)`（本次新增）**：按 `Host` 去重，每台解析一次名称写回 `HostName`。**刻意放在枚举超时之外**，避免解析耗时导致已发现的共享被丢弃。
- **`GetServerName(host)`（本次新增）**：先 `NetServerGetInfo(host, 100)`（`Netapi32.dll`，读 `SERVER_INFO_100.sv100_name`）；**用 IP 连接时它常原样回显输入**，故过滤“等于 host”的结果；再回退反向 DNS `Dns.GetHostEntry(host).HostName`，并**取第一个标签**（`FLEISCH.lan` → `FLEISCH`），过滤等于 host 的结果。均失败返回 `""`（调用方回退 IP）。
  - 实测：本机 `192.168.10.220` → 反向 DNS 得 `FLEISCH.lan` → 返回 `FLEISCH`；`NetServerGetInfo` 对 IP 返回 IP（被过滤）。
- `GetLocalIp()`（`NetworkInterface` 取首个非回环 IPv4）、`PingHost(host, timeout)`。
- 依赖：`netapi32.dll`（`NetShareEnum`/`NetApiBufferFree`/`NetServerGetInfo`）、`System.Net`（`Dns`、`Ping`）。

### 3.11 `Models/*`
- `PrinterInfo`：`Name/ServerName/ShareName/PortName/DriverName/Comment/Location/Attributes/Status/JobCount/IsDefault`；只读派生 `IsShared/IsNetwork/IsLocal/StatusText/TypeText`（`StatusText` 把 `Status` 位映射为中文）。
- `DriverInfo`：`Name/Environment/DriverPath/DataFile/ConfigFile/Version` + `VersionText`。
- `ScanProgress`：`Done/Total/Host`。

### 3.12 `Helpers/ProcessRunner.cs` / `Helpers/OperationRunner.cs`
- `ProcessRunner.Run(file, args)`：重定向 stdout/stderr，两个并行 Task 读取**防死锁**；60s 超时强杀抛 `TimeoutException`；`.NET Framework` 下超时 `WaitForExit(timeout)` 后**必须再调用无参 `WaitForExit()`** 确保管道读完。返回 `ProcessResult(Success, Output)`（含 `[Error]` 段，支持解构）。
- `OperationRunner.Run(owner, message, work, onSuccess, onError)`：显示 `WaitForm` 模态框，后台 `Task` 执行 `work`，`ReportProgress` 回调经 `BeginInvoke` 切回 UI 更新文字；完成后关闭等待框并按 `onError`/`onSuccess` 处理。重载 `Action` ↔ `Action<ReportProgress>`。
- `Helpers/Progress.cs` 已删除（见第 2 节说明）。

### 3.13 `UI/MainForm.cs`（**本次改动**）
- 三个标签页：`tabPrinters`（上 ListView + 下日志）、`tabDrivers`、`tabLog`；底部 `panelStatus`（状态文字 + 跑马灯 `progressBar`）。
- 工具栏按钮（打印机页）：刷新、添加网络打印机、删除打印机、设为默认、共享设置、重启打印服务、清空打印任务、**打印测试页（本次新增）**。驱动页：刷新、安装驱动程序、删除驱动程序。
- 日志：`LogInfo/Success/Warning/Error` → `LogThreadSafe`（必要时 `BeginInvoke` 回 UI）→ 同时写入 `rtbLog/rtbPrinterLog/rtbDriverLog`。
- 选中联动：`lvPrinters_SelectedIndexChanged` 里按 `selected` 启用 删除/设为默认/共享设置/**打印测试页**。
- **`btnPrintTestPage_Click`（本次新增）**：无选中提示；确认框 → `OperationRunner.Run` 调 `PrinterOperations.PrintTestPage(printer.Name)` → 成功写日志并 `RefreshPrinters()`（刷新作业数），失败 `ShowError`。
- **`SetupListViews()` 改为在 `MainForm_Load` 中调用**（确保句柄/`DeviceDpi` 就绪），列宽由写死像素改为 `LogicalToDeviceUnits(...)`（**ListView 列宽不随 AutoScale 缩放**，须手动换算）。随后 `RefreshAll()`。
- `ConfirmAndDeleteDriver`：删驱动前若检测到有打印机在用，先自动删这些打印机。
- 空选中/网络打印机共享等均有中文提示。

### 3.14 `UI/AddNetworkPrinterForm.cs`（**本次改动**）
- 手动输入 UNC（`txtUncPath`）、`浏览...`（`explorer.exe \\`）、`扫描局域网`（弹 `ScanPrinterForm`）。
- **`btnScanLan_Click`（本次改动）**：读取 `dlg.UseHostName`，用 `p.GetUncPath(useHostName)` 生成 `UncPaths` 与输入框回显。手动输入路径不受影响。

### 3.15 `UI/ScanPrinterForm.cs`（**本次重点改动**）
- 输入单 IP 或网段（`TryParseTarget`：三段=`x.x.x` 扫 1-254，四段末位 0=扫网段、非 0=单机），后台扫描、`btnStop` 可取消、进度条 + 文字、`SetPlaceholder` 用 `EM_SETCUEBANNER`。
- 结果 `ListView`（复选框）：主机 IP / 共享名 / **UNC 路径（随选项变化）** / 备注。
- **新增复选框 `chkUseHostName`**：文字「连接路径使用计算机名（取消勾选则用 IP）」，**默认勾选**；`CheckedChanged` 只刷新「UNC 路径」列 = `p.GetUncPath(UseHostName)`。
- 暴露 **`public bool UseHostName => chkUseHostName.Checked;`**。
- 单机扫描分支在 `GetSharedPrinters(input)` 后调用 `NetworkScanner.ResolveHostNames(found)`；`OnScanComplete` 用 `GetUncPath(UseHostName)` 填 UNC 列。
- `ScanPrinterForm_Load` 末尾按 DPI 换算 `lvResults` 列宽（120/150/220/160）。
- `SelectedPrinters`（勾选项）传回 `AddNetworkPrinterForm`。

### 3.16 `UI/WaitForm.cs`
- 代码构建的模态等待框（`AutoScaleMode.Font`，固定 `ClientSize=380x92`，跑马灯 + 可更新文字）。`SetMessage` 需在 UI 线程调用（`OperationRunner` 已保证）。
- **注意**：这个简单对话框仍为 `Font` 模式、未改为 `Dpi`；因内容简单（Dock=Fill）当前未反馈异常，后续若要统一可一并改为 `Dpi`。

## 4. 本次关键改动与根因

### 4.1 主界面「打印测试页」按钮
- 需求：为选中打印机打印 Windows 原生测试页。
- 实现：`PrinterOperations.PrintTestPage`（WMI） + `MainForm.btnPrintTestPage`（选中才启用、确认框、后台执行、成功后刷新作业数）。设计器里放在「清空打印任务」之后，`lblPrinterCount` 右移避让。
- **后续修复（WMI 返回值）**：`InvokeMethod("PrintTestPage", null, null)` 返回 `ManagementBaseObject`（out 参数对象），返回值在 `["ReturnValue"]`。原实现直接 `Convert.ToUInt32` 在真实打印机上报“无法转换为 `IConvertible`”，已改为兼容取 `["ReturnValue"]`。见 3.6。

### 4.2 目标框架 4.0 → 4.8
- 现象：`msbuild` 报 `MSB3644 找不到 .NETFramework,Version=v4.0 的引用程序集`；本机 `Reference Assemblies\...\v4.0` 只有中文文档、无 `mscorlib.dll`/`RedistList`，即 **4.0 目标包缺失**（安装 4.0 运行时会被系统提示“已是 OS 一部分”）。
- 处理：先用 NuGet `Microsoft.NETFramework.ReferenceAssemblies.net40` 过渡；后决定**直接升级目标到 4.8**，移除该包与 `RestoreProjectStyle`。
- 升级副作用：自定义 `Helpers/Progress.cs` 的 `IProgress<T>`/`Progress<T>` 与系统同名类型冲突 → **删除该文件**，改用 `System.IProgress<T>`/`Progress<T>`（行为一致：构造时捕获 `SynchronizationContext`，回调 marshal 回 UI）。
- 注意：切换目标框架后需**清掉旧 `obj/`**（残留 NuGet 资产会报“项目未引用 v4.8…请重新还原”）。

### 4.3 高 DPI（150%）界面“挤在一起”（**本次根因 + 修复**）
- 根因：WinForms 4.7+ 高 DPI 需 `App.config` 开启 `DpiAwareness`（缺失）；且这些窗体是**手写 Designer**，`AutoScaleMode.Font` 对控件树实际未生效——150% 下字体按 DPI 变大而控件位置/尺寸不缩放，于是表头标签、工具栏重叠；扫描窗体表头在 100% 下也已过挤。
- 修复：
  - 新增 `App.config`（`DpiAwareness=PerMonitorV2`）。
  - 三个窗体（`MainForm`/`ScanPrinterForm`/`AddNetworkPrinterForm`）统一 `AutoScaleMode.Dpi` + `AutoScaleDimensions=(96,96)`：按 DPI 比例（150%→1.5×）**整体缩放布局且不放大字体**。已验证该机制：2× 基准下按钮 80×30→160×60、字体仍 9pt；`Control.Scale` 亦会缩放 Dock 面板高度（70→105）。
  - 扫描窗体表头：面板高度 70→96，标题/说明/本机 IP 三行重排，消除重叠。
  - **ListView 列宽不随 AutoScale 缩放**：`MainForm.SetupListViews`（`LogicalToDeviceUnits` 换算）与 `ScanPrinterForm_Load` 均手动换算。
- 复现限制：开发机（opencode 会话）实测 `GetDpiForSystem()=96`，无法直接复现 150%；结论分别用「缩放机制单测」与「96 DPI 下表头无重叠的程序化检查」验证。

### 4.4 连接路径可选「IP / 计算机名」
- 需求：连接共享打印机时可选 `\\IP\共享名` 或 `\\计算机名\共享名`。
- 实现：`SharedPrinterEntry.HostName` + `GetUncPath(useHostName)`；`NetworkScanner.GetServerName`/`ResolveHostNames` 解析计算机名；`ScanPrinterForm` 复选框（默认计算机名）；`AddNetworkPrinterForm` 按选项生成路径。
- 注意：`NetServerGetInfo` 用 IP 连接时不可靠（常回显 IP），实际名称主要靠**反向 DNS**；解析失败自动回退 IP。见 3.10。

## 5. 已知行为 / 注意点

- **必须管理员权限**：否则大部分操作失败（manifest 已强制）。
- **无 NuGet 依赖**；改 `.csproj` 的 `<Compile>` 列表才能增删源文件。
- **高 DPI**：四个窗体里 `WaitForm` 仍是 `Font` 模式；其余三个为 `Dpi` 模式。若新增窗体，建议同样用 `AutoScaleMode.Dpi`+`(96,96)`；`ListView` 列宽记得 `LogicalToDeviceUnits`。
- **打印测试页**：走 WMI `Win32_Printer.PrintTestPage`，等同系统原生测试页；需要 Spooler 正常、驱动可用。返回值必须从 `InvokeMethod` 结果的 `["ReturnValue"]` 读取（见 4.1）。
- **扫描计算机名**：依赖反向 DNS；`NetServerGetInfo` 常回显输入。解析失败 → UNC 回退 IP（复选框勾选也不影响连接）。
- **网络打印机共享设置**：连接的共享打印机（`IsNetwork`）不能本地改共享，`btnToggleShare` 会提示。
- **删除驱动会先自动删除正在使用它的打印机**（需确认）。
- **日志同步**：三个日志框（打印机页、驱动页、日志页）内容一致，`btnClearLog` 清空全部。
- 若编译报“文件被占用”，先关闭正在运行的 `PrinterManager.exe`。
- 工程路径：`D:\CSharp\WorkProject\PrinterManager`；GitHub：`https://github.com/furukawanagisadesi/PrinterManager.git`（分支 `main`）。

## 6. 关键数据流

### 6.1 扫描并安装网络打印机（含计算机名选项）
```
[添加网络打印机] → 扫描局域网 → ScanPrinterForm
  → 输入 IP/网段 → 后台 Task
  → 网段：PingHost + GetSharedPrintersWithTimeout(NetShareEnum,3s,并行32)
     单机：GetSharedPrinters
  → ResolveHostNames（按 Host 去重 → GetServerName：NetServerGetInfo → 反向 DNS 首标签）
  → OnScanComplete 填充 ListView（UNC 列 = GetUncPath(UseHostName)，复选框默认勾选=计算机名）
  → 勾选若干 → btnInstall_Click → SelectedPrinters
→ AddNetworkPrinterForm.btnScanLan_Click
  → UncPaths = p.GetUncPath(dlg.UseHostName)
→ MainForm.btnAddNetwork_Click
  → OperationRunner(后台) 逐条 PrinterOperations.AddNetworkPrinter(UNC)
       SetPointAndPrintRegistry → AddPrinterConnection
  → 成功后 RefreshPrinters()；有失败则汇总提示 + 写日志
```

### 6.2 打印测试页
```
MainForm 选中打印机 → btnPrintTestPage_Click（确认框）
  → OperationRunner(后台) → PrinterOperations.PrintTestPage(name)
       WMI: SELECT * FROM Win32_Printer WHERE DeviceID='<转义名>'
            → InvokeMethod("PrintTestPage", null, null) → 从结果取 ["ReturnValue"] → 返回码≠0 抛异常
  → 成功：LogSuccess + RefreshPrinters()（刷新作业数）
  → 失败：ShowError 写日志 + 弹窗
```
