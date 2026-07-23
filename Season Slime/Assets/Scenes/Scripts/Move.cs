using Unity.VisualScripting;
using UnityEngine;

public class Move : MonoBehaviour
{
    [SerializeField]private float moveSpeed = 3f;
    [SerializeField]private float jumpForce = 8f;
    [SerializeField]private float groundCheckDistance = 0.1f;
    [SerializeField]private bool isGrounded;
    [SerializeField]private LayerMask groundLayer;

    private float xInput;
    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void SlimeMove()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }

    private void SlimeJump()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

    }

    private void HandleAnim()
    {
        bool isMoving = rb.linearVelocity.x != 0;
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
    }

    private void HandleFlip()
    {
        if(rb.linearVelocity.x > 0 && !facingRight)
        {
            SlimeFlip();
        }
        else if(rb.linearVelocity.x < 0 && facingRight)
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
