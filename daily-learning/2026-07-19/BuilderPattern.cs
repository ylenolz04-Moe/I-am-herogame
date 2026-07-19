// ============================================================
// 每日脚本学习 Day 12 — 2026-07-19
// 主题: C# 建造者模式 (Builder Pattern)
// 适用: Unity 游戏开发 · 角色创建 · 武器打造 · 关卡构建 · UI 弹窗
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 建造者模式核心思想 — "分步骤构建复杂对象"
//
//   [指挥者 Director] ──► [建造者 Builder] ──► [产品 Product]
//        │                    │
//        │   BuildPartA()     │  一步一步添加零件
//        │   BuildPartB()     │
//        │   BuildPartC()     │
//        │   GetResult()      │
//
// 现实类比: 定制汉堡 —
//   你说: "要全麦面包+牛肉饼+芝士+生菜"
//   店员(Builder)一步步组装，最后给你一个完整的汉堡(Product)
//   和新手直接 new 的区别: 新手把所有参数塞进构造函数, Builder 一步步来
// ============================================================

// ============================================================
// Part 1: 建造者模式的核心结构
// 知识点 2: 产品 → 抽象建造者 → 具体建造者 → 指挥者
// ============================================================

// --- 产品 ---

/// <summary>
/// 知识点 3: Weapon — 我们要构建的复杂产品
/// 一把武器有多个可配置的属性
/// </summary>
public class Weapon
{
    public string Name { get; set; }
    public float BaseAttack { get; set; }
    public float AttackSpeed { get; set; }
    public float CritChance { get; set; }
    public float CritDamage { get; set; }
    public string ElementType { get; set; }   // 火/冰/雷/毒
    public int ElementDamage { get; set; }
    public List<string> SpecialEffects { get; set; } = new List<string>();
    public string Rarity { get; set; }        // 白/蓝/紫/金
    public int Level { get; set; }

    public void PrintStats()
    {
        Debug.Log($"╔══════════════════════════════╗");
        Debug.Log($"║ ⚔ {Name,-24} ║");
        Debug.Log($"╠══════════════════════════════╣");
        Debug.Log($"║ 攻击力:   {BaseAttack,-6}               ║");
        Debug.Log($"║ 攻击速度: {AttackSpeed,-6:F1}              ║");
        Debug.Log($"║ 暴击率:   {CritChance,-6:P0}              ║");
        Debug.Log($"║ 暴击伤害: {CritDamage,-6:P0}              ║");
        if (!string.IsNullOrEmpty(ElementType))
            Debug.Log($"║ 元素:     {ElementType}(+{ElementDamage})           ║");
        Debug.Log($"║ 稀有度:   {Rarity,-6}               ║");
        Debug.Log($"║ 等级需求: Lv.{Level,-4}              ║");
        if (SpecialEffects.Count > 0)
            Debug.Log($"║ 特效:     {string.Join(", ", SpecialEffects)}  ║");
        Debug.Log($"╚══════════════════════════════╝");
    }
}

// --- 抽象建造者 ---

/// <summary>
/// 知识点 4: IWeaponBuilder — 定义构建步骤的接口
/// 每个步骤都是独立的, 调用方可以选择调用哪些步骤
/// </summary>
public interface IWeaponBuilder
{
    void SetBaseStats(float attack, float speed);
    void SetCritStats(float chance, float damage);
    void SetElement(string element, int damage);
    void AddSpecialEffect(string effect);
    void SetRarity(string rarity);
    void SetLevel(int level);
    Weapon GetResult();
}

// --- 具体建造者 ---

/// <summary>
/// 知识点 5: SwordBuilder — 剑类武器建造者
/// 剑的特点是: 均衡的攻击和速度
/// </summary>
public class SwordBuilder : IWeaponBuilder
{
    private Weapon _weapon = new Weapon();

    public SwordBuilder(string name)
    {
        _weapon.Name = name;
    }

    public void SetBaseStats(float attack, float speed)
    {
        // 剑的加成: 速度+10%
        _weapon.BaseAttack = attack;
        _weapon.AttackSpeed = speed * 1.1f;
        Debug.Log($"[剑建造] 设置基础: 攻击{attack} 速度{speed} (+10%剑类加成)");
    }

