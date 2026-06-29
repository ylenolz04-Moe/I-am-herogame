// ============================================================
// 每日脚本学习 Day 6 — 2026-06-29
// 主题: C# 策略模式 (Strategy Pattern)
// 适用: Unity 游戏开发 · AI 行为 · 战斗系统 · 移动系统 · 技能系统
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// ============================================================
// 知识点 1: 策略模式核心思想 — "定义算法族，分别封装，让它们可以互换"
//
//   上下文(Context) ──→ [策略接口 IStrategy] ←── 具体策略 A
//                                           ←── 具体策略 B
//                                           ←── 具体策略 C
//
// 把"做一件事的方式"从"谁来做"中解耦出来
// 策略是一个可插拔的算法，运行时随时更换
// ============================================================

// ============================================================
// Part 1: 策略接口 — 所有策略的统一契约
// 知识点 2: 策略接口只定义"做什么"，不定义"怎么做"
// ============================================================

/// <summary>
/// 知识点 3: IStrategy 接口 — 策略模式的最小契约
///   Execute() — 执行策略逻辑
/// 每个具体策略实现一种不同的算法/行为
/// </summary>
public interface IStrategy<TContext>
{
    void Execute(TContext context);
}

// ============================================================
// Part 2: 游戏 AI 行为策略 — 敌人战斗 AI
// 知识点 4: 每种 AI 行为是一个独立策略类
// 敌人可以在不同策略间切换 → 改变行为模式
// ============================================================

/// <summary>
/// 知识点 5: 敌人状态数据 — 策略间共享的上下文
/// 策略只关心算法，数据由外部传入
/// </summary>
public class EnemyContext
{
    public Transform Self;
    public Transform Target;
    public float Health;
    public float MaxHealth;
    public float AttackRange;
    public float MoveSpeed;
    public float AttackCooldown;
    public float LastAttackTime;

    // 知识点 6: 方便的策略判断辅助属性
    public float HealthPercent => Health / MaxHealth;
    public float DistanceToTarget => Vector3.Distance(Self.position, Target.position);
    public bool InAttackRange => DistanceToTarget <= AttackRange;
    public bool CanAttack => Time.time - LastAttackTime >= AttackCooldown;
}

// ============================================================
// Part 2a: 具体 AI 策略
// 知识点 7: 每个策略类实现一种独立的 AI 行为
// ============================================================

/// <summary>
/// 知识点 8: 激进攻击策略 — 不顾一切冲上去打
/// 行为: 主动接近目标 → 进入攻击范围 → 持续攻击
/// 适用: 近战敌人、Boss 阶段 3、狂暴状态
/// </summary>
public class AggressiveAI : IStrategy<EnemyContext>
{
    public void Execute(EnemyContext ctx)
    {
        // 没有目标 → 待机
        if (ctx.Target == null) return;

        float dist = ctx.DistanceToTarget;

        if (dist > ctx.AttackRange)
        {
            // 冲向目标
            Vector3 dir = (ctx.Target.position - ctx.Self.position).normalized;
            ctx.Self.position += dir * ctx.MoveSpeed * Time.deltaTime;
            Debug.Log("[激进AI] 冲向目标!");
        }
        else if (ctx.CanAttack)
        {
            // 在范围内 → 攻击
            ctx.LastAttackTime = Time.time;
            Debug.Log("[激进AI] 近战攻击! 造成 30 伤害");
        }
    }
}

/// <summary>
/// 知识点 9: 防御策略 — 保持距离，低血量时撤退
/// 行为: 远离目标 → 保持安全距离 → 仅在绝对安全时攻击
/// 适用: 远程敌人、低血量撤退、Boss 阶段 1
/// </summary>
public class DefensiveAI : IStrategy<EnemyContext>
{
    private readonly float _safeDistance = 5f;

