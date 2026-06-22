<#
.SYNOPSIS
    Unity 项目资产管理自动化工具集
.DESCRIPTION
    提供 Unity 项目日常维护的实用函数：
    - 检查 .meta 文件完整性
    - 清理空目录
    - 生成项目资产统计报告
    - 查找孤立文件（无对应 .meta 的资产 / 无对应资产的 .meta）
.NOTES
    学习日期: 2026-06-22
    适用环境: Windows PowerShell 5.1 + Unity 项目
    知识要点: 文件系统遍历、哈希表查找、正则匹配、CSV 导出
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ============================================================
# 区域 1: .meta 文件完整性检查
# ============================================================

<#
    知识点 1: 使用 Get-ChildItem 递归遍历，配合 Where-Object 过滤
    Unity 中每个资产文件和文件夹都必须有一个对应的 .meta 文件。
    .meta 文件包含 GUID，是 Unity 引用系统的核心。
#>
function Test-UnityMetaIntegrity {
    param(
        [Parameter(Mandatory)]
        [string]$ProjectRoot,

        [switch]$AutoFix
    )

    Write-Host "=== .meta 文件完整性检查 ===" -ForegroundColor Cyan

    # 获取所有非 .meta 文件（排除 Library、.git 等目录）
    $allFiles = Get-ChildItem -Path $ProjectRoot -Recurse -File |
        Where-Object {
            $_.FullName -notmatch '\\(Library|\.git|Temp|Obj|Logs|Build|Builds)\\' -and
            $_.Extension -ne '.meta'
        }

    # 获取所有 .meta 文件
    $metaFiles = Get-ChildItem -Path $ProjectRoot -Recurse -File |
        Where-Object {
            $_.FullName -notmatch '\\(Library|\.git|Temp|Obj|Logs|Build|Builds)\\' -and
            $_.Extension -eq '.meta'
        }

    # 知识点 2: 使用哈希表 (Hashtable) 实现 O(1) 查找
    $metaLookup = @{}
    foreach ($meta in $metaFiles) {
        # 去掉 .meta 后缀得到对应资产路径
        $assetPath = $meta.FullName -replace '\.meta$', ''
        $metaLookup[$assetPath] = $meta.FullName
    }

    # 知识点 3: 使用 ArrayList 高效收集结果（避免 += 的 O(n²) 问题）
    $missingMeta = [System.Collections.ArrayList]::new()
    $orphanMeta  = [System.Collections.ArrayList]::new()

    foreach ($file in $allFiles) {
        if (-not $metaLookup.ContainsKey($file.FullName)) {
            [void]$missingMeta.Add($file.FullName)
        }
        # 标记已匹配的 meta
        if ($metaLookup.ContainsKey($file.FullName)) {
            $metaLookup.Remove($file.FullName)
        }
    }

    # 剩余未被匹配的 .meta 即为孤立文件
    foreach ($key in $metaLookup.Keys) {
        [void]$orphanMeta.Add($metaLookup[$key])
    }

    # 知识点 4: 格式化输出报告
    Write-Host "`n[结果] 缺失 .meta 的资产: $($missingMeta.Count) 个" -ForegroundColor Yellow
    if ($missingMeta.Count -gt 0) {
        $missingMeta | ForEach-Object { Write-Host "  ! $_" -ForegroundColor Red }
    }

    Write-Host "[结果] 孤立的 .meta 文件: $($orphanMeta.Count) 个" -ForegroundColor Yellow
    if ($orphanMeta.Count -gt 0) {
        $orphanMeta | ForEach-Object { Write-Host "  ! $_" -ForegroundColor Red }
    }

    if ($missingMeta.Count -eq 0 -and $orphanMeta.Count -eq 0) {
        Write-Host "[OK] .meta 文件完整性检查通过!" -ForegroundColor Green
    }

    # 返回结果供管道使用
    return [PSCustomObject]@{
        MissingMeta = $missingMeta
        OrphanMeta  = $orphanMeta
        IsHealthy   = ($missingMeta.Count -eq 0 -and $orphanMeta.Count -eq 0)
    }
}

