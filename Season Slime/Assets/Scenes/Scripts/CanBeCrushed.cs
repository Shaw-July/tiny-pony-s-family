using UnityEngine;

public class CanBeCrushed : MonoBehaviour
{
    private Animator anim;
    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void DestroyObj()
    {
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CurrentSeason identity = other.gameObject.GetComponent<CurrentSeason>();
            if (identity != null && identity.currentSeason == CurrentSeason.SeasonIdentifier.Autumn)
            {
                anim.SetTrigger("Crushed");
            }
        }
    }
}
