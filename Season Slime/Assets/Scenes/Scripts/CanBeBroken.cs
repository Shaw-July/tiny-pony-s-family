using UnityEngine;

public class CanBeBroken : MonoBehaviour
{
    private WinterSlimeMode winSlime;
    private Animator animator;
    private bool isBreaking = false;   // 防止重复触发

    private void Start()
    {
        animator = GetComponent<Animator>();

        // animator.enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            winSlime = other.gameObject.GetComponent<WinterSlimeMode>();
        }
    }

    private void Update()
    {
        if (isBreaking) return;   // 已经在破裂中，不再重复

        if (winSlime != null && winSlime.breakIce)
        {
            isBreaking = true;
            winSlime.breakIce = false;

            // 触发破裂动画
            animator.SetTrigger("Break");

            // 等动画播完再销毁
            Destroy(gameObject, 1f);
        }
    }
}