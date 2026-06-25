// ============================================================
// 每日脚本学习 Day 3 — 2026-06-25
// 主题: C# 事件系统与观察者模式 (Event System & Observer Pattern)
// 适用: Unity 游戏开发 · 解耦游戏系统间的通信
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// ============================================================
// 知识点 1: 观察者模式核心思想 — "发布-订阅"解耦
// 发送方 (Publisher) 不直接依赖接收方 (Subscriber)
// 双方只依赖共同的事件通道 → 模块间零耦合
// ============================================================

// ============================================================
// 层级 1: ScriptableObject 事件通道 — 现代 Unity 推荐方案
// 知识点 2: 使用 ScriptableObject 作为事件通道
// 优势: 可在 Inspector 中拖拽配置，无需代码引用，策划友好
// ============================================================

/// <summary>
/// 知识点 3: 泛型事件通道基类
/// 继承 ScriptableObject → 可作为 .asset 资源文件存在于项目中
/// 不同事件类型只需继承此类，无需重复编写监听/触发逻辑
/// </summary>
/// <typeparam name="T">事件数据类型 (struct 可避免装箱)</typeparam>
public abstract class GameEventChannel<T> : ScriptableObject
{
    // 知识点 4: 使用 C# event 关键字而非 UnityEvent
    // C# event 性能优于 UnityEvent（无序列化开销，无反射调用）
    // 但 UnityEvent 可在 Inspector 中配置 → 两者各有适用场景
    private event Action<T> _onEvent;

    /// <summary>注册监听</summary>
    public void Register(Action<T> listener)
    {
        _onEvent += listener;
    }

    /// <summary>移除监听（必须在 OnDestroy/OnDisable 中调用，防止内存泄漏）</summary>
    public void Unregister(Action<T> listener)
    {
        _onEvent -= listener;
    }

    /// <summary>触发事件</summary>
    public void Raise(T data)
    {
        // 知识点 5: 使用 ?.Invoke 空条件运算符
        // 当没有任何监听者时不会抛 NullReferenceException
        _onEvent?.Invoke(data);
    }
}

// ============================================================
// 知识点 6: 无参事件通道 — 适用于不需要传递数据的通知
// 例如: 游戏暂停、关卡加载完成
// ============================================================

[CreateAssetMenu(menuName = "Events/Void Event Channel", fileName = "VoidEventChannel")]
public class VoidEventChannel : ScriptableObject
{
    private event Action _onEvent;

    public void Register(Action listener) => _onEvent += listener;
    public void Unregister(Action listener) => _onEvent -= listener;
    public void Raise()
    {
        _onEvent?.Invoke();
    }
}

// ============================================================
// 知识点 7: 具体游戏事件定义 — struct 而非 class 避免堆分配
// ============================================================

/// <summary>玩家得分变化事件</summary>
public struct ScoreChangedEvent
{
    public int NewScore;
    public int Delta;         // 本次变化量（+10 表示捡到樱桃）
    public string Source;     // 来源: "Cherry" / "Enemy" / "Bonus"
}

/// <summary>玩家死亡事件</summary>
public struct PlayerDiedEvent
{
    public Vector3 DeathPosition;
    public string KillerName; // 死亡原因: "Spike" / "Saw" / "Fall"
}

/// <summary>关卡通关事件</summary>
public struct LevelCompleteEvent
{
    public string LevelName;
    public float TimeTaken;
    public int CherriesCollected;
    public int TotalCherries;
}

/// <summary>游戏状态变化事件</summary>
public struct GameStateEvent
{
    // 知识点 8: 使用 enum 而非 string 表示状态 — 类型安全 + 编译器检查
    public GameState NewState;
    public GameState PreviousState;
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    LevelComplete
}

// ============================================================
// 知识点 9: 具体事件通道 — 一行代码创建一个新事件类型
// ============================================================

[CreateAssetMenu(menuName = "Events/Score Changed Event", fileName = "ScoreChangedEventChannel")]
public class ScoreChangedEventChannel : GameEventChannel<ScoreChangedEvent> { }

