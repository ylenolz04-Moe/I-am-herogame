// ============================================================
// 每日脚本学习 Day 14 — 2026-07-21
// 主题: C# 观察者模式 (Observer Pattern)
// 适用: Unity 游戏开发 · 事件驱动 · UI更新 · 成就系统 · 解耦通信
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// ============================================================
// 知识点 1: 观察者模式核心思想 — "你订阅, 我通知"
//
//   Subject (主题/被观察者) ──► 持有 Observer 列表
//                                   │
//                           当状态变化时
//                                   │
//                                   ▼
//                              Notify() 遍历所有 Observer
//                                   │
//                     ┌─────────────┼─────────────┐
//                     ▼             ▼             ▼
//                ObserverA     ObserverB     ObserverC
//                (更新UI)      (播放音效)    (记录成就)
//
// 现实类比: 微信公众号 —
//   你关注(Subscribe)了一个公众号(Subject)
//   公众号发新文章时, 你自动收到推送(Notify)
//   你取关(Unsubscribe)后, 就不再收到推送
//   公众号不需要知道你是谁, 只管群发
// ============================================================

// ============================================================
// Part 1: 经典观察者 — 手写 IObserver / ISubject 接口
// 知识点 2: 理解观察者的底层原理
// ============================================================

/// <summary>
/// 知识点 3: IObserver<T> — 观察者接口
/// 泛型 T 是通知时传递的数据类型
/// </summary>
public interface IMyObserver<T>
{
    void OnNotified(T data);
}

/// <summary>
/// 知识点 4: ISubject<T> — 被观察者(主题)接口
/// </summary>
public interface IMySubject<T>
{
    void Subscribe(IMyObserver<T> observer);
    void Unsubscribe(IMyObserver<T> observer);
    void Notify(T data);
}

// ============================================================
// Part 2: 实战案例 — 玩家血量变化通知系统
// 知识点 5: 血量一变, 多个系统自动响应
// ============================================================

/// <summary>
/// 知识点 6: PlayerHealthData — 通知时携带的数据包
/// </summary>
public struct PlayerHealthData
{
    public float currentHp;
    public float maxHp;
    public float previousHp;
    public float changeAmount;  // 正=治疗, 负=受伤
    public string source;       // 伤害来源

    public float HpPercent => maxHp > 0 ? currentHp / maxHp : 0;
    public bool IsHeal => changeAmount > 0;
    public bool IsDamage => changeAmount < 0;

    public override string ToString()
    {
        string type = IsHeal ? "治疗" : (IsDamage ? "受伤" : "无变化");
        return $"HP:{currentHp}/{maxHp} ({HpPercent:P0}) | {type} {Mathf.Abs(changeAmount)} | 来源:{source}";
    }
}

/// <summary>
/// 知识点 7: PlayerHealthSubject — 玩家血量(被观察者)
/// 核心: 维护观察者列表, 血量变化时逐一通知
/// </summary>
public class PlayerHealthSubject : IMySubject<PlayerHealthData>
{
    // 知识点 8: 用 List 存所有观察者 (也可以用 HashSet 防重复)
    private List<IMyObserver<PlayerHealthData>> _observers
        = new List<IMyObserver<PlayerHealthData>>();

    private float _hp;
    private float _maxHp;

    public float HP => _hp;
    public float MaxHP => _maxHp;
    public float HpPercent => _maxHp > 0 ? _hp / _maxHp : 0;

    public PlayerHealthSubject(float maxHp)
    {
        _maxHp = maxHp;
        _hp = maxHp;
    }

    public void Subscribe(IMyObserver<PlayerHealthData> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
            Debug.Log($"[血量主题] ➕ 新观察者订阅: {observer.GetType().Name} (当前共 {_observers.Count} 个)");
        }
    }

    public void Unsubscribe(IMyObserver<PlayerHealthData> observer)
    {
        if (_observers.Remove(observer))
        {
            Debug.Log($"[血量主题] ➖ 观察者取消订阅: {observer.GetType().Name} (剩余 {_observers.Count} 个)");
        }
    }

    public void Notify(PlayerHealthData data)
    {
        // 知识点 9: 遍历通知 — 每个观察者都收到消息
        Debug.Log($"[血量主题] 📢 通知 {_observers.Count} 个观察者: {data}");
        for (int i = _observers.Count - 1; i >= 0; i--) // 倒序遍历, 防止移除时索引错乱
        {
            _observers[i].OnNotified(data);
        }
    }

    /// <summary>
    /// 知识点 10: 状态改变 → 触发通知 (核心流程)
    /// </summary>
    public void TakeDamage(float amount, string source = "unknown")
    {
        float previousHp = _hp;
        _hp = Mathf.Max(0, _hp - amount);
        var data = new PlayerHealthData
        {
            currentHp = _hp,
            maxHp = _maxHp,
            previousHp = previousHp,
            changeAmount = -amount,
            source = source
        };
        Notify(data);

        if (_hp <= 0)
            Debug.Log($"[血量主题] 💀 玩家死亡!");
    }

    public void Heal(float amount, string source = "potion")
    {
        float previousHp = _hp;
        _hp = Mathf.Min(_maxHp, _hp + amount);
        var data = new PlayerHealthData
        {
            currentHp = _hp,
            maxHp = _maxHp,
            previousHp = previousHp,
            changeAmount = amount,
            source = source
        };
        Notify(data);
    }
}

