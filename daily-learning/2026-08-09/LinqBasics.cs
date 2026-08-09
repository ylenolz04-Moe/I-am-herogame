// ============================================================
// 每日脚本学习 Day 18 — 2026-08-09
// 主题: LINQ 入门 — 让集合操作像写SQL一样简单
// 适用: Unity 游戏开发 · 背包筛选 · 敌人查询 · 排行榜 · 数据汇总
// 难度: ★★☆☆☆ (进阶级 — 但用起来很容易!)
// 前置: Day16 Dictionary/foreach + Day17 Lambda表达式
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================
// 知识点 1: LINQ 是什么?
//
// LINQ = Language Integrated Query (语言集成查询)
//
// 大白话:
//   LINQ 是一套"对集合问问题"的方法库
//   只要 using System.Linq; 就能用!
//
//   之前写法 (foreach):
//     List<string> result = new List<string>();
//     foreach (var item in list) { if (条件) result.Add(item); }
//
//   LINQ 写法:
//     var result = list.Where(item => 条件).ToList();
//
//   类比:
//   foreach = 你挨个翻箱子找东西
//   LINQ    = 你对箱子说"把红色的都给我" — 一句话搞定!
//
// 核心思想: 声明式编程 — 你只要说"我要什么", 不用管"怎么找"
// ============================================================

// ============================================================
// 知识点 2: LINQ 两大写法
//
//   ① 方法语法 (Method Syntax) — 今天学的重点!
//     list.Where(x => x > 10).OrderBy(x => x).ToList();
//     → 链式调用, 从左往右读: 筛选→排序→转列表
//
//   ② 查询语法 (Query Syntax) — 像SQL, 了解即可
//     from x in list where x > 10 orderby x select x;
//     → C# 编译器会把它转成方法语法, 两者完全等价
//
//   我们只学方法语法! 因为:
//    - 更灵活 (不是所有操作查询语法都支持)
//    - 更直观 (从左到右, 数据流一目了然)
//    - Lambda 你已经会了! (Day 17 学过)
// ============================================================


// ============================================================
// Part 1: 英雄数据类 — 给 LINQ 准备"食材"
// ============================================================
[Serializable]
public class Hero
{
    public string Name;
    public int Level;
    public int HP;
    public int Attack;
    public HeroClass Class;
    public bool IsAlive;

    public Hero(string name, int level, int hp, int attack, HeroClass heroClass)
    {
        Name = name;
        Level = level;
        HP = hp;
        Attack = attack;
        Class = heroClass;
        IsAlive = hp > 0;
    }
}

public enum HeroClass
{
    Warrior,   // 战士
    Mage,      // 法师
    Archer,    // 弓箭手
    Priest,    // 牧师
}


// ============================================================
// Part 2: LINQ 实战演示
// ============================================================
public class LinqBasicsDemo : MonoBehaviour
{
    private List<Hero> _heroes;

    void Start()
    {
        Debug.Log("========== Day 18: LINQ 入门 — 集合操作一句话搞定 ==========\n");

        // ----- 准备测试数据: 8个英雄 -----
        _heroes = new List<Hero>
        {
            new Hero("亚瑟",   15, 1200, 180, HeroClass.Warrior),
            new Hero("梅林",   12,  600, 250, HeroClass.Mage),
            new Hero("罗宾",   10,  800, 150, HeroClass.Archer),
            new Hero("丽莎",   14,  700, 220, HeroClass.Priest),
            new Hero("亚历山大", 8, 1500, 200, HeroClass.Warrior),
            new Hero("吉安娜", 18,  500, 300, HeroClass.Mage),
            new Hero("莱戈拉斯", 9, 750, 160, HeroClass.Archer),
            new Hero("安度因",  6,  200,  50, HeroClass.Priest),
        };

        // ============================================================
        // Section 1: Where — 筛选 (最常用!)
        // ============================================================
        Demo_Where();

        // ============================================================
        // Section 2: Select — 转换/投影
        // ============================================================
        Demo_Select();

        // ============================================================
        // Section 3: First / FirstOrDefault — 获取第一个
        // ============================================================
        Demo_First();

        // ============================================================
        // Section 4: Any / All / Count — 判断 + 统计
        // ============================================================
        Demo_Judgement();

        // ============================================================
        // Section 5: OrderBy — 排序
        // ============================================================
        Demo_OrderBy();

        // ============================================================
        // Section 6: 链式调用 — 组合拳!
        // ============================================================
        Demo_Chaining();

        // ============================================================
        // Section 7: Skip / Take — 分页 (排行榜必备)
        // ============================================================
        Demo_SkipTake();
    }

