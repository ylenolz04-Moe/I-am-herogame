using UnityEngine;

namespace TarodevController.RefactoringGuide
{
    /// <summary>
    /// 【参考文件】扩展自 Tarodev 的 ScriptableStats，加入你的游戏特有的参数。
    ///
    /// 相比你原来把参数散落在各个 MonoBehaviour 的 [SerializeField] 里：
    ///   优点1: 一个 .asset 文件管理全部参数，不同角色/难度可以创建不同预设
    ///   优点2: 策划可以在 Inspector 里直接调参，不需要改代码
    ///   优点3: 多个 Prefab 可以共用同一个 Stats 资产，一处修改全局生效
    ///   优点4: 运行时修改的值不会污染资产文件（ScriptableObject 在 Play 后的修改不会保存）
    ///
    /// 使用方法：在 Unity 中右键 → Create → Hero Stats 创建资产，拖到 Controller 上即可。
    /// </summary>
    [CreateAssetMenu(menuName = "Hero/Hero Stats", fileName = "HeroStats")]
    public class HeroScriptableStats : ScriptableObject
    {
        // ==================== 层级设置 ====================
        [Header("LAYERS")]
        [Tooltip("玩家的 Layer，用于地面检测时排除自身")]
        public LayerMask PlayerLayer;

        [Tooltip("可踩踏的地面 Layer（对应你原来的 jumpableGround）")]
        public LayerMask GroundLayer;

        // ==================== 输入设置 ====================
        [Header("INPUT")]
        [Tooltip("开启后输入会吸附到整数 -1/0/1，防止手柄摇杆漂移。推荐开启")]
        public bool SnapInput = true;

        [Tooltip("水平方向死区：输入绝对值小于此值时视为0。防止手柄漂移")]
        [Range(0.01f, 0.99f)]
        public float HorizontalDeadZone = 0.1f;

        [Tooltip("竖直方向死区")]
        [Range(0.01f, 0.99f)]
        public float VerticalDeadZone = 0.3f;

        // ==================== 移动参数 ====================
        [Header("MOVEMENT")]
        [Tooltip("最大水平移动速度（对应你原来的 moveSpeed=7）")]
        public float MaxSpeed = 14f;

        [Tooltip("水平加速度：值越大，达到 MaxSpeed 越快。你原来没有这个，是瞬间变速")]
        public float Acceleration = 120f;

        [Tooltip("地面摩擦力/减速度：松开方向键后多快停下来")]
        public float GroundDeceleration = 60f;

        [Tooltip("空中减速度：空中松开方向键后的减速")]
        public float AirDeceleration = 30f;

        [Tooltip("着地时向下的恒定力，帮助贴合斜坡"), Range(0f, -10f)]
        public float GroundingForce = -1.5f;

        [Tooltip("地面检测距离（你原来硬编码为 .1f）"), Range(0f, 0.5f)]
        public float GrounderDistance = 0.05f;

        // ==================== 跳跃参数 ====================
        [Header("JUMP")]
        [Tooltip("一段跳的力度（对应你原来的 FirstjumpForce=7）")]
        public float FirstJumpPower = 36f;

        [Tooltip("二段跳的力度（对应你原来的 SecondjumpForce=5）。"
               + "注意 Tarodev 用物理加速度而非直接设速度，所以数值比你原来大很多")]
        public float SecondJumpPower = 28f;

        [Tooltip("最大下落速度")]
        public float MaxFallSpeed = 40f;

        [Tooltip("空中重力加速度（值越大下落越快）")]
        public float FallAcceleration = 110f;

        [Tooltip("提前松开跳跃键时的额外重力倍率。让玩家可以控制跳跃高度："
               + "轻按=小跳，长按=大跳。这是你原来没有的功能")]
        public float JumpEndEarlyGravityModifier = 3f;

        [Tooltip("土狼时间（秒）：离开平台后的宽限期，期间仍可跳跃。"
               + "你原来没有这个，离开平台瞬间就无法跳跃，手感偏硬")]
        public float CoyoteTime = 0.15f;

        [Tooltip("跳跃缓冲（秒）：落地前提前按跳跃的缓冲时间。"
               + "你原来没有这个，必须在着地后才能按跳跃")]
        public float JumpBuffer = 0.2f;

        [Tooltip("二段跳次数（对应你原来的 extraJumpsValue=1）")]
        public int ExtraJumps = 1;

        // ==================== 音效 ====================
        [Header("AUDIO")]
        [Tooltip("跳跃音效（对应你原来的 JumpSoundEffect）。放在这里是方便统一管理，"
               + "你也可以选择保留在 Animator 那边")]
        public AudioClip JumpSound;

        [Tooltip("二段跳音效")]
        public AudioClip DoubleJumpSound;

        [Tooltip("死亡音效")]
        public AudioClip DeathSound;

        [Tooltip("落地音效")]
        public AudioClip[] LandSounds;

        [Tooltip("脚步声")]
        public AudioClip[] FootstepSounds;

        // ==================== 视觉特效 ====================
        [Header("VISUAL FX")]
        [Tooltip("跳跃时产生的粒子效果预制体")]
        public GameObject JumpParticlesPrefab;

        [Tooltip("二段跳粒子")]
        public GameObject DoubleJumpParticlesPrefab;

        [Tooltip("落地粒子")]
        public GameObject LandParticlesPrefab;

        // ==================== 编辑器校验 ====================
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 确保数值在合理范围内
            if (FirstJumpPower <= 0) FirstJumpPower = 1;
            if (MaxSpeed <= 0) MaxSpeed = 1;
            if (ExtraJumps < 0) ExtraJumps = 0;
        }
#endif
    }
}
