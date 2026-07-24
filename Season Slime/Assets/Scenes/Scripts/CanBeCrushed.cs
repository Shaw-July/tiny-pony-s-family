using UnityEngine;

public class CanBeCrushed : MonoBehaviour
{
    private Animator anim;
    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
                anim.SetTrigger("Crushed");
                Destroy(gameObject);
        }
    }
}