    // ============================================================
    // Section 1: Where — 筛选 (等价于 foreach + if)
    //
    // 知识点 3: Where 方法
    //
    //   签名: IEnumerable<T> Where<T>(Func<T, bool> predicate)
    //
    //   拆解:
    //     IEnumerable<T>  → 返回一个"可遍历的结果集" (还没真正执行!)
    //     Func<T, bool>    → 接收 T 类型的元素, 返回 bool (条件是真是假)
    //     predicate        → 参数名, 意思是"判断条件"
    //
    //   简单说: Where(元素 => 条件表达式)
    //           返回所有"条件=true"的元素
    //
    //   类比:
    //     foreach (var h in heroes)
    //         if (h.Level >= 10)    ← 这就是 Where 里的条件!
    //             result.Add(h);
    //
    //   对比:
    //     旧写法: 4行代码 (foreach + if + Add + 结果列表)
    //     LINQ:   1行代码  (heroes.Where(h => h.Level >= 10))
    //
    // 知识点 4: 延迟执行 (Deferred Execution)
    //
    //   LINQ 查询不会立即执行! 只有在"真正需要结果"时才执行:
    //     var query = heroes.Where(h => h.Level > 10);  ← 还没执行!
    //     var result = query.ToList();                   ← 现在才执行!
    //
    //   好处: 可以在最后时刻才确定要什么, 中间不断拼接条件
    //   缺点: 如果多次遍历同一个 query, 每次都会重新执行 (用 ToList() 缓存)
    // ============================================================
    void Demo_Where()
    {
        Debug.Log("--- Section 1: Where — 筛选英雄 ---");

        // ----- 例1: 筛选等级 >= 10 的英雄 -----
        // 知识点 5: Lambda 表达式回忆 (Day 17 学过!)
        //
        //   h => h.Level >= 10
        //    ↑    ↑
        //    |    └─ 条件表达式 (返回 bool)
        //    └─ 参数: 集合里的每个元素
        //
        var highLevel = _heroes.Where(h => h.Level >= 10);
        PrintList("等级 >= 10 的英雄", highLevel);

        // ----- 例2: 筛选战士 (Warrior) -----
        var warriors = _heroes.Where(h => h.Class == HeroClass.Warrior);
        PrintList("战士职业", warriors);

        // ----- 例3: 多重条件 (用 &&) -----
        var eliteMages = _heroes.Where(h => h.Class == HeroClass.Mage && h.Level >= 12);
        PrintList("法师 + 等级>=12 (精英法师)", eliteMages);

        // ----- 例4: 筛选存活且攻击力 > 180 -----
        var powerHitters = _heroes.Where(h => h.IsAlive && h.Attack > 180);
        PrintList("存活 + 攻击>180", powerHitters);

        Debug.Log("");
    }

