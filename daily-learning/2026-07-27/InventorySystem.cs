// ============================================================
// 每日脚本学习 Day 16 — 2026-07-27
// 主题: Dictionary 字典 + foreach 循环入门 — 背包系统实战
// 适用: Unity 游戏开发 · 道具管理 · 商店系统 · 数据查询
// 难度: ★☆☆☆☆ (入门级)
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================
// 知识点 1: Dictionary<TKey, TValue> — 万能的键值对字典
//
// 类比: 现实中的字典
//   查"苹果"这个词 → 翻到对应页 → 看到解释
//   ["apple"] → "一种水果，有红的也有绿的"
//
// 代码里的字典:
//   ["potion"] → 5      (药水有5个)
//   ["gold"]   → 100    (金币有100个)
//
// 和数组的区别:
//   数组:  inventory[0] = 5  ← "第0个是5", 要记住0是药水
//   字典:  inventory["potion"] = 5  ← "药水有5个", 一目了然!
// ============================================================

public class InventoryDemo : MonoBehaviour
{
    void Start()
    {
        Debug.Log("========== Day 16: Dictionary + foreach 入门 ==========\n");

        // ============================================================
        // Part 1: Dictionary 基础操作 — 创建、添加、查找、修改
        // ============================================================
        Demo_DictionaryBasics();

        // ============================================================
        // Part 2: 背包 + 商店系统 — foreach 遍历判断
        // ============================================================
        Demo_ShopSystem();

        // ============================================================
        // Part 3: 进阶 — 用 foreach 筛选 + 汇总
        // ============================================================
        Demo_AdvancedFilter();
    }

    // ============================================================
    // Part 1: Dictionary 四种基本操作
    // 知识点 2: 声明、添加、访问、修改 — 字典的四项基本功
    // ============================================================
    void Demo_DictionaryBasics()
    {
        Debug.Log("--- Part 1: Dictionary 基础操作 ---");

        // ----- 知识点 2.1: 声明 + 初始化 -----
        // 用 { } 集合初始化器, 一行创建并填好数据
        // 格式: { {"Key名字", Value值}, {"Key名字", Value值}, ... }
        Dictionary<string, int> ItemInventory = new Dictionary<string, int>()
        {
            {"potion", 5},    // Key="potion",  Value=5
            {"Antidote", 3},  // Key="Antidote", Value=3
            {"Elixir", 2},    // Key="Elixir", Value=2
        };
        Debug.Log("[初始化] 背包创建完成, 共 " + ItemInventory.Count + " 种物品");

        // ----- 知识点 2.2: 用 [key] 访问 -----
        // 像数组一样用中括号, 但里面是 Key 名字而不是数字
        int potionCount = ItemInventory["potion"];
        Debug.Log("[访问] potion 有 " + potionCount + " 个");

        // ----- 知识点 2.3: 安全访问 — ContainsKey -----
        // 直接用 [key] 如果 Key 不存在会报错!
        // 先用 ContainsKey 检查是最佳实践
        if (ItemInventory.ContainsKey("Elixir"))
            Debug.Log("[安全检查] Elixir 存在, 数量: " + ItemInventory["Elixir"]);
        else
            Debug.Log("[安全检查] Elixir 不存在");

        // ----- 知识点 2.4: 添加新物品 -----
        // Add() 添加新键值对, 但如果 Key 已经存在也会报错
        if (!ItemInventory.ContainsKey("Sword"))
        {
            ItemInventory.Add("Sword", 1);
            Debug.Log("[添加] 获得新武器 Sword x1");
        }

        // ----- 知识点 2.5: 修改已有物品 -----
        // [key] = 新值 — 直接覆盖
        ItemInventory["potion"] = ItemInventory["potion"] + 3;  // 捡到3瓶
        Debug.Log("[修改] potion 现在有 " + ItemInventory["potion"] + " 个 (捡了3瓶)");
    }

