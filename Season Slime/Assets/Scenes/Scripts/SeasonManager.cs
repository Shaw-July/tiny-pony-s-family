using System;
using System.Collections;
using UnityEngine;

public class SeasonManager : MonoBehaviour
{
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    [Serializable]
    public class SeasonSetting
    {
        public Season season;
        public string displayName = "春";
        [Min(0.1f)] public float duration = 30f;
        public Color color = Color.white;
    }

    [Header("Seasons")]
    [SerializeField]
    private SeasonSetting[] seasons = new SeasonSetting[]
    {
        new SeasonSetting { season = Season.Spring, displayName = "春", duration = 30f, color = new Color(0.55f, 0.85f, 0.45f) },
        new SeasonSetting { season = Season.Summer, displayName = "夏", duration = 30f, color = new Color(0.30f, 0.75f, 0.90f) },
        new SeasonSetting { season = Season.Autumn, displayName = "秋", duration = 30f, color = new Color(0.95f, 0.65f, 0.25f) },
        new SeasonSetting { season = Season.Winter, displayName = "冬", duration = 30f, color = new Color(0.80f, 0.88f, 0.95f) },
    };

    [Header("Settings")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private int startIndex = 0;
    [Tooltip("颜色过渡时间，0 为瞬间切换")]
    [SerializeField] private float blendDuration = 2f;

    [Header("Optional Targets")]
    [SerializeField] private SpriteRenderer[] tintTargets;
    [SerializeField] private Camera backgroundCamera;

    // 事件：切换到新季节时触发
    public event Action<SeasonSetting> OnSeasonChanged;
    // 事件：颜色每帧变化时触发（含过渡过程）
    public event Action<Color> OnColorChanged;

    private int currentIndex;
    private Coroutine loopCoroutine;
    private Coroutine blendCoroutine;
    private Color currentColor;

    public SeasonSetting CurrentSeason =>
        (seasons != null && seasons.Length > 0) ? seasons[currentIndex] : null;
    public Color CurrentColor => currentColor;
    public float TimeRemaining { get; private set; }

    private void Start()
    {
        if (seasons == null || seasons.Length == 0)
        {
            Debug.LogError("[SeasonManager] 季节列表为空。");
            enabled = false;
            return;
        }

        currentIndex = Mathf.Clamp(startIndex, 0, seasons.Length - 1);
        currentColor = seasons[currentIndex].color;
        ApplyColor(currentColor);

        if (autoStart)
            StartCycle();
    }

    public void StartCycle()
    {
        StopCycle();
        loopCoroutine = StartCoroutine(SeasonLoop());
    }

    public void StopCycle()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
    }

    private IEnumerator SeasonLoop()
    {
        while (true)
        {
            EnterSeason(currentIndex);

            float duration = seasons[currentIndex].duration;
            TimeRemaining = duration;

            while (TimeRemaining > 0f)
            {
                TimeRemaining -= Time.deltaTime;
                yield return null;
            }
            TimeRemaining = 0f;

            int next = currentIndex + 1;
            if (next >= seasons.Length)
            {
                if (!loop)
                    yield break;
                next = 0;
            }
            currentIndex = next;
        }
    }

    private void EnterSeason(int index)
    {
        SeasonSetting setting = seasons[index];
        OnSeasonChanged?.Invoke(setting);

        if (blendCoroutine != null)
            StopCoroutine(blendCoroutine);

        if (blendDuration <= 0f)
        {
            currentColor = setting.color;
            ApplyColor(currentColor);
            OnColorChanged?.Invoke(currentColor);
        }
        else
        {
            blendCoroutine = StartCoroutine(BlendColor(setting.color, blendDuration));
        }
    }

    private IEnumerator BlendColor(Color target, float duration)
    {
        Color from = currentColor;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            currentColor = Color.Lerp(from, target, t / duration);
            ApplyColor(currentColor);
            OnColorChanged?.Invoke(currentColor);
            yield return null;
        }

        currentColor = target;
        ApplyColor(currentColor);
        OnColorChanged?.Invoke(currentColor);
        blendCoroutine = null;
    }

    private void ApplyColor(Color color)
    {
        if (tintTargets != null)
        {
            foreach (var sr in tintTargets)
            {
                if (sr != null)
                    sr.color = color;
            }
        }

        if (backgroundCamera != null)
            backgroundCamera.backgroundColor = color;
    }

    // ---- 外部控制 ----

    /// <summary>立即跳到下一个季节</summary>
    public void NextSeason()
    {
        int next = (currentIndex + 1) % seasons.Length;
        SetSeason(next);
    }

    /// <summary>按枚举跳转</summary>
    public void SetSeason(Season season)
    {
        for (int i = 0; i < seasons.Length; i++)
        {
            if (seasons[i].season == season)
            {
                SetSeason(i);
                return;
            }
        }
    }

    /// <summary>按索引跳转，会重启当前季节的计时</summary>
    public void SetSeason(int index)
    {
        if (seasons == null || seasons.Length == 0) return;

        currentIndex = Mathf.Clamp(index, 0, seasons.Length - 1);

        if (loopCoroutine != null)
            StartCycle();          // 重启循环，从新季节开始计时
        else
            EnterSeason(currentIndex);
    }
}