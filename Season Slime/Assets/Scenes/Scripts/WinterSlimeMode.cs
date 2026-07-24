using UnityEngine;

public class WinterSlimeMode : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    private float xInput;
    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void SlimeMove()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }

    private void SlimeJumpAndSmash()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
        }
        else if (isGrounded && Input.GetKeyDown(KeyCode.E))
        {
            rb.gravityScale = 2f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce *2);
            anim.SetTrigger("Smash");
            Invoke(nameof(ResetGravity), 1.5f);
        }
    }
    private void ResetGravity()
    {
        rb.gravityScale = 1f;
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
        HandleFlip();
        HandleAnim();
        SlimeJumpAndSmash();
    }
}
