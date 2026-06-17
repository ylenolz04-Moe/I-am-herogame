# 🦸 I Am Hero (Remake)

> 一个使用 Unity 开发的像素风格 2D 横版平台跳跃游戏。

---

## 🎮 游戏简介

**I Am Hero** 是一款经典的 2D 平台跳跃游戏。玩家将操控一位像素英雄，穿越充满陷阱和障碍的关卡，收集樱桃🍒，最终抵达终点旗帜🚩。

本项目是 "I am hero" 的**重制版（Remake）**，基于 Unity 2022.3.54f1 引擎，使用 **Pixel Adventure 1** 美术资源包，致力于打造流畅的平台跳跃体验。

---

## ✨ 核心特性

| 特性 | 描述 |
|------|------|
| 🏃 **角色移动** | 左右移动、二段跳，手感流畅 |
| 🍒 **收集系统** | 收集关卡中的樱桃，实时显示得分 |
| ⚙️ **机关陷阱** | 旋转锯齿、巡逻电锯、尖刺等多种陷阱 |
| 🏗️ **移动平台** | 可搭载的粘性平台，增加关卡变化 |
| 💀 **死亡机制** | 触碰陷阱即死，自动重载关卡 |
| 🏁 **关卡通关** | 抵达终点旗帜即可通关，进入下一关 |
| 🎬 **动画系统** | 完整的角色动画状态机（待机、奔跑、跳跃、坠落、二段跳、死亡） |
| 🔊 **音效音乐** | 跳跃、收集、死亡音效，以及背景音乐 |
| 📷 **摄像机跟随** | 摄像机实时跟随玩家位置 |

---

## 🗺️ 关卡流程

```
开始菜单 (StartMenu)  →  关卡 1 (Level1)  →  关卡 2 (Level2)  →  结束画面 (End)
```

---

## 🎛️ 操作方式

| 按键 | 操作 |
|------|------|
| `A` / `D` 或 `←` / `→` | 左右移动 |
| `空格键 (Space)` | 跳跃（支持二段跳） |

---

## 🛠️ 技术架构

### 引擎与工具
- **Unity 2022.3.54f1** — 游戏引擎
- **C#** — 脚本语言
- **Pixel Adventure 1** — 像素美术资源
- **Casual Game Sounds U6** — 音效资源

### 核心脚本

| 脚本 | 功能 |
|------|------|
| `PlayerMoment.cs` | 玩家移动、跳跃、地面检测、动画状态切换 |
| `PlayerLife.cs` | 玩家死亡判定、死亡动画、关卡重载 |
| `item_collector.cs` | 樱桃收集与得分更新 |
| `Allcontrol.cs` | 游戏管理器单例，跨场景得分持久化 |
| `Camera.cs` | 摄像机硬跟随 |
| `Finish.cs` | 关卡终点触发器 |
| `WaypointFollwer.cs` | 巡逻路线移动（锯齿等） |
| `Rotate.cs` | 持续自转（旋转锯齿） |
| `StickyPlatform.cs` | 粘性移动平台 |
| `StartMenu.cs` | 主菜单控制 |
| `EndMenu.cs` | 结束画面控制 |

### 标签 & 图层
- **标签:** `Cherry`（樱桃）、`Trap`（陷阱）
- **图层:** `Ground`（地面，Layer 6）

---

## 🚀 快速开始

### 环境要求
- Unity Hub 已安装 Unity 2022.3.54f1
- Git LFS（如有大型资源文件）

### 克隆项目

```bash
git clone https://github.com/ylenolz04-Moe/I-am-herogame.git
```

### 打开项目
1. 启动 Unity Hub
2. 点击「打开」→ 选择克隆的项目文件夹
3. 等待 Unity 导入资源包
4. 双击 `Assets/Scenes/StartMenu.unity` 即可运行

---

## 📝 开发计划

- [ ] 将 `PlayerMoment.cs` 重构为 Tarodev 风格的现代化控制器（已添加参考代码与重构指南）
- [ ] 添加土狼时间（Coyote Time）与跳跃缓冲（Jump Buffer）
- [ ] 实现可变跳跃高度
- [ ] 添加敌人 AI
- [ ] 添加生命值系统
- [ ] 添加检查点/存档功能
- [ ] 添加趴墙跳（Wall Jump）
- [ ] 添加更多关卡
- [ ] 摄像机平滑跟随

---

## 📂 项目结构

```
I am hero_remake/
├── Assets/
│   ├── Animation/         # 动画控制器与动画片段
│   ├── Audios/            # 音频资源（音效与音乐）
│   ├── Fonts/             # 字体（PressStart2P 像素字体）
│   ├── profab/            # 预制体（樱桃、锯齿、尖刺等）
│   ├── Scenes/            # 场景文件
│   ├── Scripts/           # C# 脚本
│   ├── Sprites/           # 精灵图资源
│   ├── Tarodev 2D Controller/  # Tarodev 控制器参考代码与重构指南
│   └── Tile/              # 瓦片地图数据
├── ProjectSettings/       # Unity 项目设置
└── README.md
```

---

## 🙏 致谢

- 美术资源：[Pixel Adventure 1 (Unity Asset Store)](https://assetstore.unity.com/packages/2d/characters/pixel-adventure-1-155360)
- 音效资源：Casual Game Sounds U6
- 参考架构：Tarodev 2D Controller

---

## 📄 许可证

本项目仅用于学习与个人开发用途。美术与音频资源的版权归原作者所有。

---

*Made with ❤️ by [ylenolz04-Moe](https://github.com/ylenolz04-Moe)*
