using UnityEngine;
using UnityEngine.SceneManagement;

public class SummerSlimeMode : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Trap"))
        {
            anim.SetTrigger("Dead");
            Invoke(nameof(Restart), 1f);
        }
    }

    private void Restart()
    {
        SceneManager.LoadScene(1);
    }

    void Update()
    {
        SlimeMove();
        HandleFlip();
        HandleAnim();
        HandleSlim();
    }
}
