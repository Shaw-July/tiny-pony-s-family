using UnityEngine;

public class SeasonChange : MonoBehaviour
{
    [SerializeField] private int season;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(season == 1)
            {
                Destroy(gameObject);
            }
            else if(season == 2)
            {
                //Destroy(gameObject);
            }
            else if(season == 3)
            {
                Destroy(gameObject);
            }
            else if(season == 4)
            {
                Destroy(gameObject);
            }
        }
    }
}
