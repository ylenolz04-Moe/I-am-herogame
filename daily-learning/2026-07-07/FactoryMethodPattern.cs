// ============================================================
// 每日脚本学习 Day 9 — 2026-07-07
// 主题: C# 工厂方法模式 (Factory Method Pattern)
// 适用: Unity 游戏开发 · 敌人生成 · 道具创建 · UI 工厂 · 技能系统
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 工厂方法模式核心思想 — "定义创建对象的接口，让子类决定实例化哪个类"
//
//   [抽象工厂] ──► FactoryMethod() → IProduct  ← 返回产品接口
//                      ↑
//   [具体工厂A] ──► CreateProduct() → ProductA
//   [具体工厂B] ──► CreateProduct() → ProductB
//
// 简单来说: 把 new 操作封装起来，让创建对象变得灵活可控
// 和直接 new 的区别: new 是写死的，工厂可以加逻辑(缓存/池化/条件判断)
// ============================================================

// ============================================================
// Part 1: 简单工厂 — 工厂模式的起点
// 知识点 2: 最简单的工厂 — 一个静态方法根据参数创建不同对象
// ============================================================

/// <summary>
/// 知识点 3: 敌人基类 — 所有敌人的抽象父类
/// </summary>
public abstract class Enemy
{
    public string Name { get; protected set; }
    public float HP { get; protected set; }
    public float Attack { get; protected set; }
    public float Speed { get; protected set; }

    public abstract void OnSpawn();
    public abstract void OnDefeated();

    public void TakeDamage(float damage)
    {
        HP -= damage;
        Debug.Log($"[{Name}] 受到 {damage} 点伤害, 剩余 HP: {Mathf.Max(0, HP)}");
        if (HP <= 0) OnDefeated();
    }
}

/// <summary>
/// 知识点 4: 具体敌人 — 史莱姆
/// </summary>
public class Slime : Enemy
{
    public Slime()
    {
        Name = "绿色史莱姆";
        HP = 50f;
        Attack = 8f;
        Speed = 2f;
    }

    public override void OnSpawn()
    {
        Debug.Log($"🟢 {Name} 从草丛中跳出来! (HP:{HP} ATK:{Attack} SPD:{Speed})");
    }

    public override void OnDefeated()
    {
        Debug.Log($"💚 {Name} 被击败! 掉落: 史莱姆凝胶 ×1");
    }
}

/// <summary>
/// 知识点 5: 具体敌人 — 骷髅士兵
/// </summary>
public class Skeleton : Enemy
{
    public Skeleton()
    {
        Name = "骷髅士兵";
        HP = 80f;
        Attack = 15f;
        Speed = 3f;
    }

    public override void OnSpawn()
    {
        Debug.Log($"💀 {Name} 从地下爬出来! (HP:{HP} ATK:{Attack} SPD:{Speed})");
    }

    public override void OnDefeated()
    {
        Debug.Log($"🦴 {Name} 散架了! 掉落: 骨头碎片 ×2 + 锈剑 ×1");
    }
}

/// <summary>
/// 知识点 6: 具体敌人 — 哥布林
/// </summary>
public class Goblin : Enemy
{
    public Goblin()
    {
        Name = "狡猾哥布林";
        HP = 35f;
        Attack = 10f;
        Speed = 5f;
    }

    public override void OnSpawn()
    {
        Debug.Log($"👺 {Name} 从暗处偷袭! (HP:{HP} ATK:{Attack} SPD:{Speed})");
    }

    public override void OnDefeated()
    {
        Debug.Log($"💰 {Name} 逃跑失败! 掉落: 金币 ×15");
    }
}

// ============================================================
// Part 2: 简单工厂 (Simple Factory)
// 知识点 7: 用一个类封装所有创建逻辑 — 最常用的工厂变体
// ============================================================

/// <summary>
/// 知识点 8: EnemyFactory — 简单工厂
///   优点: 创建逻辑集中管理，调用方不需要知道具体类
///   缺点: 每加一个新敌人就要改这个类 (违反开闭原则)
///   但实际项目中这个"缺点"往往无所谓 — 简单就是美
/// </summary>
public static class EnemyFactory
{
    /// <summary>
    /// 知识点 9: 根据类型字符串创建敌人 — 最简单的方式
    /// </summary>
    public static Enemy CreateEnemy(string type)
    {
        switch (type.ToLower())
        {
            case "slime":
                return new Slime();
            case "skeleton":
                return new Skeleton();
            case "goblin":
                return new Goblin();
            default:
                Debug.LogWarning($"未知敌人类型: {type}，返回默认史莱姆");
                return new Slime();
        }
    }