    public void Execute(EnemyContext ctx)
    {
        if (ctx.Target == null) return;

        float dist = ctx.DistanceToTarget;

        if (dist < _safeDistance)
        {
            // 太近了 → 后退
            Vector3 dir = (ctx.Self.position - ctx.Target.position).normalized;
            ctx.Self.position += dir * ctx.MoveSpeed * Time.deltaTime;
            Debug.Log("[防御AI] 保持距离! 后退中...");
        }
        else if (dist < ctx.AttackRange && ctx.CanAttack)
        {
            // 安全距离内攻击
            ctx.LastAttackTime = Time.time;
            Debug.Log("[防御AI] 远程射击! 造成 15 伤害");
        }
    }
}

/// <summary>
/// 知识点 10: 巡逻策略 — 在预设路点间移动，发现敌人后切换策略
/// 行为: 沿路点移动 → 检测到目标 → 通知切换到战斗策略
/// </summary>
public class PatrolAI : IStrategy<EnemyContext>
{
    private readonly Vector3[] _waypoints;
    private int _currentWaypoint;
    private readonly float _detectionRange;

    public PatrolAI(Vector3[] waypoints, float detectionRange = 8f)
    {
        _waypoints = waypoints;
        _detectionRange = detectionRange;
    }

    public void Execute(EnemyContext ctx)
    {
        if (_waypoints == null || _waypoints.Length == 0) return;

        // 检测目标 — 如果发现敌人，通知切换策略
        if (ctx.Target != null && ctx.DistanceToTarget < _detectionRange)
        {
            Debug.Log($"[巡逻AI] 发现目标在 {ctx.DistanceToTarget:F1}m 内! 请求切换策略...");
            // 通过事件通知 AI 控制器切换到战斗策略
            OnTargetDetected?.Invoke(ctx);
            return;
        }

        // 移动到当前路点
        Vector3 target = _waypoints[_currentWaypoint];
        Vector3 dir = (target - ctx.Self.position).normalized;
        ctx.Self.position += dir * (ctx.MoveSpeed * 0.5f) * Time.deltaTime;

        // 到达路点时切换到下一个
        if (Vector3.Distance(ctx.Self.position, target) < 0.3f)
        {
            _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Length;
            Debug.Log($"[巡逻AI] 到达路点 {_currentWaypoint}/{_waypoints.Length}");
        }
    }

    // 知识点 11: 策略内的事件 — 通知外部切换策略
    public event Action<EnemyContext> OnTargetDetected;
}

/// <summary>
/// 知识点 12: 埋伏策略 — 静止不动，目标靠近时突袭
/// 适用: 陷阱型敌人、宝箱怪、草丛伏击
/// </summary>
public class AmbushAI : IStrategy<EnemyContext>
{
    private bool _triggered;
    private readonly float _ambushRange = 3f;
    private readonly float _ambushDamage = 50f;

    public void Execute(EnemyContext ctx)
    {
        if (_triggered || ctx.Target == null) return;

        if (ctx.DistanceToTarget < _ambushRange)
        {
            _triggered = true;
            Debug.Log($"[埋伏AI] 伏击触发! 突袭造成 {_ambushDamage} 伤害!");
            // 突袭后可以切换到激进AI继续追击
            OnAmbushTriggered?.Invoke(ctx);
        }
    }

    public event Action<EnemyContext> OnAmbushTriggered;

    // 重置埋伏状态（用于对象池回收）
    public void Reset() => _triggered = false;
}

// ============================================================
// Part 3: 上下文类 — 使用策略的"客户"
// 知识点 13: EnemyAIController — 持有策略引用，委托执行
// ============================================================

