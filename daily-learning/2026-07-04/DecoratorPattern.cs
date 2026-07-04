// ============================================================
// 每日脚本学习 Day 7 — 2026-07-04
// 主题: C# 装饰器模式 (Decorator Pattern)
// 适用: Unity 游戏开发 · 装备附魔 · Buff/Debuff · 武器升级 · 技能修饰
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 装饰器模式核心思想 — "动态地给对象添加额外职责"
//
//   客户端 ──→ [IWeapon 接口] ←── 基础武器 (原始对象)
//                              ←── 装饰器 A (附加行为)
//                              ←── 装饰器 B (附加行为)
//                              ←── 装饰器 A + B (嵌套组合!)
//
// 装饰器像洋葱一样层层包裹，每一层添加新功能
// 和继承的区别：继承是编译时静态的，装饰器是运行时动态可组合的
// ============================================================

// ============================================================
// Part 1: 核心装饰器模式 — 武器附魔系统
// 知识点 2: IComponent 接口 — 装饰器和被装饰对象的统一契约
// ============================================================

/// <summary>
/// 知识点 3: IWeapon 接口 — 装饰器模式的最小契约
///   所有武器（基础/附魔）都实现同一接口
///   客户端不需要知道拿的是基础武器还是套了5层附魔的神器
/// </summary>
public interface IWeapon
{
    string Name { get; }
    float Damage { get; }
    float Range { get; }
    float Cooldown { get; }
    string Description { get; }

    /// <summary>
    /// 知识点 4: 武器实际攻击 — 装饰器可以在此前后插入逻辑
    /// </summary>
    void Attack(Transform target);
}

// ============================================================
// Part 2: 基础武器 — 被装饰的原始对象
// 知识点 5: ConcreteComponent — 装饰的起点
// ============================================================

/// <summary>
/// 知识点 6: 基础剑 — 最原始的武器，装饰的起点
/// 所有附魔都从它开始套
/// </summary>
public class Sword : IWeapon
{
    public string Name => "铁剑";
    public float Damage => 25f;
    public float Range => 1.5f;
    public float Cooldown => 0.8f;
    public string Description => "一把普通的铁剑";

    public void Attack(Transform target)
    {
        Debug.Log($"[{Name}] 挥砍! 造成 {Damage} 物理伤害");
    }
}

/// <summary>
/// 基础弓 — 另一个可被装饰的起点
/// </summary>
public class Bow : IWeapon
{
    public string Name => "短弓";
    public float Damage => 15f;
    public float Range => 10f;
    public float Cooldown => 1.2f;
    public string Description => "一把普通的短弓";

    public void Attack(Transform target)
    {
        Debug.Log($"[{Name}] 射箭! 造成 {Damage} 穿刺伤害 (射程 {Range})");
    }
}

/// <summary>
/// 基础法杖 — 第三个可被装饰的起点
/// </summary>
public class Staff : IWeapon
{
    public string Name => "学徒法杖";
    public float Damage => 18f;
    public float Range => 8f;
    public float Cooldown => 1.5f;
    public string Description => "一根基础的施法法杖";

    public void Attack(Transform target)
    {
        Debug.Log($"[{Name}] 魔法飞弹! 造成 {Damage} 魔法伤害");
    }
}

// ============================================================
// Part 3: 武器装饰器基类 — 所有附魔的父类
// 知识点 7: BaseDecorator 抽象类 — 持有一个被装饰对象的引用
// ============================================================

/// <summary>
/// 知识点 8: WeaponDecorator — 装饰器基类
///   关键设计：同时 IS 一个武器 且 HAS 一个武器
///   IS-A: 实现 IWeapon → 对外是武器
///   HAS-A: 持有 IWeapon → 对内可以委托/增强
///   这种"既是又包含"的关系是装饰器模式的精髓
/// </summary>
public abstract class WeaponDecorator : IWeapon
{
    // 知识点 9: 被装饰的武器 — 可以是基础武器，也可以是另一个装饰器!
    // 这就是"层层嵌套"的关键
    protected readonly IWeapon _decoratedWeapon;