    // ============================================================
    // Section 2: Select — 转换 (把A变成B)
    //
    // 知识点 6: Select 方法
    //
    //   签名: IEnumerable<TResult> Select<T, TResult>(Func<T, TResult> selector)
    //
    //   简单说: Select(元素 => 你想要的东西)
    //           把集合里的每个元素"映射"成另一种东西
    //
    //   类比:
    //     你有一盒乐高小人, Select 就是"把所有小人的名字抄到一张纸上"
    //     → 原来是小人对象, 现在是一张名字列表
    //
    //   Where = 筛选 (保留元素, 只去掉不满足条件的)
    //   Select = 转换 (把元素变成另一种类型)
    //
    //   记忆口诀: Where过滤, Select变形
    // ============================================================
    void Demo_Select()
    {
        Debug.Log("--- Section 2: Select — 转换/投影 ---");

        // ----- 例1: 只要英雄名字 (Hero对象 → string) -----
        // 知识点 7: Select 把 Hero 变成 string
        var names = _heroes.Select(h => h.Name);
        Debug.Log("[Select·名字] " + string.Join(", ", names));

        // ----- 例2: 计算战斗力 (Hero对象 → int) -----
        // 战斗力公式: HP + Attack * 10
        var powerScores = _heroes.Select(h => h.HP + h.Attack * 10);
        Debug.Log("[Select·战力] " + string.Join(", ", powerScores));

        // ----- 例3: 生成描述文字 (Hero对象 → string) -----
        // 知识点 8: Select 里可以做字符串拼接!
        var descriptions = _heroes.Select(h => $"{h.Name}(Lv.{h.Level} {h.Class})");
        Debug.Log("[Select·描述] " + string.Join(" | ", descriptions));

        // ----- 例4: 匿名类型 — 只选几个字段 (了解即可) -----
        var summaries = _heroes.Select(h => new { h.Name, h.Level, IsStrong = h.Attack > 180 });
        foreach (var s in summaries)
            Debug.Log($"  [摘要] {s.Name} Lv{s.Level}, 强力? {s.IsStrong}");

        Debug.Log("");
    }

    // ============================================================
    // Section 3: First / FirstOrDefault — 拿第一个
    //
    // 知识点 9: First 三兄弟
    //
    //   First(条件)            → 找第一个满足条件的, 找不到就抛异常 💥
    //   FirstOrDefault(条件)   → 找第一个满足条件的, 找不到就返回默认值 ★推荐
    //   Last(条件)             → 找最后一个满足条件的
    //
    //   默认值是什么?
    //     class (引用类型) → null
    //     int/float (值类型) → 0
    //     bool              → false
    //
    //   铁律: 永远用 FirstOrDefault ! 除非你100%确定一定有结果
    // ============================================================
    void Demo_First()
    {
        Debug.Log("--- Section 3: First / FirstOrDefault — 获取第一个 ---");

        // ----- 例1: 找到第一个战士 -----
        Hero firstWarrior = _heroes.FirstOrDefault(h => h.Class == HeroClass.Warrior);
        if (firstWarrior != null)
            Debug.Log($"[First] 第一个战士: {firstWarrior.Name} Lv.{firstWarrior.Level}");

        // ----- 例2: 找不存在的 —— FirstOrDefault 返回 null, 不报错! -----
        Hero nobody = _heroes.FirstOrDefault(h => h.Level >= 100);
        Debug.Log($"[First] 等级>=100的英雄: {(nobody == null ? "不存在 (返回null, 安全!)" : nobody.Name)}");

        // ----- 例3: 如果用了 First (不带OrDefault), 找不到会怎样? -----
        // Hero crash = _heroes.First(h => h.Level >= 100);  ← InvalidOperationException! 💥
        // 所以永远用 FirstOrDefault !

        // ----- 例4: 找到攻击力最高的 (用 Max 不如用排序+First) -----
        Hero strongestAttack = _heroes.OrderByDescending(h => h.Attack).FirstOrDefault();
        Debug.Log($"[First+排序] 攻击最高: {strongestAttack.Name} ATK={strongestAttack.Attack}");

        Debug.Log("");
    }