/// <summary>
/// 知识点 14: AI 控制器 — 策略模式的"上下文"
/// 职责: 持有当前策略、切换策略、执行策略
/// 配合 Day4 状态机: 状态负责"是什么" → 策略负责"怎么做"
/// </summary>
public class EnemyAIController : MonoBehaviour
{
    [Header("AI 设置")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _attackCooldown = 1.5f;
    [SerializeField] private Transform[] _patrolWaypoints;

    // 知识点 15: 当前策略 — 运行时可以随时替换
    private IStrategy<EnemyContext> _currentStrategy;
    private EnemyContext _context;

    // 知识点 16: 策略缓存 — 预创建避免 GC
    private Dictionary<string, IStrategy<EnemyContext>> _strategyCache;

    private void Awake()
    {
        _context = new EnemyContext
        {
            Self = transform,
            MaxHealth = _maxHealth,
            Health = _maxHealth,
            AttackRange = _attackRange,
            MoveSpeed = _moveSpeed,
            AttackCooldown = _attackCooldown,
        };

        // 知识点 17: 策略缓存 — 常用策略提前创建，避免每次新建
        InitializeStrategies();

        // 默认使用巡逻策略
        SwitchStrategy("patrol");
    }

    private void InitializeStrategies()
    {
        var patrolAI = new PatrolAI(
            _patrolWaypoints != null
                ? Array.ConvertAll(_patrolWaypoints, w => w.position)
                : new[] { transform.position + Vector3.right * 5f, transform.position + Vector3.left * 5f }
        );

        // 巡逻时发现目标 → 切换到激进AI
        patrolAI.OnTargetDetected += ctx => SwitchStrategy("aggressive");

        var ambushAI = new AmbushAI();
        // 埋伏触发 → 切换到激进AI追击
        ambushAI.OnAmbushTriggered += ctx => SwitchStrategy("aggressive");

        _strategyCache = new Dictionary<string, IStrategy<EnemyContext>>
        {
            { "aggressive", new AggressiveAI() },
            { "defensive",  new DefensiveAI() },
            { "patrol",     patrolAI },
            { "ambush",     ambushAI },
        };
    }

    /// <summary>
    /// 知识点 18: 切换策略 — 运行时更换行为
    /// 这就是策略模式的核心价值: 同一套数据，不同的处理方式
    /// </summary>
    public void SwitchStrategy(string name)
    {
        if (_strategyCache.TryGetValue(name, out var strategy))
        {
            _currentStrategy = strategy;
            Debug.Log($"[AI控制器] 策略切换 → {name}");
        }
        else
        {
            Debug.LogWarning($"[AI控制器] 未知策略: {name}");
        }
    }

    /// <summary>
    /// 知识点 19: 动态切换策略 — 根据血量自动调整
    /// 低血量 → 防御策略，高血量 → 激进策略
    /// </summary>
    public void EvaluateAndSwitch()
    {
        if (_context.HealthPercent < 0.3f)
        {
            SwitchStrategy("defensive");
        }
        else
        {
            SwitchStrategy("aggressive");
        }
    }

    private void Update()
    {
        if (_currentStrategy == null) return;

        // 更新上下文
        _context.Health = _maxHealth; // 简化示例
        _context.Target = FindClosestPlayer();

        // 知识点 20: 委托给当前策略 — 上下文不关心具体算法
        _currentStrategy.Execute(_context);
    }

    private Transform FindClosestPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return null;

        Transform closest = players[0].transform;
        float minDist = Vector3.Distance(transform.position, closest.position);

        for (int i = 1; i < players.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, players[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = players[i].transform;
            }
        }

        return closest;
    }
}

// ============================================================
// Part 4: 移动策略 — 不同移动方式的策略封装
// 知识点 21: 同一个接口，完全不同的实现
// ============================================================

public class MovementContext
{
    public Transform Transform;
    public Rigidbody2D Rigidbody;
    public Vector3 Target;
    public float Speed;
}

/// <summary>
/// 知识点 22: 地面移动 — 沿地面走向目标
/// </summary>
public class GroundMovement : IStrategy<MovementContext>
{
    public void Execute(MovementContext ctx)
    {
        Vector3 dir = (ctx.Target - ctx.Transform.position).normalized;
        ctx.Transform.position += dir * ctx.Speed * Time.deltaTime;
    }
}