    protected WeaponDecorator(IWeapon weapon)
    {
        _decoratedWeapon = weapon;
    }

    // 知识点 10: 默认委托 — 装饰器默认原样传递
    // 子类只覆写需要增强的属性/方法
    public virtual string Name => _decoratedWeapon.Name;
    public virtual float Damage => _decoratedWeapon.Damage;
    public virtual float Range => _decoratedWeapon.Range;
    public virtual float Cooldown => _decoratedWeapon.Cooldown;
    public virtual string Description => _decoratedWeapon.Description;

    public virtual void Attack(Transform target)
    {
        // 知识点 11: 先执行被装饰武器的原始行为
        _decoratedWeapon.Attack(target);
        // 子类在这里插入额外逻辑
    }
}

// ============================================================
// Part 4: 具体武器附魔 — 火焰/冰霜/毒液/雷电
// 知识点 12: 每个附魔是一个独立装饰器，可以任意组合
// ============================================================

/// <summary>
/// 知识点 13: 火焰附魔 — 增加火焰伤害 + 灼烧效果
/// </summary>
public class FireEnchant : WeaponDecorator
{
    private readonly float _fireBonus = 10f;
    private readonly float _burnDuration = 3f;

    public FireEnchant(IWeapon weapon) : base(weapon) { }

    public override string Name => $"烈焰 {_decoratedWeapon.Name}";
    public override float Damage => _decoratedWeapon.Damage + _fireBonus;

    public override string Description =>
        $"{_decoratedWeapon.Description}\n  └ 烈焰附魔: +{_fireBonus} 火焰伤害, 灼烧 {_burnDuration}秒";

    public override void Attack(Transform target)
    {
        base.Attack(target);
        // 知识点 14: 火焰附魔的额外效果 — 叠加在原始攻击之后
        Debug.Log($"  🔥 烈焰灼烧! 对目标造成 {_fireBonus} 额外火焰伤害, 灼烧 {_burnDuration}秒");
        ApplyBurn(target);
    }

    private void ApplyBurn(Transform target)
    {
        // 实际的灼烧 debuff 逻辑
        // target.GetComponent<StatusEffect>()?.AddEffect("Burn", _burnDuration, _fireBonus);
    }
}

/// <summary>
/// 知识点 15: 冰霜附魔 — 减速效果
/// </summary>
public class IceEnchant : WeaponDecorator
{
    private readonly float _iceBonus = 8f;
    private readonly float _slowPercent = 0.3f;
    private readonly float _slowDuration = 2f;

    public IceEnchant(IWeapon weapon) : base(weapon) { }

    public override string Name => $"冰霜 {_decoratedWeapon.Name}";
    public override float Damage => _decoratedWeapon.Damage + _iceBonus;

    public override string Description =>
        $"{_decoratedWeapon.Description}\n  └ 冰霜附魔: +{_iceBonus} 冰霜伤害, 减速 {_slowPercent:P0} 持续 {_slowDuration}秒";

    public override void Attack(Transform target)
    {
        base.Attack(target);
        Debug.Log($"  ❄️ 冰霜之力! 减速目标 {_slowPercent:P0}, 持续 {_slowDuration}秒");
    }
}

/// <summary>
/// 知识点 16: 毒液附魔 — 持续伤害 (DoT)
/// </summary>
public class PoisonEnchant : WeaponDecorator
{
    private readonly float _poisonDamage = 5f;
    private readonly float _poisonDuration = 5f;

    public PoisonEnchant(IWeapon weapon) : base(weapon) { }

    public override string Name => $"剧毒 {_decoratedWeapon.Name}";

    public override string Description =>
        $"{_decoratedWeapon.Description}\n  └ 剧毒附魔: {_poisonDamage}/秒 持续伤害, 持续 {_poisonDuration}秒";

