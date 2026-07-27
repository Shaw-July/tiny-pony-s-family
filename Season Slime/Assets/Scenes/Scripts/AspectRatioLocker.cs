using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioLocker : MonoBehaviour
{
    // 目标宽高比 16:9
    [SerializeField] private float targetAspect = 16f / 9f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateViewport();
    }

    void Update()
    {
        UpdateViewport();   // 窗口尺寸变化时实时调整；若不需要可只在 Start 调用
    }

    void UpdateViewport()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // 窗口太高（比 16:9 窄）→ 上下加黑边
            Rect rect = cam.rect;
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1f - scaleHeight) / 2f;
            cam.rect = rect;
        }
        else
        {
            // 窗口太宽 → 左右加黑边
            float scaleWidth = 1f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}