using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("教学内容")]
    [SerializeField] private GameObject tutorialUI;   // 要显示的教学UI/物体

    [Header("设置")]
    [SerializeField] private bool showOnce = false;   // 是否只显示一次

    private bool hasShown = false;

    private void Start()
    {
        // 一开始先隐藏教学
        if (tutorialUI != null)
            tutorialUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (showOnce && hasShown) return;

        if (tutorialUI != null)
            tutorialUI.SetActive(true);

        hasShown = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (showOnce) return;   // 只显示一次的话，离开也不隐藏

        if (tutorialUI != null)
            tutorialUI.SetActive(false);
    }
}