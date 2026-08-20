# 📅 每日脚本学习 — 2026-06-22

> **今日主题**: PowerShell 脚本 — Unity 项目资产管理自动化


## 📖 学习内容概览

今天学习编写一个 **Unity 项目资产管理自动化工具集** (`Unity-AssetTools.ps1`)，包含 4 个实用函数，覆盖 Unity 项目日常维护中最常见的 3 个场景。

| # | 函数 | 功能 | 核心知识点 |
|---|------|------|-----------|
| 1 | `Test-UnityMetaIntegrity` | 检查 .meta 文件完整性 | Hashtable O(1) 查找、ArrayList 高效收集 |
| 2 | `Remove-UnityEmptyFolders` | 清理空目录 | 自底向上遍历、WhatIf 预览模式 |
| 3 | `Get-UnityAssetReport` | 生成资产统计报告 | Group-Object 聚合、CSV 导出 |
| 4 | `Invoke-UnityProjectHealthCheck` | 一键健康检查入口 | 参数集设计、管道组合 |

---

## 🔑 10 个核心知识点

### 1. 递归文件遍历 + 条件过滤
```powershell
Get-ChildItem -Path $root -Recurse -File |
    Where-Object { $_.FullName -notmatch '\\(Library|\.git)\\ }
```
- `-Recurse` 递归所有子目录
- `-File` 只返回文件（PowerShell 3.0+）
- 用正则排除不需要扫描的目录

### 2. Hashtable 实现 O(1) 查找
```powershell
$lookup = @{}
foreach ($item in $items) { $lookup[$key] = $value }
# O(1) 判断是否存在
if ($lookup.ContainsKey($target)) { ... }
```
**为什么不用数组?** `$array -contains $x` 是 O(n)，大项目中性能差距明显。

### 3. ArrayList 代替 `+=` 避免性能陷阱
```powershell
# ❌ 错误: 每次 += 都创建新数组，O(n²)
$result = @()
foreach ($x in $largeList) { $result += $x }

# ✅ 正确: ArrayList 的 Add() 是 O(1)
$result = [System.Collections.ArrayList]::new()
foreach ($x in $largeList) { [void]$result.Add($x) }
```

### 4. 格式化输出报告
- `Write-Host -ForegroundColor` 区分信息层级
- `Format-Table -AutoSize` 自动调整列宽
- 返回 `PSCustomObject` 供下游管道使用

### 5. 自底向上处理目录树
```powershell
$dirs = @(Get-ChildItem -Recurse -Directory |
    Sort-Object { $_.FullName.Length } -Descending)
```
删除空目录必须从最深层开始，否则父目录仍含子目录不会被识别为空。

### 6. `@()` 强制数组包装
```powershell
# 单元素时可能变成标量，导致 .Count 行为异常
$unsafe = Get-ChildItem | Where-Object { ... }
$safe   = @(Get-ChildItem | Where-Object { ... })
```

### 7. Group-Object 数据聚合
```powershell
$files | Group-Object -Property Extension |
    Sort-Object Count -Descending |
    Select-Object Name, Count
```
等效于 SQL 的 `GROUP BY` + `ORDER BY`，用于快速了解数据分布。

### 8. Select-Object 计算属性
```powershell
Select-Object @(
    @{Name = 'TotalMB'; Expression = { [math]::Round($_.Sum / 1MB, 2) }}
)
```
可以动态计算并重命名列，生成更可读的输出。

### 9. Export-Csv 数据导出
```powershell
$data | Export-Csv -Path "report.csv" -Encoding UTF8 -NoTypeInformation
```
- `-Encoding UTF8` 确保中文路径正常
- `-NoTypeInformation` 去掉 PowerShell 5.1 的 `#TYPE` 头

### 10. 参数集设计 (Parameter Sets)
```powershell
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidateScript({ Test-Path $_ })]
    [string]$ProjectRoot,

    [switch]$SkipMetaCheck,
    [switch]$SkipReport
)
```
- `ValidateScript` 在函数体执行前验证参数
- `switch` 参数天然是布尔值，不需要传 `$true/$false`

---

## 🛠️ 使用方法

```powershell
# 1. 引入脚本 (dot-source)
. .\Unity-AssetTools.ps1

# 2. 一键健康检查
Invoke-UnityProjectHealthCheck -ProjectRoot "D:\ylenol\geme\I am hero_remake"

# 3. 单独使用各函数
Test-UnityMetaIntegrity -ProjectRoot "."
Remove-UnityEmptyFolders -ProjectRoot "." -WhatIf   # 预览模式
Get-UnityAssetReport -ProjectRoot "." -OutputCsv "report.csv"
```

---

## 💡 为什么 Unity 项目需要这些工具?

| 问题 | 后果 | 本工具解决方案 |
|------|------|---------------|
| .meta 文件缺失 | 资源引用丢失 (Missing Reference) | `Test-UnityMetaIntegrity` |
| 孤儿 .meta 文件 | Git 混乱、编译警告 | `Test-UnityMetaIntegrity` |
| 空目录堆积 | 项目臃肿、AssetDatabase 遍历变慢 | `Remove-UnityEmptyFolders` |
| 资产膨胀 | 不知哪些资源占空间 | `Get-UnityAssetReport` |

---

## 📊 延伸思考

1. **可以扩展的方向**: 添加 `-AutoFix` 参数，用 `UnityEditor.AssetDatabase` API 自动重新生成缺失的 .meta 文件（需在 Unity Editor 内运行）。

2. **性能考虑**: 对于超大型项目（10 万+ 文件），可以改用 `[System.IO.Directory]::EnumerateFiles()` 实现惰性遍历，避免 `Get-ChildItem` 一次性加载全部。

3. **跨平台**: 此脚本使用 `\` 作为路径分隔符，若要跨 macOS/Linux，可将路径匹配改为 `[\\/]` 正则。

---

## 🏷️ 标签

`#PowerShell` `#Unity` `#自动化` `#资产管理` `#.meta`

---

*2026-06-22 学习记录 · 每日脚本知识积累 Day 1*
