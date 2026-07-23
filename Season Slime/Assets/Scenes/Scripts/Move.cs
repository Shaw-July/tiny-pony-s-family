using Unity.VisualScripting;
using UnityEngine;

public class Move : MonoBehaviour
{
    [SerializeField]private float moveSpeed = 3f;
    [SerializeField] private float jumpForce = 8f;

    private float xInput;
    private Rigidbody2D rb;
    private bool facingRight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void SlimeMove()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }

    private void SlimeJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
        SlimeFlip();
    }
}
