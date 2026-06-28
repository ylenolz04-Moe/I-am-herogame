// ============================================================
// 每日脚本学习 Day 5 — 2026-06-28
// 主题: C# 命令模式 (Command Pattern)
// 适用: Unity 游戏开发 · 输入处理 · 回放系统 · 撤销重做 · 按键重绑定
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 知识点 1: 命令模式核心思想 — "将请求封装为对象"
//
//   用户输入 ──→ [命令对象] ──→ 执行者
//                  ↑
//              可存储、排队、撤销、回放
//
// 把"做什么事"从"谁来做"中解耦出来
// 命令是一个对象，可以像普通数据一样被存储和传递
// ============================================================

// ============================================================
// Part 1: 命令接口 — 所有命令的统一契约
// 知识点 2: 核心只有两个方法 — Execute 和 Undo
// ============================================================

/// <summary>
/// 知识点 3: ICommand 接口 — 命令模式的最小契约
///   Execute() — 执行命令
///   Undo()    — 撤销命令（倒带）
/// 每个具体命令封装一次操作所需的所有数据
/// </summary>
public interface ICommand
{
    void Execute();
    void Undo();
}

// ============================================================
// Part 2: 游戏中的具体命令
// 知识点 4: 每个命令类包含 Execute 所需的所有信息
// 就像"快递包裹"一样，自包含、可传递、可存储
// ============================================================

/// <summary>
/// 知识点 5: 移动命令 — 封装一次移动操作
/// 包含: 谁移动 (transform)、移动方向、移动距离、持续时间
/// </summary>
public class MoveCommand : ICommand
{
    private readonly Transform _target;
    private readonly Vector3 _direction;
    private readonly float _distance;

    // 记录移动前的位置，用于 Undo
    private Vector3 _previousPosition;

    public MoveCommand(Transform target, Vector3 direction, float distance)
    {
        _target = target;
        _direction = direction.normalized;
        _distance = distance;
    }

    public void Execute()
    {
        // 知识点 6: Execute 前保存状态 — Undo 的关键
        _previousPosition = _target.position;
        _target.position += _direction * _distance;
    }

    public void Undo()
    {
        // 撤销 = 回到之前的位置
        _target.position = _previousPosition;
    }
}

/// <summary>
/// 知识点 7: 跳跃命令 — 封装一次跳跃
/// 适合 2D 平台游戏，给 Rigidbody2D 施加力
/// </summary>
public class JumpCommand : ICommand
{
    private readonly Rigidbody2D _rb;
    private readonly float _force;
    private Vector3 _previousPosition;

    public JumpCommand(Rigidbody2D rb, float force)
    {
        _rb = rb;
        _force = force;
    }

    public void Execute()
    {
        _previousPosition = _rb.position;
        _rb.velocity = new Vector2(_rb.velocity.x, _force);
    }

    public void Undo()
    {
        _rb.position = _previousPosition;
        _rb.velocity = new Vector2(_rb.velocity.x, 0);
    }
}

/// <summary>
/// 知识点 8: 收集物品命令 — 操作游戏数据而非 Transform
/// 命令模式不限于移动，任何可逆操作都能封装
/// </summary>
public class CollectItemCommand : ICommand
{
    private readonly GameObject _item;
    private readonly Action<int> _onScoreChanged;
    private readonly int _points;

    private bool _wasActive;

    public CollectItemCommand(GameObject item, Action<int> onScoreChanged, int points)
    {
        _item = item;
        _onScoreChanged = onScoreChanged;
        _points = points;
    }

    public void Execute()
    {
        _wasActive = _item.activeSelf;
        _item.SetActive(false);
        _onScoreChanged?.Invoke(_points);
    }

    public void Undo()
    {
        _item.SetActive(_wasActive);
        _onScoreChanged?.Invoke(-_points);  // 扣回分数
    }
}

/// <summary>
/// 知识点 9: 延时命令 — 组合模式 + 协程
/// 不是立刻执行的命令，封装了等待逻辑
/// </summary>
public class DelayedCommand : ICommand
{
    private readonly ICommand _inner;
    private readonly float _delay;
    private readonly MonoBehaviour _runner;

    public DelayedCommand(ICommand inner, float delay, MonoBehaviour runner)
    {
        _inner = inner;
        _delay = delay;
        _runner = runner;
    }

    public void Execute()
    {
        // 知识点 10: 启动协程延时执行内部命令
        _runner.StartCoroutine(DelayedExecute());
    }

    private System.Collections.IEnumerator DelayedExecute()
    {
        yield return new WaitForSeconds(_delay);
        _inner.Execute();
    }

    public void Undo()
    {
        _inner.Undo();
    }
}