    /// <summary>
    /// 知识点 10: 根据难度等级创建随机敌人 — 参数化工厂
    /// </summary>
    public static Enemy CreateRandomEnemy(int difficultyLevel)
    {
        // 难度 1-2: 只有史莱姆
        // 难度 3-5: 史莱姆 + 哥布林
        // 难度 6+: 所有敌人
        int roll = UnityEngine.Random.Range(0, 100);

        switch (difficultyLevel)
        {
            case <= 2:
                return new Slime();
            case <= 5:
                return roll < 50 ? new Slime() : new Goblin();
            default:
                return roll switch
                {
                    < 40 => new Slime(),
                    < 70 => new Goblin(),
                    _    => new Skeleton()
                };
        }
    }

    /// <summary>
    /// 知识点 11: 批量生成 — 一键生成一波敌人
    /// </summary>
    public static List<Enemy> CreateWave(int count, int difficulty)
    {
        var wave = new List<Enemy>();
        for (int i = 0; i < count; i++)
        {
            wave.Add(CreateRandomEnemy(difficulty));
        }
        Debug.Log($"🌊 生成第 {difficulty} 波敌人, 共 {count} 个");
        return wave;
    }
}

// ============================================================
// Part 3: 工厂方法模式 (Factory Method)
// 知识点 12: 真正的工厂方法 — 每个子类一个工厂，符合开闭原则
// ============================================================

/// <summary>
/// 知识点 13: IEnemySpawner — 工厂接口 (Creator)
///   定义"创建敌人"的契约，不同的 Spawner 创建不同的敌人
/// </summary>
public interface IEnemySpawner
{
    Enemy CreateEnemy();
    string SpawnerName { get; }
}

/// <summary>
/// 知识点 14: 史莱姆生成器 — 专门生产史莱姆
/// </summary>
public class SlimeSpawner : IEnemySpawner
{
    public string SpawnerName => "史莱姆巢穴";

    public Enemy CreateEnemy()
    {
        Debug.Log($"  🟢 {SpawnerName} 诞生了一只史莱姆!");
        return new Slime();
    }
}

/// <summary>
/// 知识点 15: 骷髅生成器 — 专门生产骷髅
/// </summary>
public class SkeletonSpawner : IEnemySpawner
{
    public string SpawnerName => "骷髅墓地";

    public Enemy CreateEnemy()
    {
        Debug.Log($"  💀 {SpawnerName} 召唤了一具骷髅!");
        return new Skeleton();
    }
}

/// <summary>
/// 知识点 16: 哥布林生成器
/// </summary>
public class GoblinSpawner : IEnemySpawner
{
    public string SpawnerName => "哥布林营地";

    public Enemy CreateEnemy()
    {
        Debug.Log($"  👺 {SpawnerName} 派出一只哥布林!");
        return new Goblin();
    }
}

/// <summary>
/// 知识点 17: 精英敌人生成器 — 带 Buff 的强化版
///   工厂方法可以添加额外逻辑(加 Buff / 套装饰器 / 从对象池取)
/// </summary>
public class EliteEnemySpawner : IEnemySpawner
{
    private readonly IEnemySpawner _baseSpawner;

    public EliteEnemySpawner(IEnemySpawner baseSpawner)
    {
        _baseSpawner = baseSpawner;
    }

    public string SpawnerName => $"精英 {_baseSpawner.SpawnerName}";

    public Enemy CreateEnemy()
    {
        Enemy enemy = _baseSpawner.CreateEnemy();
        // 知识点 18: 工厂方法的威力 — 创建后可以对对象做任何处理
        enemy.HP *= 2f;
        enemy.Attack *= 1.5f;
        enemy.Name = $"精英 {enemy.Name}";
        Debug.Log($"  ⭐ 精英强化! {enemy.Name} (HP:{enemy.HP} ATK:{enemy.Attack})");
        return enemy;
    }
}

// ============================================================
// Part 4: 道具/物品工厂 — 游戏中最常见的工厂场景
// 知识点 19: 道具工厂让掉落系统、商店系统、宝箱系统都变得简单
// ============================================================

/// <summary>
/// 知识点 20: IItem — 所有道具的接口
/// </summary>
public interface IItem
{
    string Name { get; }
    string Description { get; }
    int Value { get; }
    ItemRarity Rarity { get; }
    void Use(GameObject user);
}