/// <summary>
/// 知识点 23: 飞行移动 — 无视地面障碍，直接飞过去
/// </summary>
public class FlyingMovement : IStrategy<MovementContext>
{
    public void Execute(MovementContext ctx)
    {
        // 飞行可以走直线，Y 轴也可以移动
        Vector3 dir = (ctx.Target - ctx.Transform.position).normalized;
        ctx.Transform.position += dir * ctx.Speed * Time.deltaTime;
    }
}

/// <summary>
/// 知识点 24: 传送移动 — 瞬间到达目标附近
/// 适合: 闪现技能、法师传送、Boss 瞬移
/// </summary>
public class TeleportMovement : IStrategy<MovementContext>
{
    private readonly float _teleportRange = 5f;
    private readonly float _cooldown = 3f;
    private float _lastTeleportTime;

    public void Execute(MovementContext ctx)
    {
        if (Time.time - _lastTeleportTime < _cooldown) return;

        // 传送到目标附近随机位置
        Vector3 randomOffset = Random.insideUnitSphere * _teleportRange;
        randomOffset.y = 0; // 保持在地面
        ctx.Transform.position = ctx.Target + randomOffset;
        _lastTeleportTime = Time.time;
        Debug.Log("[传送移动] 闪现到目标附近!");
    }
}

/// <summary>
/// 知识点 25: 弹跳移动 — 模拟史莱姆/弹簧的移动方式
/// </summary>
public class BounceMovement : IStrategy<MovementContext>
{
    private readonly float _bounceHeight = 2f;
    private float _verticalVelocity;
    private bool _isAirborne;

    public void Execute(MovementContext ctx)
    {
        Vector3 dir = (ctx.Target - ctx.Transform.position).normalized;

        // 水平移动
        ctx.Transform.position += dir * ctx.Speed * Time.deltaTime;

        // 弹跳逻辑
        if (!_isAirborne)
        {
            _verticalVelocity = _bounceHeight;
            _isAirborne = true;
        }

        _verticalVelocity += Physics.gravity.y * Time.deltaTime;
        ctx.Transform.position += Vector3.up * (_verticalVelocity * Time.deltaTime);

        if (ctx.Transform.position.y <= 1f)
        {
            ctx.Transform.position = new Vector3(ctx.Transform.position.x, 1f, ctx.Transform.position.z);
            _isAirborne = false;
        }
    }
}

// ============================================================
// Part 5: 伤害计算策略 — 不同伤害类型的算法
// 知识点 26: 策略模式在战斗系统中的经典应用
// ============================================================

public class DamageContext
{
    public float BaseDamage;
    public float Armor;
    public float MagicResist;
    public bool IsCritical;
    public float CritMultiplier = 2f;
    public string ElementType;
}

/// <summary>
/// 知识点 27: 物理伤害 — 受护甲减免
/// </summary>
public class PhysicalDamage : IStrategy<DamageContext>
{
    public void Execute(DamageContext ctx)
    {
        // 护甲减免公式: 伤害 = 基础伤害 * (100 / (100 + 护甲))
        float damage = ctx.BaseDamage * (100f / (100f + ctx.Armor));

        if (ctx.IsCritical)
            damage *= ctx.CritMultiplier;

        Debug.Log($"[物理伤害] {ctx.BaseDamage} → 减免后 {damage:F1} (护甲: {ctx.Armor})");
    }
}

/// <summary>
/// 知识点 28: 魔法伤害 — 受魔抗减免，公式不同
/// </summary>
public class MagicDamage : IStrategy<DamageContext>
{
    public void Execute(DamageContext ctx)
    {
        // 魔抗减免: 伤害 = 基础伤害 * (1 - 魔抗 / (魔抗 + 200))
        float reduction = ctx.MagicResist / (ctx.MagicResist + 200f);
        float damage = ctx.BaseDamage * (1f - reduction);

        Debug.Log($"[魔法伤害] {ctx.BaseDamage} → 减免后 {damage:F1} (魔抗: {ctx.MagicResist})");
    }
}