// ============================================================
// Part 3: 命令调用者 — 管理和执行命令的容器
// 知识点 11: CommandInvoker — 命令模式的"遥控器"
// ============================================================

/// <summary>
/// 知识点 12: 命令调用者 — 命令历史 + 撤销/重做
/// 这是命令模式中最实用的部分
/// </summary>
public class CommandInvoker
{
    // 知识点 13: 两个栈 — 一个记录已执行的，一个记录已撤销的
    private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    /// <summary>
    /// 知识点 14: 执行命令 — 自动记录到撤销栈
    /// 每次执行新命令时清空重做栈（经典行为）
    /// </summary>
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();  // 新操作使重做历史失效
    }

    /// <summary>
    /// 知识点 15: 撤销 — 从撤销栈弹出，压入重做栈
    /// </summary>
    public void Undo()
    {
        if (_undoStack.Count == 0) return;

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
    }

    /// <summary>
    /// 知识点 16: 重做 — 从重做栈弹出，压回撤销栈
    /// </summary>
    public void Redo()
    {
        if (_redoStack.Count == 0) return;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
    }

    // 知识点 17: 状态查询
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;
}

// ============================================================
// Part 4: 命令回放系统 — 记录玩家操作并重播
// 知识点 18: 回放是命令模式最酷的应用之一
// 记录的不是画面帧，而是输入命令 → 文件极小
// ============================================================

/// <summary>
/// 知识点 19: 带时间戳的命令记录
/// 用于回放系统 — 知道何时执行每个命令
/// </summary>
public struct CommandRecord
{
    public ICommand Command;
    public float Timestamp;  // 从游戏开始计时
}

/// <summary>
/// 知识点 20: 回放管理器 — 录制 / 播放 / 停止
/// </summary>
public class ReplayManager
{
    private readonly List<CommandRecord> _records = new List<CommandRecord>();
    private bool _isRecording;
    private bool _isPlaying;
    private float _startTime;
    private int _playIndex;

    /// <summary>开始录制 — 从此刻开始记录所有命令</summary>
    public void StartRecording()
    {
        _records.Clear();
        _isRecording = true;
        _startTime = Time.time;
        Debug.Log($"[Replay] 开始录制...");
    }

    /// <summary>停止录制</summary>
    public void StopRecording()
    {
        _isRecording = false;
        Debug.Log($"[Replay] 录制完成，共 {_records.Count} 条命令");
    }

    /// <summary>
    /// 知识点 21: 记录一条命令
    /// 在命令执行的同时调用此方法
    /// </summary>
    public void Record(ICommand command)
    {
        if (!_isRecording) return;

        _records.Add(new CommandRecord
        {
            Command = command,
            Timestamp = Time.time - _startTime
        });
    }

    /// <summary>开始回放</summary>
    public void StartPlayback(MonoBehaviour runner)
    {
        if (_records.Count == 0)
        {
            Debug.LogWarning("[Replay] 没有可回放的数据");
            return;
        }

        _isPlaying = true;
        _playIndex = 0;
        runner.StartCoroutine(PlaybackRoutine());
        Debug.Log($"[Replay] 开始回放 {_records.Count} 条命令...");
    }

    /// <summary>
    /// 知识点 22: 回放协程 — 按时间戳依次执行命令
    /// </summary>
    private System.Collections.IEnumerator PlaybackRoutine()
    {
        float playbackStart = Time.time;

        while (_playIndex < _records.Count && _isPlaying)
        {
            var record = _records[_playIndex];

            // 等待到该命令应该执行的时间点
            float elapsed = Time.time - playbackStart;
            if (elapsed >= record.Timestamp)
            {
                record.Command.Execute();
                _playIndex++;
            }

            yield return null;
        }

        _isPlaying = false;
        Debug.Log("[Replay] 回放完成");
    }

    public void StopPlayback()
    {
        _isPlaying = false;
    }
}

// ============================================================
// Part 5: Unity 集成 — 可挂载到 GameObject 的命令管理器
// 知识点 23: 把纯 C# 逻辑与 Unity 输入系统桥接
// ============================================================

/// <summary>
/// 知识点 24: 用命令模式处理玩家输入
/// 每个按键绑定一个命令 → 改按键 = 改命令绑定，不改游戏逻辑
/// 配合 Day3 事件系统 → 按键变化时发布事件通知 UI 更新
/// 配合 Day4 状态机 → 不同状态下同一按键可绑定不同命令
/// </summary>
public class PlayerCommandHandler : MonoBehaviour
{
    [Header("命令管理")]
    private CommandInvoker _invoker = new CommandInvoker();
    private ReplayManager _replay = new ReplayManager();

