using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public RectTransform arrow;
    public RectTransform[] buttons;
    public float xOffset = -30f;
    public InputActionReference navigate;   // 拖入 Navigate 动作
    public InputActionReference submit;      // 拖入 Submit 动作

    private int currentIndex = 0;

    void OnEnable()
    {
        navigate.action.Enable();
        submit.action.Enable();
        navigate.action.performed += OnNavigate;
        submit.action.performed += OnSubmit;
    }

    void OnDisable()
    {
        navigate.action.performed -= OnNavigate;
        submit.action.performed -= OnSubmit;
    }

    void Start() { UpdateArrow(); }

    void OnNavigate(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();
        if (dir.y < -0.5f)        // 向下
            currentIndex = (currentIndex + 1) % buttons.Length;
        else if (dir.y > 0.5f)    // 向上
            currentIndex = (currentIndex - 1 + buttons.Length) % buttons.Length;
        UpdateArrow();
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (currentIndex == 0)
        {
            Debug.Log("开始游戏");
            SceneManager.LoadScene("Level 1");  
        }
        else if (currentIndex == 1)
        {
            Debug.Log("退出游戏");
            Application.Quit();
        }
    }

    void UpdateArrow()
    {
        RectTransform target = buttons[currentIndex];
        Vector3 pos = target.position;
        pos.x = target.position.x - (target.rect.width * target.lossyScale.x) / 2f + xOffset;
        arrow.position = pos;
    }
}