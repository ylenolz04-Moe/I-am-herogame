// ============================================================
// 每日脚本学习 Day 15 — 2026-07-24
// 主题: C# 结构体入门 — struct 与 class 的区别
// 适用: Unity 游戏开发 · 性能优化 · 数据容器 · Vector3/Color
// 难度: ★★☆☆☆ (入门级)
// ============================================================

using System;
using UnityEngine;

// ============================================================
// 知识点 1: 什么是结构体 (struct)?
//
// struct = 一种"值类型"的数据容器, 和 class 很像但行为不同
//
// 一句话区别:
//   struct 是"值类型" — 赋值时复制一份新的 (像复印文件)
//   class  是"引用类型" — 赋值时共享同一份 (像共享链接)
//
// 现实类比:
//   📄 struct = 复印文件 → 你在复印件上改, 原件不变
//   🔗 class   = 共享链接 → 你点链接改了内容, 所有人都看到变化
//
// Unity 中你已经用过很多 struct 了!
//   Vector3, Vector2, Color, Quaternion, Rect, RaycastHit...
//   这些都是 struct!
// ============================================================

// ============================================================
// Part 1: struct vs class — 核心区别演示
// 知识点 2: 值类型 vs 引用类型 — 这是最重要的区别!
// ============================================================

/// <summary>
/// 一个简单的 class (引用类型)
/// </summary>
public class PlayerClass
{
    public string Name;
    public int HP;

    public PlayerClass(string name, int hp)
    {
        Name = name;
        HP = hp;
    }
}

/// <summary>
/// 一个简单的 struct (值类型)
/// 知识点 3: struct 的定义语法和 class 几乎一模一样!
///           唯一的区别就是把 class 关键字换成 struct
/// </summary>
public struct PlayerStruct
{
    public string Name;
    public int HP;

    // struct 也可以有构造函数
    // 注意: struct 的构造函数必须给所有字段赋值!
    public PlayerStruct(string name, int hp)
    {
        Name = name;
        HP = hp;
    }

    // struct 也可以有方法!
    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP < 0) HP = 0;
    }

    public string GetInfo()
    {
        return $"{Name} | HP: {HP}";
    }
}

// ============================================================
// Part 2: 更多 struct 实战示例
// 知识点 4: struct 适合做"小而简单"的数据容器
// ============================================================

/// <summary>
/// 武器属性 — 典型的"数据容器"
/// 几个数字打包在一起, 不需要继承, 不需要多态 → 用 struct 更好
/// </summary>
public struct WeaponStats
{
    public string WeaponName;
    public int Attack;
    public int Defense;
    public float CriticalRate;  // 暴击率 0~1
    public float AttackSpeed;   // 攻击速度

    // 构造函数
    public WeaponStats(string name, int atk, int def, float crit, float speed)
    {
        WeaponName = name;
        Attack = atk;
        Defense = def;
        CriticalRate = crit;
        AttackSpeed = speed;
    }

    /// <summary>
    /// 计算平均伤害 (考虑暴击)
    /// struct 里也可以有计算逻辑!
    /// </summary>
    public float GetAverageDamage()
    {
        // 平均伤害 = 基础攻击 × (1 + 暴击率 × 暴击倍率)
        float critMultiplier = 2.0f; // 暴击造成2倍伤害
        return Attack * (1 + CriticalRate * critMultiplier);
    }

    /// <summary>
    /// 打印武器信息
    /// </summary>
    public void PrintInfo()
    {
        Debug.Log($"🗡️ {WeaponName} | 攻击:{Attack} 防御:{Defense} " +
                  $"暴击:{CriticalRate:P0} 攻速:{AttackSpeed:F1} " +
                  $"平均伤害:{GetAverageDamage():F0}");
    }
}

/// <summary>
/// 物品掉落信息 — 另一个轻量数据容器
/// </summary>
public struct DropInfo
{
    public string ItemName;
    public int Quantity;
    public float DropChance; // 掉落概率 0~1

    public DropInfo(string name, int qty, float chance)
    {
        ItemName = name;
        Quantity = qty;
        DropChance = chance;
    }

    /// <summary>
    /// 判断是否掉落 (根据概率随机)
    /// </summary>
    public bool TryDrop()
    {
        float roll = UnityEngine.Random.Range(0f, 1f);
        return roll <= DropChance;
    }
}

/// <summary>
/// 坐标点 — 比 Vector2 多了标签信息
/// 知识点 5: struct 特别适合做"坐标、颜色、范围"这类小数据
/// </summary>
public struct Waypoint
{
    public Vector2 Position;
    public string Label;
    public bool IsCheckpoint; // 是否是存档点

