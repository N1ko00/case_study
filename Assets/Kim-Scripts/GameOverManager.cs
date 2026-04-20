using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームオーバー UI を管理します。
/// EnemyContactDetector から TriggerGameOver() を呼び出してください。
/// </summary>
public class GameOverManager : MonoBehaviour
{
    // シングルトン: どこからでも GameOverManager.Instance で呼べる
    public static GameOverManager Instance { get; private set; }

    [Header("UI参照")]
    // ゲームオーバーパネル
    [SerializeField] private GameObject gameOverPanel;
    // gameOverText
    [SerializeField] private Text gameOverText;
    // retryButton
    [SerializeField] private Button retryButton;

    //カメラのcanvas削除用
    [Header("追加UI参照")]
    [SerializeField] private GameObject cameraCanvas;

    [Header("設定")]
    // ゲームオーバー テキスト
    [SerializeField] private string gameOverMessage = "GAME OVER";
    // リトライ時にロードするシーン名（空欄のままにすると現在のシーンが再開されます）
    [SerializeField] private string retrySceneName = "";

    private bool _isGameOver = false;


    // ───────────────────────────────────────────
    // Unity ライフサイクル
    // ───────────────────────────────────────────
    private void Awake()
    {
        // シングルトン
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 開始時にパネルを非表示
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // リトライボタン イベント登録
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
    }

    // ───────────────────────────────────────────
    // 外部呼び出し API
    // ───────────────────────────────────────────

    /// <summary>
    /// ゲームオーバーを発生させます。
    /// EnemyContactDetector で敵と衝突した際に呼び出されます。
    /// </summary>
    public void TriggerGameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        //先にカメラのUIを消す
        if (cameraCanvas != null)
        {
            cameraCanvas.SetActive(false);
        }

        if (gameOverText != null)
            gameOverText.text = gameOverMessage;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // ゲームを一時停止
        Time.timeScale = 0f;

        // カーソル解除
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[GameOverManager] ゲームオーバー");
    }

    // ───────────────────────────────────────────
    // ボタンイベント
    // ───────────────────────────────────────────

    /// <summary>
    /// リトライボタンをクリックするとシーンが再開されます。
    /// </summary>
    private void OnRetryClicked()
    {
        // 時間スケールの復元後にシーンを再ロード
        Time.timeScale = 1f;

        string sceneName = string.IsNullOrEmpty(retrySceneName)
            ? SceneManager.GetActiveScene().name
            : retrySceneName;

        SceneManager.LoadScene(sceneName);

        Debug.Log($"[GameOverManager] リトライ → {sceneName}");
    }
}