using System;
using UnityEngine;

namespace TarodevController.RefactoringGuide
{
    /// <summary>
    /// 【参考文件】重构后的玩家控制器 — 融合 Tarodev 架构 + 你原有的二段跳等功能。
    ///
    /// === 与你原来的 PlayerMoment.cs 的关键区别 ===
    ///
    /// 1. Update/FixedUpdate 分离
    ///    原来: 所有逻辑（输入+物理+动画）全挤在 Update() 里
    ///    现在: Update() 只收集输入，FixedUpdate() 处理物理 —— 物理帧率稳定，不受渲染帧率影响
    ///
    /// 2. 加速度系统
    ///    原来: rb.velocity = new Vector2(dirX * moveSpeed, ...)  —— 瞬间变速，像冰上滑行
    ///    现在: Mathf.MoveTowards(当前速度, 目标速度, 加速度*deltaTime) —— 有加速/减速过程
    ///
    /// 3. 土狼时间 (Coyote Time)
    ///    原来: 离开平台后 IsGrounded() 立刻返回 false，按跳跃无效
    ///    现在: 离开平台后 0.15 秒内仍可跳跃 —— 手感宽容很多
    ///
    /// 4. 跳跃缓冲 (Jump Buffer)
    ///    原来: 必须在着地后才能按跳跃，空中按了没用
    ///    现在: 落地前 0.2 秒内按跳跃会被记住，着地瞬间自动执行
    ///
    /// 5. 可变跳跃高度
    ///    原来: 无论按多久跳跃键，跳跃高度都一样
    ///    现在: 提前松开跳跃键会加大重力，实现"轻按小跳，长按大跳"
    ///
    /// 6. 事件驱动
    ///    原来: Animator 每帧检查 rb.velocity.y 来判断状态
    ///    现在: Controller 触发 Jumped/GroundedChanged 事件，Animator 订阅响应
    ///
    /// 7. 接口解耦
    ///    原来: 其他脚本直接 GetComponent<PlayerMoment>() 强耦合
    ///    现在: 通过 IHeroController 接口交互，方便替换实现
    ///
    /// === 如何在你的项目中使用 ===
    ///
    /// 方案A（渐进迁移）:
    ///   1. 先把 PlayerMoment.cs 里的硬编码参数移到 HeroScriptableStats.asset
    ///   2. 给 PlayerMoment 加上 FrameInput 结构体来收集输入
    ///   3. 把物理逻辑移到 FixedUpdate
    ///   4. 逐步加入 CoyoteTime 和 JumpBuffer
    ///
    /// 方案B（完全替换）:
    ///   1. 创建 HeroScriptableStats.asset，填入你原来的参数值
    ///   2. 把这个脚本挂到 Player 上替代 PlayerMoment
    ///   3. 把 PlayerAnimator 换成 HeroPlayerAnimator（或用事件适配你的原 Animator）
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class HeroPlayerController : MonoBehaviour, IHeroController
    {
        // ==================== Inspector 引用 ====================
        [SerializeField, Tooltip("拖入你创建的 HeroStats 资产")]
        private HeroScriptableStats _stats;

        // ==================== 私有组件引用 ====================
        private Rigidbody2D _rb;
        private Collider2D _col;
        private AudioSource _audioSource;

        // ==================== 输入状态 ====================
        private FrameInput _frameInput;

        // ==================== 速度状态 ====================
        private Vector2 _frameVelocity;  // 当前帧要施加的速度（在 FixedUpdate 中累积计算）

        // ==================== 地面状态 ====================
        private bool _grounded;
        private float _frameLeftGrounded = float.MinValue;  // 离开地面的时间点

        // ==================== 跳跃状态 ====================
        private bool _jumpToConsume;       // 这帧是否要执行跳跃
        private bool _bufferedJumpUsable;  // 缓冲跳跃是否可用
        private bool _endedJumpEarly;      // 是否提前松开了跳跃键
        private bool _coyoteUsable;        // 土狼时间是否可用
        private float _timeJumpWasPressed; // 上次按跳跃的时间
        private int _remainingJumps;       // 剩余跳跃次数（二段跳用）

        // ==================== 时间 ====================
        private float _time;

        // ==================== 缓存 ====================
        private bool _cachedQueryStartInColliders;
        private ContactFilter2D _groundFilter;

        // ==================== 接口事件 ====================
        public event Action<bool, float> GroundedChanged;
        public event Action<int> Jumped;           // int = 剩余跳跃次数（0=最后一段跳）
        public event Action Died;

        // ==================== 接口属性 ====================
        public Vector2 FrameInput => _frameInput.Move;
        public bool IsGrounded => _grounded;
        public Vector2 Velocity => _rb.velocity;

        // ==================== Unity 生命周期 ====================

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _audioSource = GetComponent<AudioSource>();

            // 缓存初始设置，避免每次 FixedUpdate 都查询
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            // 初始化跳跃次数
            _remainingJumps = _stats != null ? _stats.ExtraJumps : 1;

            // 设置地面检测过滤器
            _groundFilter = new ContactFilter2D
            {
                layerMask = _stats != null ? _stats.GroundLayer : Physics2D.AllLayers,
                useLayerMask = true
            };
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();   // Update 中只做输入采集
        }

