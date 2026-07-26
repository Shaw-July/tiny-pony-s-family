using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FixedUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject manager;
    private SeasonManager seasonManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI seasonNameText;
    [SerializeField] private TextMeshProUGUI seasonNumberText;  // 显示数字（代替倒计时）
    [SerializeField] private Image seasonBackground;            // 背景图（代替色块）

    [Header("Season Backgrounds (按 春→夏→秋→冬 顺序)")]
    [SerializeField] private Sprite springBg;   // 数字 1
    [SerializeField] private Sprite summerBg;   // 数字 2
    [SerializeField] private Sprite autumnBg;   // 数字 3
    [SerializeField] private Sprite winterBg;   // 数字 4

    private void Awake()
    {
        if (seasonManager == null)
            seasonManager = manager.GetComponent<SeasonManager>();
    }

    private void OnEnable()
    {
        if (seasonManager == null) return;
        seasonManager.OnSeasonChanged += HandleSeasonChanged;
    }

    private void OnDisable()
    {
        if (seasonManager == null) return;
        seasonManager.OnSeasonChanged -= HandleSeasonChanged;
    }

    private void Start()
    {
        var s = seasonManager != null ? seasonManager.CurrentSeason : null;
        if (s != null)
            HandleSeasonChanged(s);
    }

    private void HandleSeasonChanged(SeasonManager.SeasonSetting s)
    {
        // 显示季节名
        if (seasonNameText != null)
            seasonNameText.text = s.displayName;

        // 根据季节名判断编号 + 背景
        int number = GetSeasonNumber(s.displayName);

        if (seasonNumberText != null)
            seasonNumberText.text = number.ToString();

        if (seasonBackground != null)
        {
            Sprite bg = number switch
            {
                1 => springBg,
                2 => summerBg,
                3 => autumnBg,
                4 => winterBg,
                _ => null
            };
            if (bg != null)
                seasonBackground.sprite = bg;
        }
    }
    public void SetSeason(int season)   // season: 1春 2夏 3秋 4冬
    {
        Debug.Log("FixedUI SetSeason 被调用了，season = " + season);

        if (seasonNumberText != null)
            seasonNumberText.text = season.ToString();

        // 新增:更新名字
        if (seasonNameText != null)
        {
            seasonNameText.text = season switch
            {
                1 => "Spring",
                2 => "Summer",
                3 => "Autumn",
                4 => "Winter",
                _ => ""
            };
        }

        if (seasonBackground != null)
        {
            Sprite bg = season switch
            {
                1 => springBg,
                2 => summerBg,
                3 => autumnBg,
                4 => winterBg,
                _ => null
            };
            if (bg != null) seasonBackground.sprite = bg;
        }
    }
    // 根据季节名返回编号（改成匹配你实际的 displayName）
    private int GetSeasonNumber(string name)
    {
        switch (name)
        {
            case "Spring": return 1;
            case "Summer": return 2;
            case "Autumn": return 3;
            case "Winter": return 4;
            default: return 1;
        }
    }
}