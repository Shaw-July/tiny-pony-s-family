using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bubble : MonoBehaviour
{
    public enum PromptType
    {
        Start,
        Spring,
        Winter
    }

    [Header("Bubble")]
    [SerializeField] private GameObject bubbleObject;

    [Header("Prompt Pages")]
    [SerializeField] private GameObject startPrompt;
    [SerializeField] private GameObject springPrompt;
    [SerializeField] private GameObject winterPrompt;

    [Header("Follow Settings")]
    [SerializeField] private Transform followTarget;      // 留空则用挂载本脚本的对象
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private bool flipOffsetWithFacing = false; // 气泡是否随朝向左右偏移

    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;

    private Coroutine hideCoroutine;
    private bool isShowing;
    private Vector3 originalBubbleScale;

    public bool IsShowing => isShowing;

    private void Awake()
    {
        if (followTarget == null)
            followTarget = transform;

        if (bubbleObject != null)
        {
            originalBubbleScale = bubbleObject.transform.localScale;
            // 脱离角色层级，不再受角色翻转影响
            bubbleObject.transform.SetParent(null, true);
            bubbleObject.transform.localScale = originalBubbleScale;
        }
    }

    private void Start()
    {
        ShowPrompt(PromptType.Start);
    }

    private void Update()
    {
        if (!isShowing)
            return;

        bool clicked =
            (Mouse.current != null &&
             Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null &&
             Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

        if (clicked)
        {
            HideBubble();
        }
    }

    private void LateUpdate()
    {
        if (bubbleObject == null || followTarget == null)
            return;

        Vector3 finalOffset = offset;
        if (flipOffsetWithFacing)
        {
            float facing = Mathf.Sign(followTarget.lossyScale.x);
            finalOffset.x *= facing;
        }

        Transform bubbleTransform = bubbleObject.transform;
        bubbleTransform.position = followTarget.position + finalOffset;
        bubbleTransform.rotation = Quaternion.identity;
        bubbleTransform.localScale = originalBubbleScale;
    }

    public void ShowPrompt(PromptType type)
    {
        HideAllPrompts();

        switch (type)
        {
            case PromptType.Start:
                if (startPrompt != null)
                    startPrompt.SetActive(true);
                break;
            case PromptType.Spring:
                if (springPrompt != null)
                    springPrompt.SetActive(true);
                break;
            case PromptType.Winter:
                if (winterPrompt != null)
                    winterPrompt.SetActive(true);
                break;
        }

        if (bubbleObject != null)
        {
            bubbleObject.SetActive(true);
        }

        isShowing = true;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private void HideAllPrompts()
    {
        if (startPrompt != null)
            startPrompt.SetActive(false);
        if (springPrompt != null)
            springPrompt.SetActive(false);
        if (winterPrompt != null)
            winterPrompt.SetActive(false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(displayDuration);
        HideBubble();
    }

    public void HideBubble()
    {
        if (!isShowing)
            return;

        isShowing = false;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (bubbleObject != null)
        {
            bubbleObject.SetActive(false);
        }

        HideAllPrompts();
    }

    private void OnDestroy()
    {
        // 角色被销毁时，一并清掉已脱离层级的气泡
        if (bubbleObject != null)
        {
            Destroy(bubbleObject);
        }
    }
}