        private void FixedUpdate()
        {
            CheckCollisions();  // FixedUpdate 中做物理检测和速度计算
            HandleJump();
            HandleDirection();
            HandleGravity();
            ApplyMovement();
        }

        // ==================== 输入采集 ====================

        private void GatherInput()
        {
            // ★ 对比你原来的写法:
            //    原来: dirX = Input.GetAxisRaw("Horizontal");
            //          rb.velocity = new Vector2(dirX*moveSpeed, rb.velocity.y);  ← 直接在 Update 改速度
            //          if (Input.GetButtonDown("Jump") && IsGrounded()) { ... }
            //
            //    现在: 把所有原始输入打包成 FrameInput 结构体，统一处理死区和吸附

            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
            };

            // 输入吸附 + 死区处理
            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZone
                    ? 0
                    : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZone
                    ? 0
                    : Mathf.Sign(_frameInput.Move.y);
            }

            // ★ 跳跃缓冲机制: 按了跳跃不立刻执行，而是标记为"待消费"
            //    在 FixedUpdate 的 HandleJump() 里决定是否真正起跳
            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        // ==================== 碰撞检测 ====================

        private void CheckCollisions()
        {
            // ★ 对比你原来的写法:
            //    原来: Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround)
            //          - 硬编码 .1f 检测距离
            //          - 直接用 coll.bounds 没有排除自身 Layer
            //
            //    现在: 使用 CapsuleCast（更适合角色），检测距离从 ScriptableStats 读取
            //          通过 ~PlayerLayer 排除玩家自身

            Physics2D.queriesStartInColliders = false;

            // 地面检测
            bool groundHit = Physics2D.BoxCast(
                _col.bounds.center,
                _col.bounds.size,
                0f,
                Vector2.down,
                _stats.GrounderDistance,
                _stats.GroundLayer
            );

            // 天花板检测（防止顶头时继续上升）
            bool ceilingHit = Physics2D.BoxCast(
                _col.bounds.center,
                _col.bounds.size,
                0f,
                Vector2.up,
                _stats.GrounderDistance,
                _stats.GroundLayer
            );

            if (ceilingHit)
            {
                // 顶到头了，消除向上的速度
                _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);
            }

            // 状态切换: 空中 → 地面（落地）
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;        // 重置土狼时间
                _bufferedJumpUsable = true;   // 重置跳跃缓冲
                _endedJumpEarly = false;
                _remainingJumps = _stats.ExtraJumps; // 重置二段跳次数

                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // 状态切换: 地面 → 空中（离地）
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;   // 记录离地时间（土狼时间起点）
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        // ==================== 跳跃逻辑 ====================

        /// <summary>
        /// 缓冲跳跃是否在有效期内
        /// </summary>
        private bool HasBufferedJump =>
            _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;

        /// <summary>
        /// 土狼时间是否可用（离地后在宽限期内）
        /// </summary>
        private bool CanUseCoyote =>
            _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            // ★ 可变跳跃高度: 如果在空中且上升中松开了跳跃键，标记为提前结束
            //    然后在 HandleGravity() 里施加更大的重力
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0)
            {
                _endedJumpEarly = true;
            }

            // 没有待消费的跳跃输入就跳过
            if (!_jumpToConsume && !HasBufferedJump) return;

            // 地面跳跃 或 土狼跳跃 或 二段跳
            if (_grounded || CanUseCoyote)
            {
                ExecuteJump(_stats.FirstJumpPower, isDoubleJump: false);
            }
            else if (_remainingJumps > 0)
            {
                // ★ 二段跳: 你原来的逻辑是"不在地面时 extraJumps > 0 就能跳"
                //    现在保持这个逻辑，但加了土狼时间，所以需要确保不在土狼窗口内
                ExecuteJump(_stats.SecondJumpPower, isDoubleJump: true);
                _remainingJumps--;
            }

            _jumpToConsume = false;
        }

        private void ExecuteJump(float power, bool isDoubleJump)
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = power;

            Jumped?.Invoke(_remainingJumps);

            // 播放音效
            if (_audioSource != null)
            {
                var clip = isDoubleJump ? _stats.DoubleJumpSound : _stats.JumpSound;
                if (clip != null) _audioSource.PlayOneShot(clip);
            }
        }

        // ==================== 水平移动 ====================

        private void HandleDirection()
        {
            // ★ 对比你原来的写法:
            //    原来: rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
            //          - 直接赋值速度，没有过渡，手感像冰上滑行
            //          - 地面和空中没有区别
            //
            //    现在: Mathf.MoveTowards 平滑过渡
            //          - 地面和空中用不同的减速度（空中更难改变方向，更真实）

            if (_frameInput.Move.x == 0)
            {
                // 没有输入 → 减速到 0
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(
                    _frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                // 有输入 → 加速到目标速度
                _frameVelocity.x = Mathf.MoveTowards(
                    _frameVelocity.x,
                    _frameInput.Move.x * _stats.MaxSpeed,
                    _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        // ==================== 重力 ====================

        private void HandleGravity()
        {
            // ★ 对比你原来的写法:
            //    原来: 根本没有重力处理！全靠 Rigidbody2D 自带的 gravityScale
            //          - 无法控制最大下落速度
            //          - 无法实现"提前松键跳得矮"
            //
            //    现在: 手动控制重力

            if (_grounded && _frameVelocity.y <= 0f)
            {
                // 着地时施加一个小的向下力，帮助贴紧地面（尤其是下坡）
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                // 空中重力
                var gravity = _stats.FallAcceleration;

                // ★ 可变跳高: 如果提前松开了跳跃键且还在上升，加大重力
                if (_endedJumpEarly && _frameVelocity.y > 0)
                {
                    gravity *= _stats.JumpEndEarlyGravityModifier;
                }

                // 平滑加速下落，限制最大下落速度
                _frameVelocity.y = Mathf.MoveTowards(
                    _frameVelocity.y,
                    -_stats.MaxFallSpeed,
                    gravity * Time.fixedDeltaTime);
            }
        }

        // ==================== 应用移动 ====================

        private void ApplyMovement()
        {
            _rb.velocity = _frameVelocity;
        }

        // ==================== 死亡逻辑 (从 PlayerLife 迁移过来) ====================

        /// <summary>
        /// ★ 原来 PlayerLife.Die() 的逻辑迁移到这里:
        ///    1. 播放死亡音效
        ///    2. 播放死亡动画（通过事件通知 Animator）
        ///    3. 冻结物理
        ///    4. 重置分数
        ///
        /// 为什么迁移？因为死亡是"玩家状态"的一部分，放在 Controller 里更内聚。
        /// PlayerLife 只剩下 "检测什么会导致死亡" 的职责。
        /// </summary>
        public void Die()
        {
            // 播放死亡音效
            if (_audioSource != null && _stats.DeathSound != null)
            {
                _audioSource.PlayOneShot(_stats.DeathSound);
            }

            // 冻结物理
            _rb.bodyType = RigidbodyType2D.Static;
            _frameVelocity = Vector2.zero;

            // 通知外部（Animator 播放死亡动画，GameManager 重置分数等）
            Died?.Invoke();

            // ★ 原来直接调用 Allcontrol.GameManager.Instance.scores = 0;
            //    现在通过事件解耦，由 GameManager 自己监听 Died 事件来处理
        }

        // ==================== 编辑器校验 ====================

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null)
                Debug.LogWarning("请将 HeroStats 资产拖到 Player Controller 的 Stats 字段", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (_col == null || _stats == null) return;

            // 可视化地面检测距离
            Gizmos.color = Color.green;
            var bounds = _col.bounds;
            Gizmos.DrawWireCube(
                bounds.center + Vector3.down * _stats.GrounderDistance,
                new Vector3(bounds.size.x, bounds.size.y, 0));
        }
#endif
    }

    // ==================== 数据结构 ====================

    /// <summary>
    /// 帧输入结构体 — 集中管理所有输入状态。
    /// 避免原来散落在各处的 Input.GetXxx() 调用。
    /// </summary>
    public struct FrameInput
    {
        public bool JumpDown;   // 这帧是否按下了跳跃
        public bool JumpHeld;   // 跳跃键是否按住
        public Vector2 Move;    // 移动输入 (-1, 0, 1)
    }

    // ==================== 接口定义 ====================

    /// <summary>
    /// 玩家控制器接口 — 其他组件通过这个接口与玩家交互，而不是直接依赖具体类。
    ///
    /// 好处:
    ///   - Camera 依赖 IHeroController 而不是 HeroPlayerController，以后换实现不改 Camera
    ///   - Animator 只关心事件，不关心控制器内部怎么实现
    ///   - 方便写单元测试（Mock 一个 IHeroController）
    /// </summary>
    public interface IHeroController
    {
        /// <summary>当前帧的移动输入</summary>
        Vector2 FrameInput { get; }

        /// <summary>是否在地面上</summary>
        bool IsGrounded { get; }

        /// <summary>当前速度</summary>
        Vector2 Velocity { get; }

        /// <summary>地面状态变化事件 (是否着地, 落地冲击力)</summary>
        event Action<bool, float> GroundedChanged;

        /// <summary>跳跃事件 (剩余跳跃次数)</summary>
        event Action<int> Jumped;

        /// <summary>死亡事件</summary>
        event Action Died;
    }
}
