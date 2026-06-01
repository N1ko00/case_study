using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Title : MonoBehaviour
{
    InputSystem_Actions inputAction;

    //めんどくなったのでインスタンス参照
    [SerializeField] SceneLoader sceneLoader;

    [Header("ボタン参照")]
    [SerializeField] private Button gameSatrtButton;
    [SerializeField] private Button gameExitButton;
    // Update is called once per frame

    void OnEnable()
    {
        if (gameSatrtButton != null)
        {
            gameSatrtButton.onClick.AddListener(OnGameStartCliked);
        }
        if (gameExitButton != null)
        {
            gameExitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    void OnDisable()
    {
        if (gameSatrtButton != null)
        {
            gameSatrtButton.onClick.RemoveListener(OnGameStartCliked);
        }
        if (gameExitButton != null)
        {
            gameExitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }

    private void OnGameStartCliked()
    {
        if(sceneLoader == null)
        {
            Debug.LogWarning("[Title] SceneLoaderが　ない");
            return;
        }

        sceneLoader.LoadScene(SceneLoader.SceneName.MainScene);
    }

    private void OnQuitClicked()
    {
        Debug.Log("[Title] ゲーム終了");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
