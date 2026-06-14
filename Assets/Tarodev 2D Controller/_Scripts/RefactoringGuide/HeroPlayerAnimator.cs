using UnityEngine;

namespace TarodevController.RefactoringGuide
{
    /// <summary>
    /// 【参考文件】独立的动画控制器 — 只负责视觉和音效，不碰物理逻辑。
    ///
    /// === 与你原来写法的关键区别 ===
    ///
    /// 原来 (PlayerMoment.updateAnimationstate):
    ///   - 每帧检查 rb.velocity.y 和 dirX 来判断动画状态
    ///   - 动画逻辑和移动逻辑混在同一个类里
    ///   - 用 SetInteger("state", (int)state) 切换动画 —— 字符串查找 + 单一状态机参数
    ///
    /// 现在:
    ///   - 通过事件订阅来响应状态变化（Jumped → 播放跳跃动画，
    ///     GroundedChanged → 播放落地动画）
    ///   - 动画逻辑完全独立，不关心控制器内部怎么实现
    ///   - 用 Animator.StringToHash 缓存参数名 → 静态只读 int，零 GC 分配
    ///   - 每个动画状态有独立的 Trigger/Bool 参数，比单一 state 整数更灵活
    ///
    /// === 如何迁移 ===
    ///
    /// 你不需要一次性改成事件驱动。可以先做最简单的改进：
    ///   1. 把 anim.SetInteger("state", ...) 改成 anim.SetInteger(stateHash, ...)
    ///      其中 stateHash = Animator.StringToHash("state") 在 Start 时缓存
    ///   2. 把 MovementState 枚举拆成独立的 bool 参数 (isRunning, isJumping 等)
    ///   3. 最后再把动画逻辑抽到单独的类里
    /// </summary>
    public class HeroPlayerAnimator : MonoBehaviour
    {
        // ==================== Inspector 引用 ====================
        [Header("References")]
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private AudioSource _audioSource;

        [Header("Settings")]
        [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2f;
        [SerializeField] private float _maxTilt = 5f;
        [SerializeField] private float _tiltSpeed = 20f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _jumpParticles;
        [SerializeField] private ParticleSystem _doubleJumpParticles;
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private ParticleSystem _landParticles;

        // ==================== 私有状态 ====================
        private IHeroController _player;
        private bool _grounded;

        // ★ 缓存 Animator 参数哈希值 —— 避免每帧字符串查找
        //    你原来: anim.SetInteger("state", ...)  → 每次调用内部做字符串哈希
        //    现在:   anim.SetInteger(StateHash, ...) → 直接传整数，零开销
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int IdleSpeedHash = Animator.StringToHash("IdleSpeed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int DoubleJumpHash = Animator.StringToHash("DoubleJump");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int XVelocityHash = Animator.StringToHash("xVelocity");
        private static readonly int YVelocityHash = Animator.StringToHash("yVelocity");

        // ==================== Unity 生命周期 ====================

        private void Awake()
        {
            // 自动查找组件（如果 Inspector 没拖入）
            if (_anim == null) _anim = GetComponent<Animator>();
            if (_sprite == null) _sprite = GetComponentInChildren<SpriteRenderer>();
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

            // ★ 关键: 从父物体获取 IHeroController 接口
            //    不依赖具体类，只依赖接口 —— 这就是"依赖倒置"
            _player = GetComponentInParent<IHeroController>();
        }

        private void OnEnable()
        {
            // ★ 订阅事件 —— 只在状态变化时响应，而不是每帧轮询
            if (_player != null)
            {
                _player.Jumped += OnJumped;
                _player.GroundedChanged += OnGroundedChanged;
                _player.Died += OnDied;
            }

            if (_moveParticles != null) _moveParticles.Play();
        }

        private void OnDisable()
        {
            // ★ 取消订阅 —— 防止内存泄漏
            if (_player != null)
            {
                _player.Jumped -= OnJumped;
                _player.GroundedChanged -= OnGroundedChanged;
                _player.Died -= OnDied;
            }

            if (_moveParticles != null) _moveParticles.Stop();
        }

        private void Update()
        {
            if (_player == null) return;

            HandleSpriteFlip();
            HandleIdleSpeed();
            HandleCharacterTilt();
            UpdateVelocityParams();
        }

        // ==================== 视觉处理 ====================

        private void HandleSpriteFlip()
        {
            // ★ 对比你原来的:
            //    if (dirX > 0f) sprite.flipX = false;
            //    else if (dirX < 0f) sprite.flipX = true;
            //    现在一行搞定:
            if (_player.FrameInput.x != 0)
                _sprite.flipX = _player.FrameInput.x < 0;
        }

        private void HandleIdleSpeed()
        {
            // 根据输入强度调整 Idle 动画的播放速度（走/跑切换）
            var inputStrength = Mathf.Abs(_player.FrameInput.x);
            _anim.SetFloat(IdleSpeedHash, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));

            // 移动粒子的大小随速度变化
            if (_moveParticles != null)
            {
                _moveParticles.transform.localScale = Vector3.MoveTowards(
                    _moveParticles.transform.localScale,
                    Vector3.one * inputStrength,
                    2 * Time.deltaTime);
            }
        }

        private void HandleCharacterTilt()
        {
            // 跑步时角色微微倾斜
            var targetUp = _grounded
                ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x) * Vector2.up
                : Vector2.up;
            _anim.transform.up = Vector3.RotateTowards(
                _anim.transform.up, targetUp, _tiltSpeed * Time.deltaTime, 0f);
        }

        private void UpdateVelocityParams()
        {
            // 把速度传给 Animator，让 Blend Tree 自动混合动画
            // 这是你原来用枚举 state 做不到的平滑过渡
            _anim.SetFloat(XVelocityHash, Mathf.Abs(_player.Velocity.x));
            _anim.SetFloat(YVelocityHash, _player.Velocity.y);
        }

        // ==================== 事件响应 ====================

        private void OnJumped(int remainingJumps)
        {
            if (remainingJumps <= 0)
            {
                // 最后一段跳（二段跳）
                _anim.SetTrigger(DoubleJumpHash);
                if (_doubleJumpParticles != null) _doubleJumpParticles.Play();
            }
            else
            {
                // 一段跳
                _anim.SetTrigger(JumpHash);
            }

            _anim.ResetTrigger(GroundedHash);

            // 地面起跳才播放跳跃粒子（土狼跳不播）
            if (_grounded && _jumpParticles != null)
            {
                _jumpParticles.Play();
            }
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;

            if (grounded)
            {
                // 落地
                _anim.SetTrigger(GroundedHash);

                // 落地粒子：冲击力越大粒子越大
                if (_landParticles != null)
                {
                    _landParticles.transform.localScale =
                        Vector3.one * Mathf.InverseLerp(0, 40, impact);
                    _landParticles.Play();
                }

                if (_moveParticles != null) _moveParticles.Play();
            }
            else
            {
                // 离地
                if (_moveParticles != null) _moveParticles.Stop();
            }
        }

        private void OnDied()
        {
            // 播放死亡动画
            _anim.SetTrigger(DeathHash);

            // 停止所有粒子
            if (_moveParticles != null) _moveParticles.Stop();
            if (_jumpParticles != null) _jumpParticles.Stop();
        }

        // ==================== 脚步声（通过 Animation Event 调用） ====================

        /// <summary>
        /// 由动画事件触发，不需要每帧检测
        /// </summary>
        public void PlayFootstep(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
                _audioSource.PlayOneShot(clip);
        }
    }
}
