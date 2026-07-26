using UnityEngine;
using UnityEngine.SceneManagement;

public class ToNextLevel : MonoBehaviour
{
    [SerializeField] private int nextLevel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Load the next level
            SceneManager.LoadScene(nextLevel);
            SeasonManager.CycleCount = 0;
        }
    }
}
