using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public sealed class GameFeelController : MonoBehaviour
{
    private const float HitStopTimeScale = 0.0001f;
    private const int MaxShakeCount = 12;

    private static GameFeelController instance;

    private readonly List<ShakeImpulse> shakeImpulses = new List<ShakeImpulse>(MaxShakeCount);
    private Camera activeCamera;
    private Vector3 appliedCameraOffset;
    private Coroutine hitStopRoutine;
    private float hitStopEndTime;
    private float timeScaleBeforeHitStop = 1f;
    private int nextShakeSeed;

    private struct ShakeImpulse
    {
        public float amplitude;
        public float startTime;
        public float endTime;
        public float seed;
        public Vector2 axisScale;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void RequestShake(float amplitude, float duration)
    {
        RequestShake(amplitude, duration, Vector2.one);
    }

    public static void RequestShake(float amplitude, float duration, Vector2 axisScale)
    {
        if (amplitude <= 0f || duration <= 0f)
            return;

        EnsureInstance().AddShake(amplitude, duration, axisScale);
    }

    public static void RequestHitStop(float duration)
    {
        if (duration <= 0f)
            return;

        EnsureInstance().StartOrExtendHitStop(duration);
    }

    private static GameFeelController EnsureInstance()
    {
        if (instance != null)
            return instance;

        instance = FindFirstObjectByType<GameFeelController>();
        if (instance != null)
            return instance;

        GameObject controllerObject = new GameObject("[Game Feel Controller]");
        instance = controllerObject.AddComponent<GameFeelController>();
        DontDestroyOnLoad(controllerObject);
        return instance;
    }

    private void AddShake(float amplitude, float duration, Vector2 axisScale)
    {
        if (shakeImpulses.Count >= MaxShakeCount)
            shakeImpulses.RemoveAt(0);

        float now = Time.unscaledTime;
        shakeImpulses.Add(new ShakeImpulse
        {
            amplitude = amplitude,
            startTime = now,
            endTime = now + duration,
            seed = 17.31f * ++nextShakeSeed,
            axisScale = axisScale
        });
    }

    private void StartOrExtendHitStop(float duration)
    {
        hitStopEndTime = Mathf.Max(hitStopEndTime, Time.unscaledTime + duration);
        if (hitStopRoutine != null)
            return;

        if (Time.timeScale <= 0f)
            return;

        timeScaleBeforeHitStop = Time.timeScale;
        hitStopRoutine = StartCoroutine(HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        Time.timeScale = HitStopTimeScale;

        while (Time.unscaledTime < hitStopEndTime)
            yield return null;

        // Do not unpause the game if another system paused during the hit stop.
        if (Mathf.Approximately(Time.timeScale, HitStopTimeScale))
            Time.timeScale = timeScaleBeforeHitStop;

        hitStopRoutine = null;
        hitStopEndTime = 0f;
    }

    private void LateUpdate()
    {
        RemovePreviousCameraOffset();

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            activeCamera = null;
            return;
        }

        activeCamera = mainCamera;
        Vector2 shakeOffset = CalculateShakeOffset();
        appliedCameraOffset = new Vector3(shakeOffset.x, shakeOffset.y, 0f);
        activeCamera.transform.position += appliedCameraOffset;
    }

    private Vector2 CalculateShakeOffset()
    {
        float now = Time.unscaledTime;
        Vector2 result = Vector2.zero;

        for (int i = shakeImpulses.Count - 1; i >= 0; i--)
        {
            ShakeImpulse impulse = shakeImpulses[i];
            if (now >= impulse.endTime)
            {
                shakeImpulses.RemoveAt(i);
                continue;
            }

            float normalizedTime = Mathf.InverseLerp(impulse.startTime, impulse.endTime, now);
            float envelope = 1f - normalizedTime;
            envelope *= envelope;

            float noiseTime = now * 28f;
            float noiseX = Mathf.PerlinNoise(impulse.seed, noiseTime) * 2f - 1f;
            float noiseY = Mathf.PerlinNoise(impulse.seed + 9.73f, noiseTime) * 2f - 1f;
            Vector2 noise = Vector2.Scale(new Vector2(noiseX, noiseY), impulse.axisScale);
            result += noise * (impulse.amplitude * envelope);
        }

        return Vector2.ClampMagnitude(result, 0.35f);
    }

    private void RemovePreviousCameraOffset()
    {
        if (activeCamera != null && appliedCameraOffset != Vector3.zero)
            activeCamera.transform.position -= appliedCameraOffset;

        appliedCameraOffset = Vector3.zero;
    }

    private void OnDisable()
    {
        RemovePreviousCameraOffset();
    }

    private void OnDestroy()
    {
        RemovePreviousCameraOffset();

        if (hitStopRoutine != null && Mathf.Approximately(Time.timeScale, HitStopTimeScale))
            Time.timeScale = timeScaleBeforeHitStop;

        if (instance == this)
            instance = null;
    }
}