/// <summary>
/// 知识点 29: 真实伤害 — 无视所有防御
/// 适合: 斩杀效果、毒药伤害、特殊技能
/// </summary>
public class TrueDamage : IStrategy<DamageContext>
{
    public void Execute(DamageContext ctx)
    {
        // 真实伤害不计算任何减免
        float damage = ctx.BaseDamage;
        Debug.Log($"[真实伤害] 造成 {damage} 伤害 (无视所有防御!)");
    }
}

/// <summary>
/// 知识点 30: 百分比伤害 — 按目标当前生命值百分比
/// 适合: Boss 血量百分比技能、中毒扣血
/// </summary>
public class PercentDamage : IStrategy<DamageContext>
{
    private readonly float _percent;

    public PercentDamage(float percent) => _percent = percent;

    public void Execute(DamageContext ctx)
    {
        float damage = ctx.BaseDamage * _percent;
        Debug.Log($"[百分比伤害] 造成 {damage:F1} 伤害 (基础 {ctx.BaseDamage} × {_percent:P0})");
    }
}

// ============================================================
// Part 6: 技能施放策略 — 不同技能的逻辑封装
// 知识点 31: 把技能也当成策略 → 技能系统 = 策略集合
// ============================================================

/// <summary>
/// 知识点 32: 技能策略接口
/// 每个技能 = 不同的策略实现
/// </summary>
public interface ISkillStrategy
{
    bool CanCast(SkillContext ctx);
    void Cast(SkillContext ctx);
    float CooldownTime { get; }
}

public class SkillContext
{
    public Transform Caster;
    public Vector3 TargetPosition;
    public float Mana;
    public float MaxMana;
}

/// <summary>
/// 知识点 33: 火球技能 — 直线投射物
/// </summary>
public class FireballSkill : ISkillStrategy
{
    public float CooldownTime => 3f;
    private readonly float _manaCost = 30f;
    private readonly float _damage = 50f;

    public bool CanCast(SkillContext ctx)
    {
        return ctx.Mana >= _manaCost;
    }

    public void Cast(SkillContext ctx)
    {
        ctx.Mana -= _manaCost;
        Debug.Log($"[火球] 向 {ctx.TargetPosition} 发射火球! 消耗 {_manaCost} MP, 造成 {_damage} 伤害");
        // 实际项目中: Object.Instantiate(fireballPrefab, caster.position, ...)
    }
}

/// <summary>
/// 知识点 34: 治疗术 — 恢复生命值
/// </summary>
public class HealSkill : ISkillStrategy
{
    public float CooldownTime => 8f;
    private readonly float _manaCost = 50f;
    private readonly float _healAmount = 40f;

    public bool CanCast(SkillContext ctx)
    {
        return ctx.Mana >= _manaCost;
    }

    public void Cast(SkillContext ctx)
    {
        ctx.Mana -= _manaCost;
        Debug.Log($"[治疗术] 恢复 {_healAmount} HP! 消耗 {_manaCost} MP");
    }
}

/// <summary>
/// 知识点 35: 范围技能 — AOE 攻击
/// </summary>
public class AOESkill : ISkillStrategy
{
    public float CooldownTime => 12f;
    private readonly float _manaCost = 80f;
    private readonly float _radius = 5f;
    private readonly float _damage = 100f;

    public bool CanCast(SkillContext ctx)
    {
        return ctx.Mana >= _manaCost;
    }

    public void Cast(SkillContext ctx)
    {
        ctx.Mana -= _manaCost;
        Debug.Log($"[AOE] 在 {ctx.TargetPosition} 释放范围攻击! 半径 {_radius}m, 造成 {_damage} 伤害");
        // 实际: Physics.OverlapSphere(targetPosition, radius) 检测范围内敌人
    }
}

// ============================================================
// Part 7: Unity 集成 — 角色技能栏系统
// 知识点 36: 把多个策略组合成技能栏
// ============================================================