    public override void Attack(Transform target)
    {
        base.Attack(target);
        Debug.Log($"  ☠️ 剧毒侵袭! {_poisonDamage}/秒 持续伤害, 持续 {_poisonDuration}秒");
    }
}

/// <summary>
/// 知识点 17: 雷电附魔 — 连锁伤害
/// </summary>
public class LightningEnchant : WeaponDecorator
{
    private readonly float _lightningBonus = 12f;
    private readonly int _chainCount = 3;
    private readonly float _chainRange = 4f;

    public LightningEnchant(IWeapon weapon) : base(weapon) { }

    public override string Name => $"雷电 {_decoratedWeapon.Name}";
    public override float Damage => _decoratedWeapon.Damage + _lightningBonus;

    public override string Description =>
        $"{_decoratedWeapon.Description}\n  └ 雷电附魔: +{_lightningBonus} 雷电伤害, 连锁 {_chainCount} 个目标 (范围 {_chainRange})";

    public override void Attack(Transform target)
    {
        base.Attack(target);
        Debug.Log($"  ⚡ 雷电连锁! 弹射至 {_chainCount} 个附近敌人 (范围 {_chainRange})");
        ChainLightning(target);
    }

    private void ChainLightning(Transform target)
    {
        // 实际的连锁闪电逻辑
        // var nearby = Physics.OverlapSphere(target.position, _chainRange);
        // foreach enemy in nearby take _chainCount: deal damage
    }
}

/// <summary>
/// 知识点 18: 吸血附魔 — 生命偷取
/// </summary>
public class VampiricEnchant : WeaponDecorator
{
    private readonly float _lifestealPercent = 0.15f;

    public VampiricEnchant(IWeapon weapon) : base(weapon) { }

    public override string Name => $"吸血 {_decoratedWeapon.Name}";

    public override string Description =>
        $"{_decoratedWeapon.Description}\n  └ 吸血附魔: 造成伤害的 {_lifestealPercent:P0} 转化为生命";

    public override void Attack(Transform target)
    {
        base.Attack(target);
        float healAmount = Damage * _lifestealPercent;
        Debug.Log($"  🩸 生命偷取! 回复 {healAmount:F0} 生命值");
    }
}

// ============================================================
// Part 5: 运行时组合演示 — 装饰器的真正威力
// 知识点 19: 装饰器可以无限嵌套 — 每个组合产生独特的神器
// ============================================================

