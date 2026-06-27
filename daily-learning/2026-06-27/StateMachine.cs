// ============================================================
// 每日脚本学习 Day 4 — 2026-06-27
// 主题: C# 有限状态机模式 (Finite State Machine Pattern)
// 适用: Unity 游戏开发 · 角色状态 · AI行为 · UI流程 · 游戏流程
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 状态机核心思想 — 状态 + 转换 + 动作
//
//    [Idle] ──(按方向键)──→ [Running]
//      ↑                      │
//      │                      ↓ (按跳跃键 & 在地面)
//   [Falling] ←──(下落)── [Jumping]
//
// 每个状态封装自己的 Enter / Update / Exit 逻辑
// 转换条件独立于状态本身 → 修改一个不影响另一个
// ============================================================

// ============================================================
// Part 1: 状态接口 — 所有状态的统一契约
// 知识点 2: 接口隔离原则 — 只定义必须的方法，不关心实现细节
// ============================================================

/// <summary>
/// 知识点 3: IState 接口 — 状态的三个生命周期
///   Enter()  — 进入状态时调用一次（初始化）
///   Update() — 每帧调用（持续逻辑）
///   Exit()   — 离开状态时调用一次（清理）
/// </summary>
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}

// ============================================================
// Part 2: 转换条件 — 什么时候切换状态
// 知识点 4: 使用委托定义转换条件
// Func<bool> 是一个无参、返回 bool 的委托
// 优势: 条件可以动态计算，不硬编码在状态内部
// ============================================================

/// <summary>
/// 知识点 5: 状态转换 — 从哪个状态 → 到哪个状态，满足什么条件
/// 将转换逻辑从状态类中抽离 → 状态类更纯粹，转换规则集中管理
/// </summary>
public class StateTransition
{
    public IState From { get; }      // 源状态
    public IState To { get; }        // 目标状态
    public Func<bool> Condition { get; }  // 转换条件（返回 true 就切换）

    public StateTransition(IState from, IState to, Func<bool> condition)
    {
        From = from;
        To = to;
        Condition = condition;
    }
}

// ============================================================
// Part 3: 有限状态机核心 — 管理状态和转换
// 知识点 6: 泛型 FSM — 与具体状态类型解耦
// TKey 是状态的标识符（可以是 enum、string、int...）
// ============================================================

/// <summary>
/// 知识点 7: 有限状态机主类
/// 职责: 维护当前状态、检查转换条件、执行状态切换
/// </summary>
public class FiniteStateMachine
{
    // 知识点 8: Dictionary 存储转换规则 — O(1) 查找
    // Key = 源状态, Value = 从该状态出发的所有转换
    private readonly Dictionary<IState, List<StateTransition>> _transitions
        = new Dictionary<IState, List<StateTransition>>();

    // 知识点 9: HashSet 追踪所有状态 — 防止重复添加
    private readonly HashSet<IState> _states = new HashSet<IState>();

    public IState CurrentState { get; private set; }
    public IState PreviousState { get; private set; }

    // 知识点 10: 事件通知 — 状态切换时通知外部（配合 Day3 事件系统！）
    public event Action<IState, IState> OnStateChanged; // (旧状态, 新状态)

    /// <summary>
    /// 知识点 11: 添加状态 — 注册到 FSM 中
    /// 不添加的状态无法作为转换目标
    /// </summary>
    public void AddState(IState state)
    {
        if (_states.Add(state))  // Add 返回 false 表示已存在
        {
            _transitions[state] = new List<StateTransition>();
        }
    }

    /// <summary>
    /// 知识点 12: 添加转换规则
    /// From 和 To 都必须已经通过 AddState 注册过
    /// 同一对状态可以有多条转换（不同条件）
    /// </summary>
    public void AddTransition(IState from, IState to, Func<bool> condition)
    {
        if (!_states.Contains(from))
            throw new ArgumentException($"状态 '{from}' 尚未注册，请先调用 AddState");
        if (!_states.Contains(to))
            throw new ArgumentException($"状态 '{to}' 尚未注册，请先调用 AddState");

        _transitions[from].Add(new StateTransition(from, to, condition));
    }

    /// <summary>
    /// 知识点 13: 设置初始状态并立即进入
    /// 必须在 Update 循环开始前调用
    /// </summary>
    public void SetInitialState(IState state)
    {
        if (!_states.Contains(state))
            throw new ArgumentException($"初始状态 '{state}' 尚未注册");

        CurrentState = state;
        CurrentState.Enter();
    }