/// <summary>
/// 知识点 37: 技能栏管理器 — 管理多个技能策略
/// 不同角色可以有完全不同的技能配置（不同的策略组合）
/// </summary>
public class SkillBar : MonoBehaviour
{
    // 知识点 38: 技能栏 = 策略数组
    // 战士: 猛击(物理) + 冲锋 + 斩杀(真实伤害)
    // 法师: 火球(魔法) + 暴风雪(AOE) + 闪现(传送移动)
    // 牧师: 治疗 + 圣光(魔法) + 神圣护盾
    private readonly Dictionary<KeyCode, (ISkillStrategy strategy, float lastCastTime)> _skillSlots
        = new Dictionary<KeyCode, (ISkillStrategy, float)>();

    private SkillContext _skillContext = new SkillContext();

    private void Awake()
    {
        _skillContext.Caster = transform;
        _skillContext.MaxMana = 200f;
        _skillContext.Mana = 200f;
    }

    /// <summary>
    /// 知识点 39: 绑定技能到按键 — 一行代码添加新技能
    /// </summary>
    public void BindSkill(KeyCode key, ISkillStrategy skill)
    {
        _skillSlots[key] = (skill, 0f);
        Debug.Log($"[技能栏] 绑定 {skill.GetType().Name} 到 {key}");
    }

    /// <summary>
    /// 知识点 40: 更换技能 — 运行时替换策略
    /// 就像 RPG 中更换装备/技能书
    /// </summary>
    public void ReplaceSkill(KeyCode key, ISkillStrategy newSkill)
    {
        if (_skillSlots.ContainsKey(key))
        {
            _skillSlots[key] = (newSkill, _skillSlots[key].lastCastTime);
            Debug.Log($"[技能栏] 替换 {key} 为 {newSkill.GetType().Name}");
        }
        else
        {
            BindSkill(key, newSkill);
        }
    }

    private void Update()
    {
        // 恢复法力
        _skillContext.Mana = Mathf.Min(_skillContext.MaxMana, _skillContext.Mana + 5f * Time.deltaTime);

        // 检测技能按键
        foreach (var kvp in _skillSlots)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                TryCastSkill(kvp.Key, kvp.Value.strategy);
            }
        }
    }

    private void TryCastSkill(KeyCode key, ISkillStrategy skill)
    {
        var (strategy, lastCast) = _skillSlots[key];

        // 检查冷却
        if (Time.time - lastCast < strategy.CooldownTime)
        {
            float remaining = strategy.CooldownTime - (Time.time - lastCast);
            Debug.Log($"[技能栏] {strategy.GetType().Name} 冷却中... {remaining:F1}s");
            return;
        }

        // 检查条件
        if (!strategy.CanCast(_skillContext))
        {
            Debug.Log($"[技能栏] {strategy.GetType().Name} 无法施放 (资源不足)");
            return;
        }

        // 施放
        strategy.Cast(_skillContext);
        _skillSlots[key] = (strategy, Time.time);
    }
}

// ============================================================
// Part 8: 策略 vs 状态 — 常见混淆辨析
// 知识点 41: 何时用策略，何时用状态?
// ============================================================

/*
┌──────────────────────────────────────────────────────────────┐
│  策略模式 vs 状态模式 — 配合 Day4 回顾                           │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  策略模式 (Strategy):                                         │
│  ✅ 外部选择算法 — "我让你用什么方式做"                           │
│  ✅ 策略之间通常不互相切换 — 由外部决定                          │
│  ✅ 策略不知道彼此的存在 — 彼此独立                             │
│  ✅ 关注"怎么做"同一件事的不同方式                               │
│                                                              │
│  例: 敌人AI控制器                                               │
│     SwitchStrategy("aggressive")   ← 外部决定                  │
│     SwitchStrategy("defensive")    ← 血量低时外部切换           │
│                                                              │
│  状态模式 (State):                                            │
│  ✅ 内部自动切换 — "我自己知道什么时候该变"                       │
│  ✅ 状态持有对自己的引用 — 状态知道下一个该是什么                 │
│  ✅ 状态之间有明确的转换关系 — 构成了状态图                      │
│  ✅ 关注"是什么"和"什么时候变"                                  │
│                                                              │
│  例: 角色状态机 (Day4)                                          │
│     Idle → Walk → Jump → Airborne → Land → Idle               │
│     状态内部代码: if (Input.GetKey) fsm.TransitionTo(Walk);    │
│                                                              │
│  实战组合:                                                     │
│  状态决定"当前能做什么" (Idle 时不能攻击)                        │
│  策略决定"怎么做" (攻击时用物理/魔法/真实伤害)                     │
│                                                              │
└──────────────────────────────────────────────────────────────┘
*/

