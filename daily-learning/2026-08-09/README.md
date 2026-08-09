# Day 18: LINQ 入门 — 让集合操作像写SQL一样简单

📅 **日期**: 2026-08-09  
🎯 **难度**: ★★☆☆☆ (进阶级 — 但用起来很容易!)  
📦 **技术栈**: C# · LINQ · Lambda · Unity  
🔗 **前置知识**: Day 16 (Dictionary + foreach) · Day 17 (Lambda 表达式)

---

## 🎯 今日目标

学会用 LINQ 替代繁琐的 foreach 循环，用一行代码完成筛选、转换、排序、统计。

## 📖 核心知识点

| # | 知识点 | 说明 |
|---|--------|------|
| 1 | **Where** | 筛选 — `list.Where(x => 条件)` |
| 2 | **Select** | 转换 — `list.Select(x => 新值)` |
| 3 | **FirstOrDefault** | 安全获取第一个 |
| 4 | **Any / All / Count** | 判断是否存在 / 全部满足 / 计数 |
| 5 | **Sum / Average / Max / Min** | 数值统计 |
| 6 | **OrderBy / ThenBy** | 排序 (升序/降序/多级排序) |
| 7 | **Skip / Take** | 分页 (排行榜必备!) |
| 8 | **链式调用** | 把多个 LINQ 方法串成"流水线" |
| 9 | **延迟执行** | LINQ 的查询在 ToList() 时才真正执行 |

## 🧪 演示内容

- 英雄列表的筛选 (Where: 等级/职业/战斗力)
- 数据投影转换 (Select: Hero对象 → 名字/战斗力/描述)
- 安全查找 (FirstOrDefault vs First)
- 集合判断与统计 (Any/All/Count/Sum/Average/Max/Min)
- 排行榜排序 (OrderByDescending + ThenBy)
- 链式调用组合拳 (Where + OrderBy + Select 一条龙)
- 分页系统 (Skip + Take)

## 💡 核心对比

```csharp
// ❌ 旧写法 (foreach + if + Add) — 5行
List<Hero> result = new List<Hero>();
foreach (var h in heroes)
{
    if (h.Level >= 10 && h.Class == HeroClass.Warrior)
        result.Add(h);
}

// ✅ LINQ 写法 — 1行
var result = heroes.Where(h => h.Level >= 10 && h.Class == HeroClass.Warrior).ToList();
```

## 📁 文件

- [LinqBasics.cs](./LinqBasics.cs) — 完整演示脚本 (可挂载到 Unity GameObject 运行)
