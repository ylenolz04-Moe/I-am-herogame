using UnityEngine;

namespace TarodevController.RefactoringGuide
{
    /// <summary>
    /// 【参考文件】平滑跟随相机 — 改进自你原来的 Camera.cs。
    ///
    /// === 与你原来 Camera.cs 的关键区别 ===
    ///
    /// 原来:
    ///   - 直接硬跟随: transform.position = new Vector3(player.position.x, player.position.y, z)
    ///   - 每一帧都紧贴玩家，画面抖动
    ///   - 通过 [SerializeField] private Transform player 直接引用 Transform
    ///   - 类名 "Camera" 和 Unity 内置的 Camera 类重名，容易冲突
    ///
    /// 现在:
    ///   - 使用 IHeroController 接口而非直接引用 Transform（解耦）
    ///   - 支持平滑阻尼: 相机不会紧贴玩家，而是逐渐跟过去（更自然）
    ///   - 支持 LookAhead: 相机可以预判玩家移动方向，提前偏移
    ///   - 支持垂直/水平锁定: 可以锁死某个轴
    ///   - 支持限制区域: 相机不会超出关卡边界
    ///   - 类名 CameraFollow 不会和 UnityEngine.Camera 冲突
    ///
    /// === 如何迁移 ===
    ///
    /// 最简单: 先加一个 smoothSpeed，让跟踪有阻尼感:
    ///   Vector3 target = new Vector3(player.position.x, player.position.y, transform.position.z);
    ///   transform.position = Vector3.Lerp(transform.position, target, smoothSpeed * Time.deltaTime);
    /// </summary>
    public class HeroCameraFollow : MonoBehaviour
    {
        // ==================== Inspector ====================

        [Header("Target")]
        [SerializeField] private Transform _playerTransform;  // 简单方案：直接拖入
        // 高级方案: 用接口查找（注释掉，二选一）
        // private IHeroController _player;

        [Header("Follow Settings")]
        [Tooltip("跟随平滑度: 0=紧贴, 1=最平滑")]
        [SerializeField, Range(0f, 1f)] private float _smoothSpeed = 0.15f;

        [Tooltip("是否跟随 X 轴")]
        [SerializeField] private bool _followX = true;

        [Tooltip("是否跟随 Y 轴")]
        [SerializeField] private bool _followY = true;

        [Header("Look Ahead")]
        [Tooltip("启用预判偏移: 相机看向玩家移动方向的前方")]
        [SerializeField] private bool _useLookAhead = true;

        [Tooltip("预判偏移量（玩家速度 * 此系数 = 相机偏移）")]
        [SerializeField] private float _lookAheadFactor = 0.3f;

        [Tooltip("预判偏移的平滑速度")]
        [SerializeField] private float _lookAheadSmoothSpeed = 2f;

        [Header("Bounds (Optional)")]
        [Tooltip("限制相机范围，留空表示不限制")]
        [SerializeField] private BoxCollider2D _boundsCollider;

        [Header("Offset")]
        [Tooltip("相机相对于玩家的固定偏移")]
        [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10);

        // ==================== 私有状态 ====================

        private Vector3 _velocity = Vector3.zero;       // SmoothDamp 用的速度缓冲
        private Vector2 _currentLookAhead;               // 当前预判偏移
        private Vector2 _targetLookAhead;                // 目标预判偏移
        private UnityEngine.Camera _cam;                 // 缓存 Camera 组件

        // ==================== Unity 生命周期 ====================

        private void Awake()
        {
            _cam = GetComponent<UnityEngine.Camera>();

            // ★ 高级方案: 从场景中自动查找 IHeroController
            // if (_player == null)
            //     _player = FindObjectOfType<HeroPlayerController>() as IHeroController;
        }

        private void LateUpdate()
        {
            // ★ 相机移动应该在 LateUpdate 中执行
            //    确保玩家已经完成这一帧的移动后再跟踪
            //    你原来放在 Update 里，如果玩家在 Update 中移动会有 1 帧延迟

            if (_playerTransform == null) return;

            FollowTarget();
        }

        // ==================== 核心逻辑 ====================

        private void FollowTarget()
        {
            // 1. 计算目标位置
            Vector3 targetPos = _playerTransform.position + _offset;

            // 2. 预判偏移 (Look Ahead)
            if (_useLookAhead)
            {
                UpdateLookAhead();
                targetPos += (Vector3)_currentLookAhead;
            }

            // 3. 限制范围
            if (_boundsCollider != null)
            {
                targetPos = ClampToBounds(targetPos);
            }

            // 4. 平滑跟随
            // ★ 关键改进: 使用 SmoothDamp 而非直接赋值
            //    你原来: transform.position = new Vector3(player.position.x, player.position.y, z);
            //    现在:   SmoothDamp 有缓入缓出效果
            Vector3 destination = new Vector3(
                _followX ? targetPos.x : transform.position.x,
                _followY ? targetPos.y : transform.position.y,
                targetPos.z  // Z 轴始终跟随
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                destination,
                ref _velocity,
                _smoothSpeed
            );
        }

        private void UpdateLookAhead()
        {
            // 从 Rigidbody2D 获取玩家速度做预判
            // 如果你用 Transform 引用，需要通过 GetComponent 获取
            Rigidbody2D playerRb = _playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                _targetLookAhead = playerRb.velocity * _lookAheadFactor;
            }

            // 平滑过渡预判偏移（避免相机突然跳变）
            _currentLookAhead = Vector2.Lerp(
                _currentLookAhead,
                _targetLookAhead,
                _lookAheadSmoothSpeed * Time.deltaTime);
        }

        private Vector3 ClampToBounds(Vector3 targetPos)
        {
            if (_cam == null) return targetPos;

            Bounds bounds = _boundsCollider.bounds;
            float camHalfHeight = _cam.orthographicSize;
            float camHalfWidth = camHalfHeight * _cam.aspect;

            // 限制相机边缘不超出边界
            targetPos.x = Mathf.Clamp(targetPos.x,
                bounds.min.x + camHalfWidth,
                bounds.max.x - camHalfWidth);
            targetPos.y = Mathf.Clamp(targetPos.y,
                bounds.min.y + camHalfHeight,
                bounds.max.y - camHalfHeight);

            return targetPos;
        }

        // ==================== 编辑器辅助 ====================

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_boundsCollider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(_boundsCollider.bounds.center, _boundsCollider.bounds.size);
            }

            // 显示预判偏移
            if (_useLookAhead && _playerTransform != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_playerTransform.position,
                    _playerTransform.position + (Vector3)_currentLookAhead);
                Gizmos.DrawSphere(_playerTransform.position + (Vector3)_currentLookAhead, 0.1f);
            }
        }
#endif
    }
}