/// <summary>
/// 知识点 20: 武器工坊 — 演示运行时动态组合装饰器
/// 这是继承无法做到的：不需要为每种组合创建子类!
/// </summary>
public static class WeaponForge
{
    /// <summary>
    /// 知识点 21: 装饰器嵌套演示
    ///
    /// 假设有 4 种附魔 (火/冰/毒/电) + 3 种基础武器 (剑/弓/杖)
    /// - 用继承: 需要 3×(2^4) = 48 个子类!
    /// - 用装饰器: 只需要 3+4 = 7 个类，运行时自由组合
    /// </summary>
    public static void Demonstrate()
    {
        Debug.Log("========== 武器工坊 — 装饰器组合演示 ==========");

        // 场景 1: 火焰冰霜剑 — 冰火两重天
        // 知识点 22: 装饰器可以任意顺序嵌套 — 每层独立运作
        Debug.Log("\n--- 场景 1: 打造 火焰冰霜剑 ---");
        IWeapon sword = new Sword();
        sword = new FireEnchant(sword);   // 先套火焰
        sword = new IceEnchant(sword);    // 再套冰霜
        // 此时 sword = IceEnchant(FireEnchant(Sword))
        Debug.Log($"武器: {sword.Name}");
        Debug.Log($"伤害: {sword.Damage}");
        Debug.Log($"描述: {sword.Description}");
        sword.Attack(null);

        // 场景 2: 剧毒雷电吸血弓 — 终极远程武器
        Debug.Log("\n--- 场景 2: 打造 剧毒雷电吸血弓 ---");
        IWeapon bow = new Bow();
        bow = new PoisonEnchant(bow);
        bow = new LightningEnchant(bow);
        bow = new VampiricEnchant(bow);
        Debug.Log($"武器: {bow.Name}");
        Debug.Log($"伤害: {bow.Damage}");
        Debug.Log($"描述: {bow.Description}");
        bow.Attack(null);

        // 场景 3: 雷电烈焰法杖
        Debug.Log("\n--- 场景 3: 打造 雷电烈焰法杖 ---");
        IWeapon staff = new Staff();
        staff = new LightningEnchant(staff);
        staff = new FireEnchant(staff);
        Debug.Log($"武器: {staff.Name}");
        Debug.Log($"描述: {staff.Description}");
        staff.Attack(null);

        // 场景 4: 动态切换附魔 — 战斗中武器进化
        Debug.Log("\n--- 场景 4: 战斗中动态移除附魔 ---");
        IWeapon dynamicWeapon = new Sword();
        dynamicWeapon = new FireEnchant(dynamicWeapon);
        dynamicWeapon = new PoisonEnchant(dynamicWeapon);
        Debug.Log($"附魔前: {dynamicWeapon.Name} 伤害={dynamicWeapon.Damage}");

        // 知识点 23: 移除最外层装饰器 — 只需取回内层引用
        if (dynamicWeapon is WeaponDecorator decorator)
        {
            // 通过反射访问私有字段（实际项目中用公开方法暴露内层引用）
            // 更优雅的做法：在 WeaponDecorator 上暴露 Unwrap() 方法
            Debug.Log("  → 移除最外层剧毒附魔...");
        }
    }
}

// ============================================================
// Part 6: 角色 Buff/Debuff 系统 — 装饰器的另一种应用
// 知识点 24: 装饰器不仅用于武器 — 角色属性也能装饰
// ============================================================

/// <summary>
/// 知识点 25: ICharacterStats — 角色属性接口（可被装饰）
/// </summary>
public interface ICharacterStats
{
    string CharacterName { get; }
    float MaxHealth { get; }
    float AttackPower { get; }
    float Defense { get; }
    float MoveSpeed { get; }
    float CritChance { get; }
    string StatsSummary { get; }
}

/// <summary>
/// 知识点 26: 基础角色属性 — 裸装状态
/// </summary>
public class BaseStats : ICharacterStats
{
    private readonly string _name;

    public BaseStats(string name, float maxHealth, float attack, float defense, float speed, float crit)
    {
        _name = name;
        MaxHealth = maxHealth;
        AttackPower = attack;
        Defense = defense;
        MoveSpeed = speed;
        CritChance = crit;
    }

    public string CharacterName => _name;
    public float MaxHealth { get; }
    public float AttackPower { get; }
    public float Defense { get; }
    public float MoveSpeed { get; }
    public float CritChance { get; }

    public string StatsSummary =>
        $"HP:{MaxHealth} ATK:{AttackPower} DEF:{Defense} SPD:{MoveSpeed} CRIT:{CritChance:P0}";
}

/// <summary>
/// 知识点 27: Buff 装饰器基类
/// 每个 Buff 包装一层角色属性，动态修改数值
/// </summary>
public abstract class BuffDecorator : ICharacterStats
{
    protected readonly ICharacterStats _inner;

    protected BuffDecorator(ICharacterStats inner)
    {
        _inner = inner;
    }

    public virtual string CharacterName => _inner.CharacterName;
    public virtual float MaxHealth => _inner.MaxHealth;
    public virtual float AttackPower => _inner.AttackPower;
    public virtual float Defense => _inner.Defense;
    public virtual float MoveSpeed => _inner.MoveSpeed;
    public virtual float CritChance => _inner.CritChance;
    public virtual string StatsSummary => _inner.StatsSummary;
}

