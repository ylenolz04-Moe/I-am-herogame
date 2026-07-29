using UnityEngine;


    [CreateAssetMenu]
    public class ScriptableStats : ScriptableObject
    {
        [Header("LAYERS")] [Tooltip("把这个设置为你玩家所在的图层")]
        public LayerMask PlayerLayer;

        [Header("INPUT")] [Tooltip("让所有输入强制取整，防止手柄摇杆导致角色走得太慢。建议设为 true，以保证手柄和键盘输入体验一致")]
        public bool SnapInput = true;

        [Tooltip("垂直摇杆死区阈值，低于此值的输入将被忽略，防止手柄误触导致误爬梯子"), Range(0.01f, 0.99f)]
        public float VerticalDeadZoneThreshold = 0.3f;

        [Tooltip("水平摇杆死区阈值，低于此值的输入将被忽略，防止手柄误触导致角色自动移动"), Range(0.01f, 0.99f)]
        public float HorizontalDeadZoneThreshold = 0.1f;

        [Header("MOVEMENT")] [Tooltip("最大水平移动速度")]
        public float MaxSpeed = 14;

        [Tooltip("水平加速度，值越大角色起跑越快")]
        public float Acceleration = 120;

        [Tooltip("地面减速度，值越大角色停下越快")]
        public float GroundDeceleration = 60;

        [Tooltip("空中减速度，在空中松开方向键后的减速力度")]
        public float AirDeceleration = 30;

        [Tooltip("着地时持续施加的向下力，帮助贴合斜坡"), Range(0f, -10f)]
        public float GroundingForce = -1.5f;

        [Tooltip("地面及头顶检测的射线距离"), Range(0f, 0.5f)]
        public float GrounderDistance = 0.05f;

        [Header("JUMP")] [Tooltip("跳跃瞬间施加的初速度")]
        public float JumpPower = 36;

        [Tooltip("最大下落速度上限")]
        public float MaxFallSpeed = 40;

        [Tooltip("下落加速度（空中重力）")]
        public float FallAcceleration = 110;

        [Tooltip("提前松开跳跃键时的额外重力倍率，实现短按小跳、长按大跳")]
        public float JumpEndEarlyGravityModifier = 3;

        [Tooltip("土狼时间：离开平台后仍可跳跃的宽恕时间")]
        public float CoyoteTime = .15f;

        [Tooltip("跳跃缓冲时间：落地前提前按跳的有效窗口")]
        public float JumpBuffer = .2f;
    }