/// <summary>
/// 知识点 21: 道具稀有度 — 枚举定义
/// </summary>
public enum ItemRarity
{
    Common,      // 白色
    Uncommon,    // 绿色
    Rare,        // 蓝色
    Epic,        // 紫色
    Legendary    // 金色
}

/// <summary>
/// 知识点 22: 生命药水 — 具体道具
/// </summary>
public class HealthPotionItem : IItem
{
    public string Name => "生命药水";
    public string Description => "恢复 50 点生命值";
    public int Value => 25;
    public ItemRarity Rarity => ItemRarity.Common;

    public void Use(GameObject user)
    {
        Debug.Log($"❤️ 使用 {Name}: 恢复 50 HP");
    }
}

/// <summary>
/// 知识点 23: 法力药水 — 具体道具
/// </summary>
public class ManaPotionItem : IItem
{
    public string Name => "法力药水";
    public string Description => "恢复 30 点法力值";
    public int Value => 20;
    public ItemRarity Rarity => ItemRarity.Common;

    public void Use(GameObject user)
    {
        Debug.Log($"💙 使用 {Name}: 恢复 30 MP");
    }
}

/// <summary>
/// 知识点 24: 铁剑 — 武器道具
/// </summary>
public class IronSwordItem : IItem
{
    public string Name => "铁剑";
    public string Description => "攻击力 +15 的普通铁剑";
    public int Value => 100;
    public ItemRarity Rarity => ItemRarity.Uncommon;

    public void Use(GameObject user)
    {
        Debug.Log($"⚔️ 装备 {Name}: 攻击力 +15");
    }
}

/// <summary>
/// 知识点 25: 魔法戒指 — 稀有装备
/// </summary>
public class MagicRingItem : IItem
{
    public string Name => "魔法戒指";
    public string Description => "法力上限 +50, 技能冷却 -20%";
    public int Value => 500;
    public ItemRarity Rarity => ItemRarity.Rare;

    public void Use(GameObject user)
    {
        Debug.Log($"💍 佩戴 {Name}: 法力+50 冷却-20%");
    }
}

/// <summary>
/// 知识点 26: 龙鳞铠甲 — 传奇装备
/// </summary>
public class DragonScaleArmorItem : IItem
{
    public string Name => "龙鳞铠甲";
    public string Description => "防御 +80, 火焰免疫, 生命恢复 +5/秒";
    public int Value => 5000;
    public ItemRarity Rarity => ItemRarity.Legendary;

    public void Use(GameObject user)
    {
        Debug.Log($"🛡️ 穿戴 {Name}: 防御+80 火焰免疫 回血+5/s");
    }
}

/// <summary>
/// 知识点 27: ItemFactory — 物品工厂
///   支持按类型、稀有度、随机等多种方式创建
/// </summary>
public static class ItemFactory
{
    // 知识点 28: 内部字典存储类型到构造函数的映射
    private static readonly Dictionary<string, Func<IItem>> _itemRegistry = new()
    {
        ["HealthPotion"] = () => new HealthPotionItem(),
        ["ManaPotion"]   = () => new ManaPotionItem(),
        ["IronSword"]    = () => new IronSwordItem(),
        ["MagicRing"]    = () => new MagicRingItem(),
        ["DragonScale"]  = () => new DragonScaleArmorItem(),
    };

    /// <summary>
    /// 知识点 29: 按名称创建 — 使用了注册表模式，新增物品只需注册
    /// </summary>
    public static IItem CreateItem(string itemName)
    {
        if (_itemRegistry.TryGetValue(itemName, out var constructor))
        {
            return constructor();
        }
        Debug.LogWarning($"未知道具: {itemName}");
        return null;
    }

    /// <summary>
    /// 知识点 30: 按稀有度随机创建 — 宝箱/掉落专用
    /// </summary>
    public static IItem CreateRandomItemByRarity(ItemRarity rarity)
    {
        // 筛选出对应稀有度的所有物品
        var allItems = new List<IItem>
        {
            new HealthPotionItem(), new ManaPotionItem(),
            new IronSwordItem(), new MagicRingItem(),
            new DragonScaleArmorItem()
        };

        var filtered = allItems.FindAll(item => item.Rarity == rarity);
        if (filtered.Count == 0)
        {
            Debug.LogWarning($"没有 {rarity} 稀有度的物品");
            return null;
        }

        return filtered[UnityEngine.Random.Range(0, filtered.Count)];
    }