    public void SetCritStats(float chance, float damage)
    {
        _weapon.CritChance = chance;
        _weapon.CritDamage = damage;
        Debug.Log($"[剑建造] 暴击: {chance:P0}/{damage:P0}");
    }

    public void SetElement(string element, int damage)
    {
        _weapon.ElementType = element;
        _weapon.ElementDamage = damage;
        Debug.Log($"[剑建造] 附魔: {element}+{damage}");
    }

    public void AddSpecialEffect(string effect)
    {
        _weapon.SpecialEffects.Add(effect);
        Debug.Log($"[剑建造] 特效: {effect}");
    }

    public void SetRarity(string rarity)
    {
        _weapon.Rarity = rarity;
    }

    public void SetLevel(int level)
    {
        _weapon.Level = level;
    }

    public Weapon GetResult()
    {
        Debug.Log($"[剑建造] ✅ 完成: {_weapon.Name}");
        return _weapon;
    }
}

/// <summary>
/// 知识点 6: BowBuilder — 弓类武器建造者 (弓的速度加成更高)
/// </summary>
public class BowBuilder : IWeaponBuilder
{
    private Weapon _weapon = new Weapon();

    public BowBuilder(string name)
    {
        _weapon.Name = name;
    }

    public void SetBaseStats(float attack, float speed)
    {
        // 弓的特点: 速度快但攻击稍低
        _weapon.BaseAttack = attack * 0.85f;
        _weapon.AttackSpeed = speed * 1.4f;
        Debug.Log($"[弓建造]  设置基础: 攻击{_weapon.BaseAttack} 速度{_weapon.AttackSpeed} (弓类: 攻-15% 速+40%)");
    }

    public void SetCritStats(float chance, float damage)
    {
        // 弓的暴击率更高
        _weapon.CritChance = chance * 1.3f;
        _weapon.CritDamage = damage;
        Debug.Log($"[弓建造]  暴击: {_weapon.CritChance:P0}/{_weapon.CritDamage:P0} (+30%暴击率)");
    }

    public void SetElement(string element, int damage)
    {
        _weapon.ElementType = element;
        _weapon.ElementDamage = damage;
    }

    public void AddSpecialEffect(string effect)
    {
        _weapon.SpecialEffects.Add(effect);
    }

    public void SetRarity(string rarity) { _weapon.Rarity = rarity; }
    public void SetLevel(int level) { _weapon.Level = level; }

    public Weapon GetResult()
    {
        Debug.Log($"[弓建造]  ✅ 完成: {_weapon.Name}");
        return _weapon;
    }
}

// --- 指挥者 (可选但推荐) ---

/// <summary>
/// 知识点 7: WeaponSmith — 指挥者, 封装预设的构建流程
/// 让客户端不需要一步步手动调 Builder
/// </summary>
public class WeaponSmith
{
    private IWeaponBuilder _builder;

    public WeaponSmith(IWeaponBuilder builder)
    {
        _builder = builder;
    }

    public void SwitchBuilder(IWeaponBuilder newBuilder)
    {
        _builder = newBuilder;
    }

    /// <summary>
    /// 知识点 8: 预设配方 — 新手木剑
    /// </summary>
    public Weapon CraftBeginnerSword()
    {
        Debug.Log("--- [铁匠] 打造: 新手木剑 ---");
        _builder.SetBaseStats(15, 1.0f);
        _builder.SetRarity("白色");
        _builder.SetLevel(1);
        return _builder.GetResult();
    }

    /// <summary>
    /// 知识点 9: 预设配方 — 火焰传说之剑
    /// </summary>
    public Weapon CraftFlameLegendary()
    {
        Debug.Log("--- [铁匠] 打造: 火焰传说之剑 ---");
        _builder.SetBaseStats(120, 1.5f);
        _builder.SetCritStats(0.25f, 2.5f);
        _builder.SetElement("🔥火", 45);
        _builder.AddSpecialEffect("击杀回复HP");
        _builder.AddSpecialEffect("灼烧敌人3秒");
        _builder.SetRarity("金色");
        _builder.SetLevel(50);
        return _builder.GetResult();
    }

