using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class KeyPadController : MonoBehaviour
{
    public Button[] buttons;
    public RectTransform cursor;

    int index = 0;
    Vector2 moveInput;

void Awake()
{
    InputManager.Instance.inputActions.Password.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
    InputManager.Instance.inputActions.Password.Move.canceled += ctx => moveInput = Vector2.zero;
     InputManager.Instance.inputActions.Password.Submit.performed += OnSubmit; // ← これ追加
}
    void Start()
    {
        MoveCursor();
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
    void Update()
    {
        if (moveInput.x > 0) index++;
        if (moveInput.x < 0) index--;

        if (moveInput.y > 0) index -= 3;
        if (moveInput.y < 0) index += 3;

        index = Mathf.Clamp(index, 0, buttons.Length - 1);

        MoveCursor();

        moveInput = Vector2.zero; // ←これ重要（1回だけ動かす）
    }

    void MoveCursor()
    { 
        cursor.position = buttons[index].transform.position;
    }

    void OnSubmit(InputAction.CallbackContext ctx)
    {
        Debug.Log("押された: " + index);

        buttons[index].onClick.Invoke();
    }
}