using UnityEngine;

[DefaultExecutionOrder(500)]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class SlimeSquashStretch : MonoBehaviour
{
    [Header("Velocity Shape")]
    [SerializeField] private float verticalVelocityForFullStretch = 9f;
    [SerializeField] private float maximumAirStretch = 0.16f;
    [SerializeField] private float horizontalVelocityForFullStretch = 4f;
    [SerializeField] private float maximumRunStretch = 0.06f;
    [SerializeField] private float shapeSmoothTime = 0.06f;

    [Header("Jump And Landing")]
    [SerializeField] private float jumpStretch = 0.18f;
    [SerializeField] private float jumpVelocityThreshold = 1.5f;
    [SerializeField] private float minimumLandingSpeed = 2f;
    [SerializeField] private float maximumLandingSpeed = 12f;
    [SerializeField] private float minimumLandingSquash = 0.1f;
    [SerializeField] private float maximumLandingSquash = 0.28f;

    [Header("Rebound")]
    [SerializeField] private float springStrength = 160f;
    [SerializeField] private float springDamping = 12f;

    private Rigidbody2D rb;
    private SpriteRenderer sourceRenderer;
    private SpriteRenderer visualRenderer;
    private Transform visualTransform;
    private MaterialPropertyBlock propertyBlock;

    private Vector2 continuousShape;
    private Vector2 continuousShapeVelocity;
    private Vector2 impulseShape;
    private Vector2 impulseVelocity;
    private float previousVerticalVelocity;
    private float fastestDownwardSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sourceRenderer = GetComponent<SpriteRenderer>();

        if (sourceRenderer == null)
        {
            enabled = false;
            return;
        }

        CreateVisualProxy();
    }

    private void OnEnable()
    {
        if (sourceRenderer == null || visualRenderer == null)
            return;

        sourceRenderer.forceRenderingOff = true;
        visualRenderer.enabled = sourceRenderer.enabled;
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;

        if (velocity.y < 0f)
            fastestDownwardSpeed = Mathf.Max(fastestDownwardSpeed, -velocity.y);

        if (velocity.y >= jumpVelocityThreshold &&
            previousVerticalVelocity < jumpVelocityThreshold)
        {
            impulseShape = new Vector2(-jumpStretch * 0.55f, jumpStretch);
            impulseVelocity = Vector2.zero;
        }

        previousVerticalVelocity = velocity.y;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool landed = false;
        float collisionSpeed = 0f;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;
            if (normal.y <= 0.55f)
                continue;

            landed = true;
            collisionSpeed = Mathf.Max(
                collisionSpeed,
                Mathf.Abs(Vector2.Dot(collision.relativeVelocity, normal)));
        }

        if (!landed)
            return;

        float landingSpeed = Mathf.Max(collisionSpeed, fastestDownwardSpeed);
        fastestDownwardSpeed = 0f;

        if (landingSpeed < minimumLandingSpeed)
            return;

        float strength = Mathf.InverseLerp(
            minimumLandingSpeed,
            maximumLandingSpeed,
            landingSpeed);
        float squash = Mathf.Lerp(minimumLandingSquash, maximumLandingSquash, strength);
        impulseShape = new Vector2(squash, -squash);
        impulseVelocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (visualRenderer == null)
            return;

        CopyRendererState();
        UpdateShape();
    }

    private void UpdateShape()
    {
        Vector2 velocity = rb.linearVelocity;

        float verticalAmount = Mathf.Clamp01(
            Mathf.Abs(velocity.y) / Mathf.Max(0.01f, verticalVelocityForFullStretch));
        float airStretch = verticalAmount * maximumAirStretch;

        float horizontalAmount = Mathf.Clamp01(
            Mathf.Abs(velocity.x) / Mathf.Max(0.01f, horizontalVelocityForFullStretch));
        float runStretch = horizontalAmount * maximumRunStretch;

        Vector2 targetShape = new Vector2(
            runStretch - airStretch * 0.55f,
            airStretch - runStretch * 0.55f);

        float deltaTime = Mathf.Min(Time.deltaTime, 0.033f);
        continuousShape = Vector2.SmoothDamp(
            continuousShape,
            targetShape,
            ref continuousShapeVelocity,
            shapeSmoothTime,
            Mathf.Infinity,
            deltaTime);

        impulseVelocity += -impulseShape * springStrength * deltaTime;
        impulseVelocity *= Mathf.Exp(-springDamping * deltaTime);
        impulseShape += impulseVelocity * deltaTime;

        Vector2 shape = continuousShape + impulseShape;
        float scaleX = Mathf.Clamp(1f + shape.x, 0.68f, 1.38f);
        float scaleY = Mathf.Clamp(1f + shape.y, 0.68f, 1.38f);

        visualTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        AnchorSpriteBottom(scaleY);
    }

    private void CreateVisualProxy()
    {
        GameObject visualObject = new GameObject("Squash Visual");
        visualObject.layer = gameObject.layer;
        visualTransform = visualObject.transform;
        visualTransform.SetParent(transform, false);

        visualRenderer = visualObject.AddComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();

        sourceRenderer.forceRenderingOff = true;
        CopyRendererState();
    }

    private void CopyRendererState()
    {
        visualRenderer.enabled = sourceRenderer.enabled;
        visualRenderer.sprite = sourceRenderer.sprite;
        visualRenderer.color = sourceRenderer.color;
        visualRenderer.flipX = sourceRenderer.flipX;
        visualRenderer.flipY = sourceRenderer.flipY;
        visualRenderer.drawMode = sourceRenderer.drawMode;
        visualRenderer.size = sourceRenderer.size;
        visualRenderer.maskInteraction = sourceRenderer.maskInteraction;
        visualRenderer.spriteSortPoint = sourceRenderer.spriteSortPoint;
        visualRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        visualRenderer.sortingOrder = sourceRenderer.sortingOrder;
        visualRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
        visualRenderer.renderingLayerMask = sourceRenderer.renderingLayerMask;

        sourceRenderer.GetPropertyBlock(propertyBlock);
        visualRenderer.SetPropertyBlock(propertyBlock);
    }

    private void AnchorSpriteBottom(float scaleY)
    {
        Sprite sprite = visualRenderer.sprite;
        if (sprite == null)
        {
            visualTransform.localPosition = Vector3.zero;
            return;
        }

        float bottom = sprite.bounds.min.y;
        visualTransform.localPosition = new Vector3(0f, bottom * (1f - scaleY), 0f);
    }

    private void OnDisable()
    {
        if (sourceRenderer != null)
            sourceRenderer.forceRenderingOff = false;

        if (visualRenderer != null)
            visualRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        if (sourceRenderer != null)
            sourceRenderer.forceRenderingOff = false;
    }
}
