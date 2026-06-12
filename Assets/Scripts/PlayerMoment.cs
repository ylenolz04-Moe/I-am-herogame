using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMoment : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private BoxCollider2D coll;
    //这个是我在unity里面设置的一个变量
    // 这些都是用来控制玩家的物理属性，动画属性和碰撞属性的
    private float dirX = 0f;
    [SerializeField] private LayerMask jumpableGround;
    //这个就是我在unity里面设置的一个变量，jumpableGround是一个LayerMask类型的变量，
    // LayerMask就是一个用来判断某个物体是否在某个层级上的工具，这个变量就是用来判断玩家是否在地面上的
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private AudioSource JumpSoundEffect;
    //serializefield有点像public，但是在unity编辑器里可以设置值，private则不行
    /// </summary>
    /// // Start is called before the first frame update
    /// 
    private enum MovementState { idle, running, jumping, falling }
    //这个就是我在unity里面加的那些动画状态，idle就是站立，running就是跑步，jumping就是跳跃，falling就是下落
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(dirX*moveSpeed,rb.velocity.y);
        
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            JumpSoundEffect.Play();
            rb.velocity = new Vector2(rb.velocity.x,jumpForce);
        }
        updateAnimationstate();
    }
    private void updateAnimationstate()
    {
        MovementState state;
        if (dirX > 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;
        }
        else if (dirX < 0f)
        {
            state = MovementState.running;
    
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;

        }
        if (rb.velocity.y > .1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }
        anim.SetInteger("state", (int)state);
    }
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
}
