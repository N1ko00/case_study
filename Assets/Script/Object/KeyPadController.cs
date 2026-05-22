using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class KeyPadController : MonoBehaviour
{
    public Button[] buttons;
    public RectTransform cursor;

    int x = 0;
    int y = 0;

    Vector2 moveInput;

    public Color normalColor;
    public Color selectedColor;
    public Color enteredColor;

    void Awake()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager.Instance が null です");
            return;
        }

        if (InputManager.Instance.inputActions == null)
        {
            Debug.LogError("inputActions が null です");
            return;
        }

        InputManager.Instance.inputActions.Password.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        InputManager.Instance.inputActions.Password.Move.canceled += ctx => moveInput = Vector2.zero;
        InputManager.Instance.inputActions.Password.Submit.performed += OnSubmit;
    }

    private void Start()
    {
        UpdateSelection();
    }
    void OnEnable()
    {
        InputManager.Instance.inputActions.UI.Enable();
        InputManager.Instance.inputActions.Player.Disable();
    }

    void OnDisable()
    {
        InputManager.Instance.inputActions.UI.Disable();
        InputManager.Instance.inputActions.Player.Enable();
    }


    void UpdateSelection()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Image img = buttons[i].GetComponent<Image>();

            if (i == GetIndex())
                img.color = selectedColor;
            else
                img.color = normalColor;
        }
    }


    void Update()
    {
        if (moveInput == Vector2.zero)
            return;

        int nextX = x;
        int nextY = y;

        if (moveInput.x > 0) nextX++;
        if (moveInput.x < 0) nextX--;

        if (moveInput.y > 0) nextY--;
        if (moveInput.y < 0) nextY++;

        nextX = Mathf.Clamp(nextX, 0, 2);
        nextY = Mathf.Clamp(nextY, 0, 3);

        if (IsValidPosition(nextX, nextY))
        {
            x = nextX;
            y = nextY;
            UpdateSelection();
        }

        moveInput = Vector2.zero;
    }

    //特定の位置を無効にする(最下段の0に移動するときに使用)
    bool IsValidPosition(int px, int py)
    {
        if (py == 3)
            return px == 0 || px == 1;

        return true;
    }

    int GetIndex()
    {
        if (y == 0) return x;
        if (y == 1) return x + 3;
        if (y == 2) return x + 6;

        if (y == 3)
        {
            if (x == 0) return 9;   // 0
            if (x == 1) return 10;  // Enter
        }

        return 0;
    }


    void OnSubmit(InputAction.CallbackContext ctx)
    {
        Button current = buttons[GetIndex()];

        StartCoroutine(PressEffect(current));

        current.onClick.Invoke();
    }

    IEnumerator PressEffect(Button button)
    {
        RectTransform rt = button.GetComponent<RectTransform>();
        Image img = button.GetComponent<Image>();

        Vector3 originalScale = rt.localScale;
        Color originalColor = img.color;

        rt.localScale = originalScale * 0.9f;
        img.color = Color.Lerp(originalColor, Color.white, 0.45f);

        yield return new WaitForSeconds(0.08f);

        rt.localScale = originalScale;

        if (button == buttons[GetIndex()])
            img.color = selectedColor;
        else
            img.color = normalColor;
    }
}