    /// <summary>
    /// 知识点 31: 宝箱掉落 — 根据宝箱品质决定掉落
    /// </summary>
    public static List<IItem> OpenTreasureChest(string chestQuality)
    {
        var loot = new List<IItem>();

        switch (chestQuality)
        {
            case "木箱":
                loot.Add(new HealthPotionItem());
                break;
            case "铁箱":
                loot.Add(new HealthPotionItem());
                loot.Add(new IronSwordItem());
                break;
            case "金箱":
                loot.Add(new HealthPotionItem());
                loot.Add(new ManaPotionItem());
                loot.Add(new MagicRingItem());
                break;
            case "传说宝箱":
                loot.Add(new DragonScaleArmorItem());
                loot.Add(new MagicRingItem());
                loot.Add(new HealthPotionItem());
                break;
        }

        Debug.Log($"📦 打开 {chestQuality}，获得 {loot.Count} 件物品!");
        return loot;
    }
}

// ============================================================
// Part 5: 抽象工厂 (Abstract Factory)
// 知识点 32: 抽象工厂创建"一组相关的对象"
//   比如: 森林主题工厂 创建 树人+藤蔓+毒蘑菇
//        沙漠主题工厂 创建 蝎子+木乃伊+仙人掌
// ============================================================

/// <summary>
/// 知识点 33: ILevelFactory — 抽象工厂接口
///   一个工厂创建一组配套的敌人+道具+地形
/// </summary>
public interface ILevelFactory
{
    Enemy CreateBoss();
    Enemy CreateMinion();
    IItem CreateReward();
    string ThemeName { get; }
}

/// <summary>
/// 知识点 34: 森林主题工厂 — 森林关卡的所有内容
/// </summary>
public class ForestLevelFactory : ILevelFactory
{
    public string ThemeName => "🌳 森林";

    public Enemy CreateBoss()
    {
        // 森林 Boss: 巨型史莱姆王
        var boss = new Slime();
        boss.Name = "史莱姆王";
        boss.HP = 300;
        boss.Attack = 25;
        return boss;
    }

    public Enemy CreateMinion()
    {
        return new Slime();
    }

    public IItem CreateReward()
    {
        return new IronSwordItem();
    }
}

/// <summary>
/// 知识点 35: 墓地主题工厂 — 完全不同的怪物和奖励
/// </summary>
public class GraveyardLevelFactory : ILevelFactory
{
    public string ThemeName => "💀 墓地";

    public Enemy CreateBoss()
    {
        var boss = new Skeleton();
        boss.Name = "骷髅将军";
        boss.HP = 500;
        boss.Attack = 35;
        return boss;
    }

    public Enemy CreateMinion()
    {
        return new Skeleton();
    }

    public IItem CreateReward()
    {
        return new MagicRingItem();
    }
}

/// <summary>
/// 知识点 36: 地牢主题工厂
/// </summary>
public class DungeonLevelFactory : ILevelFactory
{
    public string ThemeName => "🏰 地牢";

    public Enemy CreateBoss()
    {
        var boss = new Goblin();
        boss.Name = "哥布林大祭司";
        boss.HP = 250;
        boss.Attack = 30;
        return boss;
    }

    public Enemy CreateMinion()
    {
        return new Goblin();
    }

    public IItem CreateReward()
    {
        return new DragonScaleArmorItem();
    }
}

/// <summary>
/// 知识点 37: 关卡生成器 — 使用抽象工厂构建完整关卡
/// </summary>
public static class LevelGenerator
{
    /// <summary>
    /// 知识点 38: 传入任何主题工厂，自动生成配套关卡
    ///   换了工厂就换了整个关卡体验 — 这就是抽象工厂的威力
    /// </summary>
    public static void GenerateLevel(ILevelFactory factory)
    {
        Debug.Log($"\n========== {factory.ThemeName} 主题关卡 ==========");

        // 生成小怪
        Debug.Log("--- 小怪波次 ---");
        for (int i = 0; i < 3; i++)
        {
            var minion = factory.CreateMinion();
            minion.OnSpawn();
        }

        // 生成 Boss
        Debug.Log("\n--- Boss 登场 ---");
        var boss = factory.CreateBoss();
        boss.OnSpawn();

        // Boss 掉落
        var reward = factory.CreateReward();
        Debug.Log($"\n🎁 Boss 掉落: [{reward.Rarity}] {reward.Name} — {reward.Description}");
    }
}

// ============================================================
// Part 6: Unity MonoBehaviour + 工厂 — 实际游戏场景用法
// 知识点 39: 在 MonoBehaviour 中使用工厂动态生成 GameObject
// ============================================================

/// <summary>
/// 知识点 40: 敌人预制体类型枚举
/// </summary>
public enum EnemyPrefabType
{
    Slime,
    Skeleton,
    Goblin,
    Bat,
    Spider
}

