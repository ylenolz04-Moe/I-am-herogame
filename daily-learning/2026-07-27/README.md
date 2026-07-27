# Day 16 — Dictionary 字典 + foreach 循环入门

> **日期:** 2026-07-27  
> **难度:** ★☆☆☆☆ (入门级)  
> **技术栈:** C# · Unity · Dictionary · foreach · KeyValuePair · 背包系统

---

## 🎯 今日主题

今天学两个写游戏脚本最常用的 C# 基础知识：

### 📖 Dictionary<TKey, TValue> — 万能字典

**字典 = 一本可以通过"名字"快速找到"内容"的书。** 比如"药水" → 5个，"金币" → 100个。

和数组/List 的最大区别：数组用数字索引（`[0]` `[1]`），字典用任意类型的键（`["potion"]` `["gold"]`）。读起来像自然语言，代码一下子就清晰了。

### 🔄 foreach — 遍历集合的最简单方式

**foreach = "这个集合里的每个东西，挨个拿给我看看"** — 不用管索引、不用管长度、不会越界。字典、数组、List … 所有集合都能用。

---

## 📖 核心知识点

| # | 知识点 | 说明 |
|---|--------|------|
| 1 | Dictionary 是什么 | 键值对（Key-Value Pair）— 通过 Key 快速找到 Value |
| 2 | 声明和初始化 | `new Dictionary<string, int>()` + 集合初始化器 |
| 3 | 添加/访问/修改 | `Add()`、`[key]` 索引器、`ContainsKey()` |
| 4 | foreach 语法 | `foreach (var item in 集合)` — 自动遍历每个元素 |
| 5 | KeyValuePair 是什么 | 字典里每个元素 = 一个 KeyValuePair<TKey, TValue> |
| 6 | foreach + 字典 | `.Key` 拿名字、`.Value` 拿数据 |
| 7 | 金币和价格判断 | `if (gold >= price)` — 买得起/买不起 |
| 8 | 遍历所有物品 | 商店物品一个个检查 → 告诉玩家能不能买 |
| 9 | 代码类型 | 都是数据逻辑，不依赖 Unity → 比 MonoBehaviour 更简单 |

---

## 🔗 与其他学习日的关系

- **Day 15 (struct 入门):** Dictionary 的 Value 可以用 struct — 比如物品数据用 struct 更省内存
- **Day 2 (对象池):** 对象池内部用 Dictionary 管理不同类型的池
- **Day 5 (策略模式):** 用 Dictionary 存储策略对象，Key=武器名，Value=攻击策略
- **Day 14 (观察者模式):** EventBus 内部用 Dictionary 存储事件 → 监听者列表

---

## 💡 一句话总结

> **Dictionary：** 给每个数据起个名字，用名字找数据比用数字快得多也清晰得多。  
> **foreach：** "每个都看一遍"这件事，你只管说"做什么"，不管"怎么遍历"。

---

## 🧪 运行方式

将 `InventorySystem.cs` 放入 Unity 项目的任意 `Scripts` 文件夹中，在场景中创建空 GameObject 并挂载 `InventoryDemo` 组件，运行后即可在 Console 中看到输出：
- Dictionary 基础操作演示（创建、添加、查找、修改）
- 背包 + 商店系统 → foreach 遍历判断买得起/买不起
- 进阶用法：用 LINQ 筛选买得起的物品

---

## 🤔 延伸思考

1. **Dictionary 如果 Key 不存在就直接用 `[key]` 访问会发生什么？** → 会报 `KeyNotFoundException`！所以访问前先用 `ContainsKey()` 检查。
2. **Dictionry 的 Key 能自定义类型吗？** → 可以！比如用 `enum` 做 Key 更安全：`Dictionary<ItemType, int>`
3. **foreach 里面能不能修改集合？** → 不能！遍历中增删元素会报 `InvalidOperationException`。可以先用 `ToList()` 复制一份再删。
4. **什么时候用 Dictionary，什么时候用 List？** → 需要通过"名字/ID"快速查找 → Dictionary；只需要全部遍历一遍 → List/数组就够了。

---

## 🏷️ 标签

`C#基础` `Dictionary` `foreach` `键值对` `KeyValuePair` `背包系统` `商店系统` `Unity` `入门`
