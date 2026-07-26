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
    private SpringSuperJumpVFX superJumpVFX;
    private bool facingRight;
    private bool isGrounded;

    private PlayerAudio playerAudio; //����PlayerAudio���
    private bool wasGrounded;  //���ڼ���Ƿ�ӿ������
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        playerAudio = GetComponent<PlayerAudio>(); //��ȡPlayerAudio���

        superJumpVFX = GetComponent<SpringSuperJumpVFX>();

    }

    private void SlimeMove()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);

        //�ڵ�������ˮƽ�ٶȴ���һ��С��ֵʱ�������������������Ч
        bool isWalking = isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        playerAudio.HandleFootsteps(isWalking);
        

    }

    [SerializeField] private float minLandingSpeed = 0.5f; //��С����ٶ���ֵ

    private void SlimeJump()
    {
        float fallSpeed = rb.linearVelocity.y;
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        // ����Ƿ�ӿ������
        if (isGrounded && !wasGrounded && fallSpeed < -minLandingSpeed)
        {
            playerAudio.PlayLand(); //���������Ч
        }
        wasGrounded = isGrounded; //����wasGrounded״̬

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");

            playerAudio.PlayJump(); //������Ծ��Ч
            superJumpVFX?.Play();

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