// ============================================================
// Part 3: 具体观察者 — 各系统对血量变化的响应
// 知识点 11: 每个观察者只关心自己需要做的事
// ============================================================

/// <summary>
/// 知识点 12: HpBarObserver — UI血条更新
/// </summary>
public class HpBarObserver : IMyObserver<PlayerHealthData>
{
    private string _barName;

    public HpBarObserver(string barName) { _barName = barName; }

    public void OnNotified(PlayerHealthData data)
    {
        // 血量变化 → 更新血条
        Debug.Log($"[血条UI:{_barName}] 🩸 HP条: {data.HpPercent:P0} " +
                  $"[{'█' * (int)(data.HpPercent * 20)}{'░' * (int)((1 - data.HpPercent) * 20)}]");

        if (data.HpPercent < 0.3f)
            Debug.Log($"[血条UI:{_barName}] 🔴 低血量警告! ({data.HpPercent:P0})");
    }
}

/// <summary>
/// 知识点 13: ScreenEffectObserver — 屏幕特效 (红屏/恢复)
/// </summary>
public class ScreenEffectObserver : IMyObserver<PlayerHealthData>
{
    private bool _isLowHp;

    public void OnNotified(PlayerHealthData data)
    {
        if (data.HpPercent < 0.3f && !_isLowHp)
        {
            _isLowHp = true;
            Debug.Log($"[屏幕特效] 🔴 启用红屏警告效果 (血量低于30%)");
        }
        else if (data.HpPercent >= 0.3f && _isLowHp)
        {
            _isLowHp = false;
            Debug.Log($"[屏幕特效] ✅ 关闭红屏效果 (血量恢复)");
        }
    }
}

/// <summary>
/// 知识点 14: AchievementObserver — 成就系统监听
/// </summary>
public class AchievementObserver : IMyObserver<PlayerHealthData>
{
    private HashSet<string> _unlockedAchievements = new HashSet<string>();
    private float _totalDamageReceived;
    private int _nearDeathCount;

    public void OnNotified(PlayerHealthData data)
    {
        if (data.IsDamage)
        {
            _totalDamageReceived += Mathf.Abs(data.changeAmount);

            // 成就: 单次受到大伤害
            if (Mathf.Abs(data.changeAmount) >= 30
                && _unlockedAchievements.Add("big_damage"))
            {
                Debug.Log($"[成就系统] 🏆 成就解锁: '迎头痛击' — 单次受到30+伤害!");
            }

            // 成就: 丝血逃生
            if (data.HpPercent < 0.1f && data.HpPercent > 0
                && _unlockedAchievements.Add("near_death"))
            {
                _nearDeathCount++;
                Debug.Log($"[成就系统] 🏆 成就解锁: '绝处逢生' — 血量低于10%存活! (第{_nearDeathCount}次)");
            }
        }

        // 成就: 累计承伤
        if (_totalDamageReceived >= 200 && _unlockedAchievements.Add("tank_200"))
        {
            Debug.Log($"[成就系统] 🏆 成就解锁: '钢铁之躯' — 累计承受200+伤害!");
        }
    }
}

/// <summary>
/// 知识点 15: AudioObserver — 根据血量变化播放音效
/// </summary>
public class AudioObserver : IMyObserver<PlayerHealthData>
{
    private float _lastHeartbeatHpPercent = 1f;

    public void OnNotified(PlayerHealthData data)
    {
        if (data.IsDamage)
        {
            Debug.Log($"[音效系统] 🔊 播放: 受伤音效 (来自{data.source})");
        }
        else if (data.IsHeal)
        {
            Debug.Log($"[音效系统] 🔊 播放: 治疗音效");
        }

        // 心跳音效: 血量越低, 心跳越快
        if (data.HpPercent < 0.3f)
        {
            float interval = Mathf.Lerp(0.3f, 1.5f, data.HpPercent / 0.3f);
            Debug.Log($"[音效系统] 💓 播放心跳音效 (间隔{interval:F1}s, 血量{data.HpPercent:P0})");
        }
    }
}

/// <summary>
/// 知识点 16: CombatLogObserver — 战斗日志记录
/// </summary>
public class CombatLogObserver : IMyObserver<PlayerHealthData>
{
    private List<string> _log = new List<string>();
    private int _entryCount;

    public void OnNotified(PlayerHealthData data)
    {
        _entryCount++;
        string entry = $"[{_entryCount}] {data}";
        _log.Add(entry);
        Debug.Log($"[战斗日志] 📋 {entry}");
    }

    public void PrintFullLog()
    {
        Debug.Log("\n=== 📜 完整战斗记录 ===");
        foreach (var entry in _log)
            Debug.Log($"  {entry}");
        Debug.Log("=======================\n");
    }
}

// ============================================================
// Part 4: C# event 关键字 — 语言内置的观察者
// 知识点 17: 不用手写接口, 用 C# 的 event/delegate
// ============================================================

/// <summary>
/// 知识点 18: 用 event Action<T> 代替手写接口
/// 优点: 代码更少, 类型安全, 编译器帮你管理订阅/取消
/// </summary>
public class EventDrivenPlayer
{
    // 知识点 19: event 关键字 — 只有声明者才能 Invoke (防止外部乱触发)
    public event Action<PlayerHealthData> OnHealthChanged;
    public event Action OnPlayerDied;
    public event Action<float> OnLowHealthWarning; // 传当前血量百分比

    private float _hp;
    private float _maxHp;
    private string _name;