    /// <summary>
    /// 知识点 14: 每帧调用 — 执行当前状态逻辑 + 检查转换
    /// 先检查转换条件，再执行当前状态的 Update
    /// 这样可以一帧内完成状态切换
    /// </summary>
    public void Tick()
    {
        if (CurrentState == null) return;

        // 检查从当前状态出发的所有转换
        var transitions = _transitions[CurrentState];
        foreach (var transition in transitions)
        {
            if (transition.Condition())
            {
                ChangeState(transition.To);
                break;  // 知识点 15: 一帧只切换一次，避免状态乒乓
            }
        }

        // 执行当前状态的每帧逻辑
        CurrentState.Update();
    }

    /// <summary>
    /// 知识点 16: 状态切换的核心方法
    /// 顺序: Exit(旧) → 更新引用 → Enter(新) → 触发事件
    /// </summary>
    private void ChangeState(IState newState)
    {
        PreviousState = CurrentState;
        CurrentState.Exit();           // 1. 退出旧状态
        CurrentState = newState;       // 2. 切换
        CurrentState.Enter();          // 3. 进入新状态
        OnStateChanged?.Invoke(PreviousState, newState);  // 4. 通知外部
    }

    /// <summary>
    /// 知识点 17: 强制切换到指定状态（跳过条件检查）
    /// 用于外部事件强制切换，如受到伤害 → 受伤状态
    /// </summary>
    public void ForceChangeState(IState state)
    {
        if (!_states.Contains(state))
            throw new ArgumentException($"目标状态 '{state}' 尚未注册");
        if (state == CurrentState) return;

        ChangeState(state);
    }
}

// ============================================================
// Part 4: Unity 集成 — MonoBehaviour 驱动的状态机
// 知识点 18: 将纯 C# 逻辑与 Unity 生命周期桥接
// ============================================================

/// <summary>
/// 知识点 19: 可挂载到 GameObject 的状态机运行器
/// 在 Start 中初始化，在 Update 中驱动 FSM.Tick()
/// </summary>
public class StateMachineRunner : MonoBehaviour
{
    public FiniteStateMachine FSM { get; private set; } = new FiniteStateMachine();

    // 知识点 20: 使用 Action 委托让使用者配置 FSM
    // 在 Inspector 或 Awake 中可以调用此委托来注册状态和转换
    public Action<FiniteStateMachine> OnSetup;

    private void Start()
    {
        OnSetup?.Invoke(FSM);

        if (FSM.CurrentState == null)
        {
            Debug.LogError($"[{gameObject.name}] 状态机未设置初始状态！");
            enabled = false;
        }
    }

    private void Update()
    {
        FSM.Tick();
    }
}

// ============================================================
// Part 5: 实战示例 — 玩家移动状态机
// 重构自: Assets/Scripts/PlayerMoment.cs 中的 MovementState enum
// 知识点 21: 将 if-else 地狱重构为独立状态类
// ============================================================

// 知识点 22: 共享上下文 — 所有状态共享的数据
// 状态之间通过 context 访问共享数据（物理、输入、动画等）
public class PlayerContext
{
    public Rigidbody2D Rb;
    public Animator Anim;
    public SpriteRenderer Sprite;
    public BoxCollider2D Collider;
    public LayerMask GroundLayer;

    public float MoveSpeed = 7f;
    public float JumpForce = 7f;

    public float MoveInput;        // 水平输入 (-1, 0, 1)
    public bool JumpPressed;       // 是否按下了跳跃键
    public bool IsGrounded;        // 是否在地面上
}

/// <summary>
/// 知识点 23: 具体状态示例 — Idle 状态
/// 每个状态类只关注自己的行为，不关心其他状态
/// </summary>
public class PlayerIdleState : IState
{
    private readonly PlayerContext _ctx;

    public PlayerIdleState(PlayerContext ctx) { _ctx = ctx; }

    public void Enter()
    {
        _ctx.Anim.SetInteger("state", 0);  // idle 动画
    }

    public void Update()
    {
        // Idle 状态不做额外逻辑，只在等输入
    }

    public void Exit()
    {
        // 离开 idle 时无需清理
    }
}

/// <summary>
/// 知识点 24: Running 状态
/// </summary>
public class PlayerRunningState : IState
{
    private readonly PlayerContext _ctx;

    public PlayerRunningState(PlayerContext ctx) { _ctx = ctx; }

    public void Enter()
    {
        _ctx.Anim.SetInteger("state", 1);  // running 动画
    }