    // ============================================================
    // Section 4: Any / All / Count — 判断 + 统计
    //
    // 知识点 10: 判断三件套
    //
    //   Any()      → 是否存在? (有一个满足就 true)
    //   All()      → 全部满足? (全部满足才 true)
    //   Count()    → 有几个满足条件的?
    //
    //   对比 foreach 写法:
    //     旧: bool found = false; foreach(...) { if(条件) { found = true; break; } }
    //     LINQ: bool found = list.Any(x => 条件);
    //
    //     旧: int count = 0; foreach(...) { if(条件) count++; }
    //     LINQ: int count = list.Count(x => 条件);
    // ============================================================
    void Demo_Judgement()
    {
        Debug.Log("--- Section 4: Any / All / Count — 判断统计 ---");

        // ----- 例1: Any — 是否存在 -----
        bool hasDeadHero = _heroes.Any(h => !h.IsAlive);
        Debug.Log($"[Any] 有没有死掉的英雄? {Answer(hasDeadHero)}");

        bool hasMage = _heroes.Any(h => h.Class == HeroClass.Mage);
        Debug.Log($"[Any] 有没有法师? {Answer(hasMage)}");

        // ----- 例2: All — 全部满足? -----
        bool allAlive = _heroes.All(h => h.IsAlive);
        Debug.Log($"[All] 所有英雄都活着? {Answer(allAlive)}");

        bool allHighLevel = _heroes.All(h => h.Level >= 10);
        Debug.Log($"[All] 所有英雄等级都 >= 10? {Answer(allHighLevel)}");

        // ----- 例3: Count — 计数 (比 foreach 累加省3行!) -----
        int warriorCount = _heroes.Count(h => h.Class == HeroClass.Warrior);
        Debug.Log($"[Count] 战士数量: {warriorCount}");

        int highPowerCount = _heroes.Count(h => h.Attack >= 200);
        Debug.Log($"[Count] 攻击 >= 200 的英雄: {highPowerCount}人");

        // ----- 例4: Sum / Average / Max / Min (数值专用) -----
        float avgLevel = _heroes.Average(h => h.Level);
        int totalHP = _heroes.Sum(h => h.HP);
        int maxAttack = _heroes.Max(h => h.Attack);
        int minLevel = _heroes.Min(h => h.Level);

        Debug.Log($"[统计] 平均等级: {avgLevel:F1}  |  总HP: {totalHP}  |  最高攻击: {maxAttack}  |  最低等级: {minLevel}");

        Debug.Log("");
    }

    // ============================================================
    // Section 5: OrderBy — 排序
    //
    // 知识点 11: 排序方法
    //
    //   OrderBy(字段)            → 从小到大 (升序)
    //   OrderByDescending(字段)  → 从大到小 (降序)
    //   ThenBy(字段)             → 第一排序相同时, 用第二排序 (升序)
    //   ThenByDescending(字段)   → 第一排序相同时, 用第二排序 (降序)
    //
    //   记忆: OrderBy = "按XX排序", ThenBy = "然后按YY排序"
    // ============================================================
    void Demo_OrderBy()
    {
        Debug.Log("--- Section 5: OrderBy — 排序 ---");

        // ----- 例1: 按等级升序 (从小到大) -----
        var byLevel = _heroes.OrderBy(h => h.Level);
        PrintList("按等级升序", byLevel);

        // ----- 例2: 按攻击降序 (从大到小) — 排行榜! -----
        var byAttackDesc = _heroes.OrderByDescending(h => h.Attack);
        PrintList("按攻击降序 (谁是输出担当?)", byAttackDesc);

        // ----- 例3: 先按职业排序, 同一职业内按等级降序 -----
        var byClassThenLevel = _heroes
            .OrderBy(h => h.Class)
            .ThenByDescending(h => h.Level);
        PrintList("先按职业 → 再按等级降序", byClassThenLevel);

        Debug.Log("");
    }