    public float HP => _hp;
    public float HpPercent => _maxHp > 0 ? _hp / _maxHp : 0;

    public EventDrivenPlayer(string name, float maxHp)
    {
        _name = name;
        _maxHp = maxHp;
        _hp = maxHp;
    }

    public void TakeDamage(float amount, string source = "unknown")
    {
        float previousHp = _hp;
        _hp = Mathf.Max(0, _hp - amount);

        var data = new PlayerHealthData
        {
            currentHp = _hp,
            maxHp = _maxHp,
            previousHp = previousHp,
            changeAmount = -amount,
            source = source
        };

        // 知识点 20: 用 ?.Invoke 安全调用 — 没有订阅者也不会报错
        OnHealthChanged?.Invoke(data);

        if (_hp <= 0)
        {
            OnPlayerDied?.Invoke();
        }
        else if (HpPercent < 0.3f)
        {
            OnLowHealthWarning?.Invoke(HpPercent);
        }
    }

    public void Heal(float amount)
    {
        float previousHp = _hp;
        _hp = Mathf.Min(_maxHp, _hp + amount);
        OnHealthChanged?.Invoke(new PlayerHealthData
        {
            currentHp = _hp,
            maxHp = _maxHp,
            previousHp = previousHp,
            changeAmount = amount,
            source = "heal"
        });
    }
}

/// <summary>
/// 知识点 21: EventObserverExample — 演示 C# event 的订阅方式
/// </summary>
public class EventObserverExample : MonoBehaviour
{
    private EventDrivenPlayer _player;
    // 知识点 22: 存引用以便取消订阅 (防止内存泄漏!)
    private Action<PlayerHealthData> _onHpChanged;
    private Action _onDied;
    private Action<float> _onLowHp;

    private void Start()
    {
        _player = new EventDrivenPlayer("勇者", 100);

        // 知识点 23: 订阅方式 — lambda / 方法引用 / 匿名委托
        // 方式1: lambda (最常用)
        _onHpChanged = (data) =>
        {
            Debug.Log($"[Event模式] HP变化: {data.currentHp}/{data.maxHp}");
        };
        _player.OnHealthChanged += _onHpChanged;

        // 方式2: 方法引用
        _player.OnHealthChanged += HandleHealthChanged_Detailed;

        // 方式3: 匿名委托
        _onDied = () => Debug.Log("[Event模式] 💀 玩家死亡 — 显示GameOver界面");
        _player.OnPlayerDied += _onDied;

        _onLowHp = (pct) => Debug.Log($"[Event模式] ⚠️ 低血量警告: {pct:P0}");
        _player.OnLowHealthWarning += _onLowHp;
    }

    private void HandleHealthChanged_Detailed(PlayerHealthData data)
    {
        Debug.Log($"[Event模式-详细] 伤害来源:{data.source} | 变动:{data.changeAmount:+0;-#}");
    }

    private void OnDestroy()
    {
        // 知识点 24: 取消订阅 — 忘记的话会导致内存泄漏!
        if (_player != null)
        {
            _player.OnHealthChanged -= _onHpChanged;
            _player.OnHealthChanged -= HandleHealthChanged_Detailed;
            _player.OnPlayerDied -= _onDied;
            _player.OnLowHealthWarning -= _onLowHp;
        }
        Debug.Log("[Event模式] 🧹 已清理所有订阅");
    }
}

// ============================================================
// Part 5: UnityEvent — Unity 原生的观察者
// 知识点 25: 可在 Inspector 面板拖拽绑定, 策划友好!
// ============================================================

/// <summary>
/// 知识点 26: 可序列化的 UnityEvent
/// UnityEvent 继承自 UnityEventBase, 可被 Unity 序列化
/// 在 Inspector 中能看到 "+" "-" 按钮来添加/移除监听
/// </summary>
[System.Serializable]
public class HealthChangedEvent : UnityEvent<PlayerHealthData> { }

[System.Serializable]
public class PlayerDiedEvent : UnityEvent { }

[System.Serializable]
public class LowHealthEvent : UnityEvent<float> { }

/// <summary>
/// 知识点 27: UnityEventDrivenPlayer — 使用 UnityEvent 的玩家类
/// 关键区别: UnityEvent 可序列化 → 能在 Inspector 里拖拽绑定!
/// </summary>
public class UnityEventDrivenPlayer : MonoBehaviour
{
    // 知识点 28: public UnityEvent 在 Inspector 中可见可编辑
    public HealthChangedEvent OnHealthChanged;
    public PlayerDiedEvent OnPlayerDied;
    public LowHealthEvent OnLowHealthWarning;

    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _hp;

    public float HP => _hp;
    public float HpPercent => _maxHp > 0 ? _hp / _maxHp : 0;

    private void Awake()
    {
        _hp = _maxHp;
        // 知识点 29: 在代码中也安全 — UnityEvent 有自己的 null 检查
    }

    [ContextMenu("测试: 受到 15 点伤害")]
    public void TestTakeDamage15()
    {
        TakeDamage(15, "DebugMenu");
    }

    [ContextMenu("测试: 受到 50 点伤害")]
    public void TestTakeDamage50()
    {
        TakeDamage(50, "DebugMenu");
    }

    [ContextMenu("测试: 治疗 30 点")]
    public void TestHeal30()
    {
        Heal(30);
    }