    public void Update()
    {
        // 根据输入控制水平移动
        _ctx.Rb.velocity = new Vector2(_ctx.MoveInput * _ctx.MoveSpeed, _ctx.Rb.velocity.y);

        // 翻转精灵朝向
        if (_ctx.MoveInput > 0) _ctx.Sprite.flipX = false;
        else if (_ctx.MoveInput < 0) _ctx.Sprite.flipX = true;
    }

    public void Exit() { }
}

/// <summary>
/// 知识点 25: Jumping 状态
/// </summary>
public class PlayerJumpingState : IState
{
    private readonly PlayerContext _ctx;

    public PlayerJumpingState(PlayerContext ctx) { _ctx = ctx; }

    public void Enter()
    {
        _ctx.Anim.SetInteger("state", 2);  // jumping 动画
        _ctx.Rb.velocity = new Vector2(_ctx.Rb.velocity.x, _ctx.JumpForce);
    }

    public void Update()
    {
        // 空中也可以水平移动
        _ctx.Rb.velocity = new Vector2(_ctx.MoveInput * _ctx.MoveSpeed, _ctx.Rb.velocity.y);
    }

    public void Exit() { }
}

/// <summary>
/// 知识点 26: Falling 状态
/// </summary>
public class PlayerFallingState : IState
{
    private readonly PlayerContext _ctx;

    public PlayerFallingState(PlayerContext ctx) { _ctx = ctx; }

    public void Enter()
    {
        _ctx.Anim.SetInteger("state", 3);  // falling 动画
    }

    public void Update()
    {
        _ctx.Rb.velocity = new Vector2(_ctx.MoveInput * _ctx.MoveSpeed, _ctx.Rb.velocity.y);
    }

    public void Exit() { }
}

// ============================================================
// Part 6: 状态机工厂 — 一键创建完整的玩家状态机
// 知识点 27: 工厂方法模式 — 封装复杂的创建逻辑
// ============================================================

public static class PlayerStateMachineFactory
{
    /// <summary>
    /// 知识点 28: 创建并配置完整的玩家状态机
    /// 调用者只需要传入 context，返回一个配置好的 FSM
    /// </summary>
    public static FiniteStateMachine Create(PlayerContext ctx)
    {
        var fsm = new FiniteStateMachine();

        // 创建所有状态
        var idle = new PlayerIdleState(ctx);
        var running = new PlayerRunningState(ctx);
        var jumping = new PlayerJumpingState(ctx);
        var falling = new PlayerFallingState(ctx);

        // 注册状态
        fsm.AddState(idle);
        fsm.AddState(running);
        fsm.AddState(jumping);
        fsm.AddState(falling);

        // 配置转换规则 — 知识点 29: 转换条件的可读性
        // 条件从 PlayerContext 读取，由外部每帧更新输入状态
        fsm.AddTransition(idle, running, () => ctx.MoveInput != 0 && ctx.IsGrounded);
        fsm.AddTransition(running, idle, () => ctx.MoveInput == 0 && ctx.IsGrounded);
        fsm.AddTransition(idle, jumping, () => ctx.JumpPressed && ctx.IsGrounded);
        fsm.AddTransition(running, jumping, () => ctx.JumpPressed && ctx.IsGrounded);
        fsm.AddTransition(jumping, falling, () => ctx.Rb.velocity.y < -0.1f);
        fsm.AddTransition(falling, idle, () => ctx.IsGrounded);
        fsm.AddTransition(falling, running, () => ctx.IsGrounded && ctx.MoveInput != 0);

        // 初始状态
        fsm.SetInitialState(idle);

        return fsm;
    }
}

// ============================================================
// Part 7: 对比 — 重构前 vs 重构后
// 知识点 30: 状态模式的价值 — 对比学习
// ============================================================