/// <summary>
/// 知识点 41: EnemySpawnManager — 游戏中的敌人管理器
///   这是工厂模式在 Unity 中最典型的用法
/// </summary>
public class EnemySpawnManager : MonoBehaviour
{
    // 知识点 42: 在 Inspector 中拖入敌人预制体
    [Header("敌人预制体")]
    [SerializeField] private GameObject _slimePrefab;
    [SerializeField] private GameObject _skeletonPrefab;
    [SerializeField] private GameObject _goblinPrefab;

    [Header("生成设置")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _spawnInterval = 3f;
    [SerializeField] private int _maxEnemies = 10;

    private readonly List<GameObject> _activeEnemies = new();
    // 知识点 43: 用字典缓存预制体引用 — O(1) 查找
    private Dictionary<EnemyPrefabType, GameObject> _prefabMap;

    private void Awake()
    {
        // 知识点 44: 初始化预制体映射表 — 工厂的"原材料库"
        _prefabMap = new Dictionary<EnemyPrefabType, GameObject>
        {
            [EnemyPrefabType.Slime]    = _slimePrefab,
            [EnemyPrefabType.Skeleton] = _skeletonPrefab,
            [EnemyPrefabType.Goblin]   = _goblinPrefab,
        };
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 知识点 45: 工厂方法 — 根据类型创建敌人 GameObject
    ///   封装了 Instantiate 和初始化逻辑
    /// </summary>
    public GameObject SpawnEnemy(EnemyPrefabType type, Vector3 position)
    {
        if (!_prefabMap.TryGetValue(type, out var prefab) || prefab == null)
        {
            Debug.LogError($"没有注册敌人类型: {type}");
            return null;
        }

        // 知识点 46: 工厂方法的核心 — 创建 + 初始化一起完成
        GameObject enemyObj = Instantiate(prefab, position, Quaternion.identity);
        enemyObj.name = $"{type}_{_activeEnemies.Count}";

        // 初始化敌人数据 (在真实项目中通过 GetComponent 获取脚本设置参数)
        // var enemyScript = enemyObj.GetComponent<EnemyBehavior>();
        // enemyScript.Initialize(type);

        _activeEnemies.Add(enemyObj);
        Debug.Log($"🎯 生成敌人: {type} 于位置 {position} (总数:{_activeEnemies.Count}/{_maxEnemies})");

        return enemyObj;
    }

    /// <summary>
    /// 知识点 47: 随机位置生成 — 封装了位置选择逻辑
    /// </summary>
    public GameObject SpawnRandomEnemy()
    {
        if (_activeEnemies.Count >= _maxEnemies) return null;

        // 随机类型
        var types = new[] { EnemyPrefabType.Slime, EnemyPrefabType.Skeleton, EnemyPrefabType.Goblin };
        var randomType = types[UnityEngine.Random.Range(0, types.Length)];

        // 随机生成点
        var spawnPos = _spawnPoints.Length > 0
            ? _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)].position
            : Vector3.zero;

        return SpawnEnemy(randomType, spawnPos);
    }

    /// <summary>
    /// 知识点 48: 清理死亡的敌人 — 配合对象池效果更佳 (回顾 Day2)
    /// </summary>
    public void DespawnEnemy(GameObject enemy)
    {
        _activeEnemies.Remove(enemy);
        Destroy(enemy);
        Debug.Log($"💥 敌人被消灭 (剩余:{_activeEnemies.Count})");
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);
            SpawnRandomEnemy();
        }
    }
}

// ============================================================
// Part 7: UI 工厂 — 动态创建 UI 元素
// 知识点 49: 工厂模式创建 UI 弹窗/提示/按钮
// ============================================================

/// <summary>
/// 知识点 50: 伤害数字类型
/// </summary>
public enum DamageTextType
{
    Normal,     // 白色普通伤害
    Critical,   // 黄色暴击
    Heal,       // 绿色治疗
    Poison,     // 紫色毒伤
}

/// <summary>
/// 知识点 51: DamageTextFactory — UI 工厂
///   在战斗中频繁创建伤害数字，工厂统一管理样式
/// </summary>
public static class DamageTextFactory
{
    // 知识点 52: 根据伤害类型返回不同颜色/大小的文字配置
    public static (Color color, int fontSize, string prefix) GetDamageTextConfig(DamageTextType type)
    {
        return type switch
        {
            DamageTextType.Normal   => (Color.white,  24, ""),
            DamageTextType.Critical => (Color.yellow, 36, "暴击! "),
            DamageTextType.Heal     => (Color.green,  28, "+"),
            DamageTextType.Poison   => (new Color(0.7f, 0, 1f), 20, "☠ "),
            _                       => (Color.white,  24, ""),
        };
    }

