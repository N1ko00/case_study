using UnityEngine;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public InputSystem_Actions inputActions;

    void Awake()
    {
        // シングルトン化（1つだけ存在）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            inputActions = new InputSystem_Actions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }
}