    public void TakeDamage(float amount, string source = "unknown")
    {
        float previousHp = _hp;
        _hp = Mathf.Max(0, _hp - amount);
        var data = new PlayerHealthData
        {
            currentHp = _hp,
            maxHp = _maxHp,
            previousHp = previousHp,
            changeAmount = -amount,
            source = source
        };

        // UnityEvent.Invoke — 即使没有监听也不会报错
        OnHealthChanged?.Invoke(data);

        if (_hp <= 0)
        {
            Debug.Log($"[UnityEvent] 💀 {gameObject.name} 死亡!");
            OnPlayerDied?.Invoke();
        }
        else if (HpPercent < 0.3f)
        {
            OnLowHealthWarning?.Invoke(HpPercent);
        }
    }

    public void Heal(float amount)
    {
        float previousHp = _hp;
        _hp = Mathf.Min(_maxHp, _hp + amount);
        OnHealthChanged?.Invoke(new PlayerHealthData
        {
            currentHp = _hp,
            maxHp = _maxHp,
            previousHp = previousHp,
            changeAmount = amount,
            source = "heal"
        });
    }
}

// ============================================================
// Part 6: 事件总线 (EventBus) — 全局解耦的观察者
// 知识点 30: 发送者和接收者互不认识, 通过"频道"通信
// ============================================================

/// <summary>
/// 知识点 31: GameEvent — 游戏中可能发生的事件类型
/// </summary>
public enum GameEventType
{
    PlayerDamaged,
    PlayerHealed,
    PlayerDied,
    EnemyKilled,
    ItemCollected,
    LevelComplete,
    BossDefeated,
    CoinPickedUp,
    AchievementUnlocked,
    GamePaused,
    GameResumed,
}

/// <summary>
/// 知识点 32: GameEventData — 事件携带的数据 (用对象初始化器灵活赋值)
/// </summary>
public class GameEventData
{
    public GameEventType EventType;
    public GameObject Source;
    public GameObject Target;
    public float FloatValue;
    public int IntValue;
    public string StringValue;
    public Vector3 Position;
    public object CustomData; // 万能兜底

    public override string ToString()
    {
        return $"[事件] {EventType} | 来源:{Source?.name} | 目标:{Target?.name} | " +
               $"float:{FloatValue} int:{IntValue} str:{StringValue}";
    }
}

/// <summary>
/// 知识点 33: EventBus — 全局事件总线 (单例 + 观察者)
/// 任何系统都可以发事件 / 听事件, 完全解耦
/// </summary>
public class EventBus
{
    // 知识点 34: 每个事件类型对应一个 Action 委托链
    private Dictionary<GameEventType, Action<GameEventData>> _listeners
        = new Dictionary<GameEventType, Action<GameEventData>>();

    private static EventBus _instance;
    public static EventBus Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EventBus();
            return _instance;
        }
    }

    /// <summary>
    /// 知识点 35: Subscribe — 订阅某类事件
    /// 返回 Action 引用, 方便后续取消订阅
    /// </summary>
    public void Subscribe(GameEventType type, Action<GameEventData> listener)
    {
        if (_listeners.ContainsKey(type))
            _listeners[type] += listener;
        else
            _listeners[type] = listener;

        Debug.Log($"[EventBus] ➕ 订阅 {type} (当前监听数: {GetListenerCount(type)})");
    }

    /// <summary>
    /// 知识点 36: Unsubscribe — 取消订阅 (必须! 否则内存泄漏)
    /// </summary>
    public void Unsubscribe(GameEventType type, Action<GameEventData> listener)
    {
        if (_listeners.ContainsKey(type))
        {
            _listeners[type] -= listener;
            Debug.Log($"[EventBus] ➖ 取消订阅 {type} (剩余监听数: {GetListenerCount(type)})");
        }
    }

    /// <summary>
    /// 知识点 37: Publish — 发布事件, 所有订阅者收到通知
    /// </summary>
    public void Publish(GameEventType type, GameEventData data = null)
    {
        if (data == null)
            data = new GameEventData();
        data.EventType = type;

        Debug.Log($"[EventBus] 📢 发布事件: {type}");

        if (_listeners.TryGetValue(type, out Action<GameEventData> handler))
        {
            // 知识点 38: GetInvocationList — 逐个调用, 某个出错不影响其他
            foreach (Action<GameEventData> singleHandler in handler.GetInvocationList())
            {
                try
                {
                    singleHandler?.Invoke(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] ❌ 处理事件 {type} 时出错: {ex.Message}");
                    // 不中断, 继续通知其他监听者
                }
            }
        }
    }

    /// <summary>
    /// 知识点 39: Publish — 便捷重载, 自动构建数据包
    /// </summary>
    public void Publish(GameEventType type, GameObject source = null, GameObject target = null,
        float floatVal = 0, int intVal = 0, string strVal = "")
    {
        Publish(type, new GameEventData
        {
            Source = source,
            Target = target,
            FloatValue = floatVal,
            IntValue = intVal,
            StringValue = strVal
        });
    }

    public int GetListenerCount(GameEventType type)
    {
        if (!_listeners.ContainsKey(type)) return 0;
        var handler = _listeners[type];
        return handler != null ? handler.GetInvocationList().Length : 0;
    }

    /// <summary>
    /// 知识点 40: 清空所有订阅 (切换场景时调用)
    /// </summary>
    public void ClearAll()
    {
        _listeners.Clear();
        Debug.Log("[EventBus] 🧹 已清空所有事件订阅");
    }
}