/// <summary>
/// 知识点 28: 力量祝福 — 攻击力增益
/// </summary>
public class StrengthBlessing : BuffDecorator
{
    private readonly float _attackBonus;
    private readonly float _duration;

    public StrengthBlessing(ICharacterStats inner, float bonus = 15f, float duration = 30f)
        : base(inner)
    {
        _attackBonus = bonus;
        _duration = duration;
    }

    public override float AttackPower => _inner.AttackPower + _attackBonus;
    public override string StatsSummary =>
        $"{_inner.StatsSummary} | 力量祝福(+{_attackBonus}ATK, {_duration}s)";
}

/// <summary>
/// 知识点 29: 铁壁光环 — 防御增益
/// </summary>
public class IronAura : BuffDecorator
{
    private readonly float _defenseBonus;

    public IronAura(ICharacterStats inner, float bonus = 10f) : base(inner)
    {
        _defenseBonus = bonus;
    }

    public override float Defense => _inner.Defense + _defenseBonus;
    public override string StatsSummary =>
        $"{_inner.StatsSummary} | 铁壁(+{_defenseBonus}DEF)";
}

/// <summary>
/// 知识点 30: 疾风步 — 速度增益
/// </summary>
public class SwiftFoot : BuffDecorator
{
    private readonly float _speedMultiplier;

    public SwiftFoot(ICharacterStats inner, float multiplier = 1.3f) : base(inner)
    {
        _speedMultiplier = multiplier;
    }

    public override float MoveSpeed => _inner.MoveSpeed * _speedMultiplier;
    public override string StatsSummary =>
        $"{_inner.StatsSummary} | 疾风(x{_speedMultiplier}SPD)";
}

/// <summary>
/// 知识点 31: 狂战士之怒 — 攻击大幅提升但防御下降（双刃剑 Buff）
/// </summary>
public class BerserkerRage : BuffDecorator
{
    public BerserkerRage(ICharacterStats inner) : base(inner) { }

    public override float AttackPower => _inner.AttackPower * 1.5f;
    public override float Defense => _inner.Defense * 0.6f;
    public override float MaxHealth => _inner.MaxHealth * 0.8f;

    public override string StatsSummary =>
        $"{_inner.StatsSummary} | 狂怒(ATK×1.5 DEF×0.6 HP×0.8)";
}

/// <summary>
/// 知识点 32: Debuff — 虚弱诅咒（属性降低）
/// </summary>
public class WeaknessCurse : BuffDecorator
{
    public WeaknessCurse(ICharacterStats inner) : base(inner) { }

    public override float AttackPower => _inner.AttackPower * 0.7f;
    public override float Defense => _inner.Defense * 0.7f;
    public override float MoveSpeed => _inner.MoveSpeed * 0.8f;

    public override string StatsSummary =>
        $"{_inner.StatsSummary} | 虚弱(ATK×0.7 DEF×0.7 SPD×0.8)";
}

// ============================================================
// Part 7: Buff 系统演示
// 知识点 33: Buff 可以像洋葱一样叠加 — 力量祝福 + 铁壁 + 疾风 + 狂怒
// ============================================================

