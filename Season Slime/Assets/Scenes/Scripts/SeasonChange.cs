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
                print("changed to season 1");
            }
            else if(season == 2)
            {
                //Destroy(gameObject);
                print("changed to season 2");
            }
            else if(season == 3)
            {
                Destroy(gameObject);
                print("changed to season 3");
            }
            else if(season == 4)
            {
                Destroy(gameObject);
                print("changed to season 4");
            }
        }
    }
}
