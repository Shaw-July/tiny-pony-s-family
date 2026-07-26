using UnityEngine;
using UnityEngine.UI;

public class UIanimCopy : MonoBehaviour
{
    public SpriteRenderer sourceSpriteRenderer;
    public Image targetImage;

    private void Start()
    {
        targetImage = GetComponent<Image>();
    }

    void LateUpdate()
    {
        if (sourceSpriteRenderer != null && targetImage != null)
        {
            // 核心：将2D物体的当前Sprite，赋值给UI Image的sprite
            targetImage.sprite = sourceSpriteRenderer.sprite;
        }
    }
}
