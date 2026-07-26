using UnityEngine;
using UnityEngine.SceneManagement;

public class WinterSlimeMode : MonoBehaviour
{
    public bool breakIce = false;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField, Range(0.05f, 0.12f)] private float smashHitStopDuration = 0.08f;

    private float xInput;
    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight;
    private bool isGrounded;
    private bool isSmashing = false;
    private bool hasLeftGround;

    private PlayerAudio playerAudio; //引用PlayerAudio组件

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

    private void SlimeJumpAndSmash()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
            playerAudio.PlayJump(); //播放跳跃音效
        }
        else if (isGrounded && Input.GetKeyDown(KeyCode.E) && !isSmashing)
        {
            rb.gravityScale = 2f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce *2);
            anim.SetTrigger("Smash");
            isSmashing = true;
            hasLeftGround = false;
        }
    }

    private void CheckSmashLanding()
    {
        if (!isSmashing) return;
        if (!isGrounded)
            hasLeftGround = true;

        if (hasLeftGround && isGrounded)
        {
            rb.gravityScale = 1f;
            breakIce = true;
            GameFeelController.RequestHitStop(smashHitStopDuration);
            isSmashing = false;
            hasLeftGround = false;
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

    private void Restart()
    {
        SceneManager.LoadScene(2);
        SeasonManager.CycleCount = 0;
    }

    void Update()
    {
        SlimeMove();
        HandleFlip();
        HandleAnim();
        SlimeJumpAndSmash();
        CheckSmashLanding();

        if(SeasonManager.CycleCount >= 2)
        {
            anim.SetTrigger("Dead");
            Invoke(nameof(Restart), 1f);
        }
    }
}
