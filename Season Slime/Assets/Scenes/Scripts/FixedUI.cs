using UnityEngine;
using UnityEngine.UI;

public class FixedUI : MonoBehaviour
{
    [Header("Season Backgrounds (´º¡úÏÄ¡úÇï¡ú¶¬)")]
    [SerializeField] private Image seasonBackground;   // ±³¾°Í¼
    [SerializeField] private Sprite springBg;   // 1
    [SerializeField] private Sprite summerBg;   // 2
    [SerializeField] private Sprite autumnBg;   // 3
    [SerializeField] private Sprite winterBg;   // 4

    public void SetSeason(int season)   // season: 1´º 2ÏÄ 3Çï 4¶¬
    {
        Debug.Log("FixedUI SetSeason ±»µ÷ÓÃÁË£¬season = " + season);

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
}