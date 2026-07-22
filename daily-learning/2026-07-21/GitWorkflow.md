# Day 14 补充 — Git 日常开发工作流

> **日期:** 2026-07-21  
> **难度:** ★☆☆☆☆  
> **技术栈:** Git · GitHub · 版本控制

---

## 🎯 今日踩坑实录

今天写了 Day 14 的观察者模式代码，提交了也 merge 了，但 GitHub contribution 没绿。排查后发现：**contribution 图有延迟，最长等 24 小时**。

借此机会，梳理 Git 日常最核心的工作流。

---

## 📖 核心流程 (每天都会用到)

### 1. 写代码前 — 切分支

```bash
# 永远不要在 main 上直接写代码
git checkout -b DailyLearning    # 创建并切换到新分支
```

> **为什么？** main 是"干净可发布"的。你在这改代码，别人也在这改，100% 冲突。新功能 = 新分支。

### 2. 写完了 — 提交

```bash
git status                        # 先看改了啥
git add daily-learning/2026-07-21/    # 添加要提交的文件
git commit -m "Day 14: 观察者模式"   # 本地存档
```

> ⚠️ **commit ≠ 上传**。commit 只是存到本地 `.git` 里，GitHub 上看不到。

### 3. 上传 — 推送

```bash
git push origin DailyLearning    # 推送到 GitHub
```

> 现在 GitHub 上你的分支里能看到文件了，但 **main 分支还没有**。

### 4. 合入主线 — Pull Request

```bash
# 在 GitHub 网页上操作:
# Pull Requests → New Pull Request → DailyLearning → main → Create PR → Merge
```

> 或者用命令行直接 merge：
> ```bash
> git checkout main
> git merge DailyLearning
> git push origin main
> ```

---

## 🔧 常见踩坑速查

| 现象 | 原因 | 解决 |
|------|------|------|
| `git push` 报 rejected | 远程比你新 | `git pull` 先拉再推 |
| 文件在 GitHub 看不到 | 只 commit 了没 push | `git push` |
| main 上看不到 | 没 merge 到 main | 走 PR 流程或直接 merge |
| contribution 没绿 | 图刷新延迟 | 等 24 小时 |
| contribution 一直不绿 | 邮箱未验证 | GitHub Settings → Emails |
| `git status` 一堆红 | 忘切分支就在 main 改了 | `git stash` → 切分支 → `git stash pop` |

---

## 💡 一句话总结

> commit = 本地存档 → push = 上传云端 → merge = 合入主线。三步缺一不可。

---

## 🔗 进阶扩展 (可选)

- `git stash` — 临时保存改动，切分支后恢复
- `git rebase` — 整理 commit 历史（让 log 更干净）
- `git log --oneline --graph` — 可视化分支图
- `git reflog` — 后悔药，找回"丢失"的 commit
- `.gitignore` — 配置哪些文件不提交（Unity: `Library/`, `Temp/`, `Obj/`）
