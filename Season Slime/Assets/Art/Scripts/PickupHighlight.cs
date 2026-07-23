using UnityEngine;

/// <summary>
/// Companion component for the "SeasonSlime/PickupHighlight" shader.
/// Passes the UV rect of the SpriteRenderer's current sprite frame to the
/// material, so the rotating outline works correctly on sprite-sheet
/// (flipbook) sprites. Updates automatically when the animation swaps frames.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class PickupHighlight : MonoBehaviour
{
    private static readonly int SpriteRectId = Shader.PropertyToID("_SpriteRect");

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Sprite lastSprite;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        lastSprite = null;
        UpdateSpriteRect();
    }

    private void LateUpdate()
    {
        if (spriteRenderer.sprite != lastSprite)
        {
            UpdateSpriteRect();
        }
    }

    private void UpdateSpriteRect()
    {
        lastSprite = spriteRenderer.sprite;
        if (lastSprite == null)
        {
            return;
        }

        Texture2D texture = lastSprite.texture;
        Rect rect = lastSprite.textureRect;
        var uvRect = new Vector4(
            rect.x / texture.width,
            rect.y / texture.height,
            rect.width / texture.width,
            rect.height / texture.height);

        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetVector(SpriteRectId, uvRect);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}