[CreateAssetMenu(menuName = "Events/Player Died Event", fileName = "PlayerDiedEventChannel")]
public class PlayerDiedEventChannel : GameEventChannel<PlayerDiedEvent> { }

[CreateAssetMenu(menuName = "Events/Level Complete Event", fileName = "LevelCompleteEventChannel")]
public class LevelCompleteEventChannel : GameEventChannel<LevelCompleteEvent> { }

[CreateAssetMenu(menuName = "Events/Game State Event", fileName = "GameStateEventChannel")]
public class GameStateEventChannel : GameEventChannel<GameStateEvent> { }

// ============================================================
// 层级 2: 全局 EventBus — 适用于代码驱动的快速通信
// 知识点 10: EventBus 单例 — 集中管理所有事件类型
// 优势: 无需创建 .asset 文件，纯代码即可使用
// 劣势: 全局依赖，不方便单元测试
// ============================================================

/// <summary>
/// 全局事件总线。线程不安全（Unity 主线程专用）。
/// 适用于: 快速原型、Game Jam、小规模项目
/// </summary>
public class EventBus
{
    // 知识点 11: 使用 Dictionary<Type, Delegate> 存储不同类型的事件处理器
    private readonly Dictionary<Type, Delegate> _handlers = new();

    private static EventBus _instance;
    public static EventBus Instance => _instance ??= new EventBus();

    /// <summary>注册泛型事件监听</summary>
    public void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var existing))
        {
            _handlers[type] = Delegate.Combine(existing, handler);
        }
        else
        {
            _handlers[type] = handler;
        }
    }

    /// <summary>移除泛型事件监听</summary>
    public void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var existing))
        {
            var result = Delegate.Remove(existing, handler);
            if (result == null)
                _handlers.Remove(type);
            else
                _handlers[type] = result;
        }
    }

    /// <summary>触发事件 — 所有注册的监听者都会收到通知</summary>
    public void Publish<T>(T eventData)
    {
        if (_handlers.TryGetValue(typeof(T), out var handler))
        {
            (handler as Action<T>)?.Invoke(eventData);
        }
    }

    /// <summary>清空所有事件监听（场景切换时调用，防止残留引用）</summary>
    public void Clear()
    {
        _handlers.Clear();
    }
}

// ============================================================
// 知识点 12: MonoBehaviour 监听器基类 — 自动管理生命周期
// 继承此类即可自动在 OnEnable/OnDisable 中注册/注销事件
// ============================================================

/// <summary>
/// 事件监听器基类。自动处理注册/注销，防止忘记 Unregister 导致的内存泄漏。
/// 知识点: 模板方法模式 — 子类只需实现 OnEventRaised，基类管理生命周期。
/// </summary>
public abstract class EventListener<T> : MonoBehaviour
{
    [SerializeField] protected GameEventChannel<T> _eventChannel;

    // 知识点 13: OnEnable/OnDisable 是 Unity 中管理事件注册的标准时机
    protected virtual void OnEnable()
    {
        if (_eventChannel != null)
            _eventChannel.Register(OnEventRaised);
    }

    protected virtual void OnDisable()
    {
        if (_eventChannel != null)
            _eventChannel.Unregister(OnEventRaised);
    }

    /// <summary>子类重写此方法处理事件</summary>
    protected abstract void OnEventRaised(T data);
}

// ============================================================
// 知识点 14: 具体监听器示例
// ============================================================

/// <summary>分数变化监听器 — 更新 UI 和播放音效</summary>
public class ScoreEventListener : EventListener<ScoreChangedEvent>
{
    [SerializeField] private UnityEngine.UI.Text _scoreText;
    [SerializeField] private AudioSource _collectSound;

    protected override void OnEventRaised(ScoreChangedEvent data)
    {
        _scoreText.text = $"Score: {data.NewScore}";
        _collectSound?.Play();
    }
}

// ============================================================
// 知识点 15: 发布者示例 — 在游戏逻辑中触发事件
// ============================================================

