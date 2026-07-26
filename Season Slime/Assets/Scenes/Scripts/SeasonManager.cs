using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonManager : MonoBehaviour
{
    public enum Season { Spring, Summer, Autumn, Winter }

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
    [SerializeField] private float blendDuration = 2f;

    [Header("Color Targets")]
    [SerializeField] private SpriteRenderer[] tintTargets;

    [Header("Ground Tiles")]
    [SerializeField] private Transform groundParent;          // 所有地面块位置
    [SerializeField] private GameObject[] seasonGroundPrefabs;     // 顺序与 seasons 一致
    [SerializeField] private float groundAnimationDuration = 0.1f; // 消失动画时长

    [Header("Slime Prefabs")]
    [SerializeField] private GameObject[] slimePrefabs; // 史莱姆预制体

    public event Action<SeasonSetting> OnSeasonChanged;
    public event Action<Color> OnColorChanged;

    private int currentIndex;
    private Coroutine cycleCoroutine;
    private Color currentColor;
    private List<GameObject> currentGroundTiles = new List<GameObject>();
    private bool isCycleRunning;

    public SeasonSetting CurrentSeason => seasons[currentIndex];
    public Color CurrentColor => currentColor;
    public float TimeRemaining { get; private set; }
    public static int CycleCount = 0;

    private Vector3[] spawnPositions;
    private Quaternion[] spawnRotations;

    void Awake()
    {
        int childCount = groundParent.childCount;
        spawnPositions = new Vector3[childCount];
        spawnRotations = new Quaternion[childCount];
        currentGroundTiles.Clear();

        for (int i = 0; i < childCount; i++)
        {
            Transform child = groundParent.GetChild(i);
            spawnPositions[i] = child.position;
            spawnRotations[i] = child.rotation;
            currentGroundTiles.Add(child.gameObject); // 直接将现有物体作为初始地面块
        }
    }
    private void Start()
    {
        currentIndex = Mathf.Clamp(startIndex, 0, seasons.Length - 1);
        currentColor = seasons[currentIndex].color;
        ApplyColor(currentColor);

        if (autoStart) StartCycle();
    }

    private void ClearGroundTiles()
    {
        foreach (var tile in currentGroundTiles)
            if (tile != null) Destroy(tile);
        currentGroundTiles.Clear();
    }

    // ---------- 循环控制 ----------
    public void StartCycle()
    {
        StopCycle();
        isCycleRunning = true;
        cycleCoroutine = StartCoroutine(SeasonCycle());
    }

    public void StopCycle()
    {
        if (cycleCoroutine != null)
        {
            StopCoroutine(cycleCoroutine);
            cycleCoroutine = null;
        }
        isCycleRunning = false;
    }

    // ---------- 核心循环 ----------
    private IEnumerator SeasonCycle()
    {
        while (true)
        {
            // 1. 进入当前季节（颜色混合）
            yield return StartCoroutine(ApplySeasonWithBlend(currentIndex));

            // 2. 等待剩余持续时间（混合已耗去 blendDuration）
            float remaining = Mathf.Max(0, seasons[currentIndex].duration - blendDuration);
            TimeRemaining = remaining;  // 赋值

            while (TimeRemaining > 0f)
            {
                TimeRemaining -= Time.deltaTime;
                yield return null;
            }
            TimeRemaining = 0f;

            // 3. 计算下一季节索引
            int next = currentIndex + 1;
            if (next >= seasons.Length)
            {
                if (!loop) yield break;
                next = 0;
                CycleCount++;
            }

            // 4. ★ 替换地面块（播放消失动画，等待完成后生成新块）
            yield return StartCoroutine(ReplaceGroundTiles(next));

            // 5. 更新索引，进入下一季节（下次循环会执行颜色混合）
            currentIndex = next;
        }
    }

    // ---------- 颜色混合协程 ----------
    private IEnumerator ApplySeasonWithBlend(int index)
    {
        SeasonSetting setting = seasons[index];
        OnSeasonChanged?.Invoke(setting);

        Color from = currentColor;
        Color target = setting.color;
        float elapsed = 0f;

        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            currentColor = Color.Lerp(from, target, elapsed / blendDuration);
            ApplyColor(currentColor);
            OnColorChanged?.Invoke(currentColor);
            yield return null;
        }

        currentColor = target;
        ApplyColor(currentColor);
        OnColorChanged?.Invoke(currentColor);
    }

    // ---------- 地面块替换协程（含消失动画） ----------
    private IEnumerator ReplaceGroundTiles(int newSeasonIndex)
    {
        foreach (var tile in currentGroundTiles)
        {
            Animator anim = tile.GetComponent<Animator>();
            anim.SetTrigger("ChangeState");
        }

        GameObject targetPlayer = GameObject.FindGameObjectWithTag("Player");
        Animator slimeAnim = targetPlayer.GetComponent<Animator>();
        if(SeasonManager.CycleCount < 2)
            slimeAnim.SetBool("ChangeState", true);
        Vector3 playerPos = targetPlayer.transform.position;

        yield return new WaitForSeconds(groundAnimationDuration);

        // 2. 销毁旧块
        ClearGroundTiles();

        if (targetPlayer != null)
            Destroy(targetPlayer);

        // 3. 生成新块（使用新季节的预制体）
        GameObject prefab = seasonGroundPrefabs[newSeasonIndex];
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            GameObject newTile = Instantiate(prefab, spawnPositions[i], spawnRotations[i], groundParent);

            currentGroundTiles.Add(newTile);
        }

        //4. 替换史莱姆
        GameObject newSlime = Instantiate(slimePrefabs[newSeasonIndex], playerPos, Quaternion.identity);
    }

    // ---------- 颜色应用 ----------
    private void ApplyColor(Color color)
    {
        foreach (var sr in tintTargets) sr.color = color;
    }

    // ---------- 外部控制 ----------
    public void NextSeason()
    {
        SetSeason((currentIndex + 1) % seasons.Length);
    }

    public void SetSeason(Season season)
    {
        for (int i = 0; i < seasons.Length; i++)
            if (seasons[i].season == season) { SetSeason(i); return; }
    }

    public void SetSeason(int index)
    {
        bool wasRunning = isCycleRunning;
        StopCycle();
        currentIndex = Mathf.Clamp(index, 0, seasons.Length - 1);
        StartCoroutine(ApplySeasonChangeWithGround(currentIndex, wasRunning));
    }

    private IEnumerator ApplySeasonChangeWithGround(int index, bool restartCycle)
    {
        // 先替换地面（含动画）
        yield return StartCoroutine(ReplaceGroundTiles(index));
        // 再应用颜色（含混合）
        yield return StartCoroutine(ApplySeasonWithBlend(index));
        // 若之前循环在运行，则重启
        if (restartCycle)
            StartCycle();
    }
}