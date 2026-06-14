using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TarodevController.RefactoringGuide
{
    /// <summary>
    /// 【参考文件】重构后的游戏管理器 — 改进自你原来的 Allcontrol.cs。
    ///
    /// === 与你原来 Allcontrol.cs 的关键区别 ===
    ///
    /// 原来:
    ///   - GameManager 是 Allcontrol 里的嵌套类，命名不直观
    ///   - 单例用 private constructor + static field 手动实现
    ///   - scores 是 public field（违反封装原则）
    ///   - 通过 "using static Allcontrol.GameManager" 导入，增加耦合
    ///   - 不继承 MonoBehaviour，不能挂到场景里
    ///
    /// 现在:
    ///   - GameManager 直接继承 MonoBehaviour，可以挂在场景 GameObject 上
    ///   - 单例用标准的 Instance 属性 + DontDestroyOnLoad
    ///   - scores 用属性和事件封装，外部修改会触发 UI 更新
    ///   - 通过 IHeroController 事件来响应玩家状态（解耦）
    ///
    /// === 如何迁移 ===
    ///
    /// 最小改动：把你 Allcontrol.GameManager 里的 scores 改成属性，
    /// 加一个 OnScoreChanged 事件让 UI 订阅即可。
    /// </summary>
    public class HeroGameManager : MonoBehaviour
    {
        // ==================== 单例 ====================

        private static HeroGameManager _instance;
        public static HeroGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<HeroGameManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[HeroGameManager]");
                        _instance = go.AddComponent<HeroGameManager>();
                    }
                }
                return _instance;
            }
        }

        // ==================== Inspector ====================

        [Header("Player Reference")]
        [SerializeField] private HeroPlayerController _player;

        [Header("Scene Settings")]
        [SerializeField] private int _nextSceneBuildIndex = -1; // -1 = 当前场景 + 1

        // ==================== 属性 (带事件通知) ====================

        private int _scores;

        /// <summary>
        /// ★ 对比你原来: public int scores = 0;
        ///    现在是属性 + 事件，任何修改都会自动通知 UI 刷新
        /// </summary>
        public int Scores
        {
            get => _scores;
            set
            {
                if (_scores != value)
                {
                    _scores = value;
                    OnScoresChanged?.Invoke(_scores);
                }
            }
        }

        // ==================== 事件 ====================

        /// <summary>分数变化事件 —— UI 订阅这个来更新显示</summary>
        public event Action<int> OnScoresChanged;

        /// <summary>玩家死亡事件</summary>
        public event Action OnPlayerDied;

        /// <summary>关卡完成事件</summary>
        public event Action OnLevelCompleted;

        // ==================== Unity 生命周期 ====================

        private void Awake()
        {
            // 单例初始化
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 自动查找玩家
            if (_player == null)
                _player = FindObjectOfType<HeroPlayerController>();

            // ★ 订阅玩家事件 —— 解耦！GameManager 不需要知道 PlayerLife 的存在
            if (_player != null)
                _player.Died += OnPlayerDeath;
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.Died -= OnPlayerDeath;
        }

        // ==================== 事件响应 ====================

        private void OnPlayerDeath()
        {
            // ★ 你原来在 PlayerLife.Die() 里写:
            //    Allcontrol.GameManager.Instance.scores = 0;
            //
            //    现在 GameManager 自己监听死亡事件来重置分数。
            //    PlayerLife 完全不需要知道 GameManager 的存在！
            Scores = 0;
            OnPlayerDied?.Invoke();
        }

        // ==================== 公共方法 ====================

        /// <summary>
        /// 增加分数（供 item_collector 等调用）
        /// </summary>
        public void AddScore(int amount)
        {
            Scores += amount;
        }

        /// <summary>
        /// 加载下一关
        /// </summary>
        public void LoadNextLevel()
        {
            int nextIndex = _nextSceneBuildIndex >= 0
                ? _nextSceneBuildIndex
                : SceneManager.GetActiveScene().buildIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                OnLevelCompleted?.Invoke();
                SceneManager.LoadSceneAsync(nextIndex);
            }
            else
            {
                Debug.Log("已经是最后一关了！");
            }
        }

        /// <summary>
        /// 重新加载当前关卡
        /// </summary>
        public void ReloadLevel()
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
