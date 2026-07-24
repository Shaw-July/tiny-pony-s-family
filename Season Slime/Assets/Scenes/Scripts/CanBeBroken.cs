using UnityEngine;

public class CanBeBroken : MonoBehaviour
{
    [SerializeField] private GameObject hintUI;
    [SerializeField] private float animationLength = 1.0f;
    private bool isPlayerInRange = false;
    private Animator anim;
    private SlimeMode slimeMode;
    private bool isPlayingAction = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            hintUI.SetActive(true);
            anim = other.GetComponent<Animator>();
            slimeMode = other.GetComponent<SlimeMode>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            hintUI.SetActive(false);
        }
    }

    private void TriggerInteraction()
    {
        if (isPlayingAction) return;
        anim.SetTrigger("Attack");
        isPlayingAction = true;
        slimeMode.enabled = false;
        Invoke(nameof(ResetAction), animationLength);
        Destroy(gameObject);
    }

    private void ResetAction()
    {
        isPlayingAction = false;
        slimeMode.enabled = true;
    }
    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TriggerInteraction();
        }
    }
}