/// <summary>
/// 知识点 42: 策略+状态组合示例
/// 状态管理行为大类 → 策略实现具体行为
/// </summary>
public class HybridController : MonoBehaviour
{
    // 状态 (Day4 内容): 决定当前能做什么
    private enum CharacterState { Idle, Combat, Dead }

    private CharacterState _state;

    // 策略: 决定具体怎么做
    private IStrategy<EnemyContext> _combatStrategy;
    private IStrategy<MovementContext> _movementStrategy;

    private void Update()
    {
        switch (_state)
        {
            case CharacterState.Idle:
                // 待机时使用巡逻移动策略
                _movementStrategy = new GroundMovement(); // 可用缓存优化
                break;

            case CharacterState.Combat:
                // 战斗时根据血量选择策略
                _combatStrategy = GetComponent<EnemyAIController>() != null
                    ? new AggressiveAI()
                    : new DefensiveAI();
                break;

            case CharacterState.Dead:
                // 死亡后策略全部置空
                _combatStrategy = null;
                _movementStrategy = null;
                break;
        }
    }
}

// ============================================================
// Part 9: 策略工厂 — 通过配置创建策略
// 知识点 43: 结合工厂模式，通过字符串/枚举创建策略
// ============================================================

/// <summary>
/// 知识点 44: 策略工厂 — 集中管理策略创建
/// 避免各处 new 策略对象，方便做对象池、缓存、依赖注入
/// </summary>
public static class StrategyFactory
{
    private static readonly Dictionary<string, Func<IStrategy<EnemyContext>>> _aiStrategies
        = new Dictionary<string, Func<IStrategy<EnemyContext>>>
        {
            { "aggressive", () => new AggressiveAI() },
            { "defensive",  () => new DefensiveAI() },
            { "ambush",     () => new AmbushAI() },
        };

    private static readonly Dictionary<string, Func<IStrategy<DamageContext>>> _damageStrategies
        = new Dictionary<string, Func<IStrategy<DamageContext>>>
        {
            { "physical", () => new PhysicalDamage() },
            { "magic",    () => new MagicDamage() },
            { "true",     () => new TrueDamage() },
        };

    /// <summary>
    /// 知识点 45: 创建 AI 策略
    /// </summary>
    public static IStrategy<EnemyContext> CreateAI(string type)
    {
        if (_aiStrategies.TryGetValue(type, out var factory))
            return factory();
        throw new ArgumentException($"未知 AI 策略: {type}");
    }

    /// <summary>
    /// 知识点 46: 创建伤害策略
    /// </summary>
    public static IStrategy<DamageContext> CreateDamage(string type)
    {
        if (_damageStrategies.TryGetValue(type, out var factory))
            return factory();
        throw new ArgumentException($"未知伤害策略: {type}");
    }

    /// <summary>
    /// 知识点 47: 注册新策略 — 运行时扩展
    /// 新增策略不需要改代码，注册即可
    /// </summary>
    public static void RegisterAI(string key, Func<IStrategy<EnemyContext>> factory)
    {
        _aiStrategies[key] = factory;
    }

    public static void RegisterDamage(string key, Func<IStrategy<DamageContext>> factory)
    {
        _damageStrategies[key] = factory;
    }
}

// ============================================================
// Part 10: 对比 — 策略模式重构前后
// 知识点 48: 从 if-else 地狱到可插拔策略
// ============================================================

