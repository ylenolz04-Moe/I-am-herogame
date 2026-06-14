# RefactoringGuide — 玩家控制器重构参考指南

## 这是什么？

本文件夹包含一套**参考代码**，展示如何将原始 `PlayerMoment.cs` 项目逐步重构为架构更清晰、手感更现代的平台游戏控制器。

> ⚠️ **重要**: 这些文件是**教学参考**，不是可直接运行的代码。它们展示了重构的"目标形态"，不建议一次性全部替换。

## 文件清单与用途

| 文件 | 类型 | 说明 |
|------|------|------|
| `QuickMigrationGuide.cs` | 📖 速查手册 | 10 个独立「替换配方」，按难度从低到高排列，每个配方可单独应用。**建议从这里开始** |
| `HeroScriptableStats.cs` | 📦 数据容器 | ScriptableObject 参数管理方案 — 将所有调参集中到一个 `.asset` 文件，策划可直接在 Inspector 中调参 |
| `HeroPlayerController.cs` | 🎮 控制器核心 | 融合 Tarodev 架构 + 原项目二段跳等功能的完整控制器。展示 Update/FixedUpdate 分离、加速度系统、土狼时间、跳跃缓冲、可变跳高等 |
| `HeroPlayerAnimator.cs` | 🎬 动画模块 | 事件驱动的独立动画控制器 — 通过订阅 `Jumped`/`GroundedChanged`/`Died` 事件来响应状态变化 |
| `HeroPlayerLife.cs` | ❤️ 生命管理 | 只负责「检测伤害来源」，死亡逻辑通过 `Controller.Die()` 委托给控制器处理 |
| `HeroCameraFollow.cs` | 📷 相机跟随 | 平滑阻尼跟随 + LookAhead 预判 + 边界限制 — 改进自原 `Camera.cs` |
| `HeroGameManager.cs` | 🏗️ 游戏管理 | MonoBehaviour 化、属性封装 + 事件通知的单例 GameManager — 改进自原 `Allcontrol.cs` |

## 文件间关系

```
HeroScriptableStats (数据)
       │
       ▼
HeroPlayerController (核心) ──事件──▶ HeroPlayerAnimator (视觉)
       │                                   
       ├──Died事件──▶ HeroPlayerLife (处理后续)     
       │                      
       └──Died事件──▶ HeroGameManager (重置分数)
       
HeroCameraFollow ──引用──▶ Transform (或 IHeroController 接口)
```

## 迁移方案

### 渐进式迁移（推荐）

按 `QuickMigrationGuide.cs` 中的优先级分 3 周推进：

| 阶段 | 重点 | 涉及的配方 |
|------|------|------------|
| **第 1 周** — 手感 | 加速/减速、跳跃缓冲、土狼时间 | 配方 3, 4, 5 |
| **第 2 周** — 质量 | Animator 哈希缓存、FixedUpdate 分离、可变跳跃高度 | 配方 1, 2, 6 |
| **第 3 周** — 架构 | 事件解耦动画、ScriptableObject 参数、接口解耦 | 配方 7, 8, 9 |

### 完全替换

如果你希望直接从原 `PlayerMoment` 切换到新架构：

1. 创建 `HeroScriptableStats.asset`（右键 → Create → Hero → Hero Stats），填入原有参数值
2. 将 `HeroPlayerController` 挂到 Player GameObject 上，替代 `PlayerMoment`
3. 将 `HeroPlayerAnimator` 挂到 Player 上，替代原有动画逻辑
4. 在场景中创建 GameManager GameObject，挂上 `HeroGameManager`
5. 将 Camera 脚本替换为 `HeroCameraFollow`

## 关键概念速查

| 概念 | 一句话解释 | 相关配方 |
|------|-----------|----------|
| **土狼时间 (Coyote Time)** | 离开平台后 0.15 秒内仍可跳跃 | 配方 5 |
| **跳跃缓冲 (Jump Buffer)** | 落地前提前按跳跃会被记住，着地后自动执行 | 配方 4 |
| **可变跳跃高度** | 轻按=小跳，长按=大跳 | 配方 6 |
| **依赖倒置** | 依赖接口而非具体类，方便替换和测试 | 配方 9 |
| **事件驱动** | 用 C# event 解耦模块，替代直接调用 | 配方 7 |

## 核心原则

> **每次只改一个配方，改完立刻测试 — 不要一口气全改。**