    // ============================================================
    // Section 6: 链式调用 — 把多个方法串起来!
    //
    // 知识点 12: LINQ 最强大之处 — 链式调用
    //
    //   就像水管工接水管: 一条接一条, 数据从左流到右
    //
    //   _heroes
    //     .Where(...)      ← 第1步: 筛选
    //     .OrderBy(...)    ← 第2步: 排序
    //     .Select(...)     ← 第3步: 转换
    //     .ToList()        ← 第4步: 执行 + 固化结果
    //
    //   每一行只做一件事, 读起来像自然语言!
    // ============================================================
    void Demo_Chaining()
    {
        Debug.Log("--- Section 6: 链式调用 — LINQ 组合拳! ---");

        // ----- 例1: 战士按等级排序, 只取名字 -----
        // 知识点 13: 读链式调用 — 从左往右, 一步一个操作
        //
        //   _heroes                          ← 从英雄列表开始
        //   .Where(h => h.Class == Warrior)  ← 只要战士
        //   .OrderByDescending(h => h.Level) ← 按等级从高到低排
        //   .Select(h => h.Name)             ← 只取名字
        //   .ToList()                        ← 变成 List<string>
        //
        var warriorNames = _heroes
            .Where(h => h.Class == HeroClass.Warrior)
            .OrderByDescending(h => h.Level)
            .Select(h => h.Name)
            .ToList();
        Debug.Log("[链式] 战士按等级排序: " + string.Join(", ", warriorNames));

        // ----- 例2: 攻击前3强, 生成排行榜文字 -----
        var top3 = _heroes
            .OrderByDescending(h => h.Attack)
            .Take(3)                                          // ← 只要前3个!
            .Select((h, index) => $"  #{index + 1} {h.Name} — ATK {h.Attack}"); // ← 带索引!

        Debug.Log("[链式] 🏆 攻击力排行榜 TOP 3:");
        foreach (var line in top3)
            Debug.Log(line);

        // ----- 例3: 活着的中高级英雄 (等级10-15), 按HP排序 -----
        var midLevel = _heroes
            .Where(h => h.IsAlive)
            .Where(h => h.Level >= 10 && h.Level <= 15)
            .OrderByDescending(h => h.HP)
            .Select(h => $"{h.Name} (HP:{h.HP})");
        Debug.Log("[链式] 中级存活英雄按HP排序: " + string.Join(" | ", midLevel));

        Debug.Log("");
    }

    // ============================================================
    // Section 7: Skip / Take — 分页 (排行榜/背包分页必备)
    //
    // 知识点 14: Skip 和 Take
    //
    //   Take(N)  → 取前 N 个
    //   Skip(N)  → 跳过前 N 个
    //
    //   配合使用 = 分页:
    //     page1 = list.Skip(0).Take(10);   ← 第1页: 跳过0个, 取10个
    //     page2 = list.Skip(10).Take(10);  ← 第2页: 跳过10个, 取10个
    //     page3 = list.Skip(20).Take(10);  ← 第3页: 跳过20个, 取10个
    //
    //   公式: list.Skip( (页码-1) * 每页数量 ).Take(每页数量)
    // ============================================================
    void Demo_SkipTake()
    {
        Debug.Log("--- Section 7: Skip / Take — 分页 ---");

        var sorted = _heroes.OrderByDescending(h => h.Attack).ToList();
        int pageSize = 3;

        for (int page = 1; page <= 3; page++)
        {
            var pageItems = sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select((h, i) => $"{h.Name}(ATK:{h.Attack})");

            Debug.Log($"[分页] 第{page}页: " + string.Join(" | ", pageItems));
        }

        Debug.Log("");
    }

    // ============================================================
    // 辅助打印方法
    // ============================================================
    void PrintList(string label, IEnumerable<Hero> heroes)
    {
        var list = heroes.ToList();
        if (list.Count == 0)
        {
            Debug.Log($"  [{label}] (无结果)");
            return;
        }
        var names = string.Join(", ", list.Select(h => $"{h.Name}(Lv.{h.Level} {h.Class})"));
        Debug.Log($"  [{label}] 共{list.Count}人: {names}");
    }

    string Answer(bool b) => b ? "是 ✓" : "否 ✗";
}


// ============================================================
// 📌 今日速查表 (Cheat Sheet)
//
//   需求                              LINQ代码
//   ───────────────────────────────────────────────────────────────
//   筛选                              list.Where(x => 条件)
//   转换/映射                         list.Select(x => 新值)
//   获取第一个(安全)                  list.FirstOrDefault(x => 条件)
//   获取最后一个                      list.LastOrDefault(x => 条件)
//   是否存在?                         list.Any(x => 条件)
//   全部满足?                         list.All(x => 条件)
//   计数                              list.Count(x => 条件)
//   求和                              list.Sum(x => 数值字段)
//   平均值                            list.Average(x => 数值字段)
//   最大值                            list.Max(x => 数值字段)
//   最小值                            list.Min(x => 数值字段)
//   升序排序                          list.OrderBy(x => 字段)
//   降序排序                          list.OrderByDescending(x => 字段)
//   二次排序                          .ThenBy(x => 字段)
//   取前N个                           list.Take(N)
//   跳过前N个                         list.Skip(N)
//   去重                              list.Distinct()
//   转成List                          .ToList()
//   转成数组                          .ToArray()
//   立即执行遍历                      .ForEach(x => 操作)
//
// ============================================================


