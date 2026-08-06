using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMoment : MonoBehaviour
{
    [Header("跳跃手感")]
    [Tooltip("下落时额外重力倍率。1=正常重力，3=三倍重力。越大落得越快")]
    public float FallGravityMultiplier = 2.5f;
    [Tooltip("最大下落速度上限，防止无限制加速")]
    public float MaxFallSpeed = 20f;

    public ParticleSystem MoveParticle;
    private Rigidbody2D rb;

    private SpriteRenderer sprite;
    private Animator anim;
    private BoxCollider2D coll;
    private float dirX = 0f;
    
    private int extraJumps;
    private int JumpCount;
    private bool JumpHold;
    public float JumpAddition=1.5f;
    public float FallAddition=3.5f;
    [SerializeField] private LayerMask jumpableGround;
    [Header("土狼跳")]
    [SerializeField] private float coyoteTime = 0.2f;//土狼时间：离开平台后仍可跳跃的宽恕时间
    private float coyoteTimeCounter;//土狼时间计数器
    [Header("拉墙跳")]
    [SerializeField] private float wallJumpX;
    [SerializeField] private float wallJumpY;

    [SerializeField] private int extraJumpsValue = 1;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float FirstjumpForce = 10f;
    [SerializeField] private float SecondjumpForce = 7f;
    [SerializeField] private AudioSource JumpSoundEffect;
    //serializefield有点像public，但是在unity编辑器里可以设置值，private则不行
    /// </summary>
    /// // Start is called before the first frame update
    /// 
    private enum MovementState { idle, running, jumping, falling, doubleJumping }
    //这个就是我在unity里面加的那些动画状态，idle就是站立，running就是跑步，jumping就是跳跃，falling就是下落
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        extraJumps = extraJumpsValue;
        JumpCount = 0;
    }
    
    #region Update

    // Update is called once per frame
    private void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        JumpHold = Input.GetButton("Jump");
        // ★ 松手或下落时切换为高倍重力，自然减速无断层
        if (rb.velocity.y < 0 || (rb.velocity.y > 0 && !Input.GetButton("Jump")))
        {
            rb.gravityScale = FallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        // ★ 限制最大下落速度，不会无限加速
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -MaxFallSpeed, MaxFallSpeed));

        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded())
            {
                JumpSoundEffect.Play();
                rb.velocity = new Vector2(rb.velocity.x, FirstjumpForce);
                coyoteTimeCounter = coyoteTime;
                extraJumps = extraJumpsValue;
                JumpCount = 1;
            }
            else if (extraJumps > 0)
            {
                JumpSoundEffect.Play();
                rb.velocity = new Vector2(rb.velocity.x, SecondjumpForce);
                extraJumps--;
                JumpCount = 2;
            }
            else if (coyoteTimeCounter > 0f)
            {
                JumpSoundEffect.Play();
                rb.velocity = new Vector2(rb.velocity.x, FirstjumpForce);
                coyoteTimeCounter = 0f;
                extraJumps = extraJumpsValue;
                JumpCount = 1;
            }
        }
        if (IsGrounded())
        {
            JumpCount = 0;
            coyoteTimeCounter = 0f;
        }
        updateAnimationstate();
    }
    #endregion
    public void ParticlePlay()
    {
        if (IsGrounded()&&dirX != 0f)
        {
            if (!MoveParticle.isPlaying)
            {
                MoveParticle.Play();
            }
                        
        }else if(!IsGrounded())
        {
            MoveParticle.Stop();
        }
        
    }
   
    private void updateAnimationstate()
    {
        MovementState state;
        if (dirX > 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;
            ParticlePlay();
        }
        else if (dirX < 0f)
        {
            state = MovementState.running;
            sprite.flipX = true;
            ParticlePlay();
        }
        else
        {
            state = MovementState.idle;

        }
        if (rb.velocity.y > .1f && extraJumps ==1)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }
        else if(rb.velocity.y != 0 && extraJumps == 0 && JumpCount == 2)
        {
            state = MovementState.doubleJumping;
        }
        anim.SetInteger("state", (int)state);
    }
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
}