    /// <summary>
    /// 知识点 53: 创建伤害文字 — 一个方法替代四处手写 Instantiate
    /// </summary>
    public static void ShowDamageText(Vector3 worldPosition, float damage, DamageTextType type)
    {
        var config = GetDamageTextConfig(type);
        // 在真实 Unity 项目中:
        // GameObject textObj = ObjectPool.Get("DamageText");
        // textObj.transform.position = worldPosition;
        // var text = textObj.GetComponent<TextMeshPro>();
        // text.text = $"{config.prefix}{damage}";
        // text.color = config.color;
        // text.fontSize = config.fontSize;
        Debug.Log($"💬 [{type}] {config.prefix}{damage} (颜色:{config.color} 字号:{config.fontSize})");
    }
}

// ============================================================
// Part 8: 工厂方法进阶 — 结合 ScriptableObject
// 知识点 54: ScriptableObject + 工厂 = 策划可视化配置
// ============================================================

/// <summary>
/// 知识点 55: EnemyData — ScriptableObject 存储敌人配置
///   策划在 Inspector 中填写数值，工厂读取并创建敌人
/// </summary>
[CreateAssetMenu(menuName = "游戏/敌人数据", fileName = "EnemyData_")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "新敌人";
    public float hp = 100f;
    public float attack = 10f;
    public float speed = 3f;
    public EnemyPrefabType prefabType;
    [TextArea] public string description;

    /// <summary>
    /// 知识点 56: 从 ScriptableObject 创建敌人 — 数据驱动工厂
    /// </summary>
    public Enemy CreateEnemy()
    {
        Enemy enemy = prefabType switch
        {
            EnemyPrefabType.Slime    => new Slime(),
            EnemyPrefabType.Skeleton => new Skeleton(),
            EnemyPrefabType.Goblin   => new Goblin(),
            _ => new Slime()
        };

        // 用配置数据覆盖默认值
        enemy.Name = enemyName;
        enemy.HP = hp;
        enemy.Attack = attack;
        enemy.Speed = speed;

        return enemy;
    }
}

// ============================================================
// Part 9: 工厂 + 对象池 (结合 Day2 所学)
// 知识点 57: 工厂不一定要 new — 可以从对象池取
// ============================================================

/// <summary>
/// 知识点 58: PooledEnemyFactory — 结合对象池的工厂
///   创建时优先从池中取，没有才 new
/// </summary>
public static class PooledEnemyFactory
{
    // 知识点 59: 简单的对象池 (正式项目用 Day2 的 GenericObjectPool)
    private static readonly Dictionary<Type, Queue<Enemy>> _pool = new();

    /// <summary>
    /// 知识点 60: 从池中获取或创建新敌人
    /// </summary>
    public static T GetEnemy<T>() where T : Enemy, new()
    {
        var type = typeof(T);
        if (_pool.TryGetValue(type, out var queue) && queue.Count > 0)
        {
            var enemy = (T)queue.Dequeue();
            Debug.Log($"♻️ 从对象池复用: {enemy.GetType().Name}");
            return enemy;
        }

        Debug.Log($"🆕 创建新实例: {typeof(T).Name}");
        return new T();
    }

    /// <summary>
    /// 知识点 61: 归还到池中 (不销毁，下次复用)
    /// </summary>
    public static void ReturnEnemy(Enemy enemy)
    {
        var type = enemy.GetType();
        if (!_pool.ContainsKey(type))
        {
            _pool[type] = new Queue<Enemy>();
        }
        _pool[type].Enqueue(enemy);
        Debug.Log($"🔙 归还到对象池: {type.Name} (池中: {_pool[type].Count})");
    }
}

// ============================================================
// Part 10: MonoBehaviour 演示入口
// 知识点 62: FactoryDemo — 一键运行所有演示
// ============================================================

