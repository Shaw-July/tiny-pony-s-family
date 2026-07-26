using System.Collections;
using UnityEngine;

public sealed class SpringSuperJumpVFX : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material petalMaterial;
    [SerializeField] private Material softParticleMaterial;

    [Header("Burst")]
    [SerializeField] private int petalCount = 12;
    [SerializeField] private int energyMoteCount = 10;
    [SerializeField] private float horizontalSpread = 2.6f;
    [SerializeField] private float footVerticalOffset = 0.02f;

    [Header("Shockwave")]
    [SerializeField] private Color shockwaveColor = new Color(0.55f, 1f, 0.48f, 0.85f);
    [SerializeField] private float shockwaveDuration = 0.24f;
    [SerializeField] private float shockwaveWidth = 1.8f;
    [SerializeField] private float shockwaveHeight = 0.45f;

    private static readonly Color[] PetalColors =
    {
        new Color(1f, 0.64f, 0.82f, 1f),
        new Color(1f, 0.9f, 0.52f, 1f),
        new Color(0.6f, 1f, 0.58f, 1f)
    };

    private Collider2D bodyCollider;
    private SpriteRenderer sourceRenderer;
    private ParticleSystem petalSystem;
    private ParticleSystem energySystem;
    private GameObject shockwaveObject;
    private LineRenderer shockwaveLine;
    private Material shockwaveMaterial;
    private Coroutine shockwaveRoutine;
    private System.Random random;

    private void Awake()
    {
        bodyCollider = GetComponent<Collider2D>();
        sourceRenderer = GetComponent<SpriteRenderer>();
        random = new System.Random(GetInstanceID());

        petalSystem = CreateParticleSystem("Super Jump Petals", petalMaterial, 64, 0.3f);
        energySystem = CreateParticleSystem("Super Jump Energy", softParticleMaterial, 48, 0.16f);
        CreateShockwave();
    }

    public void Play()
    {
        if (!isActiveAndEnabled)
            return;

        Vector3 footPosition = GetFootPosition();
        EmitPetals(footPosition);
        EmitEnergyMotes(footPosition);
        PlayShockwave(footPosition);

        GameFeelController.RequestShake(0.025f, 0.08f, new Vector2(0.35f, 1f));
    }

    private ParticleSystem CreateParticleSystem(
        string systemName,
        Material material,
        int maxParticles,
        float defaultSize)
    {
        GameObject systemObject = new GameObject(systemName);
        systemObject.layer = gameObject.layer;
        systemObject.transform.SetParent(transform, false);

        ParticleSystem particleSystem = systemObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particleSystem.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.startSpeed = 0f;
        main.startLifetime = 0.4f;
        main.startSize = defaultSize;
        main.maxParticles = maxParticles;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.enabled = false;

        ParticleSystemRenderer particleRenderer = systemObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sharedMaterial = material;
        particleRenderer.sortingLayerID = sourceRenderer != null ? sourceRenderer.sortingLayerID : 0;
        particleRenderer.sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder + 1 : 1;

        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return particleSystem;
    }

    private void EmitPetals(Vector3 footPosition)
    {
        if (petalSystem == null)
            return;

        ParticleSystem.MainModule main = petalSystem.main;
        main.gravityModifier = 0.55f;

        for (int i = 0; i < petalCount; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float speedX = side * RandomRange(horizontalSpread * 0.45f, horizontalSpread);

            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                position = footPosition + new Vector3(RandomRange(-0.16f, 0.16f), 0f, 0f),
                velocity = new Vector3(speedX, RandomRange(0.35f, 1.65f), 0f),
                startLifetime = RandomRange(0.35f, 0.55f),
                startSize = RandomRange(0.12f, 0.23f),
                startColor = PetalColors[random.Next(PetalColors.Length)],
                rotation = RandomRange(0f, 360f),
                angularVelocity = RandomRange(-260f, 260f)
            };

            petalSystem.Emit(emit, 1);
        }
    }

    private void EmitEnergyMotes(Vector3 footPosition)
    {
        if (energySystem == null)
            return;

        ParticleSystem.MainModule main = energySystem.main;
        main.gravityModifier = 0.35f;

        for (int i = 0; i < energyMoteCount; i++)
        {
            float normalizedSide = energyMoteCount <= 1
                ? 0f
                : Mathf.Lerp(-1f, 1f, i / (energyMoteCount - 1f));

            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                position = footPosition + new Vector3(RandomRange(-0.1f, 0.1f), 0.01f, 0f),
                velocity = new Vector3(
                    normalizedSide * RandomRange(1.3f, 2.4f),
                    RandomRange(0.25f, 1.15f),
                    0f),
                startLifetime = RandomRange(0.18f, 0.32f),
                startSize = RandomRange(0.07f, 0.16f),
                startColor = Color.Lerp(
                    new Color(0.52f, 1f, 0.45f, 0.9f),
                    new Color(1f, 0.94f, 0.5f, 0.9f),
                    RandomRange(0f, 1f))
            };

            energySystem.Emit(emit, 1);
        }
    }

    private void CreateShockwave()
    {
        shockwaveObject = new GameObject("Super Jump Shockwave");
        shockwaveObject.layer = gameObject.layer;
        shockwaveObject.SetActive(false);

        shockwaveLine = shockwaveObject.AddComponent<LineRenderer>();
        shockwaveLine.useWorldSpace = false;
        shockwaveLine.loop = true;
        shockwaveLine.positionCount = 40;
        shockwaveLine.widthMultiplier = 0.055f;
        shockwaveLine.numCornerVertices = 2;
        shockwaveLine.numCapVertices = 2;
        shockwaveLine.sortingLayerID = sourceRenderer != null ? sourceRenderer.sortingLayerID : 0;
        shockwaveLine.sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder + 1 : 1;

        for (int i = 0; i < shockwaveLine.positionCount; i++)
        {
            float angle = i / (float)shockwaveLine.positionCount * Mathf.PI * 2f;
            shockwaveLine.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0f));
        }

        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader != null)
            shockwaveMaterial = new Material(lineShader);
        else if (softParticleMaterial != null)
            shockwaveMaterial = new Material(softParticleMaterial);

        if (shockwaveMaterial != null)
            shockwaveLine.sharedMaterial = shockwaveMaterial;
    }

    private void PlayShockwave(Vector3 footPosition)
    {
        if (shockwaveLine == null)
            return;

        if (shockwaveRoutine != null)
            StopCoroutine(shockwaveRoutine);

        shockwaveRoutine = StartCoroutine(AnimateShockwave(footPosition));
    }

    private IEnumerator AnimateShockwave(Vector3 footPosition)
    {
        shockwaveObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < shockwaveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, shockwaveDuration));
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            float alpha = Mathf.Pow(1f - progress, 2f) * shockwaveColor.a;

            shockwaveObject.transform.position = footPosition;
            shockwaveObject.transform.localScale = new Vector3(
                Mathf.Lerp(0.25f, shockwaveWidth, easedProgress),
                Mathf.Lerp(0.06f, shockwaveHeight, easedProgress),
                1f);

            Color currentColor = new Color(
                shockwaveColor.r,
                shockwaveColor.g,
                shockwaveColor.b,
                alpha);
            shockwaveLine.startColor = currentColor;
            shockwaveLine.endColor = currentColor;
            shockwaveLine.widthMultiplier = Mathf.Lerp(0.075f, 0.015f, progress);

            yield return null;
        }

        shockwaveObject.SetActive(false);
        shockwaveRoutine = null;
    }

    private Vector3 GetFootPosition()
    {
        if (bodyCollider == null)
            return transform.position;

        Bounds bounds = bodyCollider.bounds;
        return new Vector3(bounds.center.x, bounds.min.y + footVerticalOffset, transform.position.z);
    }

    private float RandomRange(float minimum, float maximum)
    {
        return Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
    }

    private void OnDisable()
    {
        if (petalSystem != null)
            petalSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (energySystem != null)
            energySystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (shockwaveObject != null)
            shockwaveObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (shockwaveMaterial != null)
            Destroy(shockwaveMaterial);

        if (shockwaveObject != null)
            Destroy(shockwaveObject);
    }
}
