using UnityEngine;

public class TutorialPromptTrigger2D : MonoBehaviour
{
    [SerializeField] private Bubble.PromptType promptType;
    [SerializeField] private bool showOnlyOnce = true;
    [SerializeField] private string playerTag = "Player";

    [SerializeField] private Bubble tutorialBubble;  // 在 Inspector 里直接拖引用

    private bool hasTriggered;

    private void Awake()
    {
        if (tutorialBubble == null)
        {
            tutorialBubble = FindObjectOfType<Bubble>(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (showOnlyOnce && hasTriggered) return;

        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            return;

        if (tutorialBubble == null) return;

        hasTriggered = true;
        tutorialBubble.ShowPrompt(promptType);
    }
}