public static class BuffDemonstrator
{
    public static void Demonstrate()
    {
        Debug.Log("\n========== Buff/Debuff 系统 — 角色属性装饰演示 ==========");

        // 创建基础角色
        ICharacterStats hero = new BaseStats("勇者", 100, 30, 15, 5, 0.1f);
        Debug.Log($"\n基础角色 [{hero.CharacterName}]: {hero.StatsSummary}");

        // 场景 1: 叠满 Buff 的 BOSS 战状态
        Debug.Log("\n--- 场景 1: BOSS 战前满 Buff ---");
        ICharacterStats buffed = new BaseStats("勇者", 100, 30, 15, 5, 0.1f);
        buffed = new StrengthBlessing(buffed, 20f);   // 力量祝福 +20
        buffed = new IronAura(buffed, 12f);            // 铁壁 +12
        buffed = new SwiftFoot(buffed, 1.4f);          // 疾风 ×1.4
        Debug.Log($"[{buffed.CharacterName}] {buffed.StatsSummary}");
        Debug.Log($"  HP: {buffed.MaxHealth} ATK: {buffed.AttackPower} DEF: {buffed.Defense} SPD: {buffed.MoveSpeed}");

        // 场景 2: 狂战士形态 — 牺牲防御换攻击
        Debug.Log("\n--- 场景 2: 狂战士形态 ---");
        ICharacterStats berserker = new BaseStats("狂战士", 120, 40, 10, 4, 0.15f);
        berserker = new BerserkerRage(berserker);
        Debug.Log($"[{berserker.CharacterName}] {berserker.StatsSummary}");
        Debug.Log($"  HP: {berserker.MaxHealth} ATK: {berserker.AttackPower} DEF: {berserker.Defense}");

        // 场景 3: 被诅咒的敌人 — 多个 Debuff 叠加
        Debug.Log("\n--- 场景 3: 被诅咒的 BOSS ---");
        ICharacterStats boss = new BaseStats("暗黑魔王", 500, 80, 40, 3, 0.2f);
        boss = new WeaknessCurse(boss);    // 削弱
        Debug.Log($"[{boss.CharacterName}] {boss.StatsSummary}");
        Debug.Log($"  被削弱后: HP:{boss.MaxHealth} ATK:{boss.AttackPower} DEF:{boss.Defense}");

        // 场景 4: 复合状态 — Buff 和 Debuff 共存
        Debug.Log("\n--- 场景 4: 复合状态 (狂暴 + 虚弱) ---");
        ICharacterStats complex = new BaseStats("挣扎的战士", 100, 35, 20, 5, 0.1f);
        complex = new BerserkerRage(complex);   // 先狂怒
        complex = new WeaknessCurse(complex);   // 再虚弱
        Debug.Log($"[{complex.CharacterName}] {complex.StatsSummary}");
        Debug.Log($"  狂怒叠加虚弱后: HP:{complex.MaxHealth} ATK:{complex.AttackPower} DEF:{complex.Defense}");
        // 知识点 34: 注意! 装饰器的顺序影响最终结果
        // 先狂怒(ATK×1.5)再虚弱(ATK×0.7) → ATK ×1.5×0.7 = ×1.05
        // 先虚弱(ATK×0.7)再狂怒(ATK×1.5) → ATK ×0.7×1.5 = ×1.05 (乘法和顺序无关)
        // 但加法和乘法的混合会有差异: (ATK+20)×0.7 ≠ ATK×0.7+20
    }
}

// ============================================================
// Part 8: Unity ScriptableObject 集成 — 可配置的附魔数据
// 知识点 35: 装饰器 + ScriptableObject = 策划可配置的附魔系统
// ============================================================

/// <summary>
/// 知识点 36: EnchantData — ScriptableObject 附魔配置
/// 策划在 Unity Editor 中配置附魔参数，程序动态创建装饰器
/// </summary>
[CreateAssetMenu(menuName = "游戏/附魔数据", fileName = "Enchant_")]
public class EnchantData : ScriptableObject
{
    public string enchantName = "新附魔";
    public float damageBonus;
    public float rangeBonus;
    public float cooldownMultiplier = 1f;
    [TextArea] public string description;

    /// <summary>
    /// 知识点 37: 根据数据创建装饰器 — 工厂方法
    /// 将配置数据转化为可执行的装饰器对象
    /// </summary>
    public WeaponDecorator CreateDecorator(IWeapon weapon)
    {
        // 根据附魔类型返回对应的装饰器实例
        // 实际项目中使用枚举或字符串判断
        return new GenericEnchant(weapon, this);
    }
}

/// <summary>
/// 知识点 38: 通用附魔装饰器 — 由 ScriptableObject 数据驱动的装饰器
/// 好处：策划不需要写代码就能创建新附魔
/// </summary>
public class GenericEnchant : WeaponDecorator
{
    private readonly EnchantData _data;