    public Waypoint(Vector2 pos, string label, bool isCheckpoint = false)
    {
        Position = pos;
        Label = label;
        IsCheckpoint = isCheckpoint;
    }

    /// <summary>
    /// 计算两个路点之间的距离
    /// </summary>
    public float DistanceTo(Waypoint other)
    {
        return Vector2.Distance(Position, other.Position);
    }
}

// ============================================================
// Part 3: struct 的进阶特性
// 知识点 6: readonly struct — 不可变结构体
// 像 Vector3 一样: 创建后就不能改了, 改了就返回新的
// ============================================================

/// <summary>
/// 不可变结构体 (readonly struct)
/// 一旦创建就不能修改 — 这样更安全, 避免了意外修改
/// Unity 的 Vector3/Quaternion 都是这样设计的!
/// </summary>
public readonly struct MoneyInfo
{
    // readonly 字段: 只能在构造函数中赋值
    public readonly int Gold;
    public readonly int Gems;
    public readonly string CurrencyName;

    public MoneyInfo(int gold, int gems, string name = "金币")
    {
        Gold = gold;
        Gems = gems;
        CurrencyName = name;
    }

    /// <summary>
    /// "修改"操作 — 不修改原 struct, 而是返回一个新的!
    /// 这就是 Vector3 的工作方式:
    ///   Vector3 newPos = oldPos + Vector3.right;  // oldPos 没变!
    /// </summary>
    public MoneyInfo AddGold(int amount)
    {
        // 返回一个全新的 MoneyInfo, 原来的不变
        return new MoneyInfo(Gold + amount, Gems, CurrencyName);
    }

    public MoneyInfo AddGems(int amount)
    {
        return new MoneyInfo(Gold, Gems + amount, CurrencyName);
    }

    public string GetInfo()
    {
        return $"💰 {Gold} 金币 | 💎 {Gems} 宝石";
    }
}

// ============================================================
// Part 4: struct 的限制 — class 能做但 struct 不能做的事
// 知识点 7: struct 的局限性
//
// ❌ struct 不能继承其他 struct/class (但可以实现接口)
// ❌ struct 不能有无参构造函数 (C# 10.0 之前)
// ❌ struct 不能有析构函数
// ❌ struct 的字段不能有初始值 (C# 10.0 之前)
//
// 如果满足以下条件, 你不需要 struct 的这些能力, 用 struct 就好:
//   ✅ 数据量小 (几个字段)
//   ✅ 不需要继承
//   ✅ 逻辑简单
//   ✅ 频繁创建和销毁 (struct 在栈上, 不产生 GC!)
// ============================================================

// ============================================================
// Part 5: Unity 实战 — 什么时候用 struct?
// 知识点 8: 性能考量
//
// struct 存在栈 (Stack)  上 → 速度快, 用完自动回收, 不GC
// class  存在堆 (Heap)   上 → 灵活但慢, 需要 GC 回收
//
// 简单规则:
//   数据 < 16 字节? → struct
//   数据 ≥ 16 字节? → class
//   需要继承?      → class
//   不确定?       → 先用 class, 等性能瓶颈出现再优化
// ============================================================

/// <summary>
/// 知识点 9: Struct 可以放在数组里!
/// 这样数组在内存中连续排列, CPU 缓存友好, 遍历快
/// </summary>
public struct EnemySpawnData
{
    public Vector2 Position;
    public int EnemyType;    // 0=小兵, 1=精英, 2=Boss
    public float SpawnDelay; // 生成延迟

    public EnemySpawnData(Vector2 pos, int type, float delay)
    {
        Position = pos;
        EnemyType = type;
        SpawnDelay = delay;
    }
}

// ============================================================
// Part 6: Unity 演示组件
// 在场景中创建空 GameObject, 挂载此脚本, 运行即可看到效果
// ============================================================

/// <summary>
/// 结构体学习演示 — 挂载到 GameObject 上运行
/// </summary>
public class StructDemo : MonoBehaviour
{
    void Start()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("📦 Day 15: C# 结构体入门学习演示");
        Debug.Log("═══════════════════════════════════════\n");