/// <summary>
/// 示例: 重构后的樱桃收集器 — 使用事件系统解耦
/// 不再直接调用 UIManager.UpdateScore() 和 AudioManager.PlaySound()
/// 只发布事件，谁关心谁来订阅
/// </summary>
public class CherryCollector_EventDriven : MonoBehaviour
{
    [Header("事件通道 (在 Inspector 中拖入 .asset 文件)")]
    [SerializeField] private ScoreChangedEventChannel _scoreEventChannel;
    [SerializeField] private VoidEventChannel _cherryPickupSoundChannel;

    private int _currentScore;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Cherry")) return;

        Destroy(other.gameObject);
        _currentScore += 10;

        // 知识点 16: 发布事件而非直接调用 — 收集者不需要知道谁会响应
        _scoreEventChannel?.Raise(new ScoreChangedEvent
        {
            NewScore = _currentScore,
            Delta = 10,
            Source = "Cherry"
        });

        _cherryPickupSoundChannel?.Raise();

        // 对比旧方式（紧耦合）:
        // UIManager.Instance.UpdateScore(_currentScore);  ← 依赖具体类
        // AudioManager.Instance.PlayCollectSound();        ← 依赖具体类
        // EffectManager.Instance.SpawnCollectEffect(pos);  ← 每次加新功能都要改这里
    }
}

// ============================================================
// 知识点 17: 对比 — 三种事件实现方式的取舍
// ============================================================
/*
| 方式                | 适用场景               | 优势                     | 劣势                     |
|---------------------|----------------------|--------------------------|--------------------------|
| C# event 关键字      | 一对多通知，同一程序集  | 性能最优，无 GC           | 订阅者必须持有发布者引用   |
| UnityEvent          | Inspector 可配置      | 策划/美术友好             | 有 GC 分配，性能较差      |
| ScriptableObject 通道 | 跨场景、跨预制体通信   | 零代码引用，低耦合         | 需创建 .asset 文件       |
| EventBus 单例        | 快速原型、全局事件     | 使用方便，不需 Inspector  | 全局依赖，难测试          |
*/

// ============================================================
// 知识点 18: 内存泄漏防护 — 游戏开发中事件系统的最大陷阱
// ============================================================
/*
常见泄漏场景:
  场景 A 中的 Enemy 订阅了全局事件 → 切换到场景 B → Enemy 已 Destroy
  → 但事件处理器仍持有 Enemy 的引用 → Enemy 无法被 GC 回收

防护措施:
  1. 始终在 OnDisable/OnDestroy 中 Unregister
  2. 使用 EventListener<T> 基类自动管理生命周期
  3. 场景切换时调用 EventBus.Clear()
  4. 避免在静态事件中引用场景对象（或用 WeakReference）
*/

// ============================================================
// 知识点 19: 实战 — I Am Hero 游戏中使用事件系统后的架构对比
// ============================================================
/*
【重构前 - 紧耦合】
  PlayerMoment.cs  →  直接调用 UIManager, AudioManager, EffectManager...
  问题: 加一个"成就系统"需要改 PlayerMoment 代码

【重构后 - 事件驱动】
  PlayerMoment.cs  →  只发布事件 (ScoreChanged, PlayerDied...)
  UIManager.cs     ↘
  AudioManager.cs   → 各自独立订阅感兴趣的事件
  AchievementSystem ↗
  优势: 加"成就系统"只需新建一个订阅者，不改任何现有代码
*/

// ============================================================
// 知识点 20: 完整使用流程 (在 Unity 中的操作步骤)
// ============================================================
/*
步骤 1: 创建事件通道资源
  - 右键 Project 窗口 → Create → Events → Score Changed Event
  - 命名为 "ScoreChangedEventChannel.asset"

步骤 2: 在发布者中引用
  - 在 CherryCollector 脚本上拖入 ScoreChangedEventChannel.asset
  - 调用 _scoreChannel.Raise(new ScoreChangedEvent { ... });

步骤 3: 创建监听者
  - 新建空 GameObject，挂载 ScoreEventListener 脚本
  - 拖入相同的事件通道资源和 UI Text

步骤 4: 运行 → 收集樱桃 → 分数自动更新！
  - 发布者和监听者完全解耦，互不知道对方存在
*/