# ============================================================
# 区域 2: 空目录清理
# ============================================================

<#
    知识点 5: 递归删除空目录需要自底向上处理（先处理子目录）
    使用 Sort-Object 按路径深度降序排列
#>
function Remove-UnityEmptyFolders {
    param(
        [Parameter(Mandatory)]
        [string]$ProjectRoot,

        [switch]$WhatIf
    )

    Write-Host "=== 空目录清理 ===" -ForegroundColor Cyan

    # 知识点 6: 使用 @() 强制为数组，避免单元素时变成标量
    $allDirs = @(Get-ChildItem -Path $ProjectRoot -Recurse -Directory |
        Where-Object {
            $_.FullName -notmatch '\\(Library|\.git|Temp|Obj|Logs|Build|Builds)\\'
        } |
        Sort-Object -Property @{Expression = { $_.FullName.Length }; Descending = $true})

    $removed = 0
    foreach ($dir in $allDirs) {
        # 检查目录是否为空（不含任何文件和子目录，允许只有 .meta 的情况）
        $contents = @(Get-ChildItem -Path $dir.FullName -Force |
            Where-Object { $_.Name -notmatch '\.meta$' })

        if ($contents.Count -eq 0) {
            if ($WhatIf) {
                Write-Host "  [WhatIf] 将删除: $($dir.FullName)" -ForegroundColor DarkYellow
            } else {
                # 同时删除对应的 .meta 文件
                $metaPath = "$($dir.FullName).meta"
                if (Test-Path $metaPath) {
                    Remove-Item -Path $metaPath -Force
                }
                Remove-Item -Path $dir.FullName -Force -Recurse
                Write-Host "  [已删除] $($dir.FullName)" -ForegroundColor Green
            }
            $removed++
        }
    }

    Write-Host "`n[完成] 共清理 $removed 个空目录" -ForegroundColor Green
    return $removed
}

# ============================================================
# 区域 3: 项目资产统计报告
# ============================================================

<#
    知识点 7: 使用 Group-Object 进行数据聚合统计
    可以快速了解项目中各类资产的分布情况
#>
function Get-UnityAssetReport {
    param(
        [Parameter(Mandatory)]
        [string]$ProjectRoot,

        [string]$OutputCsv
    )

    Write-Host "=== 项目资产统计报告 ===" -ForegroundColor Cyan

    $assets = Get-ChildItem -Path $ProjectRoot -Recurse -File |
        Where-Object {
            $_.FullName -notmatch '\\(Library|\.git|Temp|Obj|Logs|Build|Builds|Packages|ProjectSettings)\\'
        }

    # 知识点 8: 使用 Group-Object 按扩展名分组统计
    $typeStats = $assets |
        Where-Object { $_.Extension -ne '.meta' } |
        Group-Object -Property Extension |
        Sort-Object -Property Count -Descending |
        Select-Object -First 20 @(
            @{Name = 'Extension'; Expression = { $_.Name }},
            @{Name = 'Count';    Expression = { $_.Count }},
            @{Name = 'TotalMB';  Expression = { [math]::Round(($_.Group | Measure-Object -Property Length -Sum).Sum / 1MB, 2) }}
        )

    # 按文件夹分组统计
    $folderStats = $assets |
        Where-Object { $_.Extension -ne '.meta' } |
        Group-Object -Property { $_.DirectoryName -replace [regex]::Escape($ProjectRoot), '' } |
        Sort-Object -Property Count -Descending |
        Select-Object -First 10 @(
            @{Name = 'Folder'; Expression = { $_.Name }},
            @{Name = 'Files';  Expression = { $_.Count }},
            @{Name = 'MB';     Expression = { [math]::Round(($_.Group | Measure-Object -Property Length -Sum).Sum / 1MB, 2) }}
        )

    # 输出到控制台
    Write-Host "`n--- 按文件类型 (Top 20) ---" -ForegroundColor Yellow
    $typeStats | Format-Table -AutoSize

    Write-Host "--- 按文件夹 (Top 10) ---" -ForegroundColor Yellow
    $folderStats | Format-Table -AutoSize

    $totalAssets = ($assets | Where-Object { $_.Extension -ne '.meta' }).Count
    $totalSizeMB = [math]::Round(($assets | Where-Object { $_.Extension -ne '.meta' } | Measure-Object -Property Length -Sum).Sum / 1MB, 2)
    Write-Host "[总计] $totalAssets 个资产, $totalSizeMB MB" -ForegroundColor Green

    # 知识点 9: 导出为 CSV 文件（方便 Excel 分析）
    if ($OutputCsv) {
        $typeStats | Export-Csv -Path $OutputCsv -Encoding UTF8 -NoTypeInformation
        Write-Host "[导出] 报告已保存至: $OutputCsv" -ForegroundColor Green
    }

    return [PSCustomObject]@{
        TypeStats   = $typeStats
        FolderStats = $folderStats
        TotalAssets = $totalAssets
        TotalSizeMB = $totalSizeMB
    }
}