    public GenericEnchant(IWeapon weapon, EnchantData data) : base(weapon)
    {
        _data = data;
    }

    public override string Name => $"{_data.enchantName} {_decoratedWeapon.Name}";
    public override float Damage => _decoratedWeapon.Damage + _data.damageBonus;
    public override float Range => _decoratedWeapon.Range + _data.rangeBonus;
    public override float Cooldown => _decoratedWeapon.Cooldown * _data.cooldownMultiplier;

    public override string Description =>
        $"{_decoratedWeapon.Description}\n  └ {_data.enchantName}: {_data.description}";

    public override void Attack(Transform target)
    {
        base.Attack(target);
        Debug.Log($"  ✨ {_data.enchantName} 效果触发!");
    }
}

// ============================================================
// Part 9: 装饰器工厂 — 简化创建流程
// 知识点 39: 工厂方法封装复杂的装饰器创建逻辑
// ============================================================

/// <summary>
/// 知识点 40: EnchantFactory — 提供便捷的附魔创建方法
/// 结合对象池可进一步优化 GC
/// </summary>
public static class EnchantFactory
{
    /// <summary>
    /// 快速创建火焰冰霜剑
    /// </summary>
    public static IWeapon CreateFireAndIceSword()
    {
        IWeapon weapon = new Sword();
        weapon = new FireEnchant(weapon);
        weapon = new IceEnchant(weapon);
        return weapon;
    }

    /// <summary>
    /// 快速创建终极 BOSS 武器 — 全附魔弓
    /// </summary>
    public static IWeapon CreateUltimateBow()
    {
        IWeapon weapon = new Bow();
        weapon = new FireEnchant(weapon);
        weapon = new IceEnchant(weapon);
        weapon = new PoisonEnchant(weapon);
        weapon = new LightningEnchant(weapon);
        weapon = new VampiricEnchant(weapon);
        return weapon;
    }

    /// <summary>
    /// 知识点 41: 根据配置列表批量创建附魔
    /// </summary>
    public static IWeapon EnchantWeapon(IWeapon baseWeapon, List<EnchantData> enchants)
    {
        IWeapon result = baseWeapon;
        foreach (var enchant in enchants)
        {
            result = enchant.CreateDecorator(result);
        }
        return result;
    }
}

// ============================================================
// Part 10: Unity MonoBehaviour 演示入口
// 知识点 42: 在 Unity 场景中挂载此脚本即可看到装饰器模式运行效果
// ============================================================

/// <summary>
/// 知识点 43: DecoratorDemo — MonoBehaviour 演示启动器
/// </summary>
public class DecoratorDemo : MonoBehaviour
{
    private void Start()
    {
        // 武器附魔演示
        WeaponForge.Demonstrate();

        // Buff/Debuff 演示
        BuffDemonstrator.Demonstrate();

        // 额外演示：装饰器数量爆炸的问题
        DemonstrateCombinatorialExplosion();
    }

    /// <summary>
    /// 知识点 44: 组合爆炸 — 为什么不用继承
    ///
    /// 假设: 3 种基础武器 × 5 种附魔
    /// - 继承方案: 需要 3 × (2^5 - 1) = 93 个子类 (每种附魔组合一个类)
    /// - 装饰器方案: 只需 3 + 5 = 8 个类
    ///
    /// 这就是"组合优于继承"的最佳例证
    /// </summary>
    private void DemonstrateCombinatorialExplosion()
    {
        Debug.Log("\n========== 组合爆炸对比 ==========");
        Debug.Log("3 种基础武器 × 5 种附魔:");
        Debug.Log("  继承方案: 需要 93 个子类 ❌");
        Debug.Log("  装饰器方案: 只需 8 个类 ✓");
        Debug.Log("  任何顺序组合都有效 ✓");

        // 随机组合一些武器来证明
        Debug.Log("\n--- 随机附魔组合 ---");
        var randomWeapons = new List<IWeapon>
        {
            new FireEnchant(new PoisonEnchant(new Sword())),
            new LightningEnchant(new VampiricEnchant(new IceEnchant(new Bow()))),
            new IceEnchant(new LightningEnchant(new FireEnchant(new Staff()))),
        };

        foreach (var weapon in randomWeapons)
        {
            Debug.Log($"[{weapon.Name}] 伤害:{weapon.Damage} 射程:{weapon.Range} 冷却:{weapon.Cooldown:F1}s");
        }
    }
}