/// <summary>
/// 知识点 41: EventBus 使用示例 — 各系统互不认识, 只通过 EventBus 通信
/// </summary>
public class EventBusExample : MonoBehaviour
{
    // 存储自己的回调引用, 用于取消订阅
    private Action<GameEventData> _onPlayerDamaged;
    private Action<GameEventData> _onEnemyKilled;
    private Action<GameEventData> _onCoinPicked;
    private Action<GameEventData> _onBossDefeated;

    private int _killCount;
    private int _coinCount;

    private void Start()
    {
        // 知识点 42: 订阅感兴趣的事件
        _onPlayerDamaged = OnPlayerDamagedHandler;
        _onEnemyKilled = OnEnemyKilledHandler;
        _onCoinPicked = OnCoinPickedHandler;
        _onBossDefeated = OnBossDefeatedHandler;

        EventBus.Instance.Subscribe(GameEventType.PlayerDamaged, _onPlayerDamaged);
        EventBus.Instance.Subscribe(GameEventType.EnemyKilled, _onEnemyKilled);
        EventBus.Instance.Subscribe(GameEventType.CoinPickedUp, _onCoinPicked);
        EventBus.Instance.Subscribe(GameEventType.BossDefeated, _onBossDefeated);

        Debug.Log("[EventBus示例] ✅ 已订阅 4 种事件");
    }

    private void OnPlayerDamagedHandler(GameEventData data)
    {
        Debug.Log($"[EventBus示例-接收] 玩家受伤! 伤害量:{data.FloatValue} 来源:{data.Source?.name}");
    }

    private void OnEnemyKilledHandler(GameEventData data)
    {
        _killCount++;
        Debug.Log($"[EventBus示例-接收] 🔪 击杀敌人! (累计击杀: {_killCount}) 敌人:{data.Source?.name}");

        // 击杀5个敌人 → 发成就事件
        if (_killCount >= 5)
            EventBus.Instance.Publish(GameEventType.AchievementUnlocked, strVal: "first_blood_x5");
    }

    private void OnCoinPickedHandler(GameEventData data)
    {
        _coinCount += data.IntValue;
        Debug.Log($"[EventBus示例-接收] 🪙 拾取金币! +{data.IntValue} (累计: {_coinCount})");
    }

    private void OnBossDefeatedHandler(GameEventData data)
    {
        Debug.Log($"[EventBus示例-接收] 👑 Boss被击败! 名称:{data.StringValue}");
        // Boss死了 → 自动发关卡完成事件
        EventBus.Instance.Publish(GameEventType.LevelComplete, strVal: "level_1");
    }

    private void OnDestroy()
    {
        // 知识点 43: 取消订阅 — 防止内存泄漏和空引用!
        EventBus.Instance.Unsubscribe(GameEventType.PlayerDamaged, _onPlayerDamaged);
        EventBus.Instance.Unsubscribe(GameEventType.EnemyKilled, _onEnemyKilled);
        EventBus.Instance.Unsubscribe(GameEventType.CoinPickedUp, _onCoinPicked);
        EventBus.Instance.Unsubscribe(GameEventType.BossDefeated, _onBossDefeated);
        Debug.Log("[EventBus示例] 🧹 已取消所有订阅");
    }
}

// ============================================================
// Part 7: 弱引用观察者 — 防止内存泄漏的高级技巧
// 知识点 44: 观察者最常见的坑: 忘记取消订阅 → 内存泄漏
// ============================================================

/// <summary>
/// 知识点 45: WeakObserverWrapper — 用弱引用包装观察者
/// 即使观察者被 GC 回收, Subject 也不会持有强引用
/// </summary>
public class WeakObserverWrapper<T> : IMyObserver<T>
{
    // 知识点 46: WeakReference — 不阻止 GC 回收目标对象
    private WeakReference<IMyObserver<T>> _weakRef;
    private string _debugName;

    public bool IsAlive => _weakRef != null && _weakRef.TryGetTarget(out _);

    public WeakObserverWrapper(IMyObserver<T> observer, string debugName = "")
    {
        _weakRef = new WeakReference<IMyObserver<T>>(observer);
        _debugName = debugName;
    }

    public void OnNotified(T data)
    {
        // 知识点 47: TryGetTarget — 尝试获取真实引用
        if (_weakRef.TryGetTarget(out IMyObserver<T> observer))
        {
            observer.OnNotified(data);
        }
        else
        {
            Debug.Log($"[弱引用] ⚠️ 观察者 {_debugName} 已被GC回收, 跳过通知");
        }
    }
}

/// <summary>
/// 知识点 48: SafeSubject — 带自动清理的 Subject
/// 清理已死亡的 WeakObserverWrapper (被 GC 回收的)
/// </summary>
public class SafeSubject<T>
{
    private List<WeakObserverWrapper<T>> _observers = new List<WeakObserverWrapper<T>>();

    public void Subscribe(IMyObserver<T> observer, string name = "")
    {
        _observers.Add(new WeakObserverWrapper<T>(observer, name));
        Debug.Log($"[安全Subject] ➕ 订阅 (共 {_observers.Count} 个)");
    }

    public void Notify(T data)
    {
        int deadCount = 0;
        for (int i = _observers.Count - 1; i >= 0; i--)
        {
            if (_observers[i].IsAlive)
            {
                _observers[i].OnNotified(data);
            }
            else
            {
                _observers.RemoveAt(i);
                deadCount++;
            }
        }
        if (deadCount > 0)
            Debug.Log($"[安全Subject] 🧹 自动清理了 {deadCount} 个已死亡的观察者");
    }

