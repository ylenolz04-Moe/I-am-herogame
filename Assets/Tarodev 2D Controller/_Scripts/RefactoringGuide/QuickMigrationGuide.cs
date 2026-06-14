using UnityEngine;

namespace TarodevController.RefactoringGuide
{
    /// <summary>
    /// 【速查手册】不需要一次性重写整个项目！
    ///
    /// 这个文件不是可运行的代码，而是一系列"替换配方"。
    /// 每个配方都是对原来代码的一个独立改进，可以单独应用，互不依赖。
    /// 按照难度从低到高排列，建议从上到下逐步应用。
    ///
    /// 用法: 找到你原来的代码片段 → 用右边的代码替换 → 测试 → 继续下一个配方
    /// </summary>
    public static class QuickMigrationGuide
    {
        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 1: 缓存 Animator 参数哈希  [难度: ★☆☆]  [影响: 性能]  ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 原来:
        //   anim.SetInteger("state", (int)state);
        //
        // 改为:
        //   // 在类的顶部声明:
        //   private static readonly int StateHash = Animator.StringToHash("state");
        //   // 使用时:
        //   anim.SetInteger(StateHash, (int)state);
        //
        // 原因: Animator.StringToHash("state") 每帧做字符串哈希 → GC 分配
        //       static readonly 只算一次 → 零开销

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 2: 物理逻辑移到 FixedUpdate  [难度: ★★☆]  [影响: 稳定性] ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 原来 (PlayerMoment.Update):
        //   private void Update()
        //   {
        //       dirX = Input.GetAxisRaw("Horizontal");
        //       rb.velocity = new Vector2(dirX*moveSpeed, rb.velocity.y);  // ← 物理在 Update
        //       if (Input.GetButtonDown("Jump") && IsGrounded())
        //       {
        //           rb.velocity = new Vector2(rb.velocity.x, jumpForce);   // ← 物理在 Update
        //       }
        //       updateAnimationstate();
        //   }
        //
        // 改为:
        //   private Vector2 _pendingVelocity;  // 新增字段
        //
        //   private void Update()
        //   {
        //       dirX = Input.GetAxisRaw("Horizontal");
        //       if (Input.GetButtonDown("Jump") && IsGrounded())
        //       {
        //           _pendingVelocity.y = jumpForce;  // 只记录，不执行
        //       }
        //       updateAnimationstate();
        //   }
        //
        //   private void FixedUpdate()  // 新增方法
        //   {
        //       _pendingVelocity.x = dirX * moveSpeed;
        //       if (_pendingVelocity.y == 0)
        //           _pendingVelocity.y = rb.velocity.y;
        //       rb.velocity = _pendingVelocity;
        //       _pendingVelocity = Vector2.zero;
        //   }
        //
        // 原因: Update 随渲染帧率变化（30/60/144 FPS 结果不同）
        //       FixedUpdate 固定 50 FPS，物理模拟稳定

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 3: 加速/减速替代瞬移  [难度: ★★☆]  [影响: 手感]       ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 原来:
        //   rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        //
        // 改为:
        //   float acceleration = 120f;   // 新增 SerializeField
        //   float deceleration = 60f;    // 新增 SerializeField
        //   float maxSpeed = 7f;         // 代替 moveSpeed
        //
        //   if (dirX == 0)
        //   {
        //       // 没按方向 → 减速
        //       _velocity.x = Mathf.MoveTowards(_velocity.x, 0, deceleration * Time.fixedDeltaTime);
        //   }
        //   else
        //   {
        //       // 按了方向 → 加速
        //       _velocity.x = Mathf.MoveTowards(_velocity.x, dirX * maxSpeed, acceleration * Time.fixedDeltaTime);
        //   }
        //   rb.velocity = _velocity;
        //
        // 原因: 瞬间变速看起来像滑冰。有加速过程更自然，像"推动"角色。
        //       deceleration 比 acceleration 小 = 松手后有惯性滑行。

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 4: 跳跃缓冲 (Jump Buffer)  [难度: ★★☆]  [影响: 手感]  ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 在你的 PlayerMoment 里加这几行:
        //
        //   // 新增字段
        //   private float _timeJumpWasPressed;
        //   private float _time;
        //   private bool _jumpBuffered;
        //
        //   private void Update()
        //   {
        //       _time += Time.deltaTime;
        //       dirX = Input.GetAxisRaw("Horizontal");
        //
        //       if (Input.GetButtonDown("Jump"))
        //       {
        //           _timeJumpWasPressed = _time;
        //           _jumpBuffered = true;
        //       }
        //
        //       // ... 其他逻辑
        //   }
        //
        //   // 修改跳跃条件:
        //   // 原来: if (Input.GetButtonDown("Jump") && IsGrounded())
        //   // 改为: if (_jumpBuffered && IsGrounded())
        //   //       {
        //   //           _jumpBuffered = false;
        //   //           rb.velocity = new Vector2(rb.velocity.x, FirstjumpForce);
        //   //       }
        //
        //   // 缓冲过期（在 Update 末尾）:
        //   if (_jumpBuffered && _time > _timeJumpWasPressed + 0.2f)
        //       _jumpBuffered = false;
        //
        // 原因: 玩家经常在落地前 1~2 帧按跳跃，没有缓冲就会"吞输入"。
        //       0.2 秒的缓冲窗口让玩家感觉"按了就能跳"。

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 5: 土狼时间 (Coyote Time)  [难度: ★★☆]  [影响: 手感]  ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 在你的 PlayerMoment 里加这几行:
        //
        //   // 新增字段
        //   private float _frameLeftGrounded = float.MinValue;
        //   private float _time;
        //
        //   // 修改 IsGrounded 调用处:
        //   // 原来: if (Input.GetButtonDown("Jump") && IsGrounded())
        //   // 改为: if (Input.GetButtonDown("Jump") && (IsGrounded() || CanCoyoteJump()))
        //
        //   // 新增方法
        //   private bool CanCoyoteJump()
        //   {
        //       return !IsGrounded() && _time < _frameLeftGrounded + 0.15f;
        //   }
        //
        //   // 在 FixedUpdate 或 Update 中检测离地时间:
        //   // 每帧检查 IsGrounded()，当从 true 变 false 时:
        //   //   _frameLeftGrounded = _time;
        //
        // 原因: 玩家经常在平台边缘起跳，视觉上觉得"还在平台上"但物理已经离地。
        //       0.15 秒宽限期让玩家感觉"不会无缘无故跳不出来"。

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 6: 可变跳跃高度  [难度: ★★☆]  [影响: 手感]            ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 原来: 按了跳跃就是固定高度
        //
        // 改为:
        //   private bool _endedJumpEarly;
        //
        //   private void Update()
        //   {
        //       // 检测提前松键
        //       if (!_endedJumpEarly && !IsGrounded() && !Input.GetButton("Jump") && rb.velocity.y > 0)
        //       {
        //           _endedJumpEarly = true;
        //       }
        //   }
        //
        //   private void FixedUpdate()
        //   {
        //       // 在重力处理部分:
        //       float gravity = rb.gravityScale * Physics2D.gravity.y; // 或自定义重力值
        //       if (_endedJumpEarly && rb.velocity.y > 0)
        //       {
        //           gravity *= 3f;  // 提前松键 → 3 倍重力，快速下落
        //       }
        //       // ... 应用 gravity 到 rb.velocity
        //
        //       // 着地时重置
        //       if (IsGrounded()) _endedJumpEarly = false;
        //   }
        //
        // 原因: 这是现代平台游戏标配（超级马里奥从初代就有）。
        //       让玩家精确控制跳跃高度，大幅提升操控感。

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 7: 事件解耦动画  [难度: ★★★]  [影响: 架构]            ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 原来: 动画逻辑直接写在 PlayerMoment.updateAnimationstate() 里
        //
        // 改为分两步:
        //
        // 步骤A - 在 PlayerMoment 里加事件:
        //   using System;  // 文件顶部加这行
        //
        //   public event Action<bool, float> GroundedChanged;
        //   public event Action Jumped;
        //
        //   // 落地时:
        //   GroundedChanged?.Invoke(true, Mathf.Abs(rb.velocity.y));
        //   // 离地时:
        //   GroundedChanged?.Invoke(false, 0);
        //   // 跳跃时:
        //   Jumped?.Invoke();
        //
        // 步骤B - 创建独立的 Animator 脚本 (参考 HeroPlayerAnimator.cs):
        //   在 OnEnable 里订阅 player.Jumped += OnJumped;
        //   在 OnDisable 里取消订阅
        //   在 OnJumped 方法里只处理动画和音效
        //
        // 原因: PlayerMoment 的职责是"移动"，不是"动画"。
        //       分离后每个类只做一件事，改动画不影响移动逻辑。

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 8: ScriptableObject 管理参数  [难度: ★★★]  [影响: 架构] ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 原来: 所有参数散落在 PlayerMoment 的 [SerializeField] 里
        //       moveSpeed=7, jumpForce=7, extraJumpsValue=1 ...
        //
        // 改为:
        //   1. 创建 HeroScriptableStats.cs（参考本文件夹的 HeroScriptableStats.cs）
        //   2. 在 Unity 中右键 → Create → Hero → Hero Stats
        //   3. 在 PlayerMoment 中:
        //        [SerializeField] private HeroScriptableStats _stats;
        //      然后用 _stats.MaxSpeed 代替 moveSpeed
        //      用 _stats.FirstJumpPower 代替 FirstjumpForce
        //      ...
        //
        // 原因: 一个 .asset 文件包含所有参数，可以在不同角色/难度之间复用。
        //       策划可以直接在 Inspector 调参，不需要动代码。

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 9: 接口解耦 (依赖倒置)  [难度: ★★★]  [影响: 架构]      ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 原来: Camera 直接引用 Transform player
        //       PlayerLife 直接 GetComponent<Rigidbody2D>() 来操作物理
        //
        // 改为:
        //   1. 定义 IHeroController 接口 (参考 HeroPlayerController.cs 底部)
        //   2. PlayerMoment 实现 IHeroController
        //   3. Camera 依赖 IHeroController 而不是具体 Transform
        //   4. PlayerLife 调用 IHeroController.Die() 而不是自己改 Rigidbody2D
        //
        // 原因: 接口像"插座标准"。插头（Camera）只要符合标准，
        //       插哪个插座（PlayerMoment 或 HeroPlayerController）都能工作。