    [Header("玩家组件")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private float _moveDistance = 1f;
    [SerializeField] private float _jumpForce = 7f;

    // 知识点 25: 命令绑定字典 — 按键映射到命令
    // 换键位只需改这里的映射，游戏逻辑完全不动
    private Dictionary<KeyCode, Func<ICommand>> _keyBindings;

    private void Awake()
    {
        // 知识点 26: 使用工厂委托 — 每次按键创建新命令实例
        // Func<ICommand> = 命令工厂，延迟创建
        _keyBindings = new Dictionary<KeyCode, Func<ICommand>>
        {
            { KeyCode.D,      () => new MoveCommand(transform, Vector3.right, _moveDistance) },
            { KeyCode.A,      () => new MoveCommand(transform, Vector3.left,  _moveDistance) },
            { KeyCode.Space,  () => new JumpCommand(_rb, _jumpForce) },
        };
    }

    private void Update()
    {
        // 知识点 27: 遍历按键绑定 — 检测输入并执行对应命令
        foreach (var binding in _keyBindings)
        {
            if (Input.GetKeyDown(binding.Key))
            {
                var command = binding.Value();  // 创建命令
                ExecuteCommandWithReplay(command);
            }
        }

        // 撤销 / 重做快捷键
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            _invoker.Undo();
        }
        if (Input.GetKeyDown(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            _invoker.Redo();
        }

        // 回放快捷键
        if (Input.GetKeyDown(KeyCode.F5)) _replay.StartRecording();
        if (Input.GetKeyDown(KeyCode.F6)) _replay.StopRecording();
        if (Input.GetKeyDown(KeyCode.F7)) _replay.StartPlayback(this);
    }

    private void ExecuteCommandWithReplay(ICommand command)
    {
        _invoker.ExecuteCommand(command);
        _replay.Record(command);
    }

    /// <summary>
    /// 知识点 28: 运行时更换按键绑定
    /// 例如把 "向右移动" 从 D 键改为 RightArrow
    /// </summary>
    public void RebindKey(KeyCode oldKey, KeyCode newKey)
    {
        if (_keyBindings.TryGetValue(oldKey, out var factory))
        {
            _keyBindings.Remove(oldKey);
            _keyBindings[newKey] = factory;
            // 可配合 Day3 事件: _rebindEventChannel.Raise(new KeyRebindEvent { ... });
        }
    }

    /// <summary>
    /// 知识点 29: 不同状态使用不同按键映射
    /// 配合 Day4 状态机使用
    /// </summary>
    public void SetBindingsForState(Dictionary<KeyCode, Func<ICommand>> bindings)
    {
        _keyBindings = bindings;
    }
}

// ============================================================
// Part 6: 命令队列 — 批量执行命令
// 知识点 30: 宏命令 / 组合命令 — 一个命令包含多个子命令
// ============================================================

/// <summary>
/// 知识点 31: 组合命令 — 把多个命令打包成一个
/// 适合: 过场动画、教学关卡中的预设操作序列、快速连招
/// </summary>
public class MacroCommand : ICommand
{
    private readonly List<ICommand> _commands = new List<ICommand>();

    public void Add(ICommand command) => _commands.Add(command);

    public void Execute()
    {
        // 知识点 32: 顺序执行所有子命令
        // 任何一个失败都可以选择继续或中断
        foreach (var cmd in _commands)
        {
            cmd.Execute();
        }
    }

    public void Undo()
    {
        // 知识点 33: 撤销时逆序执行 — 后执行的要先撤销
        // 就像穿鞋脱鞋: 先穿袜子后穿鞋 → 先脱鞋后脱袜子
        for (int i = _commands.Count - 1; i >= 0; i--)
        {
            _commands[i].Undo();
        }
    }
}

// ============================================================
// Part 7: 实战示例 — 关卡中的可回放挑战
// 知识点 34: 综合运用所有概念
// ============================================================

/// <summary>
/// 知识点 35: 幽灵回放 — 在竞速游戏中显示"上一次的最佳记录"
/// 这是命令模式在游戏中最经典的应用之一
/// </summary>
public class GhostReplay : MonoBehaviour
{
    [SerializeField] private GameObject _ghostPrefab;
    [SerializeField] private float _ghostAlpha = 0.4f;

    private ReplayManager _replay;
    private GameObject _ghostInstance;

