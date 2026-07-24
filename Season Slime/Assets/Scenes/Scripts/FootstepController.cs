using FMODUnity;
using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private float stepInterval = 0.4f;

    private float timer;
    private Rigidbody2D rb;
    private string currentSurface = "Stone";

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        // 只在真正移动时计时
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            timer += Time.deltaTime;
            if (timer >= stepInterval)
            {
                PlayFootstep();
                timer = 0f;
            }
        }
        else
        {
            timer = stepInterval; // 停下后再走立刻出声
        }
    }

    void PlayFootstep()
    {
        var instance = RuntimeManager.CreateInstance(footstepEvent);
        instance.setParameterByNameWithLabel("Surface", currentSurface);
        instance.start();
        instance.release(); // one-shot 必须 release，否则内存泄漏
    }

    public void SetSurface(string surface) => currentSurface = surface;
}