    public void CleanupDeadObservers()
    {
        int removed = _observers.RemoveAll(w => !w.IsAlive);
        if (removed > 0)
            Debug.Log($"[安全Subject] 🧹 清理 {removed} 个已死亡观察者, 剩余 {_observers.Count}");
    }
}

// ============================================================
// Part 8: 观察者模式的三层境界 + 对比总结
// 知识点 49: 从小项目到大项目, 观察者怎么演进
// ============================================================
//
// 第一层: 直接调用 (最原始)
//   player.TakeDamage() 里直接写:
//     hpBar.UpdateUI();
//     audioManager.PlayHurtSound();
//     achievementManager.Check();
//   ❌ 问题: 加新功能就要改 TakeDamage, 违反开闭原则
//
// 第二层: C# event (小项目首选)
//   public event Action<float> OnHpChanged;
//   ✅ 优点: 简单, 类型安全, 编译器帮你管理
//   ⚠️ 注意: 必须手动 -= 取消订阅!
//
// 第三层: EventBus (中大型项目)
//   全局事件总线, 系统间零耦合
//   ✅ 优点: 完全解耦, 容易加新系统
//   ⚠️ 注意: 性能开销, 调试困难 (谁发了什么事件?)
//
// 第四层: ScriptableObject Event Channel (Unity 推荐)
//   [CreateAssetMenu] GameEventSO → 在 Inspector 中拖拽连线
//   ✅ 优点: 可视化, 策划可配, 场景间解耦
//   ⚠️ 注意: 需要额外资产文件

// ============================================================
// Part 9: 演示脚本 — 综合演示
// ============================================================