/*
┌──────────────────────────────────────────────────────────────┐
│  重构前 (if-else 地狱)                                         │
│                                                              │
│  void EnemyAI()                                              │
│  {                                                           │
│      if (_behavior == "aggressive")                          │
│      {                                                       │
│          // 200 行近战逻辑                                     │
│          if (dist < range) Attack(); else Chase();            │
│      }                                                       │
│      else if (_behavior == "defensive")                      │
│      {                                                       │
│          // 150 行远程逻辑                                     │
│          if (dist < safeDist) Retreat(); else Shoot();        │
│      }                                                       │
│      else if (_behavior == "patrol")                         │
│      {                                                       │
│          // 100 行巡逻逻辑                                     │
│          MoveToWaypoint(); DetectEnemy();                     │
│      }                                                       │
│      // 每加一种行为就要改这个函数，越来越臃肿                    │
│  }                                                           │
│                                                              │
│  问题:                                                       │
│  • 一个文件几百行 if-else                                     │
│  • 新增行为 = 改核心代码 (违反开闭原则 OCP)                     │
│  • 无法复用行为逻辑                                           │
│  • 测试困难 — 测一种行为要加载整个类                            │
├──────────────────────────────────────────────────────────────┤
│  重构后 (策略模式)                                             │
│                                                              │
│  // 每种行为独立成一个文件                                      │
│  AggressiveAI.cs    (~30 行)                                  │
│  DefensiveAI.cs     (~25 行)                                  │
│  PatrolAI.cs        (~40 行)                                  │
│                                                              │
│  // 上下文只做一件事: 委托                                     │
│  _currentStrategy.Execute(_context);                         │
│                                                              │
│  // 新增行为 = 新建一个策略类，不改核心代码                      │
│                                                              │
│  优势:                                                       │
│  ✅ 开闭原则 (OCP) — 新增策略不改旧代码                        │
│  ✅ 单一职责 — 每个策略类职责清晰                              │
│  ✅ 可测试 — 每个策略独立单元测试                              │
│  ✅ 可复用 — 同一策略可用于不同角色                            │
│  ✅ 运行时切换 — 动态改变行为                                 │
│  ✅ 组合使用 — 策略+状态+命令形成完整架构                       │
└──────────────────────────────────────────────────────────────┘
*/

// ============================================================
// 学习总结
// ============================================================
/*
 * 今日核心收获:
 *
 * 1. 策略模式 = IStrategy 接口 + 多个具体策略类 + Context 上下文
 * 2. 核心思想: 把"算法"从"使用算法的类"中抽离 — 算法可以独立变化
 * 3. 策略与状态的区别:
 *    - 策略: 外部决定用什么方式 (你让我怎么做)
 *    - 状态: 内部自动切换 (我自己知道什么时候变)
 * 4. AI 行为 = 多个策略类 (激进/防御/巡逻/埋伏) → 同一种数据不同处理
 * 5. 伤害系统 = 策略封装不同伤害公式 (物理/魔法/真实/百分比)
 * 6. 技能系统 = 每个技能是一个策略 → 技能栏是策略数组
 * 7. 策略工厂 = 集中管理创建 → 方便做对象池和缓存
 * 8. 与 Day4 状态机组合: 状态管"是什么"→策略管"怎么做"
 *
 * 适用场景:
 *  ✅ AI 行为系统 (不同敌人使用不同行为策略)
 *  ✅ 战斗/伤害计算 (物理/魔法/真实/百分比伤害)
 *  ✅ 移动系统 (地面/飞行/游泳/传送)
 *  ✅ 技能系统 (每个技能 = 一个策略)
 *  ✅ 渲染方案切换 (高质量/性能模式策略)
 *  ✅ 存档序列化 (JSON/XML/Binary → 不同序列化策略)
 *  ✅ 难度系统 (Easy/Normal/Hard → 不同 AI 策略组合)
 *
 * 难度: ★★★☆☆ (适中，理解"算法封装"的概念即可)
 * 前置: Day2 (泛型) + Day4 (状态机)
 * 后续: 可搭配 Day5 命令模式实现可撤销的技能施放
 */