/// <summary>
/// 知识点 63: FactoryDemo — 挂载到空 GameObject 上运行
/// </summary>
public class FactoryDemo : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("========== Day 9: 工厂方法模式 演示开始 ==========");

        DemonstrateSimpleFactory();
        DemonstrateFactoryMethod();
        DemonstrateItemFactory();
        DemonstrateAbstractFactory();
        DemonstratePooledFactory();
        DemonstrateDamageText();

        Debug.Log("\n========== Day 9: 所有演示完成 ==========");
    }

    /// <summary>
    /// 演示 1: 简单工厂 — 根据字符串创建敌人
    /// </summary>
    private void DemonstrateSimpleFactory()
    {
        Debug.Log("\n========== 演示 1: 简单工厂 — EnemyFactory ==========");

        // 按名称创建
        Enemy slime = EnemyFactory.CreateEnemy("slime");
        slime.OnSpawn();

        Enemy skeleton = EnemyFactory.CreateEnemy("skeleton");
        skeleton.OnSpawn();

        // 按难度随机
        Debug.Log("\n--- 难度 7 随机敌人 ---");
        for (int i = 0; i < 3; i++)
        {
            var enemy = EnemyFactory.CreateRandomEnemy(7);
            enemy.OnSpawn();
        }

        // 批量生成波次
        var wave = EnemyFactory.CreateWave(3, 3);
        foreach (var e in wave) e.OnSpawn();
    }

    /// <summary>
    /// 演示 2: 工厂方法 — 每种敌人专用生成器
    /// </summary>
    private void DemonstrateFactoryMethod()
    {
        Debug.Log("\n========== 演示 2: 工厂方法 — IEnemySpawner ==========");

        IEnemySpawner[] spawners =
        {
            new SlimeSpawner(),
            new SkeletonSpawner(),
            new GoblinSpawner(),
            new EliteEnemySpawner(new GoblinSpawner()),  // 精英哥布林!
        };

        foreach (var spawner in spawners)
        {
            Debug.Log($"\n--- {spawner.SpawnerName} ---");
            var enemy = spawner.CreateEnemy();
            enemy.OnSpawn();
        }
    }

    /// <summary>
    /// 演示 3: 物品工厂 — 宝箱/商店/掉落
    /// </summary>
    private void DemonstrateItemFactory()
    {
        Debug.Log("\n========== 演示 3: 物品工厂 — ItemFactory ==========");

        // 按名称创建
        IItem sword = ItemFactory.CreateItem("IronSword");
        Debug.Log($"创建物品: [{sword.Rarity}] {sword.Name} — {sword.Description}");
        sword.Use(gameObject);

        // 按稀有度随机
        Debug.Log("\n--- 按稀有度随机 ---");
        var rareItem = ItemFactory.CreateRandomItemByRarity(ItemRarity.Rare);
        if (rareItem != null)
        {
            Debug.Log($"随机稀有物品: [{rareItem.Rarity}] {rareItem.Name}");
        }

        // 开宝箱
        Debug.Log("\n--- 开宝箱 ---");
        var chests = new[] { "木箱", "铁箱", "金箱", "传说宝箱" };
        foreach (var chest in chests)
        {
            var loot = ItemFactory.OpenTreasureChest(chest);
            foreach (var item in loot)
            {
                Debug.Log($"  📜 [{item.Rarity}] {item.Name} (价值:{item.Value})");
            }
        }
    }

    /// <summary>
    /// 演示 4: 抽象工厂 — 主题关卡
    /// </summary>
    private void DemonstrateAbstractFactory()
    {
        Debug.Log("\n========== 演示 4: 抽象工厂 — 主题关卡 ==========");

        ILevelFactory[] themes =
        {
            new ForestLevelFactory(),
            new GraveyardLevelFactory(),
            new DungeonLevelFactory(),
        };

        foreach (var factory in themes)
        {
            LevelGenerator.GenerateLevel(factory);
        }
    }

    /// <summary>
    /// 演示 5: 对象池工厂
    /// </summary>
    private void DemonstratePooledFactory()
    {
        Debug.Log("\n========== 演示 5: 对象池工厂 — PooledEnemyFactory ==========");

        // 获取和归还 Slime
        var s1 = PooledEnemyFactory.GetEnemy<Slime>();
        PooledEnemyFactory.ReturnEnemy(s1);

        // 再次获取 — 从池中取 (复用)
        var s2 = PooledEnemyFactory.GetEnemy<Slime>();

        // 新的 Skeleton — 池中没有，创建新的
        var sk1 = PooledEnemyFactory.GetEnemy<Skeleton>();
        PooledEnemyFactory.ReturnEnemy(sk1);
    }

    /// <summary>
    /// 演示 6: UI 工厂 — 伤害数字
    /// </summary>
    private void DemonstrateDamageText()
    {
        Debug.Log("\n========== 演示 6: UI 伤害数字工厂 ==========");

        DamageTextFactory.ShowDamageText(Vector3.zero, 45f, DamageTextType.Normal);
        DamageTextFactory.ShowDamageText(Vector3.zero, 120f, DamageTextType.Critical);
        DamageTextFactory.ShowDamageText(Vector3.zero, 30f, DamageTextType.Heal);
        DamageTextFactory.ShowDamageText(Vector3.zero, 8f, DamageTextType.Poison);
    }
}