// ============================================================
// Part 11: 装饰器模式进阶 — 装饰器链/管道模式
// 知识点 45: 装饰器链是装饰器模式的自然延伸
// 多个装饰器像管道一样依次处理数据
// ============================================================

/// <summary>
/// 知识点 46: 伤害计算管道 — 装饰器链处理伤害
/// 武器 → 火焰加成 → 暴击计算 → 护甲减免 → 最终伤害
/// </summary>
public static class DamagePipeline
{
    /// <summary>
    /// 知识点 47: 使用装饰器链构建伤害计算管道
    /// 每一环节可以用装饰器独立控制，方便 A/B 测试和平衡调整
    /// </summary>
    public static float CalculateFinalDamage(IWeapon weapon, float targetArmor, bool isCrit)
    {
        float baseDamage = weapon.Damage;

        // 模拟管道的每个阶段
        // 阶段 1: 基础伤害
        float damage = baseDamage;
        Debug.Log($"管道阶段1 - 基础伤害: {damage}");

        // 阶段 2: 元素加成 (已由装饰器处理在 weapon.Damage 中)
        // 阶段 3: 暴击判定
        if (isCrit)
        {
            damage *= 1.5f;
            Debug.Log($"管道阶段2 - 暴击 ×1.5: {damage}");
        }

        // 阶段 4: 护甲减免
        float reduction = targetArmor / (targetArmor + 100f);
        damage *= (1f - reduction);
        Debug.Log($"管道阶段3 - 护甲减免({reduction:P0}): {damage}");

        // 阶段 5: 限幅
        damage = Mathf.Max(1f, damage);
        Debug.Log($"管道阶段4 - 最终伤害: {damage:F0}");

        return damage;
    }
}

// ============================================================
// 知识点总结 — 装饰器模式的精髓
// ============================================================
//
// ✅ 什么时候用?
//   - 需要动态给对象添加功能，且功能之间可以任意组合
//   - 不想用继承导致子类爆炸
//   - 需要在运行时增删对象的功能
//
// ❌ 什么时候不用?
//   - 装饰层次固定不会变化 → 直接用继承更简单
//   - 对象有很多非装饰器相关的公共方法 → 接口过于臃肿
//   - 需要透传大量方法 → 装饰器基类的转发代码太多
//
// 🎮 游戏开发中的典型应用:
//   1. 武器/装备附魔系统 (如本文件示例)
//   2. 角色 Buff/Debuff 叠加系统
//   3. 技能效果修饰器 (范围+ 伤害+ 冷却-)
//   4. UI 组件的视觉装饰 (边框/阴影/高亮)
//   5. 伤害计算管道 (元素加成→暴击→护甲→减免)
//   6. 道路/地形的动态特征叠加 (毒沼+燃烧+减速)
//
// 📐 与已学模式的关系:
//   - 策略模式 (Day6): 策略替换整个算法 | 装饰器在原算法上叠加
//   - 命令模式 (Day5): 命令封装操作 | 装饰器增强操作
//   - 对象池   (Day2): 装饰器实例可以池化复用
//   - 工厂模式 : 用工厂封装装饰器的创建过程
//
// 💡 一句话总结:
//   "装饰器让你像穿衣服一样给对象叠加功能 —
//    先穿内衣、再穿衬衫、最后套外套，每一层都增加新的能力，
//    而对象本身不知道(也不关心)自己被套了多少层。"
// ============================================================