        // ╔══════════════════════════════════════════════════════════════╗
        // ║  配方 10: 输入死区 + 吸附  [难度: ★☆☆]  [影响: 手柄兼容]    ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 原来:
        //   dirX = Input.GetAxisRaw("Horizontal");
        //
        // 改为:
        //   dirX = Input.GetAxisRaw("Horizontal");
        //   // 死区过滤（手柄摇杆漂移）
        //   if (Mathf.Abs(dirX) < 0.1f) dirX = 0;
        //   // 吸附到整数（键盘/手柄行为一致）
        //   else dirX = Mathf.Sign(dirX);
        //
        // 原因: 手柄摇杆在松开时可能不完全归零，导致角色微微移动。
        //       SnapInput 让手柄和键盘手感一致。

        // ╔══════════════════════════════════════════════════════════════╗
        // ║                    迁移建议优先级                           ║
        // ╚══════════════════════════════════════════════════════════════╝
        //
        // 第 1 周（手感立竿见影）:
        //   - 配方 3: 加速/减速
        //   - 配方 4: 跳跃缓冲
        //   - 配方 5: 土狼时间
        //
        // 第 2 周（代码质量）:
        //   - 配方 1: 缓存 Animator 哈希
        //   - 配方 2: FixedUpdate 分离
        //   - 配方 6: 可变跳跃高度
        //
        // 第 3 周（架构重构）:
        //   - 配方 7: 事件解耦动画
        //   - 配方 8: ScriptableObject
        //   - 配方 9: 接口解耦
        //
        // 核心原则: 每次只改一个配方，改完立刻测试！不要一口气全改。
    }
}
