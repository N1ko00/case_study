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

    // 移動の連打防止用
    bool moveConsumed = false;

    public Color normalColor;
    public Color selectedColor;
    public Color enteredColor;

    void Awake()
    {
        if (InputManager.Instance?.inputActions == null)
        {
            Debug.LogError("InputManager が null です");
            return;
        }

        InputManager.Instance.inputActions.Password.Submit.performed += OnSubmit;
    }

    void OnEnable()
    {
        InputManager.Instance.inputActions.UI.Enable();
        InputManager.Instance.inputActions.Player.Disable();
        x = 0;
        y = 0;
        moveConsumed = false;
        UpdateSelection();
    }

    void OnDisable()
    {
        InputManager.Instance.inputActions.UI.Disable();
        InputManager.Instance.inputActions.Player.Enable();
    }

    void Update()
    {
        // ★ ReadValue で毎フレーム直接取得（performedイベント経由をやめる）
        Vector2 move = InputManager.Instance.inputActions.Password.Move.ReadValue<Vector2>();

        if (move == Vector2.zero)
        {
            // 入力がなくなったらリセット（次の入力を受け付ける）
            moveConsumed = false;
            return;
        }

        // 押しっぱなし対策：入力を1回だけ消費
        if (moveConsumed)
            return;

        moveConsumed = true;

        int nextX = x;
        int nextY = y;

        if (move.x > 0.5f) nextX++;
        if (move.x < -0.5f) nextX--;
        if (move.y > 0.5f) nextY--;   // 上キー → y減（上の行へ）
        if (move.y < -0.5f) nextY++;   // 下キー → y増（下の行へ）

        nextX = Mathf.Clamp(nextX, 0, 2);
        nextY = Mathf.Clamp(nextY, 0, 3);

        if (IsValidPosition(nextX, nextY))
        {
            x = nextX;
            y = nextY;
            UpdateSelection();
        }
    }

    // 最下段(y==3)は 0(x=0), Enter(x=1), Back(x=2) の3つ全部有効
    bool IsValidPosition(int px, int py)
    {
        if (py == 3)
            return px == 0 || px == 1 || px == 2;
        return true;
    }

    int GetIndex()
    {
        if (y == 0) return x;        // 1(0), 2(1), 3(2)
        if (y == 1) return x + 3;    // 4(3), 5(4), 6(5)
        if (y == 2) return x + 6;    // 7(6), 8(7), 9(8)
        if (y == 3)
        {
            if (x == 0) return 9;    // 0
            if (x == 1) return 10;   // Enter
            if (x == 2) return 11;   // Back ← ここが元のバグ（x==1が重複していた）
        }
        return 0;
    }

    void UpdateSelection()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Image img = buttons[i].GetComponent<Image>();
            img.color = (i == GetIndex()) ? selectedColor : normalColor;
        }
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
        rt.localScale = originalScale * 0.9f;
        img.color = Color.Lerp(selectedColor, Color.white, 0.45f);

        yield return new WaitForSeconds(0.08f);

        rt.localScale = originalScale;
        img.color = (button == buttons[GetIndex()]) ? selectedColor : normalColor;
    }
}