        Demo1_ValueTypeVsReferenceType();
        Demo2_CopyBehavior();
        Demo3_StructAsReturnValue();
        Demo4_WeaponStats();
        Demo5_ReadonlyStruct();
        Demo6_GameData();
    }

    /// <summary>
    /// 演示 1: 值类型 vs 引用类型 — 最核心的区别
    /// </summary>
    void Demo1_ValueTypeVsReferenceType()
    {
        Debug.Log("━━━ 演示 1: struct 是值类型, class 是引用类型 ━━━");

        // --- class (引用类型) ---
        PlayerClass classPlayer1 = new PlayerClass("战士", 100);
        PlayerClass classPlayer2 = classPlayer1;  // 赋值 = 共享同一个对象!

        Debug.Log($"[Class] 修改前:");
        Debug.Log($"  classPlayer1: {classPlayer1.Name}, HP={classPlayer1.HP}");
        Debug.Log($"  classPlayer2: {classPlayer2.Name}, HP={classPlayer2.HP}");

        classPlayer2.HP = 50;  // 修改 classPlayer2...

        Debug.Log($"[Class] classPlayer2.HP = 50 之后:");
        Debug.Log($"  classPlayer1.HP = {classPlayer1.HP} ← 也变了! (因为是同一个对象)");
        Debug.Log($"  classPlayer2.HP = {classPlayer2.HP}");
        Debug.Log($"  ⚠️  class 赋值 = 给链接, 改了谁都影响对方");

        // --- struct (值类型) ---
        PlayerStruct structPlayer1 = new PlayerStruct("法师", 80);
        PlayerStruct structPlayer2 = structPlayer1;  // 赋值 = 完整复制一份!

        Debug.Log($"\n[Struct] 修改前:");
        Debug.Log($"  structPlayer1: HP={structPlayer1.HP}");
        Debug.Log($"  structPlayer2: HP={structPlayer2.HP}");

        structPlayer2.HP = 30;  // 修改 structPlayer2...

        Debug.Log($"[Struct] structPlayer2.HP = 30 之后:");
        Debug.Log($"  structPlayer1.HP = {structPlayer1.HP} ← 没变! (各自独立)");
        Debug.Log($"  structPlayer2.HP = {structPlayer2.HP}");
        Debug.Log($"  ✅ struct 赋值 = 复印一份, 各管各的");
    }

    /// <summary>
    /// 演示 2: struct 的复制行为
    /// </summary>
    void Demo2_CopyBehavior()
    {
        Debug.Log("\n━━━ 演示 2: struct 的复制行为 ━━━");

        WeaponStats sword = new WeaponStats("铁剑", 15, 0, 0.1f, 1.0f);
        Debug.Log("原始武器:");
        sword.PrintInfo();

        // 复制 struct → 得到完全独立的一份
        WeaponStats upgradedSword = sword;
        upgradedSword.WeaponName = "铁剑+1";
        upgradedSword.Attack = 22;  // 升级攻击力

        Debug.Log("\n升级后:");
        Debug.Log("原始 (没变):");
        sword.PrintInfo();
        Debug.Log("升级版 (独立):");
        upgradedSword.PrintInfo();
        Debug.Log("✅ struct 的复制是"深拷贝"— 改副本不影响原件");
    }

    /// <summary>
    /// 演示 3: struct 作为方法返回值
    /// </summary>
    void Demo3_StructAsReturnValue()
    {
        Debug.Log("\n━━━ 演示 3: struct 作为返回值 — 和 Vector3 一样 ━━━");

        // 这和 Vector3.Lerp(a, b, t) 返回新 Vector3 是一样的道理!
        WeaponStats baseWeapon = new WeaponStats("木棍", 5, 0, 0.05f, 0.8f);

        WeaponStats fireWeapon = EnchantWeapon(baseWeapon, "火焰木棍", 10);
        WeaponStats iceWeapon = EnchantWeapon(baseWeapon, "冰霜木棍", 8);

        Debug.Log("附魔结果 (原武器不变, 返回新武器):");
        baseWeapon.PrintInfo();
        fireWeapon.PrintInfo();
        iceWeapon.PrintInfo();
        Debug.Log("✅ 就像 Vector3.zero + Vector3.right 返回新 Vector3 — 原值不变!");
    }

    /// <summary>
    /// 给武器附魔 — 返回新的 WeaponStats, 不修改原来的
    /// </summary>
    WeaponStats EnchantWeapon(WeaponStats original, string newName, int bonusAtk)
    {
        // 创建新 struct, 在原来的基础上修改
        return new WeaponStats(
            newName,
            original.Attack + bonusAtk,
            original.Defense,
            original.CriticalRate + 0.05f, // 附魔加暴击
            original.AttackSpeed
        );
    }

    /// <summary>
    /// 演示 4: 武器数据管理
    /// </summary>
    void Demo4_WeaponStats()
    {
        Debug.Log("\n━━━ 演示 4: 武器数据管理 (struct 数组) ━━━");

        // struct 数组 — 数据在内存中连续排列, 访问快!
        WeaponStats[] weapons = new WeaponStats[]
        {
            new WeaponStats("🗡️ 铁剑", 15, 0, 0.10f, 1.2f),
            new WeaponStats("🪓 战斧", 25, -5, 0.15f, 0.7f),
            new WeaponStats("🏹 长弓", 18, -2, 0.20f, 0.9f),
            new WeaponStats("🔮 法杖", 12, 0, 0.05f, 1.5f),
        };

        Debug.Log("武器库 (struct数组):");
        float totalDPS = 0;
        foreach (var weapon in weapons)
        {
            weapon.PrintInfo();
            totalDPS += weapon.GetAverageDamage();
        }
        Debug.Log($"\n总平均伤害: {totalDPS:F0}");
        Debug.Log("✅ struct 数组内存连续, 遍历效率高!");
    }

    /// <summary>
    /// 演示 5: readonly struct — 不可变结构体
    /// </summary>
    void Demo5_ReadonlyStruct()
    {
        Debug.Log("\n━━━ 演示 5: readonly struct (不可变结构体) ━━━");

        MoneyInfo wallet = new MoneyInfo(100, 5);
        Debug.Log($"初始钱包: {wallet.GetInfo()}");

        // "修改"操作 — 实际是返回新的
        MoneyInfo afterQuest = wallet.AddGold(50);
        MoneyInfo afterBoth = afterQuest.AddGems(3);

        Debug.Log($"完成任务后: {afterQuest.GetInfo()}");
        Debug.Log($"领取宝石后: {afterBoth.GetInfo()}");
        Debug.Log($"原始钱包:    {wallet.GetInfo()} ← 没变!");
        Debug.Log("✅ readonly struct 永远不会被意外修改 — 更安全!");
    }

    /// <summary>
    /// 演示 6: 游戏数据 — 敌人生成配置
    /// </summary>
    void Demo6_GameData()
    {
        Debug.Log("\n━━━ 演示 6: 敌人生成配置 (struct数组) ━━━");

        EnemySpawnData[] spawnPoints = new EnemySpawnData[]
        {
            new EnemySpawnData(new Vector2(0, 0), 0, 0f),
            new EnemySpawnData(new Vector2(5, 3), 1, 3f),
            new EnemySpawnData(new Vector2(10, 0), 2, 10f),
            new EnemySpawnData(new Vector2(3, -2), 0, 1f),
        };

        Debug.Log("敌人生成计划:");
        foreach (var spawn in spawnPoints)
        {
            string typeName = spawn.EnemyType switch
            {
                0 => "小兵",
                1 => "精英",
                2 => "Boss",
                _ => "未知"
            };
            Debug.Log($"  📍 {spawn.Position} → {typeName} (延迟:{spawn.SpawnDelay}s)");
        }
        Debug.Log("✅ struct 做配置数据: 轻量、高效、无GC压力!");
    }
}

