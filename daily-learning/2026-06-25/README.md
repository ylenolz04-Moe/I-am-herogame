# 📅 每日脚本学习 — 2026-06-25

> **今日主题**: C# 事件系统与观察者模式 (Observer Pattern & Event System)

---

## 📖 学习内容概览

事件系统是游戏开发中 **最重要的架构模式之一**。它通过"发布-订阅"模式将游戏系统从紧耦合变为松耦合，让代码更易扩展和维护。

今天实现两个层级的事件系统：

| 层级 | 核心类 | 适用场景 |
|------|--------|---------|
| ScriptableObject 通道 | `GameEventChannel<T>` | 正式项目，Inspector 配置，跨场景通信 |
| 全局 EventBus | `EventBus` | 快速原型，纯代码驱动，Game Jam |

---

## 🔑 20 个核心知识点

### 1. 观察者模式核心思想
```
发送方 (Publisher) ←→ [事件通道] ←→ 接收方 (Subscriber)
双方互不知道对方存在，只依赖共同的事件通道 → 模块间零耦合
```

### 2. ScriptableObject 作为事件通道
```csharp
public abstract class GameEventChannel<T> : ScriptableObject
```
- 继承 ScriptableObject → 可作为 `.asset` 文件存在于项目中
- 可在 Inspector 中拖拽配置，策划/美术无需写代码即可连线
- 一个通道可以连接任意数量的发布者和订阅者

### 3. 泛型事件通道基类
不同事件类型（得分、死亡、通关...）只需继承 `GameEventChannel<T>`，无需重复编写监听/触发逻辑。

### 4. C# event vs UnityEvent
```csharp
// C# event — 性能优先
private event Action<T> _onEvent;

// UnityEvent — Inspector 可视化优先
[SerializeField] private UnityEvent<T> _unityEvent;
```
- **C# event**: 无序列化开销，无反射，适合代码层通信
- **UnityEvent**: 可在 Inspector 拖拽连线，适合策划配置

### 5. 空条件运算符 `?.Invoke()`
```csharp
_onEvent?.Invoke(data);  // 无监听者时不会抛异常
```
比 `if (_onEvent != null) _onEvent(data);` 更简洁且线程安全。

### 6. 无参事件通道 — VoidEventChannel
适用于不需要传递数据的通知：游戏暂停、关卡加载完成、场景切换等。

### 7. 事件数据用 struct 而非 class
```csharp
public struct ScoreChangedEvent { ... }   // ✓ 栈分配，零 GC
public class ScoreChangedEvent { ... }    // ✗ 堆分配，触发 GC
```
游戏运行时事件触发频繁，struct 避免堆分配 → 减少 GC 卡顿。

### 8. 用 enum 表示状态而非 string
```csharp
public enum GameState { MainMenu, Playing, Paused, GameOver }
```
类型安全 + 编译期检查 + IDE 自动补全 > 魔法字符串。

### 9. 一行代码创建新事件类型
```csharp
[CreateAssetMenu(menuName = "Events/Score Changed Event")]
public class ScoreChangedEventChannel : GameEventChannel<ScoreChangedEvent> { }
```
利用 C# 泛型 + ScriptableObject 的强大组合。

### 10. EventBus 全局单例
```csharp
public static EventBus Instance => _instance ??= new EventBus();
```
- 优势: 无需创建 .asset 文件，纯代码直接使用
- 劣势: 全局依赖，不方便单元测试（小型项目可忽略）

### 11. Dictionary\<Type, Delegate\> 存储事件
```csharp
private readonly Dictionary<Type, Delegate> _handlers = new();
```
一个 Dictionary 管理所有类型的事件 → O(1) 查找 → 极简实现。

### 12. EventListener\<T\> 基类 — 自动管理生命周期
```csharp
public abstract class EventListener<T> : MonoBehaviour
```
继承此基类后，子类只需实现 `OnEventRaised(T data)`，无需手动处理注册/注销。

### 13. OnEnable/OnDisable — Unity 事件注册的标准时机
```csharp
void OnEnable()  => _channel.Register(OnEventRaised);
void OnDisable() => _channel.Unregister(OnEventRaised);
```
- OnEnable: 对象激活时注册（包括首次加载和 SetActive(true)）
- OnDisable: 对象禁用时注销（自动防止内存泄漏和空引用）

