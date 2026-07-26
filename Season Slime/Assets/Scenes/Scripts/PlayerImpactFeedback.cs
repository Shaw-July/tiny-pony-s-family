using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerImpactFeedback : MonoBehaviour
{
    [Header("Landing Shake")]
    [SerializeField] private float minimumLandingSpeed = 3.5f;
    [SerializeField] private float maximumLandingSpeed = 13f;
    [SerializeField] private float minimumLandingAmplitude = 0.025f;
    [SerializeField] private float maximumLandingAmplitude = 0.16f;
    [SerializeField] private float minimumLandingDuration = 0.08f;
    [SerializeField] private float maximumLandingDuration = 0.18f;

    [Header("Wall Impact Shake")]
    [SerializeField] private float minimumWallImpactSpeed = 2.5f;
    [SerializeField] private float maximumWallImpactSpeed = 8f;
    [SerializeField] private float minimumWallAmplitude = 0.025f;
    [SerializeField] private float maximumWallAmplitude = 0.12f;
    [SerializeField] private float wallImpactMultiplier = 1f;
    [SerializeField] private float wallImpactCooldown = 0.12f;

    [Header("Speed Trail")]
    [SerializeField] private Color trailColor = new Color(0.7f, 1f, 0.8f, 0.65f);
    [SerializeField] private float horizontalTrailSpeed = 2.8f;
    [SerializeField] private float upwardTrailSpeed = 3.5f;
    [SerializeField] private float downwardTrailSpeed = 5f;
    [SerializeField] private float trailLifetime = 0.14f;
    [SerializeField] private float trailStartWidth = 0.38f;

    private Rigidbody2D rb;
    private TrailRenderer speedTrail;
    private Material trailMaterial;
    private float fastestDownwardSpeed;
    private float lastWallImpactTime = float.NegativeInfinity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        CreateSpeedTrail();
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.y < 0f)
            fastestDownwardSpeed = Mathf.Max(fastestDownwardSpeed, -rb.linearVelocity.y);
    }

    private void Update()
    {
        Vector2 velocity = rb.linearVelocity;
        speedTrail.emitting =
            Mathf.Abs(velocity.x) >= horizontalTrailSpeed ||
            velocity.y >= upwardTrailSpeed ||
            velocity.y <= -downwardTrailSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool hitGround = false;
        bool hitWall = false;
        float groundImpactSpeed = 0f;
        float wallImpactSpeed = 0f;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;

            if (normal.y > 0.55f)
            {
                hitGround = true;
                groundImpactSpeed = Mathf.Max(
                    groundImpactSpeed,
                    Mathf.Abs(Vector2.Dot(collision.relativeVelocity, normal)));
            }

            if (Mathf.Abs(normal.x) > 0.65f)
            {
                hitWall = true;
                wallImpactSpeed = Mathf.Max(
                    wallImpactSpeed,
                    Mathf.Abs(Vector2.Dot(collision.relativeVelocity, normal)));
            }
        }

        if (hitGround)
        {
            groundImpactSpeed = Mathf.Max(groundImpactSpeed, fastestDownwardSpeed);
            PlayLandingShake(groundImpactSpeed);
            fastestDownwardSpeed = 0f;
        }

        if (hitWall)
            PlayWallImpactShake(wallImpactSpeed);
    }

    private void PlayLandingShake(float impactSpeed)
    {
        if (impactSpeed < minimumLandingSpeed)
            return;

        float strength = Mathf.InverseLerp(minimumLandingSpeed, maximumLandingSpeed, impactSpeed);
        float amplitude = Mathf.Lerp(minimumLandingAmplitude, maximumLandingAmplitude, strength);
        float duration = Mathf.Lerp(minimumLandingDuration, maximumLandingDuration, strength);
        GameFeelController.RequestShake(amplitude, duration, new Vector2(0.45f, 1f));
    }

    private void PlayWallImpactShake(float impactSpeed)
    {
        if (impactSpeed < minimumWallImpactSpeed)
            return;

        if (Time.unscaledTime < lastWallImpactTime + wallImpactCooldown)
            return;

        lastWallImpactTime = Time.unscaledTime;
        float strength = Mathf.InverseLerp(minimumWallImpactSpeed, maximumWallImpactSpeed, impactSpeed);
        float amplitude = Mathf.Lerp(minimumWallAmplitude, maximumWallAmplitude, strength);
        GameFeelController.RequestShake(
            amplitude * wallImpactMultiplier,
            Mathf.Lerp(0.07f, 0.15f, strength),
            new Vector2(1f, 0.45f));
    }

    private void CreateSpeedTrail()
    {
        GameObject trailObject = new GameObject("Speed Trail");
        trailObject.transform.SetParent(transform, false);

        speedTrail = trailObject.AddComponent<TrailRenderer>();
        speedTrail.time = trailLifetime;
        speedTrail.startWidth = trailStartWidth;
        speedTrail.endWidth = 0f;
        speedTrail.minVertexDistance = 0.06f;
        speedTrail.alignment = LineAlignment.View;
        speedTrail.textureMode = LineTextureMode.Stretch;
        speedTrail.shadowCastingMode = ShadowCastingMode.Off;
        speedTrail.receiveShadows = false;
        speedTrail.emitting = false;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(trailColor, 0f),
                new GradientColorKey(trailColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(trailColor.a, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        speedTrail.colorGradient = gradient;

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            speedTrail.sortingLayerID = spriteRenderer.sortingLayerID;
            speedTrail.sortingOrder = spriteRenderer.sortingOrder - 1;
        }

        Shader trailShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (trailShader == null)
            trailShader = Shader.Find("Sprites/Default");

        if (trailShader != null)
        {
            trailMaterial = new Material(trailShader);
            speedTrail.sharedMaterial = trailMaterial;
        }
        else if (spriteRenderer != null)
        {
            speedTrail.sharedMaterial = spriteRenderer.sharedMaterial;
        }
    }

    private void OnDisable()
    {
        if (speedTrail == null)
            return;

        speedTrail.emitting = false;
        speedTrail.Clear();
    }

    private void OnDestroy()
    {
        if (trailMaterial != null)
            Destroy(trailMaterial);
    }
}
