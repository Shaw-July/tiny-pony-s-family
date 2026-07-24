using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeasonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SeasonManager seasonManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI seasonNameText;
    [SerializeField] private TextMeshProUGUI timeRemainingText;
    [SerializeField] private Image seasonColorImage;   // 色块或面板背景
    [SerializeField] private Image progressBar;        // Image Type 设为 Filled

    [Header("Format")]
    [SerializeField] private string timeFormat = "{0:0.0}s";
    [SerializeField] private bool tintTextWithSeason = false;

    private float currentSeasonDuration = 1f;

    private void Awake()
    {
        if (seasonManager == null)
            seasonManager = FindObjectOfType<SeasonManager>();
    }

    private void OnEnable()
    {
        if (seasonManager == null) return;
        seasonManager.OnSeasonChanged += HandleSeasonChanged;
        seasonManager.OnColorChanged += HandleColorChanged;
    }

    private void OnDisable()
    {
        if (seasonManager == null) return;
        seasonManager.OnSeasonChanged -= HandleSeasonChanged;
        seasonManager.OnColorChanged -= HandleColorChanged;
    }

    private void Start()
    {
        var s = seasonManager != null ? seasonManager.CurrentSeason : null;
        if (s != null)
            HandleSeasonChanged(s);
    }

    private void Update()
    {
        if (seasonManager == null) return;

        float remaining = seasonManager.TimeRemaining;

        if (timeRemainingText != null)
            timeRemainingText.text = string.Format(timeFormat, remaining);

        if (progressBar != null && currentSeasonDuration > 0f)
            progressBar.fillAmount = remaining / currentSeasonDuration;
    }

    private void HandleSeasonChanged(SeasonManager.SeasonSetting s)
    {
        currentSeasonDuration = Mathf.Max(s.duration, 0.0001f);

        if (seasonNameText != null)
            seasonNameText.text = s.displayName;
    }

    private void HandleColorChanged(Color color)
    {
        if (seasonColorImage != null)
            seasonColorImage.color = color;

        if (progressBar != null)
            progressBar.color = color;

        if (tintTextWithSeason && seasonNameText != null)
            seasonNameText.color = color;
    }
}