    // ============================================================
    // Part 2: 背包 + 商店 — 这才是今天的主角!
    //
    // 知识点 3: foreach 遍历字典
    //
    // foreach 语法:
    //   foreach (var 变量名 in 字典变量)
    //   {
    //       变量名.Key   → 当前这轮的 Key (名字)
    //       变量名.Value → 当前这轮的 Value (数值)
    //   }
    //
    // 为什么能直接写 foreach(字典)?
    //   因为 Dictionary 实现了 IEnumerable 接口
    //   遍历时每个元素是 KeyValuePair<TKey, TValue>
    //   KeyValuePair = 一个装着 Key 和 Value 的小盒子
    //
    // 知识点 4: KeyValuePair — 字典里每个"条目"的类型
    //
    //   字典里存的每一条东西, 不是一个数字也不是一个字符串
    //   而是一个"键值对" — KeyValuePair
    //
    //   KeyValuePair<string, int>  pair
    //    ├── pair.Key   ← string 类型, 存名字
    //    └── pair.Value ← int 类型, 存数量/价格
    //
    //   就像快递包裹:
    //    ├── 贴纸上的"收件人名字" (Key)
    //    └── 盒子里的"实际货物"   (Value)
    // ============================================================
    void Demo_ShopSystem()
    {
        Debug.Log("\n--- Part 2: 商店系统 — foreach 判断买不买得起 ---");

        // ----- 角色数据 -----
        // 知识点 5: 普通的 int 变量 — 存金币
        int gold = 100;  // ★ 这就是用户问的 "角色有多少金币"

        // ----- 商店: 物品名 → 价格 -----
        // 知识点 6: Dictionary<string, int> — 字典存价格
        Dictionary<string, int> ItemPrices = new Dictionary<string, int>()
        {
            {"potion",  20},  // 药水 20金
            {"Antidote", 50},  // 解毒剂 50金
            {"Elixir",   80},  // 圣灵药 80金
            {"Sword",   150},  // 剑 150金 (买不起!)
        };

        Debug.Log("💰 你的金币: " + gold + " G\n");

        // ----- 核心: foreach 遍历商店, 判断买不买得起 -----
        // 知识点 7: foreach (var item in ItemPrices)
        //
        // 这是今天最重要的部分!
        //
        // 第1轮循环: item.Key="potion",   item.Value=20
        //   → gold(100) >= 20 ?  ✓ 买得起!
        //
        // 第2轮循环: item.Key="Antidote",  item.Value=50
        //   → gold(100) >= 50 ?  ✓ 买得起!
        //
        // 第3轮循环: item.Key="Elixir",    item.Value=80
        //   → gold(100) >= 80 ?  ✓ 买得起!
        //
        // 第4轮循环: item.Key="Sword",     item.Value=150
        //   → gold(100) >= 150? ✗ 买不起! (差50金)
        //
        foreach (var item in ItemPrices)      // ← item 是 KeyValuePair<string, int>
        {
            // 知识点 8: 拆解 KeyValuePair
            string itemName = item.Key;       // 道具名字 — item.Key 返回 string
            int price = item.Value;           // 道具价格 — item.Value 返回 int

            // 知识点 9: 判断买得起/买不起
            if (gold >= price)
            {
                Debug.Log("✓ 买得起 [" + itemName + "] — 价格 " + price + "金, 剩余 " + (gold - price) + "金");
            }
            else
            {
                Debug.Log("✗ 买不起 [" + itemName + "] — 价格 " + price + "金, 还差 " + (price - gold) + "金");
            }
        }
    }