    /// <summary>
    /// 知识点 10: 预设配方 — 暗影猎人弓
    /// </summary>
    public Weapon CraftShadowHunterBow()
    {
        Debug.Log("--- [铁匠] 打造: 暗影猎人弓 ---");
        _builder.SetBaseStats(95, 2.0f);
        _builder.SetCritStats(0.35f, 2.0f);
        _builder.SetElement("🌑暗", 30);
        _builder.AddSpecialEffect("穿透射击");
        _builder.AddSpecialEffect("击杀重置冷却");
        _builder.SetRarity("紫色");
        _builder.SetLevel(35);
        return _builder.GetResult();
    }
}

// ============================================================
// Part 2: 流式建造者 (Fluent Builder) — C# 中最常用的建造者变体
// 知识点 11: 每个 Set 方法返回 this, 可以链式调用
// ============================================================

/// <summary>
/// 知识点 12: Character — RPG 角色 (更复杂的产品)
/// </summary>
public class Character
{
    public string Name { get; set; }
    public string Class { get; set; }        // 战士/法师/射手
    public int HP { get; set; }
    public int MP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public List<string> Skills { get; set; } = new List<string>();
    public string Weapon { get; set; }
    public string Armor { get; set; }
    public string Appearance { get; set; }

    public void PrintSheet()
    {
        Debug.Log($"╔══════════════════════════════╗");
        Debug.Log($"║ 🧙 {Name,-24} ║");
        Debug.Log($"╠══════════════════════════════╣");
        Debug.Log($"║ 职业: {Class,-20} ║");
        Debug.Log($"║ HP:{HP,-4} MP:{MP,-4} 攻:{Attack,-4}   ║");
        Debug.Log($"║ 防:{Defense,-4} 速:{Speed,-4}            ║");
        Debug.Log($"║ 武器: {Weapon,-20} ║");
        Debug.Log($"║ 防具: {Armor,-20} ║");
        Debug.Log($"║ 外观: {Appearance,-20} ║");
        Debug.Log($"║ 技能: {string.Join(",", Skills),-20} ║");
        Debug.Log($"╚══════════════════════════════╝");
    }
}

/// <summary>
/// 知识点 13: FluentCharacterBuilder — 流式建造者
/// 每个方法返回 this, 可以写成一条链
/// 这是 C# 中最流行的建造者写法
/// </summary>
public class FluentCharacterBuilder
{
    private Character _character = new Character();

    /// <summary>
    /// 知识点 14: 每个 Set 方法返回自身 → 链式调用
    /// </summary>
    public FluentCharacterBuilder WithName(string name)
    {
        _character.Name = name;
        return this;
    }

    public FluentCharacterBuilder AsClass(string className)
    {
        _character.Class = className;
        return this;
    }

    public FluentCharacterBuilder WithHP(int hp)
    {
        _character.HP = hp;
        return this;
    }

    public FluentCharacterBuilder WithMP(int mp)
    {
        _character.MP = mp;
        return this;
    }

    public FluentCharacterBuilder WithAttack(int attack)
    {
        _character.Attack = attack;
        return this;
    }

    public FluentCharacterBuilder WithDefense(int defense)
    {
        _character.Defense = defense;
        return this;
    }

    public FluentCharacterBuilder WithSpeed(int speed)
    {
        _character.Speed = speed;
        return this;
    }

    public FluentCharacterBuilder WithSkill(string skill)
    {
        _character.Skills.Add(skill);
        return this;
    }

    public FluentCharacterBuilder WithWeapon(string weapon)
    {
        _character.Weapon = weapon;
        return this;
    }

    public FluentCharacterBuilder WithArmor(string armor)
    {
        _character.Armor = armor;
        return this;
    }

    public FluentCharacterBuilder WithAppearance(string appearance)
    {
        _character.Appearance = appearance;
        return this;
    }

    /// <summary>
    /// 知识点 15: Build() — 流式建造的终点
    /// </summary>
    public Character Build()
    {
        Debug.Log($"[角色建造] ✅ 角色创建完成: {_character.Name}");
        return _character;
    }
}

/// <summary>
/// 知识点 16: 静态工厂方法 — 提供预设模板的快捷入口
/// </summary>
public static class CharacterPresets
{
    public static Character CreateWarrior(string name)
    {
        return new FluentCharacterBuilder()
            .WithName(name)
            .AsClass("⚔️战士")
            .WithHP(200).WithMP(30)
            .WithAttack(45).WithDefense(60).WithSpeed(20)
            .WithSkill("猛击").WithSkill("盾牌格挡").WithSkill("旋风斩")
            .WithWeapon("钢铁长剑").WithArmor("重甲")
            .WithAppearance("红色披风+板甲")
            .Build();
    }

