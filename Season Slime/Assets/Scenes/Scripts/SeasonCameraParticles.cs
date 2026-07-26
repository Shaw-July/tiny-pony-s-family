using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public sealed class SeasonCameraParticles : MonoBehaviour
{
#pragma warning disable 0649
    [Serializable]
    private struct SeasonParticlePrefab
    {
        public SeasonManager.Season season;
        public GameObject prefab;
    }
#pragma warning restore 0649

    [SerializeField] private SeasonManager seasonManager;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SeasonParticlePrefab[] particlePrefabs;
    [SerializeField] private float distanceFromCamera = 10f;
    [SerializeField] private Vector2 screenPadding = new Vector2(4f, 4f);
    [SerializeField] private int sortingOrder = 20;

    private readonly Dictionary<SeasonManager.Season, GameObject> instances = new Dictionary<SeasonManager.Season, GameObject>();
    private readonly List<ParticleSystem> cachedParticleSystems = new List<ParticleSystem>();
    private SeasonManager.Season activeSeason;
    private bool hasActiveSeason;
    private Vector2 lastCameraSize;

    private void Awake()
    {
        if (seasonManager == null)
            seasonManager = GetComponent<SeasonManager>();
    }

    private void OnEnable()
    {
        if (seasonManager != null)
            seasonManager.OnSeasonChanged += HandleSeasonChanged;
    }

    private void Start()
    {
        EnsureCamera();
        BuildInstances();

        if (seasonManager != null)
            SetSeason(seasonManager.CurrentSeason.season);
    }

    private void OnDisable()
    {
        if (seasonManager != null)
            seasonManager.OnSeasonChanged -= HandleSeasonChanged;
    }

    private void LateUpdate()
    {
        if (!EnsureCamera())
            return;

        UpdateParticleTransformAndShape();
    }

    private void HandleSeasonChanged(SeasonManager.SeasonSetting setting)
    {
        SetSeason(setting.season);
    }

    public void SetSeason(SeasonManager.Season season)
    {
        activeSeason = season;
        hasActiveSeason = true;
        BuildInstances();

        foreach (KeyValuePair<SeasonManager.Season, GameObject> pair in instances)
        {
            bool shouldPlay = pair.Key == season;
            pair.Value.SetActive(shouldPlay);

            ParticleSystem[] particleSystems = pair.Value.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                if (shouldPlay)
                    particleSystems[i].Play(true);
                else
                    particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        lastCameraSize = Vector2.zero;
        UpdateParticleTransformAndShape();
    }

    private void BuildInstances()
    {
        if (!EnsureCamera())
            return;

        for (int i = 0; i < particlePrefabs.Length; i++)
        {
            SeasonParticlePrefab entry = particlePrefabs[i];
            if (entry.prefab == null || instances.ContainsKey(entry.season))
                continue;

            GameObject instance = Instantiate(entry.prefab, targetCamera.transform);
            instance.name = entry.prefab.name + "_CameraFollow";
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instance.SetActive(false);
            instances.Add(entry.season, instance);

            ConfigureRenderers(instance);
            ConfigureParticleSimulation(instance);
        }
    }

    private bool EnsureCamera()
    {
        if (targetCamera != null)
            return true;

        targetCamera = Camera.main;
        return targetCamera != null;
    }

    private void ConfigureRenderers(GameObject instance)
    {
        ParticleSystemRenderer[] renderers = instance.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder = sortingOrder;
            renderers[i].maxParticleSize = 1.5f;
        }
    }

    private void ConfigureParticleSimulation(GameObject instance)
    {
        ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem.MainModule main = particleSystems[i].main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.playOnAwake = false;
        }
    }

    private void UpdateParticleTransformAndShape()
    {
        if (!hasActiveSeason || !instances.TryGetValue(activeSeason, out GameObject instance) || instance == null)
            return;

        float height = targetCamera.orthographic
            ? targetCamera.orthographicSize * 2f
            : Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distanceFromCamera * 2f;
        float width = height * targetCamera.aspect;
        Vector2 cameraSize = new Vector2(width, height);

        instance.transform.localPosition = new Vector3(0f, 0f, distanceFromCamera);
        instance.transform.localRotation = Quaternion.identity;

        if ((cameraSize - lastCameraSize).sqrMagnitude < 0.001f)
            return;

        lastCameraSize = cameraSize;
        cachedParticleSystems.Clear();
        instance.GetComponentsInChildren(true, cachedParticleSystems);

        for (int i = 0; i < cachedParticleSystems.Count; i++)
        {
            ParticleSystem particleSystem = cachedParticleSystems[i];
            ParticleSystem.ShapeModule shape = particleSystem.shape;
            if (!shape.enabled)
                continue;

            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(
                width + screenPadding.x,
                height + screenPadding.y,
                1f);
            shape.position = Vector3.zero;
        }
    }
}