/*
┌─────────────────────────────────────────────────────────────┐
│  重构前 (PlayerMoment.cs 中的 updateAnimationstate)          │
│                                                             │
│  void UpdateAnimation()                                     │
│  {                                                          │
│      MovementState state;                                   │
│      if (dirX > 0f) { state = running; ... }                │
│      else if (dirX < 0f) { state = running; ... }           │
│      else { state = idle; }                                 │
│      if (rb.velocity.y > .1f && extraJumps == 1)            │
│          state = jumping;   // <-- 覆盖前面的判断！          │
│      else if (rb.velocity.y < -.1f)                         │
│          state = falling;                                   │
│      else if (rb.velocity.y > .1f && extraJumps == 0)       │
│          state = doubleJumping;                             │
│      anim.SetInteger("state", (int)state);                  │
│  }                                                          │
│  问题:                                                      │
│  • 状态判断顺序依赖 — 改一个可能影响其他                     │
│  • 加新状态要改整个方法 — 违反开闭原则                       │
│  • 逻辑与动画耦合 — 难以测试和复用                           │
│  • 多状态 (二段跳) 靠 extraJumps 变量打补丁                  │
├─────────────────────────────────────────────────────────────┤
│  重构后 (使用状态机)                                         │
│                                                             │
│  // 每个状态独立成类，各自管理 Enter/Update/Exit             │
│  // 转换规则集中声明，一目了然:                              │
│  fsm.AddTransition(idle, running,                            │
│      () => input != 0 && grounded);                         │
│  fsm.AddTransition(jumping, falling,                         │
│      () => velocity.y < -0.1f);                             │
│  // 加二段跳? 只需:                                          │
│  fsm.AddTransition(falling, doubleJumping,                   │
│      () => jumpPressed && extraJumps > 0);                  │
│                                                             │
│  优势:                                                      │
│  • 每个状态职责单一 — 易于理解和修改                         │
│  • 加新状态 = 加新类 + 加转换规则 — 不改已有代码             │
│  • 转换条件可读 — 变量名直接表达意图                         │
│  • 可配合 Day3 事件系统 — 状态切换时广播事件                 │
└─────────────────────────────────────────────────────────────┘
*/

// ============================================================
// 知识点 31 (Bonus): 简易状态机 — 如果你只需要轻量方案
// enum + switch 的改进版，代码量少但仍有状态模式的好处
// ============================================================

/// <summary>
/// 知识点 32: 极简状态机 — 当完整 FSM 过度设计时的替代方案
/// 适合只有 3-5 个简单状态的小型对象
/// </summary>
public class SimpleStateMachine : MonoBehaviour
{
    private enum State { Idle, Running, Jumping, Falling }
    private State _current;

    // 知识点 33: 属性 setter 中触发 Enter/Exit 逻辑
    private State Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            OnExitState(_current);   // 退出旧状态
            _current = value;
            OnEnterState(_current);  // 进入新状态
        }
    }

    private void OnEnterState(State state)
    {
        switch (state)
        {
            case State.Idle:    Debug.Log("进入 Idle");    break;
            case State.Running: Debug.Log("进入 Running"); break;
            // ... 每个状态的进入逻辑
        }
    }

    private void OnExitState(State state)
    {
        switch (state)
        {
            case State.Idle:    /* 清理 Idle 状态 */    break;
            case State.Running: /* 清理 Running 状态 */ break;
        }
    }

    private void Update()
    {
        // 根据条件切换状态
        if (IsGrounded() && HasInput()) Current = State.Running;
        else if (IsGrounded()) Current = State.Idle;
        else if (Rb.velocity.y > 0) Current = State.Jumping;
        else Current = State.Falling;

        // 执行当前状态逻辑
        switch (_current)
        {
            case State.Running: Move(); break;
            case State.Jumping: AirMove(); break;
            // ...
        }
    }

    // 占位方法 — 实际项目中替换为真实逻辑
    private bool IsGrounded() => true;
    private bool HasInput() => false;
    private Rigidbody2D Rb => null;
    private void Move() { }
    private void AirMove() { }
}

// ============================================================
// 学习总结
// ============================================================
/*
 * 今日核心收获:
 *
 * 1. 状态机 = 状态 (State) + 转换 (Transition) + 动作 (Action)
 * 2. IState 接口定义统一契约 → 所有状态遵循 Enter/Update/Exit 生命周期
 * 3. 转换条件用 Func<bool> 委托 → 灵活、可读、可组合
 * 4. FSM.Tick() 先检查转换再执行逻辑 → 一帧内完成切换
 * 5. 状态切换事件可以对接 Day3 的事件系统 → 松耦合通信
 * 6. 工厂方法封装复杂创建 → 调用者一行代码拿到配置好的 FSM
 * 7. 小项目用 SimpleStateMachine, 大项目用完整 FSM → 按需选择
 *
 * 适用场景:
 *  ✅ 角色控制器 (idle/run/jump/fall/attack/die...)
 *  ✅ 敌人 AI (patrol/chase/attack/flee/dead)
 *  ✅ UI 流程 (mainMenu/settings/gameplay/pause/gameOver)
 *  ✅ 游戏流程 (loading/menu/playing/paused/gameOver)
 *  ✅ 动画状态 (配合 Animator 做逻辑层状态管理)
 *
 * 当前项目可立即重构:
 *  PlayerMoment.cs 的 updateAnimationstate() 方法
 *  → 用 PlayerStateMachineFactory.Create() 替换 if-else 链
 */