    // ============================================================
    // Part 3: 进阶 — 用 foreach 做更复杂的事情
    //
    // 知识点 10: foreach 不只是"看一遍", 还能:
    //   - 筛选: 把符合条件的挑出来
    //   - 汇总: 计算总价 / 平均值
    //   - 查找: 找到最贵的 / 最便宜的
    // ============================================================
    void Demo_AdvancedFilter()
    {
        Debug.Log("\n--- Part 3: 进阶用法 — 筛选 + 汇总 ---");

        int gold = 100;

        Dictionary<string, int> ItemPrices = new Dictionary<string, int>()
        {
            {"potion",  20},
            {"Antidote", 50},
            {"Elixir",   80},
            {"Sword",   150},
            {"Shield",  120},
            {"Bomb",    25},
        };

        // ----- 知识点 10.1: 筛选 — 把买得起的收集起来 -----
        // 先建一个空 List, foreach 里符合条件的就加进去
        List<string> affordable = new List<string>();
        foreach (var item in ItemPrices)
        {
            if (gold >= item.Value)           // 价格 <= 金币
                affordable.Add(item.Key);      // 加入"买得起列表"
        }
        Debug.Log("[筛选] 买得起的物品: " + string.Join(", ", affordable));

        // ----- 知识点 10.2: 汇总 — 计算所有物品总价 -----
        int totalPrice = 0;
        foreach (var item in ItemPrices)
        {
            totalPrice += item.Value;  // 累加每个物品的价格
        }
        Debug.Log("[汇总] 商店全部物品总价: " + totalPrice + "金 (每样买一个的话)");

        // ----- 知识点 10.3: 查找 — 找到最贵的和最便宜的 -----
        string cheapestName = "";
        int cheapestPrice = int.MaxValue;  // 从最大值开始, 找到的任何都更小
        string mostExpensiveName = "";
        int mostExpensivePrice = 0;

        foreach (var item in ItemPrices)
        {
            if (item.Value < cheapestPrice)
            {
                cheapestPrice = item.Value;
                cheapestName = item.Key;
            }
            if (item.Value > mostExpensivePrice)
            {
                mostExpensivePrice = item.Value;
                mostExpensiveName = item.Key;
            }
        }
        Debug.Log("[最便宜] " + cheapestName + " — " + cheapestPrice + "金");
        Debug.Log("[最贵] " + mostExpensiveName + " — " + mostExpensivePrice + "金");

        // ----- 知识点 10.4: 用 LINQ 一句话搞定 (扩展, 了解一下) -----
        // LINQ 让 foreach 写起来更短 (底层还是 foreach)
        var affordableItems = ItemPrices.Where(item => gold >= item.Value)
                                        .Select(item => item.Key);
        Debug.Log("[LINQ版] 买得起的物品: " + string.Join(", ", affordableItems));
    }
}

// ============================================================
// 📌 今日速查表 (Cheat Sheet)
//
//   需求                     代码
//   ─────────────────────────────────────────────────────
//   声明字典                  Dictionary<string, int> d = new() { {"a",1} };
//   添加                      d.Add("key", value);
//   安全访问                  if (d.ContainsKey("k")) { var v = d["k"]; }
//   修改                      d["k"] = newValue;
//   遍历                      foreach (var item in d) { item.Key / item.Value }
//   判断买得起                if (gold >= price) { 买! }
//   筛选买得起的              foreach + if + List.Add()
//   找最便宜/最贵            foreach + if 比较
//
// ============================================================

// ============================================================
// 🤔 常见问题 FAQ
//
// Q1: foreach 里的 item 是什么?
// A1: 你在 foreach 括号里当场声明的变量, 每次循环自动赋值为下一个元素。
//     不需要提前声明! foreach 帮你全包了。
//
// Q2: item.Key 和 item.Value 是固定的吗?
// A2: .Key 和 .Value 是 KeyValuePair 类型的属性, 名字不能改。
//     但 foreach 的变量名 (item) 你可以随便取:
//     foreach (var kvp in dict)  ← kvp.Key, kvp.Value 一样能用
//     foreach (var pair in dict) ← pair.Key, pair.Value
//
// Q3: 能不能在 foreach 里修改字典?
// A3: 不能添加或删除元素 (会报 InvalidOperationException)
//     但可以修改已有条目的 Value: dict["key"] = 新值 (这个没问题)
//
// Q4: 怎么遍历 Dictionary 的 Key 或 Value 集合?
// A4: foreach (var key in dict.Keys)    ← 只看 Key
//     foreach (var val in dict.Values)  ← 只看 Value
//     foreach (var item in dict)        ← 两个一起看
//
// Q5: 字典和 List 选哪个?
// A5: 需要用名字/ID快速查找 → Dictionary (O(1) 查找)
//     只需要全部遍历一遍     → List 就行
//     不确定                  → List 先写, 后面需要查找再换 Dictionary
// ============================================================