    public static Character CreateMage(string name)
    {
        return new FluentCharacterBuilder()
            .WithName(name)
            .AsClass("🔮法师")
            .WithHP(80).WithMP(200)
            .WithAttack(70).WithDefense(15).WithSpeed(25)
            .WithSkill("火球术").WithSkill("暴风雪").WithSkill("魔力护盾")
            .WithWeapon("秘法权杖").WithArmor("法师袍")
            .WithAppearance("蓝色法袍+魔法书")
            .Build();
    }

    public static Character CreateRanger(string name)
    {
        return new FluentCharacterBuilder()
            .WithName(name)
            .AsClass("🏹射手")
            .WithHP(120).WithMP(60)
            .WithAttack(55).WithDefense(30).WithSpeed(50)
            .WithSkill("精准射击").WithSkill("陷阱").WithSkill("后跳")
            .WithWeapon("复合弓").WithArmor("皮甲")
            .WithAppearance("绿色斗篷+轻甲")
            .Build();
    }
}

// ============================================================
// Part 3: Builder 用于 UI 弹窗 — 非常实用的 Unity 场景
// 知识点 17: 不是只有数据对象才能用 Builder
// ============================================================

/// <summary>
/// 知识点 18: DialogData — 弹窗的配置数据
/// </summary>
public class DialogData
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string ConfirmText { get; set; } = "确定";
    public string CancelText { get; set; } = "取消";
    public bool ShowCancel { get; set; } = true;
    public Action OnConfirm { get; set; }
    public Action OnCancel { get; set; }
    public Color TintColor { get; set; } = Color.white;
    public bool IsModal { get; set; } = true;

    public void Execute()
    {
        Debug.Log($"╔══════════════════════════════╗");
        Debug.Log($"║ [弹窗] {Title}");
        Debug.Log($"║ {Message}");
        Debug.Log($"║ [{ConfirmText}] {(ShowCancel ? $"[{CancelText}]" : "")}");
        Debug.Log($"╚══════════════════════════════╝");
    }
}

/// <summary>
/// 知识点 19: DialogBuilder — 构建弹窗的流式建造者
/// 实际项目中 Build() 会调 UIManager.ShowDialog(data)
/// </summary>
public class DialogBuilder
{
    private DialogData _dialog = new DialogData();

    public DialogBuilder SetTitle(string title)
    {
        _dialog.Title = title;
        return this;
    }

    public DialogBuilder SetMessage(string message)
    {
        _dialog.Message = message;
        return this;
    }

    public DialogBuilder SetConfirm(string text, Action onConfirm = null)
    {
        _dialog.ConfirmText = text;
        _dialog.OnConfirm = onConfirm;
        return this;
    }

    public DialogBuilder SetCancel(string text, Action onCancel = null)
    {
        _dialog.CancelText = text;
        _dialog.OnCancel = onCancel;
        return this;
    }

    public DialogBuilder HideCancel()
    {
        _dialog.ShowCancel = false;
        return this;
    }

    public DialogBuilder SetColor(Color color)
    {
        _dialog.TintColor = color;
        return this;
    }

    public DialogBuilder AsNonModal()
    {
        _dialog.IsModal = false;
        return this;
    }

    public DialogData Build()
    {
        Debug.Log($"[弹窗建造] ✅ 弹窗构建完成");
        return _dialog;
    }
}

/// <summary>
/// 知识点 20: 常用弹窗的快捷预设
/// </summary>
public static class DialogPresets
{
    public static DialogData ConfirmDelete(string itemName, Action onConfirm)
    {
        return new DialogBuilder()
            .SetTitle("确认删除")
            .SetMessage($"确定要删除 \"{itemName}\" 吗？此操作不可撤销。")
            .SetConfirm("删除", onConfirm)
            .SetCancel("保留")
            .SetColor(Color.red)
            .Build();
    }

    public static DialogData ShowTip(string message)
    {
        return new DialogBuilder()
            .SetTitle("提示")
            .SetMessage(message)
            .SetConfirm("知道了")
            .HideCancel()
            .Build();
    }

