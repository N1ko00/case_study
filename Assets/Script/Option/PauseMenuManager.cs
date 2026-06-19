using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// MainSceneでESCキーを押すと表示されるポーズメニュー。
/// </summary>

public class PauseMenuManager : MonoBehaviour
{
   // 外部用シングルトン
   public static PauseMenuManager Instance {  get; private set; }

   public enum ExitMode
    {
        QuitApplication, // ゲーム終了
        GoToTitle　　　　// タイトルへ
    }

    [Header("UI参照")]
    [Tooltip("半透明背景＋ボタンを含むポーズUIルート")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;

    [Header("シーン参照")]
    [Tooltip("DontDestroyOnLoadのSceneLoader。未設定なら自動検索")]
    [SerializeField] private SceneLoader sceneLoader;
    [Tooltip("Retryで読み込むシーン")]
    [SerializeField] private SceneLoader.SceneName retryScene = SceneLoader.SceneName.MainScene;

    [Header("Exit動作設定")]
    [SerializeField] private ExitMode exitMode = ExitMode.QuitApplication;
    [Tooltip("ExitMode = GoToTitle のときに読み込むシーン")]
    [SerializeField] private SceneLoader.SceneName titleScene = SceneLoader.SceneName.TitleScene;

    [Header("動作制限")]
    [Tooltip("このシーン名のときだけESCで開ける")]
    [SerializeField] private string allowedSceneName = "MainScene";
    [Tooltip("キーパッドが開いている時はESCを無視する（任意）")]
    [SerializeField] private GameObject keypadUI;

    private bool _isPaused = false;

    /// <summary>外部からポーズ中か確認するためのプロパティ</summary>
    public bool IsOpen => _isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (sceneLoader == null)
        {
            sceneLoader = FindAnyObjectByType<SceneLoader>(FindObjectsInactive.Include);
        }
    }

    private void Start()
    {
        if (pausePanel != null)   pausePanel.SetActive(false);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (retryButton != null)  retryButton.onClick.AddListener(OnRetryClicked);
        if (exitButton != null)   exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        // 対象シーン以外では機能無効
        if(SceneManager.GetActiveScene().name != allowedSceneName) return;

        // ゲームオーバー中は無効（ポーズ中なら解除して進行を止めない）
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameOver)
        {
            if (_isPaused)
            {
                _isPaused = false;
                if (pausePanel != null) pausePanel.SetActive(false);
                Time.timeScale = 1f;
            }
            return;
        }
        // キーバッドが開いているときは無視
        if (keypadUI != null && keypadUI.activeSelf) return;

        // インベントリが開いている時は無視
        if (!_isPaused && UIInventory.Instance != null && UIInventory.Instance.IsOpen) return;

        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (_isPaused) Resume();
        else Pause();
    }

    // ----------------------------------------------------------------
    // 公開API
    // ----------------------------------------------------------------

    public void Pause()
    {
        _isPaused = true;

        if (pausePanel != null) pausePanel.SetActive(true);

        // カーソル表示
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 時間停止
        Time.timeScale = 0f;

        Debug.Log("[PauseMenuManager] Paused");
    }

    public void Resume()
    {
        if (!_isPaused) return;

        _isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);

        // カーソル非表示
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        Debug.Log("[PauseMenuManager] Resumed");
    }

    // ----------------------------------------------------------------
    // ボタンイベント
    // ----------------------------------------------------------------

    private void OnResumeClicked()
    {
        Resume();
    }

    private void OnRetryClicked()
    {
        Time.timeScale = 1f; // 念のため時間を戻す
        _isPaused = false;
        if(pausePanel != null) pausePanel.SetActive(false);

        // カーソル非表示
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (sceneLoader != null)
        {
            // SceneLoaderのノイズ演出で遷移
            sceneLoader.LoadScene(retryScene);
        }
        else
        {
            Debug.LogWarning("[PauseMenuManager] SceneLoader未設定 → 直接遷移");
            SceneManager.LoadScene(retryScene.ToString());
        }
    }

    private void OnExitClicked()
    {
        Time.timeScale = 1f;

        if (exitMode == ExitMode.GoToTitle)
        {
            _isPaused = false;
            if (pausePanel != null) pausePanel.SetActive(false);

            if (sceneLoader != null)
                sceneLoader.LoadScene(titleScene);
            else
                SceneManager.LoadScene(titleScene.ToString());
            return;
        }

        // QuitApplication
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