# ============================================================
# 区域 4: 主入口 — 一键检查
# ============================================================

<#
    知识点 10: 参数集 (ParameterSetName) 允许不同的参数组合
    使用 switch 参数控制执行哪些检查
#>
function Invoke-UnityProjectHealthCheck {
    param(
        [Parameter(Mandatory, Position = 0)]
        [ValidateScript({ Test-Path $_ -PathType Container })]
        [string]$ProjectRoot,

        [switch]$SkipMetaCheck,
        [switch]$SkipEmptyDirCheck,
        [switch]$SkipReport,

        [string]$ReportCsv
    )

    Write-Host "`n========== Unity 项目健康检查 ==========" -ForegroundColor Magenta
    Write-Host "项目路径: $ProjectRoot"
    Write-Host "检查时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    Write-Host "========================================`n"

    $results = @{}

    if (-not $SkipMetaCheck) {
        Write-Host "`n[步骤 1/3]" -ForegroundColor Cyan
        $results.MetaCheck = Test-UnityMetaIntegrity -ProjectRoot $ProjectRoot
    }

    if (-not $SkipEmptyDirCheck) {
        Write-Host "`n[步骤 2/3]" -ForegroundColor Cyan
        $results.EmptyDirs = Remove-UnityEmptyFolders -ProjectRoot $ProjectRoot -WhatIf
    }

    if (-not $SkipReport) {
        Write-Host "`n[步骤 3/3]" -ForegroundColor Cyan
        $csvPath = if ($ReportCsv) { $ReportCsv } else { Join-Path $ProjectRoot "AssetReport.csv" }
        $results.AssetReport = Get-UnityAssetReport -ProjectRoot $ProjectRoot -OutputCsv $csvPath
    }

    Write-Host "`n========== 检查完成 ==========" -ForegroundColor Magenta
    return $results
}

# ============================================================
# 导出函数 & 使用示例
# ============================================================
Export-ModuleMember -Function @(
    'Test-UnityMetaIntegrity',
    'Remove-UnityEmptyFolders',
    'Get-UnityAssetReport',
    'Invoke-UnityProjectHealthCheck'
)

<#
使用示例 (在脚本所在目录运行):

    # 引入脚本
    . .\Unity-AssetTools.ps1

    # 一键健康检查
    Invoke-UnityProjectHealthCheck -ProjectRoot "D:\ylenol\geme\I am hero_remake"

    # 仅检查 .meta 完整性
    Test-UnityMetaIntegrity -ProjectRoot "."

    # 仅清理空目录（预览模式）
    Remove-UnityEmptyFolders -ProjectRoot "." -WhatIf

    # 仅生成资产报告
    Get-UnityAssetReport -ProjectRoot "." -OutputCsv "my_report.csv"

    # 流水线组合使用
    $health = Invoke-UnityProjectHealthCheck -ProjectRoot "." -SkipEmptyDirCheck
    if ($health.MetaCheck.IsHealthy) {
        Write-Host "项目健康!" -ForegroundColor Green
    }
#>
