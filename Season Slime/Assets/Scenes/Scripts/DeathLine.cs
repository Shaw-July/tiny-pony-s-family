using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathLine : MonoBehaviour
{
    [SerializeField] private AudioSource deathAudioSource; // Reference to the AudioSource component
    [SerializeField] private AudioClip deathClip; // Reference to the death sound clip
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DieRoutine()); // Start the death routine
        }
    }

    private IEnumerator DieRoutine()
    {
        deathAudioSource.PlayOneShot(deathClip); // Play the death sound
        yield return new WaitForSeconds(deathClip.length); // Wait for the sound to finish
        SceneManager.LoadScene(1); // Reload the scene

    }
}