// ============================================================
// Part 7: struct vs class 速查表
// 知识点 10: 一张表记住所有区别
//
// ┌──────────────┬─────────────────┬─────────────────┐
// │    特性      │    struct       │     class       │
// ├──────────────┼─────────────────┼─────────────────┤
// │ 类型         │ 值类型          │ 引用类型         │
// │ 存储位置     │ 栈 (Stack)      │ 堆 (Heap)       │
// │ 赋值行为     │ 复制一份        │ 共享引用         │
// │ 默认值       │ 不为 null       │ 可以为 null      │
// │ 继承         │ 不能继承        │ 可以继承         │
// │ 实现接口     │ ✅ 可以         │ ✅ 可以          │
// │ 构造函数     │ 必须有参        │ 可以有参/无参    │
// │ GC 压力      │ 无 (栈上)       │ 有 (堆上)        │
// │ 适合场景     │ 小数据容器      │ 复杂对象         │
// │ Unity 例子   │ Vector3, Color  │ GameObject      │
// └──────────────┴─────────────────┴─────────────────┘
// ============================================================

// ============================================================
// 📝 今日要点总结:
//
// ✅ struct = 值类型 (赋值复制)  vs  class = 引用类型 (赋值共享)
// ✅ struct 在栈上, 无 GC → 频繁创建的小数据用 struct
// ✅ Unity 里的 Vector3/Color/Quaternion 都是 struct
// ✅ struct 可以有方法、属性、构造函数 (和 class 很像)
// ✅ readonly struct 更安全 — 不可变, 修改返回新的
// ✅ struct 不能继承 (但可以实现接口)
// ✅ 小数据 (< 16 字节) → struct, 复杂对象 → class
// ✅ 不确定用啥? 先用 class, 性能出问题再考虑 struct
//
// 🔗 与之前学习的关系:
//   - Day 2 (对象池): 池里的小数据用 struct 更省 GC
//   - Day 6 (策略模式): 策略接口返回 struct 数据
//   - Day 12 (建造者): Builder 构建的配置对象可用 struct
// ============================================================
