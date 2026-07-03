using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Result : MonoBehaviour
{
    InputSystem_Actions inputAction;

    [SerializeField] SceneLoader sceneLoader;

    [Header("ボタン参照")]
    [SerializeField] private Button ReturnTitle;


    void Awake()
    {
        inputAction = new InputSystem_Actions();
        inputAction.Scene.Move.performed += OnquitClicked;
    }

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    void OnEnable()
    {
        inputAction.Enable();
        if (ReturnTitle != null)
        {
            ReturnTitle.onClick.AddListener(OnReturnTitleClicked);
        }
    }
    
    void OnDisable()
    {
        inputAction.Disable();
        if (ReturnTitle != null)
        {
            ReturnTitle.onClick.RemoveListener(OnReturnTitleClicked);
        }
    }

    private void OnquitClicked(InputAction.CallbackContext ctx)
    {
        sceneLoader.LoadScene(SceneLoader.SceneName.TitleScene);
    }

    private void OnReturnTitleClicked()
    {
        Debug.Log("[Result] 終了");
        sceneLoader.LoadScene(SceneLoader.SceneName.TitleScene);

    }
}
