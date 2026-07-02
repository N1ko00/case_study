using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Title : MonoBehaviour
{
    InputSystem_Actions inputAction;

    //シーン遷移用のSceneLoader参照
    [SerializeField] SceneLoader sceneLoader;

    [Header("ボタン参照")]
    [SerializeField] private Button gameSatrtButton;
    [SerializeField] private Button gameExitButton;

    [Header("コントローラー対応")]
    [Tooltip("起動時に最初に選択状態となるボタン")]
    [SerializeField] private Button firstSelectedButton;
    [Tooltip("マウスクリックなどで選択が外れた時に自動で再選択する")]
    [SerializeField] private bool keepSelectionAlive = true;

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

        // コントローラー操作のため、最初のボタンを選択状態にする
        StartCoroutine(SelectFirstButtonNextFrame());
    }

    private System.Collections.IEnumerator SelectFirstButtonNextFrame()
    {
        yield return null;
        SelectFirstButton();
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

    void Update()
    {
        if (!keepSelectionAlive) return;
        if (EventSystem.current == null) return;

        // マウスクリックで選択が解除された場合に備えて自動で再選択
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SelectFirstButton();
        }
    }

    /// <summary>
    /// 最初のボタンをEventSystemの選択対象に設定する。
    /// 設定されていなければゲーム開始ボタンをフォールバックに使う。
    /// </summary>
    private void SelectFirstButton()
    {
        if (EventSystem.current == null) return;

        Button target = firstSelectedButton != null ? firstSelectedButton : gameSatrtButton;
        if (target == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }

    private void OnGameStartCliked()
    {
        if (sceneLoader == null)
        {
            Debug.LogWarning("[Title] SceneLoaderが未設定");
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