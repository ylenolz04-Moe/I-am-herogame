using UnityEngine;
using UnityEngine.SceneManagement;

namespace TarodevController.RefactoringGuide
{
    /// <summary>
    /// 【参考文件】重构后的玩家生命管理 — 只负责"检测伤害"，不处理死亡逻辑。
    ///
    /// === 与你原来 PlayerLife.cs 的关键区别 ===
    ///
    /// 原来:
    ///   - Die() 方法里做了三件事: 播音效 + 改动画 + 重置分数
    ///   - 直接调用 Allcontrol.GameManager.Instance.scores = 0 (强耦合单例)
    ///   - 直接改 rb.bodyType = Static (跨职责操作)
    ///   - RestartLevel() 定义了但从未被调用
    ///
    /// 现在:
    ///   - PlayerLife 只检测伤害来源，调用 Controller.Die()
    ///   - Controller.Die() 通过事件通知 Animator 和 GameManager
    ///   - 单例通过接口抽象，方便测试和替换
    ///   - 死亡后的重启逻辑有明确的生命周期
    ///
    /// === 如何迁移 ===
    ///
    /// 最简单的第一步：把 Die() 里的 rb.bodyType 操作移到 PlayerMoment 里，
    /// 只让 PlayerLife 调用一个 public void Die() 方法即可。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HeroPlayerLife : MonoBehaviour
    {
        // ==================== Inspector ====================

        [Header("References")]
        [SerializeField] private HeroPlayerController _controller;

        [Header("Settings")]
        [SerializeField] private float _deathFreezeDelay = 0.5f;     // 死亡后多久冻结
        [SerializeField] private float _restartDelay = 2f;            // 死亡后多久重开
        [SerializeField] private bool _restartOnDeath = true;

        [Header("Damage Layers")]
        [Tooltip("哪些 Layer 的碰撞算作伤害")]
        [SerializeField] private LayerMask _damageLayers;

        [Tooltip("哪些 Tag 的碰撞算作伤害")]
        [SerializeField] private string[] _damageTags = { "Trap" };

        // ==================== 私有状态 ====================
        private bool _isDead;

        // ==================== Unity 生命周期 ====================

        private void Awake()
        {
            if (_controller == null)
                _controller = GetComponent<HeroPlayerController>();

            // 订阅自己的死亡事件（Controller 触发 Died 事件后，这里做后续处理）
            if (_controller != null)
                _controller.Died += OnPlayerDied;
        }

        private void OnDestroy()
        {
            if (_controller != null)
                _controller.Died -= OnPlayerDied;
        }

        // ==================== 碰撞检测 ====================

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_isDead) return; // 已经死了就别再触发

            if (IsDamageSource(collision))
            {
                HandleDamage(collision);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isDead) return;

            if (IsDamageSource(collision.collider))
            {
                HandleDamage(collision.collider);
            }
        }

        /// <summary>
        /// 判断碰撞体是否是伤害来源
        /// ★ 原来只用 CompareTag("Trap")，现在同时支持 Layer 和 Tag 判断，更灵活
        /// </summary>
        private bool IsDamageSource(Collider2D other)
        {
            // 检查 Layer
            if (_damageLayers != 0 && ((1 << other.gameObject.layer) & _damageLayers) != 0)
                return true;

            // 检查 Tag（兼容你原来的写法）
            foreach (var tag in _damageTags)
            {
                if (other.CompareTag(tag))
                    return true;
            }

            return false;
        }

        // ==================== 伤害处理 ====================

        private void HandleDamage(Collider2D source)
        {
            _isDead = true;

            // ★ 核心改变: 只通知 Controller 执行死亡，不自己动手改 Rigidbody2D
            //    Controller 负责: 冻结物理 + 播放死亡音效 + 触发 Died 事件
            //    Animator 监听 Died → 播放死亡动画
            //    GameManager 监听 Died → 重置分数
            _controller?.Die();
        }

        // ==================== 死亡后续 ====================

        /// <summary>
        /// Controller 触发 Died 事件后执行
        /// </summary>
        private void OnPlayerDied()
        {
            // 延迟后重启关卡
            if (_restartOnDeath)
            {
                Invoke(nameof(RestartLevel), _restartDelay);
            }
        }

        private void RestartLevel()
        {
            // ★ 原来你用的是 SceneManager.LoadScene(name)，这会同步加载
            //    LoadSceneAsync 不会卡顿，体验更好
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }

        // ==================== 公共 API ====================

        /// <summary>
        /// 外部也可以调用（比如从 UI 按钮重开）
        /// </summary>
        public void RequestRestart()
        {
            if (!_isDead) _controller?.Die();
            RestartLevel();
        }

        public bool IsDead() => _isDead;
    }
}