    public static DialogData ShowReward(string itemName, int count)
    {
        return new DialogBuilder()
            .SetTitle("🎁 获得奖励")
            .SetMessage($"恭喜获得: {itemName} ×{count}!")
            .SetConfirm("太棒了!")
            .HideCancel()
            .SetColor(Color.yellow)
            .Build();
    }
}

// ============================================================
// Part 4: Builder + 不可变对象 — 构建完就锁定
// 知识点 21: 建造者模式的高级用法
// ============================================================

/// <summary>
/// 知识点 22: ImmutableConfig — 不可变配置对象
/// 一旦构建完成就不能修改 → 适合游戏配置/平衡数据
/// </summary>
public class ImmutableConfig
{
    public string ConfigName { get; }
    public float Value { get; }
    public bool Enabled { get; }
    public string Description { get; }

    // 构造函数是 private 的 → 只能通过 Builder 创建
    private ImmutableConfig(string name, float value, bool enabled, string desc)
    {
        ConfigName = name;
        Value = value;
        Enabled = enabled;
        Description = desc;
    }

    public void Print()
    {
        Debug.Log($"[配置] {ConfigName}: {Value} (启用:{Enabled}) — {Description}");
    }

    /// <summary>
    /// 知识点 23: Builder 作为内部类 — 只有它能访问 private 构造函数
    /// </summary>
    public class Builder
    {
        private string _name;
        private float _value;
        private bool _enabled = true;
        private string _desc = "";

        public Builder(string name) { _name = name; }

        public Builder SetValue(float v) { _value = v; return this; }
        public Builder SetEnabled(bool e) { _enabled = e; return this; }
        public Builder SetDescription(string d) { _desc = d; return this; }

        public ImmutableConfig Build()
        {
            // 可以在这里做验证
            if (string.IsNullOrEmpty(_name))
                throw new ArgumentException("配置名不能为空!");

            Debug.Log($"[不可变建造] ✅ {_name} 构建完成(只读)");
            return new ImmutableConfig(_name, _value, _enabled, _desc);
        }
    }
}

// ============================================================
// Part 5: 建造者 vs 工厂 — 经典对比
// 知识点 24: 两者的区别和各自适用场景
// ============================================================
//
// 工厂方法 (Day 9):       建造者模式 (Day 12):
// ─────────────────────  ─────────────────────
// 一步创建                 分步创建
// 返回产品                 逐步组装,最后 Build()
// 关注"创建哪种对象"        关注"怎么一步步构建"
// 产品接口统一即可          产品构建过程可能完全不同
// 简单工厂一个 switch       建造者链式调用 N 步
// 举例: 根据类型创敌人       举例: 打造一把定制武器
//
// 💡 一句话记忆:
//   工厂 = 点菜 ("来份牛排")
//   建造者 = DIY ("面包选全麦, 肉饼选牛肉, 加芝士, 不要洋葱...")

// ============================================================
// Part 6: 演示脚本
// ============================================================