public class ObserverDemo : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RunAllDemos());
    }

    private IEnumerator RunAllDemos()
    {
        Debug.Log("╔══════════════════════════════════════════════╗");
        Debug.Log("║  每日脚本学习 Day 14 — 观察者模式           ║");
        Debug.Log("║  Observer Pattern Demo                      ║");
        Debug.Log("╚══════════════════════════════════════════════╝\n");

        yield return StartCoroutine(DemoClassicObserver());
        yield return new WaitForSeconds(0.1f);

        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("⚡ 演示 2: C# event 模式");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoEventPattern();

        yield return new WaitForSeconds(0.1f);
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🔌 演示 3: EventBus 全局事件总线");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoEventBus();

        yield return new WaitForSeconds(0.1f);
        Debug.Log("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🛡️ 演示 4: 弱引用安全观察者");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        DemoWeakReference();

        yield return new WaitForSeconds(0.1f);
        PrintSummary();
    }

    private IEnumerator DemoClassicObserver()
    {
        Debug.Log("━━━ 演示 1: 经典观察者 — 血量变化 → 多系统响应 ━━━");

        // 创建被观察者
        var playerHealth = new PlayerHealthSubject(100f);

        // 创建各系统观察者
        var hpBar = new HpBarObserver("主HUD");
        var screenEffect = new ScreenEffectObserver();
        var achievement = new AchievementObserver();
        var audio = new AudioObserver();
        var combatLog = new CombatLogObserver();

        // 订阅 (各系统"关注"血量变化)
        playerHealth.Subscribe(hpBar);
        playerHealth.Subscribe(screenEffect);
        playerHealth.Subscribe(achievement);
        playerHealth.Subscribe(audio);
        playerHealth.Subscribe(combatLog);

        Debug.Log("✅ 5个系统已订阅血量变化\n");

        // 模拟战斗
        Debug.Log("--- 战斗开始! ---");
        playerHealth.TakeDamage(15, "哥布林");
        yield return new WaitForSeconds(0.05f);

        playerHealth.TakeDamage(35, "Boss重击"); // 触发"迎头痛击"成就 + 低血量
        yield return new WaitForSeconds(0.05f);

        playerHealth.Heal(20, "治疗药水");
        yield return new WaitForSeconds(0.05f);

        playerHealth.TakeDamage(40, "火龙吐息"); // 丝血!
        yield return new WaitForSeconds(0.05f);

        playerHealth.TakeDamage(40, "火龙爪击"); // 致死

        Debug.Log("\n--- 战斗日志系统可以查看完整记录 ---");

        Debug.Log("\n💡 关键点:");
        Debug.Log("  Subject(血量) 不知道有哪些 Observer");
        Debug.Log("  Observer 各干各的, 互不影响");
        Debug.Log("  加新系统(比如震屏)只需新增一个 Observer, 不改 Subject");
    }

    private void DemoEventPattern()
    {
        Debug.Log("--- C# event 方式 ---");

        var player = new EventDrivenPlayer("英雄林克", 100);

        // 订阅: 一行代码搞定
        player.OnHealthChanged += (data) =>
            Debug.Log($"  [Lambda订阅] HP → {data.currentHp} (变动:{data.changeAmount:+0;-#})");

        player.OnPlayerDied += () =>
            Debug.Log("  [死亡订阅] 触发 GameOver 流程...");

        player.OnLowHealthWarning += (pct) =>
            Debug.Log($"  [低血量订阅] ⚠️ 警告! 当前 {pct:P0}");

        // 触发事件
        player.TakeDamage(25, "骷髅兵");
        player.TakeDamage(50, "暗黑法师");
        player.Heal(30);
        player.TakeDamage(60, "Boss陨石术");

        Debug.Log("\n💡 关键点:");
        Debug.Log("  event 关键字 → 只有声明者才能 Invoke (安全)");
        Debug.Log("  ?.Invoke() → 没有订阅者也不报错");
        Debug.Log("  必须 -= 取消订阅, 否则内存泄漏!");
        Debug.Log("  UI/音效/特效 在各自的 OnEnable/OnDisable 中 +=/-=");
    }

    private void DemoEventBus()
    {
        Debug.Log("--- 模拟: 敌人死亡 → 多个系统响应 (完全解耦) ---");

        // 模拟: 成就系统监听
        Action<GameEventData> achievementHandler = (data) =>
        {
            Debug.Log($"  [成就系统] 📊 收到事件: {data.EventType} | 来源:{data.Source?.name}");
        };
        EventBus.Instance.Subscribe(GameEventType.EnemyKilled, achievementHandler);

        // 模拟: UI系统监听
        Action<GameEventData> uiHandler = (data) =>
        {
            Debug.Log($"  [UI系统] 🖼️ 收到事件: {data.EventType} | 更新击杀计数");
        };
        EventBus.Instance.Subscribe(GameEventType.EnemyKilled, uiHandler);

        // 模拟: 金币系统监听
        Action<GameEventData> coinHandler = (data) =>
        {
            Debug.Log($"  [金币系统] 🪙 收到事件: {data.EventType} | +{data.IntValue} 金币");
        };
        EventBus.Instance.Subscribe(GameEventType.EnemyKilled, coinHandler);

        // 模拟战斗: 玩家杀死敌人
        Debug.Log("\n🎮 玩家击杀 '哥布林' ...");
        var enemyObj = new GameObject("Goblin_Enemy");
        EventBus.Instance.Publish(GameEventType.EnemyKilled,
            source: enemyObj, floatVal: 50, intVal: 10);

        Debug.Log("\n🎮 玩家击杀 'Boss巨龙' ...");
        var bossObj = new GameObject("Dragon_Boss");
        EventBus.Instance.Publish(GameEventType.BossDefeated,
            source: bossObj, strVal: "Ancient Dragon");

        // 清理
        EventBus.Instance.Unsubscribe(GameEventType.EnemyKilled, achievementHandler);
        EventBus.Instance.Unsubscribe(GameEventType.EnemyKilled, uiHandler);
        EventBus.Instance.Unsubscribe(GameEventType.EnemyKilled, coinHandler);
        Destroy(enemyObj);
        Destroy(bossObj);

        Debug.Log("\n💡 关键点:");
        Debug.Log("  发送者(Publish) 和 接收者(Subscribe) 互不认识");
        Debug.Log("  加新系统 = 加新的 Subscribe, 不用改原有代码");
        Debug.Log("  适合: 成就/统计/埋点/音效/特效 等横切关注点");
    }

    private void DemoWeakReference()
    {
        Debug.Log("--- 弱引用观察者: 自动忽略已销毁的观察者 ---");

        var safeSubject = new SafeSubject<string>();

        // 创建观察者
        var observer1 = new SimpleStringObserver("观察者A");
        var observer2 = new SimpleStringObserver("观察者B");
        var observer3 = new SimpleStringObserver("观察者C");

        safeSubject.Subscribe(observer1, "A");
        safeSubject.Subscribe(observer2, "B");
        safeSubject.Subscribe(observer3, "C");

        safeSubject.Notify("第一次通知 (全部收到)");

        // 模拟: observer2 被销毁 (设 null)
        Debug.Log("\n--- 观察者B 被销毁 (null) ---");
        observer2 = null;
        System.GC.Collect(); // 强制 GC (仅演示, 实际项目不要手调)
        System.GC.WaitForPendingFinalizers();

        Debug.Log("\n--- 第二次通知 (B 不应该收到) ---");
        safeSubject.Notify("第二次通知 (B已销毁)");

        // 手动清理
        safeSubject.CleanupDeadObservers();

        Debug.Log("\n💡 关键点:");
        Debug.Log("  弱引用 = 不阻止 GC 回收对象");
        Debug.Log("  观察者被销毁后, Subject 自动跳过");
        Debug.Log("  实际项目: 场景切换时观察者被 Destroy → 不会空引用报错");
    }

    private void PrintSummary()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("📋 观察者模式 — 知识点总结");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        Debug.Log("\n🧩 观察者模式的四种实现:");
        Debug.Log("  1. 经典手写: ISubject + IObserver 接口 → 理解原理");
        Debug.Log("  2. C# event: event Action<T> → 代码简洁, 类型安全");
        Debug.Log("  3. UnityEvent: Inspector 可视 → 策划/美术可拖拽");
        Debug.Log("  4. EventBus: 全局总线 → 系统间完全解耦");

        Debug.Log("\n🎮 Unity 中的典型应用:");
        Debug.Log("  1. 血量变化 → UI更新 + 屏幕特效 + 音效 + 成就");
        Debug.Log("  2. 按钮点击 → Button.onClick (底层就是观察者)");
        Debug.Log("  3. 场景加载 → 进度条更新 + 提示切换 + 后台加载");
        Debug.Log("  4. 敌人死亡 → 掉落道具 + 经验结算 + 计数器 + 成就");
        Debug.Log("  5. 任务系统 → 任务进度更新 + UI刷新 + 奖励发放");
        Debug.Log("  6. 输入系统 → 多个系统响应同一按键 (Jump/Rewired)");

        Debug.Log("\n🆚 三种实现对比:");

        Debug.Log("  手写接口:");
        Debug.Log("    ✅ 完全可控, 可加过滤/异步/优先级");
        Debug.Log("    ❌ 代码多, 需要维护列表");

        Debug.Log("  C# event:");
        Debug.Log("    ✅ 简洁, 编译器检查, 性能最好");
        Debug.Log("    ❌ 不支持序列化, Inspector不可见");

        Debug.Log("  UnityEvent:");
        Debug.Log("    ✅ Inspector可视化, 策划友好");
        Debug.Log("    ❌ 性能略低, 只能挂 MonoBehaviour");

        Debug.Log("  EventBus:");
        Debug.Log("    ✅ 完全解耦, 跨系统通信");
        Debug.Log("    ❌ 全局状态, 难调试, 容易滥用");

        Debug.Log("\n⚠️ 常见陷阱:");
        Debug.Log("  1. 内存泄漏: 忘记取消订阅 → 观察者无法被GC");
        Debug.Log("  2. 空引用: 观察者被 Destroy 但还在列表里");
        Debug.Log("  3. 循环通知: A通知B, B通知C, C又通知A → 死循环");
        Debug.Log("  4. 过度解耦: 所有通信都用EventBus → 代码逻辑不可追踪");
        Debug.Log("  5. 性能问题: 一帧发几百个事件, 每个有几十个监听 → CPU暴涨");

        Debug.Log("\n💡 选择建议:");
        Debug.Log("  小项目/原型: C# event 足够");
        Debug.Log("  中等项目: UnityEvent (配置性) + C# event (代码)");
        Debug.Log("  大项目: EventBus 做跨系统通信 + SO Event Channel 做配置");
        Debug.Log("  永远记住: 用最简方案, 不要过度设计!");

        Debug.Log("\n📐 与已学模式的关系:");
        Debug.Log("  - 命令模式 (Day4): 命令是'封装请求', 观察者是'通知变化'");
        Debug.Log("    命令可被观察 → 命令执行完通知观察者(撤销栈更新UI)");
        Debug.Log("  - 策略模式 (Day5): 策略可被观察 → 切换策略时通知UI刷新");
        Debug.Log("  - 状态机   (Day3): 状态切换时通知观察者 → 进入Boss状态→UI变化");
        Debug.Log("  - 代理模式 (Day13): 代理可以做Subject → 数据加载完通知观察者");
        Debug.Log("  - 外观模式 (Day11): 外观内部用EventBus协调子系统");
        Debug.Log("  - 事件系统 (Day2): 自定义事件系统本质就是观察者模式");

        Debug.Log("\n💡 一句话总结:");
        Debug.Log("  \"观察者模式 = 微信公众号订阅机制 —");
        Debug.Log("   你关注(Subscribe)了一个公众号(Subject),");
        Debug.Log("   它发文时自动推送给你(Notify)。");
        Debug.Log("   公众号不需要知道你是谁,");
        Debug.Log("   你也可以随时取关(Unsubscribe)。");
        Debug.Log("   游戏里的 HP条/成就/音效 都是'关注'了血量变化的粉丝。\"");
    }
}

