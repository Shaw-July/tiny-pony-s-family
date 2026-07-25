using UnityEngine;

public class CanBeBroken : MonoBehaviour
{
    private WinterSlimeMode winSlime;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            winSlime = other.gameObject.GetComponent<WinterSlimeMode>();
        }
    }

    private void Update()
    {
        if (winSlime != null && winSlime.breakIce)
        {
            Destroy(gameObject);
            winSlime.breakIce = false;
        }
    }
}