public class BuilderDemo : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RunAllDemos());
    }

    private IEnumerator RunAllDemos()
    {
        Debug.Log("╔══════════════════════════════════════════════╗");
        Debug.Log("║  每日脚本学习 Day 12 — 建造者模式           ║");
        Debug.Log("║  Builder Pattern Demo                       ║");
        Debug.Log("╚══════════════════════════════════════════════╝\n");

        // --- 演示 1: 武器建造 ---
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("⚔️ 演示 1: 武器建造者 — 铁匠铺打造武器");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoWeaponBuilder();
        yield return null;

        // --- 演示 2: 流式角色创建 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🧙 演示 2: 流式建造者 — 创建 RPG 角色");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoFluentBuilder();
        yield return null;

        // --- 演示 3: UI 弹窗 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("💬 演示 3: UI弹窗建造 — 各种对话框一键生成");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoDialogBuilder();
        yield return null;

        // --- 演示 4: 不可变配置 ---
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🔒 演示 4: 不可变对象 — 构建完就锁定");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoImmutableConfig();
        yield return null;

        // --- 总结 ---
        Debug.Log("\n╔══════════════════════════════════════════════╗");
        Debug.Log("║  🎉 所有演示完成!                          ║");
        Debug.Log("╚══════════════════════════════════════════════╝");
        PrintSummary();
    }

    private void DemoWeaponBuilder()
    {
        // 传统方式 (反例):
        // var sword = new Weapon("新手剑", 15, 1.0f, 0.05f, 1.5f, null, 0, new List<string>(), "白色", 1);
        // ↑ 参数太多, 顺序难记, 可读性差

        // 建造者方式:
        var swordBuilder = new SwordBuilder("新手铁剑");
        var smith = new WeaponSmith(swordBuilder);
        Weapon noobSword = smith.CraftBeginnerSword();
        noobSword.PrintStats();

        Debug.Log("");

        // 打造传说武器
        var legendaryBuilder = new SwordBuilder("焚天·炎龙之牙");
        smith.SwitchBuilder(legendaryBuilder);
        Weapon legendary = smith.CraftFlameLegendary();
        legendary.PrintStats();

        Debug.Log("");

        // 换弓建造者, 同样的 Director, 不同的产品
        var bowBuilder = new BowBuilder("暗影猎手");
        smith.SwitchBuilder(bowBuilder);
        Weapon bow = smith.CraftShadowHunterBow();
        bow.PrintStats();

        Debug.Log("\n💡 关键点:");
        Debug.Log("  同一套构建流程 (WeaponSmith) → 换 Builder → 不同产品");
        Debug.Log("  SwordBuilder 和 BowBuilder 各自决定属性怎么设");
    }

    private void DemoFluentBuilder()
    {
        // 链式调用创建自定义角色
        Character hero = new FluentCharacterBuilder()
            .WithName("勇者阿历克斯")
            .AsClass("⚔️勇者")
            .WithHP(180).WithMP(50)
            .WithAttack(50).WithDefense(45).WithSpeed(30)
            .WithSkill("勇者之剑").WithSkill("光之护盾")
            .WithWeapon("圣剑").WithArmor("勇者之铠")
            .WithAppearance("蓝色勇者服+银色披风")
            .Build();
        hero.PrintSheet();

        Debug.Log("");

        // 用预设快速创建
        Character mage = CharacterPresets.CreateMage("梅林大法师");
        mage.PrintSheet();

        Debug.Log("");

        Character ranger = CharacterPresets.CreateRanger("精灵游侠·莉亚");
        ranger.PrintSheet();

        Debug.Log("\n💡 关键点:");
        Debug.Log("  链式调用 = 可读性极高, 每个步骤一目了然");
        Debug.Log("  预设方法 = 常用模板一键生成");
        Debug.Log("  对比 new Character(一堆参数) → Builder 完胜");
    }

    private void DemoDialogBuilder()
    {
        // 删除确认弹窗
        DialogData deleteDialog = DialogPresets.ConfirmDelete("传说级火焰剑", () =>
        {
            Debug.Log("  → 用户确认删除!");
        });
        deleteDialog.Execute();

        Debug.Log("");

        // 提示弹窗
        DialogData tipDialog = DialogPresets.ShowTip("背包已满，请清理后再战斗!");
        tipDialog.Execute();

        Debug.Log("");

        // 奖励弹窗
        DialogData rewardDialog = DialogPresets.ShowReward("龙鳞护甲", 1);
        rewardDialog.Execute();

        Debug.Log("\n💡 关键点:");
        Debug.Log("  不同弹窗 = 不同的 Builder 组合");
        Debug.Log("  预设方法 = UI设计师和程序员之间的\"配方\"");
        Debug.Log("  实际项目: Build() 返回后调 UIManager.Show(data)");
    }

    private void DemoImmutableConfig()
    {
        var balanceConfig = new ImmutableConfig.Builder("怪物血量倍率")
            .SetValue(1.5f)
            .SetDescription("全局怪物血量倍率，1.0=正常")
            .Build();
        balanceConfig.Print();

        var featureToggle = new ImmutableConfig.Builder("新年活动开关")
            .SetEnabled(true)
            .SetDescription("控制2026新年活动是否开启")
            .Build();
        featureToggle.Print();

        Debug.Log("\n💡 关键点:");
        Debug.Log("  构建完成后无法修改 → 避免运行时误改配置");
        Debug.Log("  Builder 内部类访问 private 构造函数 → 强制用 Builder");
        Debug.Log("  适合: 游戏配置/平衡数据/功能开关");
    }

    private void PrintSummary()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📋 建造者模式 — 知识点总结");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        Debug.Log("\n🧩 四要素:");
        Debug.Log("  1. Product (产品)    — 要构建的复杂对象");
        Debug.Log("  2. Builder (建造者)  — 定义构建步骤的接口");
        Debug.Log("  3. ConcreteBuilder   — 具体建造者, 实现每个步骤");
        Debug.Log("  4. Director (指挥者) — 封装预设流程 (可选)");

        Debug.Log("\n📐 两种常用写法:");
        Debug.Log("  传统建造者: Director + Builder → 适合固定流程");
        Debug.Log("  流式建造者: builder.SetA().SetB().Build() → 适合灵活组合");
        Debug.Log("  C# 中流式建造者更流行 (链式调用可读性高)");

        Debug.Log("\n🎮 Unity 中的典型应用:");
        Debug.Log("  1. 武器/装备生成 — 随机词缀组合 (暗黑Like)");
        Debug.Log("  2. 角色捏脸 — 一步步选种族/职业/外观/属性");
        Debug.Log("  3. UI 弹窗构建 — 标题/内容/按钮/回调灵活组合");
        Debug.Log("  4. 关卡生成 — 地图大小/怪物密度/Boss类型/宝箱数量");
        Debug.Log("  5. 技能编辑器 — 伤害/范围/特效/冷却/消耗 逐项配置");
        Debug.Log("  6. 游戏配置表 — 不可变配置对象, 构建完就锁定");

        Debug.Log("\n🆚 建造者 vs 工厂方法:");
        Debug.Log("  工厂:  一步创建, 关注\"创建哪种对象\"");
        Debug.Log("          EnemyFactory.Create('Slime') → 返回 Slime");
        Debug.Log("  建造者: 分步构建, 关注\"怎么一步步搭\"");
        Debug.Log("          builder.SetHP(200).SetMP(50).AddSkill('火球').Build()");

        Debug.Log("\n📐 与已学模式的关系:");
        Debug.Log("  - 工厂方法 (Day9): 工厂一步创建 → 建造者分步组装 (互补)");
        Debug.Log("  - 外观     (Day11): 外观简化调用 → 建造者简化构建");
        Debug.Log("  - 装饰器   (Day7):  装饰器运行时增强 → 建造者构建时配置");
        Debug.Log("  - 模板方法 (Day8):  Build() 本身就是一个模板方法!");
        Debug.Log("                      (构建步骤固定, 具体设值由子类决定)");

        Debug.Log("\n💡 一句话总结:");
        Debug.Log("  \"建造者模式就像在 Subway 点三明治 —");
        Debug.Log("   选面包→选肉→选蔬菜→选酱料 → 最后'Build'出一个三明治。");
        Debug.Log("   不用记构造函数第7个参数是什么, 一步一步来, 清晰明了。\"");
    }
}