// ============================================================
// 辅助类: SimpleStringObserver (演示用)
// ============================================================
public class SimpleStringObserver : IMyObserver<string>
{
    private string _name;
    public SimpleStringObserver(string name) { _name = name; }
    public void OnNotified(string data)
    {
        Debug.Log($"  [观察者:{_name}] 📩 收到通知: {data}");
    }
}

// ============================================================
// 知识点总结 — 观察者模式的精髓
// ============================================================
//
// ✅ 什么时候用观察者?
//   1. 一对多依赖 — 一个对象变化, 多个对象自动更新
//      (血量变化 → UI/音效/成就/特效)
//   2. 需要解耦 — 发送者和接收者不应该互相依赖
//      (敌人死亡系统不需要知道成就系统的存在)
//   3. 事件驱动架构 — 游戏逻辑通过事件串联
//      (捡道具 → 发事件 → 背包/UI/音效各自响应)
//   4. 动态订阅 — 某些系统运行时才决定是否监听
//      (Boss战开始时, "Boss特定UI"才订阅血量)
//
// ❌ 什么时候不用?
//   1. 简单的一对一调用 → 直接调用更清晰
//      (player.Jump() 直接调用 animator.SetTrigger("Jump"))
//   2. 确定不会扩展的通知 → 过度设计
//      (只有一个接收者, 且永远不会有第二个)
//   3. 需要同步返回值的场景 → 观察者是"通知后不管"
//      (需要确认扣血成功才能继续 → 用返回值/回调)
//
// 🎮 游戏开发中最实用的场景:
//   1. 血量/魔法/体力 → UI + 特效 + 音效 + 成就
//   2. 击杀/死亡 → 计分板 + 任务进度 + 统计 + 回放
//   3. 道具拾取 → 背包更新 + 快捷栏 + 模型切换 + 教学引导
//   4. 场景切换 → 加载界面 + 背景音乐 + GC清理 + 数据存档
//   5. 网络消息 → 多个UI面板各自监听需要的消息类型
//   6. 成就系统 → 不听游戏逻辑, 只听事件, 完全解耦
//
// 💡 一句话总结:
//   "观察者 = '我有新消息了, 关注我的人自己看着办' —
//    我不需要知道有多少人关注我,
//    也不需要知道他们收到消息后干什么。
//    我只负责: 1) 记录谁关注了我 2) 变化时挨个通知。
//    这就是'发布-订阅' 的精髓。"
// ============================================================
