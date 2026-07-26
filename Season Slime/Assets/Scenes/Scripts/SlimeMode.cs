using System.Collections.Generic;
using UnityEngine;

public class SlimeMode : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    private float xInput;
    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight;
    private bool isGrounded;

    private PlayerAudio playerAudio; //引用PlayerAudio组件
    private bool wasGrounded;  //用于检测是否从空中落地
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerAudio = GetComponent<PlayerAudio>(); //获取PlayerAudio组件
    
    }

    private void SlimeMove()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);

        //在地面上且水平速度大于一个小阈值时按节奏随机播放行走音效
        bool isWalking = isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        playerAudio.HandleFootsteps(isWalking);
        

    }

    [SerializeField] private float minLandingSpeed = 0.5f; //最小落地速度阈值

    private void SlimeJump()
    {
        float fallSpeed = rb.linearVelocity.y;
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        // 检测是否从空中落地
        if (isGrounded && !wasGrounded && fallSpeed < -minLandingSpeed)
        {
            playerAudio.PlayLand(); //播放落地音效
        }
        wasGrounded = isGrounded; //更新wasGrounded状态

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
            playerAudio.PlayJump(); //播放跳跃音效
        }

    }

    private void HandleAnim()
    {
        bool isMoving = rb.linearVelocity.x != 0;
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
    }

    private void HandleFlip()
    {
        if (rb.linearVelocity.x > 0 && !facingRight)
        {
            SlimeFlip();
        }
        else if (rb.linearVelocity.x < 0 && facingRight)
        {
            SlimeFlip();
        }
    }

    private void SlimeFlip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }

    void Update()
    {
        SlimeMove();
        SlimeJump();
        HandleFlip();
        HandleAnim();
    }
}