// ============================================================
// 知识点总结 — 建造者模式的精髓
// ============================================================
//
// ✅ 什么时候用建造者?
//   1. 构造函数参数太多 (>4个) → Builder 分步设置
//   2. 对象需要多种配置组合 → 比写10个构造函数重载好
//   3. 构建过程需要验证 → Build() 中统一校验
//   4. 需要构建不可变对象 → Builder 是唯一入口
//   5. 同一个构建流程, 不同"配方" → Director + Builder
//
// ❌ 什么时候不用?
//   1. 对象很简单 (2-3个属性) → 直接 new 或简单工厂
//   2. 构建步骤没有变化 → 工厂方法就够了
//   3. 团队成员觉得 Builder 过度设计 → 可读性 > 模式纯度
//
// 🎮 游戏开发真实场景:
//   1. 暗黑Like的装备生成: 基底 + 前缀 + 后缀 + 镶嵌 → Builder 完美
//   2. 捏脸系统: 性别→脸型→发型→眼睛→嘴巴→身材 → 标准 Builder
//   3. 关卡编辑器: 地图→敌人配置→道具→Boss→胜利条件 → Builder
//   4. Buff 系统: 类型→数值→持续时间→叠加层数→图标 → Builder
//
// 💡 一句话总结:
//   "建造者 = 分步骤把对象'搭'出来, 每一步都看得见。
//    工厂 = '给我一个XX', 建造者 = '我要这样的XX: ...'。
//    参数多了用建造者, 对象简单用工厂。"
// ============================================================
