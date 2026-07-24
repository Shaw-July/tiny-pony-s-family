using UnityEngine;

public class SummerSlimeMode : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float groundCheckDistance = 0.7f;

    private float xInput;
    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight;

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

    private void HandleSlim()
    {
        if (Input.GetKey(KeyCode.E))
        {
            anim.SetBool("PressE", true);
        }
        else
        {
            anim.SetBool("PressE", false);
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
        HandleFlip();
        HandleAnim();
        HandleSlim();
    }
}
