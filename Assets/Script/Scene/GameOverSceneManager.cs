using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
///  GameOverSceneの全体を管理します。
/// </summary>
public class GameOverSceneManager : MonoBehaviour
{
    [Header("SceneLoader参照")]
    [SerializeField] private SceneLoader sceneLoader;

    [Header("ボタン参照")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    [Header("遷移先シーン")]
    [SerializeField] private SceneLoader.SceneName retryScene = SceneLoader.SceneName.MainScene;
    [SerializeField] private SceneLoader.SceneName titleScene = SceneLoader.SceneName.TitleScene;

    [Header("登場フェードイン")]
    [Tooltip("CRT_Root の CanvasGroup をアサイン")]
    [SerializeField] private CanvasGroup crtRootGroup;
    [SerializeField] private float appearDelay = 0.3f;   // 入場直後の黒画面キープ
    [SerializeField] private float fadeInDuration = 1.2f;   // フェードイン時間


    private void Awake()
    {
        // timeScale が０のまま遷移して来た場合に備えてリセット
        Time.timeScale = 1f;
        // SceneLoader が未設定なら自動検索 
        if (sceneLoader == null)
            sceneLoader = FindAnyObjectByType<SceneLoader>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        // 登場前はUI非表示・非インタラクティブ
        if (crtRootGroup != null)
        {
            crtRootGroup.alpha = 0f;
            crtRootGroup.interactable = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        if (titleButton != null) titleButton.onClick.AddListener(OnTitleClicked);

        StartCoroutine(AppearRoutine());
    }

    // ───────────────────────────────────────────
    // 登場演出
    // ───────────────────────────────────────────
    private IEnumerator AppearRoutine()
    {
        yield return new WaitForSeconds(appearDelay);

        if (crtRootGroup != null)
        {
            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                crtRootGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                yield return null;
            }
            crtRootGroup.alpha = 1f;
            crtRootGroup.interactable = true;
        }
    }


    private void OnRetryClicked()
    {
        Debug.Log("[GameOverScene] Retry");
        LoadWithNoise(retryScene);
    }

    private void OnTitleClicked()
    {
        Debug.Log("[GameOverScene] Title へ戻る");
        LoadWithNoise(titleScene);
    }

    private void LoadWithNoise(SceneLoader.SceneName target)
    {
        // ボタンを無効化して二重押し防止
        if (retryButton != null) retryButton.interactable = false;
        if (titleButton != null) titleButton.interactable = false;

        if (sceneLoader != null)
        {
            sceneLoader.LoadScene(target);
        }
        else
        {
            // SceneLoader がなければ直接遷移
            Debug.LogWarning("[GameOverScene] SceneLoader 未検出 → 直接遷移");
            SceneManager.LoadScene(target.ToString());
        }
    }
}