// ============================================================
// Part 11: 三种工厂对比 — 什么时候用哪个?
// 知识点 64: 简单工厂 vs 工厂方法 vs 抽象工厂
// ============================================================

/// <summary>
/// 知识点 65: 工厂对比总结
/// </summary>
public static class FactoryComparison
{
    public static void PrintComparison()
    {
        Debug.Log("\n========== 工厂模式对比 ==========");

        Debug.Log("\n📦 简单工厂 (Simple Factory)");
        Debug.Log("  一个类 + 一个方法 + switch/if");
        Debug.Log("  适用: 创建逻辑简单, 类型不超过5种");
        Debug.Log("  典型: EnemyFactory.CreateEnemy(\"slime\")");
        Debug.Log("  ✅ 最简单, 够用就好");
        Debug.Log("  ❌ 加新类型要改代码");

        Debug.Log("\n🏭 工厂方法 (Factory Method)");
        Debug.Log("  一个接口 + 多个工厂类");
        Debug.Log("  适用: 每个子类创建逻辑不同, 或需要独立扩展");
        Debug.Log("  典型: IEnemySpawner → SlimeSpawner / SkeletonSpawner");
        Debug.Log("  ✅ 加新类型只需加新类 (开闭原则)");
        Debug.Log("  ❌ 类变多了");

        Debug.Log("\n🏗️ 抽象工厂 (Abstract Factory)");
        Debug.Log("  一个工厂创建一组相关对象");
        Debug.Log("  适用: 需要保证多个对象风格/主题一致");
        Debug.Log("  典型: ForestLevelFactory 创建树精+毒蘑菇+木质宝箱");
        Debug.Log("  ✅ 保证产品族一致性");
        Debug.Log("  ❌ 加新产品要改接口");

        Debug.Log("\n💡 一句话选型指南:");
        Debug.Log("  3种以下产品 → 简单工厂");
        Debug.Log("  产品需要独立扩展 → 工厂方法");
        Debug.Log("  一组产品要配套使用 → 抽象工厂");
    }
}

// ============================================================
// 知识点总结 — 工厂方法模式的精髓
// ============================================================
//
// ✅ 什么时候用工厂?
//   1. 创建对象需要额外逻辑 (缓存/池化/条件/日志)
//   2. 不想让调用方依赖具体类 — 面向接口编程
//   3. 对象创建过程可能变化 — 工厂隔离变化
//   4. 多个地方创建同类对象 — 工厂统一管理，避免散落的 new
//
// ❌ 什么时候不用?
//   1. 创建逻辑永远不会变 → 直接 new 更简单
//   2. 只有一个地方创建 → 工厂过度设计了
//   3. 对象很简单，不需要额外处理
//
// 🎮 游戏开发中的典型应用:
//   1. 敌人/NPC 生成系统 (如本文件示例)
//   2. 道具/装备创建 (掉落/商店/宝箱/任务奖励)
//   3. UI 元素工厂 (伤害数字/弹窗/提示/血条)
//   4. 关卡主题工厂 (森林/沙漠/冰原/火山)
//   5. 技能/特效工厂 (根据技能 ID 创建对应的 Effect)
//   6. 存档工厂 (不同版本的存档格式兼容)
//   7. AI 行为工厂 (根据角色类型创建不同的 AI 脚本)
//
// 📐 与已学模式的关系:
//   - 模板方法 (Day8): 工厂方法是模板方法的一种特例
//                     (父类定义创建流程，子类实现具体创建)
//   - 装饰器   (Day7): 工厂创建基础对象，装饰器附加功能
//   - 策略模式 (Day6): 工厂创建策略对象，客户端调用策略
//   - 对象池   (Day2): 工厂 + 对象池 = 高性能创建 (本文件 Part 9)
//   - 观察者   (Day3): 工厂创建对象后，注册事件监听
//   - 单例模式     : 工厂常常是单例 (static class 或 Singleton)
//
// 💡 一句话总结:
//   "工厂就像餐厅厨房 — 你点菜 ('给我一份牛排')，
//    厨房负责采购食材、烹饪、摆盘，最后端上来成品。
//    你不需要知道牛排怎么做，换厨师也不影响你吃饭。
//    工厂让'创建'和'使用'彻底分离。"
// ============================================================
