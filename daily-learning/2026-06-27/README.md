# 📅 每日脚本学习 — 2026-06-27

> **今日主题**: C# 有限状态机模式 (Finite State Machine Pattern)

---

## 📖 学习内容概览

状态机是游戏开发中 **最常用的行为组织模式**。它把复杂的 if-else 逻辑拆分成独立的状态类，让代码更容易理解、扩展和调试。

今天实现两个层级的状态机：

| 层级 | 核心类 | 适用场景 |
|------|--------|---------|
| 完整 FSM | `FiniteStateMachine` + `IState` | 5+ 状态、复杂转换、需要扩展 |
| 极简版 | `SimpleStateMachine` (enum + switch) | 3-5 个简单状态、快速原型 |

---

## 🔑 20 个核心知识点

### 1. 状态机三要素
```
状态 (State) → 转换 (Transition) → 动作 (Action)

[Idle] ──(按方向键)──→ [Running] ──(按跳跃)──→ [Jumping]
  ↑                                               │
  └─────────────────(落地)── [Falling] ←──────────┘
```

### 2. IState 接口 — 状态的统一契约
```csharp
public interface IState
{
    void Enter();   // 进入时调用一次
    void Update();  // 每帧调用
    void Exit();    // 离开时调用一次
}
```
所有状态都遵循这个生命周期 → FSM 不需要知道具体状态类型。

### 3. 状态的生命周期
```
创建 → Enter() → Update() × N帧 → 条件满足 → Exit() → 切换到新状态 → Enter()...
```

### 4. Func\<bool\> 委托定义转换条件
```csharp
// 条件 = 返回 bool 的委托，可以动态计算
Func<bool> canJump = () => jumpPressed && isGrounded;
```
不使用硬编码的 `if` → 条件可以组合、复用、运行时改变。

### 5. StateTransition — 转换规则对象化
```csharp
public class StateTransition
{
    public IState From;          // 从哪个状态
    public IState To;            // 到哪个状态
    public Func<bool> Condition; // 什么条件下切换
}
```
将转换规则从状态类中抽离 → 状态类更纯粹，转换规则集中管理。

### 6. 泛型与键值 — 为什么用 IState 作 Key
直接用状态对象作为 Dictionary 的 Key → 不需要 string/enum 映射，类型安全。

### 7. FSM.Tick() 的核心逻辑
```csharp
public void Tick()
{
    // 1. 先检查转换 — 条件满足就切换
    foreach (var t in _transitions[CurrentState])
        if (t.Condition()) { ChangeState(t.To); break; }

    // 2. 再执行当前状态逻辑
    CurrentState.Update();
}
```
先转换再执行 → 一帧内完成切换，不浪费帧。

### 8. 一帧只切换一次
`break` 确保每帧最多切换一次 → 避免 A→B→C 连锁切换导致的"状态乒乓"。

### 9. ChangeState 的严格顺序
```
Exit(旧状态) → 更新 CurrentState → Enter(新状态) → 触发事件
```
顺序错乱会导致：新状态的 Enter 用到旧状态的数据，或事件监听者读到不一致状态。

### 10. OnStateChanged 事件 — 配合 Day3 事件系统
```csharp
public event Action<IState, IState> OnStateChanged; // (旧, 新)
```
状态切换时可以广播给 UI、音效、成就等外部系统 → 松耦合。

### 11. ForceChangeState — 跳过条件检查
```csharp
fsm.ForceChangeState(hurtState);  // 受到伤害 → 强制进入受伤状态
```
用于外部事件驱动的切换，不依赖逐帧条件检查。

### 12. HashSet 防重复 + Dictionary O(1) 查找
```csharp
HashSet<IState> _states;                    // 状态去重
Dictionary<IState, List<Transition>> _transitions;  // 转换规则索引
```
两个数据结构各司其职 → 清晰的职责分离。

### 13. PlayerContext — 共享数据容器
所有状态通过 Context 访问共享数据（物理、输入、动画）→ 状态类无状态只读配置，Context 持有可变数据。

### 14. 每个状态类职责单一
```csharp
// ✓ Idle 状态只关心 idle 的逻辑
class PlayerIdleState : IState { ... }

// ✓ Running 状态只关心跑步的逻辑
class PlayerRunningState : IState { ... }
```
修改跑步逻辑不会影响跳跃逻辑 → 真正的隔离。

### 15. 工厂方法 — 封装创建复杂度
```csharp
var fsm = PlayerStateMachineFactory.Create(context);
// 一行代码拿到配置完整的玩家状态机
```
调用者不需要知道内部有 4 个状态 + 7 条转换规则。