### 14. 具体监听器 — ScoreEventListener
```csharp
public class ScoreEventListener : EventListener<ScoreChangedEvent>
{
    protected override void OnEventRaised(ScoreChangedEvent data)
    {
        _scoreText.text = $"Score: {data.NewScore}";
        _collectSound?.Play();
    }
}
```
一个监听器可以同时驱动 UI 更新和音效播放 — 关注点清晰分离。

### 15. 发布者示例 — 事件驱动 vs 紧耦合
```csharp
// 旧方式 — 紧耦合
UIManager.Instance.UpdateScore(score);     // 依赖 UIManager
AudioManager.Instance.PlayCollectSound();   // 依赖 AudioManager
EffectManager.Instance.SpawnEffect(pos);   // 依赖 EffectManager

// 新方式 — 事件驱动
_scoreChannel.Raise(new ScoreChangedEvent { ... });  // 零依赖！
```

### 16. 发布者不需要知道谁在监听
核心好处：加新功能（成就系统、统计系统、排行榜）不需要修改现有发布者代码。

### 17. 三种事件实现对比

| 方式 | 优势 | 劣势 |
|------|------|------|
| C# event 关键字 | 性能最优，零 GC | 订阅者需持有发布者引用 |
| UnityEvent | Inspector 可视化 | 有 GC 分配 |
| ScriptableObject 通道 | 跨场景，零引用耦合 | 需创建 .asset 文件 |
| EventBus 单例 | 使用最方便 | 全局依赖 |

### 18. 内存泄漏防护 — 最大陷阱
```
Enemy 订阅全局事件 → 场景切换 Destroy Enemy
→ 事件仍持有 Enemy 引用 → Enemy 无法被 GC → 内存泄漏！
```

防护措施：
1. 始终在 OnDisable/OnDestroy 中 Unregister
2. 使用 `EventListener<T>` 基类自动管理
3. 场景切换时调用 `EventBus.Instance.Clear()`
4. 使用 WeakReference（进阶）

### 19. 实战 — I Am Hero 架构对比
```
重构前 (紧耦合):
  PlayerMoment → UIManager, AudioManager, EffectManager, AchievementSystem
  每加一个系统 → 改 PlayerMoment 代码

重构后 (事件驱动):
  PlayerMoment → 发布事件 (ScoreChanged, PlayerDied...)
  UIManager, AudioManager, AchievementSystem → 各自独立订阅
  加新系统 → 新建订阅者，不碰现有代码 ✓
```

### 20. Unity 中使用步骤
1. `右键 Project → Create → Events → Score Changed Event` 创建通道资源
2. 在发布者 Inspector 中拖入通道资源
3. 创建监听者 GameObject，挂载 EventListener 子类
4. 运行 → 事件自动流转！

---

## 🛠️ 在 I Am Hero 中的应用

### 当前可重构的候选
| 当前代码 | 重构方式 |
|---------|---------|
| `item_collector.cs` 直接调用 `Allcontrol` | 发布 `ScoreChangedEvent`，UI 独立订阅 |
| `PlayerLife.cs` 直接加载场景 | 发布 `PlayerDiedEvent`，GameManager 独立处理 |
| `Finish.cs` 直接切换场景 | 发布 `LevelCompleteEvent`，统计/存档/UI 各自处理 |

---

## 📊 延伸思考

1. **事件风暴**: 如果事件太多太杂，可以引入"领域事件"概念 — 只发布有业务含义的事件。

2. **事件溯源 (Event Sourcing)**: 将每个事件持久化存储，可以回放游戏过程、实现录像回放系统。

3. **响应式编程**: UniRx / R3 库将事件流抽象为 `IObservable<T>`，支持 LINQ 风格的事件过滤、合并、节流。

4. **消息队列**: 大型 MMO 中事件系统需要跨进程/跨服务器，此时用 RabbitMQ/Kafka 等消息中间件替代内存事件。

---

## 🏷️ 标签

`#CSharp` `#Unity` `#设计模式` `#观察者模式` `#事件系统` `#ScriptableObject` `#架构`

---

*2026-06-25 学习记录 · 每日脚本知识积累 Day 3*
