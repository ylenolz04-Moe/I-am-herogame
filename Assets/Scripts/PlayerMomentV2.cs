using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMomentV2 : MonoBehaviour
{
    public float MaxFallSpeed = 20f;
    public ParticleSystem MoveParticle;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private BoxCollider2D coll;
    private float dirX = 0f;
    
    private int extraJumps;
    private int JumpCount;
    //滑墙所需的
    #region 滑墙
    bool isTouchingWall;
    public Transform wallCheck;
    bool wallSliding;
    public float wallSlidingSpeed;
    bool wallJumping;
    bool hasWallJumped;               // 墙跳只能一次，落地或离墙后重置
    public float xwallforce;
    public float ywallforce;
    public float wallJumpTime;

    #endregion
    public float JumpAddition = 1.5f;
    public float FallAddition = 3.5f;
    [SerializeField] private LayerMask jumpableGround;
    [Header("土狼跳")]
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    

    [SerializeField] private int extraJumpsValue = 1;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float FirstjumpForce = 10f;
    [SerializeField] private float SecondjumpForce = 7f;
    [SerializeField] private AudioSource JumpSoundEffect;

    private enum MovementState { idle, running, jumping, falling, doubleJumping ,wallsliding}

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        extraJumps = extraJumpsValue;
        JumpCount = 0;
    }

    private void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal");
       

        // ── 水平移动 ──
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        // ── 跳跃触发（必须在 Update 里，GetButtonDown 才不漏帧）──
        if (Input.GetButtonDown("Jump")&& !wallSliding)
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
            hasWallJumped = false;          // 落地重置墙跳
        }

    isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, jumpableGround);
        if (isTouchingWall && !IsGrounded() && dirX != 0)
        {
            wallSliding = true;
        }
        else
        {
            wallSliding = false;
        }
        if (wallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        if(Input.GetButtonDown("Jump") && wallSliding && !hasWallJumped)
        {
            wallJumping = true;
            hasWallJumped = true;
            Invoke("SetWallJumpingToFalse", wallJumpTime);
        }
        if (wallJumping)
        {
            rb.velocity = new Vector2(-dirX * xwallforce, ywallforce);
        }
        updateAnimationstate();
    }

    void SetWallJumpingToFalse()
    {
        wallJumping = false;
    } 
    private void FixedUpdate()
    {
        // ── 土狼时间倒计时 ──
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        // ── 可变跳跃高度 & 下落加速 ──
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (FallAddition - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (JumpAddition - 1) * Time.fixedDeltaTime;
        }

        // ── 最大下落速度限制 ──
        if (rb.velocity.y < -MaxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -MaxFallSpeed);
        }
    }

    #region 粒子 & 动画

    public void ParticlePlay()
    {
        if (IsGrounded() && dirX != 0f)
        {
            if (!MoveParticle.isPlaying)
            {
                MoveParticle.Play();
            }
        }
        else if (!IsGrounded())
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

        if (rb.velocity.y > .1f && extraJumps == 1)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }
        else if (rb.velocity.y != 0 && extraJumps == 0 && JumpCount == 2)
        {
            state = MovementState.doubleJumping;
        }
        if (wallSliding)
        {
            state = MovementState.wallsliding;
        }
        anim.SetInteger("state", (int)state);
    }

    #endregion

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
}