// ============================================================
// 🤔 常见问题 FAQ
//
// Q1: LINQ 和 foreach 什么时候用哪个?
// A1: 简单筛选/转换 → LINQ (一行搞定)
//     复杂逻辑 (多层嵌套/需要修改元素) → foreach (更清晰)
//     判断存在/统计 → LINQ (Any/Count 比 foreach + bool 优雅得多)
//     没有绝对规则, 哪个读起来更清楚用哪个!
//
// Q2: Where 和 Select 的区别?
// A2: Where = 去掉不想要的 (集合元素数量可能减少)
//     Select = 变成另一种东西 (集合元素数量不变, 类型可能变)
//
//     通俗版:
//     Where = 筛子: 把不合格的过滤掉
//     Select = 变身: 把每个东西变成另一个样子
//
//     例:
//     heroes.Where(h => h.Level > 10)           — 只要等级>10的, 可能从8人变4人
//     heroes.Select(h => h.Name)                — 变成名字列表, 还是8个, 但类型从Hero→string
//     heroes.Where(...).Select(...)              — 先筛选再变形, 组合使用!
//
// Q3: 为什么有时需要 ToList(), 有时不需要?
// A3: LINQ 返回的是"查询"不是"结果" (延迟执行!)
//
//     var query = heroes.Where(h => h.Level > 10);  ← 只是一个查询计划
//     var result = heroes.Where(h => h.Level > 10).ToList();  ← 真正执行并保存结果
//
//     什么时候必须加 ToList():
//      - 需要多次遍历结果时 (不加每次都会重新执行查询!)
//      - 需要在后面修改原始集合时 (查询引用了原始数据)
//      - 需要传参给只接受 List<T> 的方法
//
//     什么时候可以不加:
//      - 只遍历一次 (foreach 直接用)
//      - 继续拼接其他 LINQ 方法 (链式调用)
//
//     新手建议: 不确定就加 .ToList(), 不会错!
//
// Q4: FirstOrDefault 返回 null, 我该怎么处理?
// A4: 永远检查 null!
//
//     var hero = list.FirstOrDefault(h => h.Level > 100);
//     if (hero != null) { 使用 hero }     ← 安全!
//     // 或者用 C# 的 null 检查更简洁:
//     var name = list.FirstOrDefault(h => h.Level > 100)?.Name ?? "无";
//
// Q5: LINQ 跑得慢吗?
// A5: 对小于1000个元素的集合, 性能差异可以忽略不计。
//     Unity 里大部分场景 (几十个敌人、几百个道具) LINQ 完全够用。
//     如果真的有上万元素需要每帧更新, 再用 foreach 优化。
//     开发效率 > 微小的性能差异! 先把功能做对, 再考虑优化。
//
// Q6: Day 17 学的 Lambda 和今天的 LINQ 什么关系?
// A6: Lambda 是 LINQ 的"灵魂"!
//
//     LINQ 的每一个方法几乎都接受一个 Lambda 作为参数:
//     .Where( h => h.Level > 10 )
//            ↑________________↑
//               这就是 Lambda! (Day 17 学的 Func<T, bool>)
//
//     .Select( h => h.Name )
//             ↑___________↑
//              这也是 Lambda! (Func<T, TResult>)
//
//     没有 Lambda, LINQ 就得写又臭又长的匿名方法:
//     .Where(delegate(Hero h) { return h.Level > 10; })  ← 不想这样对吧?
//     .Where(h => h.Level > 10)                           ← 简洁!
//
//     所以 Day 17 (Lambda) + Day 18 (LINQ) = 绝配! 🎯
// ============================================================