    /// <summary>
    /// 知识点 36: 开始竞速挑战
    /// 1. 创建一个半透明的"幽灵"角色
    /// 2. 开始录制当前玩家的操作
    /// 3. 如果之前有记录，开始播放幽灵的移动
    /// </summary>
    public void StartChallenge(ReplayManager previousBest)
    {
        // 创建幽灵 — 半透明的"上一次的自己"
        if (_ghostPrefab != null && previousBest != null)
        {
            _ghostInstance = Instantiate(_ghostPrefab, transform.position, Quaternion.identity);
            var sprite = _ghostInstance.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                var color = sprite.color;
                color.a = _ghostAlpha;
                sprite.color = color;
            }

            // 在幽灵身上回放历史记录
            var ghostHandler = _ghostInstance.AddComponent<PlayerCommandHandler>();
            // (实际项目中需要将命令中的 transform 替换为幽灵的 transform)
        }

        // 开始录制当前玩家操作
        _replay = new ReplayManager();
        _replay.StartRecording();
    }

    public ReplayManager FinishChallenge()
    {
        _replay.StopRecording();

        // 销毁幽灵
        if (_ghostInstance != null)
            Destroy(_ghostInstance);

        // 返回本次记录，可以保存为"最佳成绩"
        return _replay;
    }
}

// ============================================================
// 知识点 37: 对比 — 命令模式重构前后
// ============================================================

/*
┌─────────────────────────────────────────────────────────────┐
│  重构前 (传统 Input.GetKey 写法)                              │
│                                                             │
│  void Update()                                              │
│  {                                                          │
│      if (Input.GetKeyDown(KeyCode.D))                       │
│      {                                                      │
│          transform.position += Vector3.right;               │
│          // 想做撤销? → 需要额外记录位置                      │
│          // 想换按键? → 要改代码                              │
│          // 想做回放? → 需要帧录像，文件巨大                   │
│          // 想做宏命令? → 需要写死一长串逻辑                   │
│      }                                                      │
│      if (Input.GetKeyDown(KeyCode.Z)) { ... }               │
│  }                                                          │
│                                                             │
│  问题:                                                      │
│  • 按键检测和游戏逻辑耦合 — 改按键要改 Update                │
│  • 无法撤销 — 没有保存历史状态                               │
│  • 无法回放 — 没有记录操作序列                               │
│  • 无法重绑定 — 按键写死在代码里                             │
├─────────────────────────────────────────────────────────────┤
│  重构后 (使用命令模式)                                        │
│                                                             │
│  // 按键 → 命令映射表                                        │
│  _keyBindings = new Dictionary<KeyCode, Func<ICommand>> {   │
│      { KeyCode.D, () => new MoveCommand(...) },            │
│      { KeyCode.Space, () => new JumpCommand(...) },        │
│  };                                                         │
│                                                             │
│  // 按 Ctrl+Z → Undo (天然支持!)                             │
│  // 按 F7 → 回放刚才的操作 (天然支持!)                       │
│  // 改按键 → 只改映射表，不动逻辑                             │
│                                                             │
│  优势:                                                      │
│  ✅ 按键可重绑定 — 改映射表即可                               │
│  ✅ 撤销/重做免费获得 — CommandInvoker 搞定                   │
│  ✅ 回放系统 — 存几十个命令 vs 存几百帧画面                   │
│  ✅ 宏命令 — MacroCommand 组合多个命令为一体                  │
│  ✅ 日志/调试 — 每条命令都有迹可循                            │
│  ✅ 网络同步 — 发送命令对象而非状态快照                       │
└─────────────────────────────────────────────────────────────┘
*/

// ============================================================
// 学习总结
// ============================================================
/*
 * 今日核心收获:
 *
 * 1. 命令模式 = ICommand 接口 + Execute/Undo + Invoker
 * 2. 命令是"把要做的事封装成对象" → 可以存储、传递、撤回
 * 3. Undo 的关键: Execute 前保存旧状态
 * 4. 双栈实现撤销/重做: undoStack + redoStack
 * 5. 回放 = 记录命令+时间戳 → 按时间线重播
 * 6. MacroCommand = 组合多个命令 → 批量执行/批量撤销
 * 7. 按键绑定 = 字典映射 KeyCode → 命令工厂
 * 8. 与 Day3 (事件)、Day4 (状态机) 可无缝组合使用
 *
 * 适用场景:
 *  ✅ 输入处理 (键盘/手柄/触摸 → 映射到不同命令)
 *  ✅ 撤销/重做 (关卡编辑器、装备系统)
 *  ✅ 回放系统 (竞速幽灵、击杀回放、Bug 复现)
 *  ✅ 网络对战 (发送命令而非状态，减少带宽)
 *  ✅ 教程系统 (预设操作序列引导玩家)
 *  ✅ 连招系统 (一键触发多个技能按序执行)
 *
 * 难度: ★★★☆☆ (适中，理解"封装操作"的概念即可)
 * 前置: Day2 (泛型) + Day3 (事件系统)
 * 后续: 可搭配 Day4 状态机实现不同状态下的不同按键映射
 */