### 16. 转换条件的可读性
```csharp
// ✓ 一目了然
fsm.AddTransition(idle, running, () => moveInput != 0 && isGrounded);

// ✗ 原代码中: if (dirX > 0f) state = running; else if ... — 难以追踪
```

### 17. 加新状态不改已有代码
```csharp
// 加二段跳?  只需:
var doubleJump = new PlayerDoubleJumpState(ctx);
fsm.AddState(doubleJump);
fsm.AddTransition(falling, doubleJump, () => jumpPressed && extraJumps > 0);
// ✓ 已有的 idle/running/jumping/falling 完全不用改
```
这体现了**开闭原则** (Open/Closed Principle)：对扩展开放，对修改关闭。

### 18. StateMachineRunner — 桥接 Unity 生命周期
```csharp
class StateMachineRunner : MonoBehaviour
{
    void Start() { OnSetup?.Invoke(FSM); }  // 配置 FSM
    void Update() { FSM.Tick(); }           // 驱动 FSM
}
```
纯 C# 逻辑不依赖 MonoBehaviour → 可单元测试，可在非 Unity 环境使用。

### 19. SimpleStateMachine — 什么时候不用完整 FSM
```
只有 3-5 个简单状态？          → SimpleStateMachine (enum + switch)
5+ 状态或频繁加新状态？        → 完整 FiniteStateMachine
需要跨项目复用状态？           → 完整 FSM + IState 接口
Prototype / Game Jam？         → SimpleStateMachine
```

### 20. 状态模式 vs if-else：量化对比
```
                  if-else 链        状态模式
添加新状态          改原方法          加新类 + 规则
修改一个状态        影响整段逻辑       只改一个类
可读性              逐行跟踪判断       转换表一目了然
可测试性            难以单元测试       每个状态独立测试
复用性              零                 状态类可跨项目复用
```

---

## 🛠️ 在 I Am Hero 中的应用

### 立即可重构的代码

| 当前代码 | 问题 | 重构方案 |
|---------|------|---------|
| `PlayerMoment.updateAnimationstate()` | if-else 链难以扩展，动画和逻辑耦合 | 用 `PlayerStateMachineFactory.Create()` 替换 |
| `PlayerLife.Die()` | 死亡后仍能移动（没有状态保护） | 增加 Dead 状态，屏蔽输入 |

### 重构前 vs 重构后

**重构前** (`PlayerMoment.cs`):
```csharp
void updateAnimationstate()
{
    MovementState state;
    if (dirX > 0f) { state = running; ... }
    else if (dirX < 0f) { state = running; ... }
    else { state = idle; }
    if (rb.velocity.y > .1f && extraJumps == 1)
        state = jumping;   // 覆盖前面的判断！
    else if (rb.velocity.y < -.1f) state = falling;
    else if (rb.velocity.y > .1f && extraJumps == 0)
        state = doubleJumping;  // 靠 extraJumps 打补丁
    anim.SetInteger("state", (int)state);
}
```

**重构后**:
```csharp
// 配置阶段 — 一次性声明所有规则
FSM.AddTransition(idle, running, () => input != 0 && grounded);
FSM.AddTransition(idle, jumping, () => jumpPressed && grounded);
FSM.AddTransition(jumping, falling, () => velocity.y < -0.1f);
FSM.AddTransition(falling, idle, () => grounded);

// 每帧 — 两行代码
ctx.MoveInput = Input.GetAxisRaw("Horizontal");
ctx.JumpPressed = Input.GetButtonDown("Jump");
ctx.IsGrounded = Physics2D.BoxCast(...);
FSM.Tick();
```

---

## 📊 延伸思考

1. **分层状态机 (Hierarchical FSM)**: 状态可以有子状态 — 如"攻击"状态下有"前摇→判定→后摇"子状态，用于格斗游戏。

2. **行为树 (Behavior Tree)**: 状态机的进阶替代，更适合复杂 AI。Unity 的 NavMesh + Behavior Tree 是工业级 AI 方案。

3. **动画状态机 vs 逻辑状态机**: Animator Controller 已经是视觉状态机，但它只管动画。代码层的逻辑状态机 + Animator 的参数驱动 = 最佳实践。

4. **配合 Day2 对象池**: 状态对象可以池化 → 避免频繁 new 状态实例。

5. **配合 Day3 事件系统**: 状态切换时广播事件 → UI、音效、成就独立响应，无需互相引用。

---

## 🏷️ 标签

`#CSharp` `#Unity` `#设计模式` `#状态模式` `#有限状态机` `#架构` `#重构`

---

*2026-06-27 学习记录 · 每日脚本知识积累 Day 4*
