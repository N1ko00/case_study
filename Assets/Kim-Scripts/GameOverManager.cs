using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

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
    [SerializeField] private TextMeshProUGUI gameOverText;
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

    [Header("カメラ移動の設定")]
    [Tooltip("現在プレイヤーを映しているメインカメラ")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("ゲームオーバー時にカメラが移動する目標地点")]
    [SerializeField] private Transform cameraTargetPosition;
    [Tooltip("カメラが目標地点に移動するまでの時間（秒）")]
    [SerializeField] private float cameraMoveDuration = 0.5f;

    [Header("敵のアニメーション設定")]
    [Tooltip("敵のアニメーター")]
    [SerializeField] private Animator enemyAnimator;
    [Tooltip("Animatorで設定したTriggerパラメーター名")]
    [SerializeField] private string animationTriggerName = "GameOverTrigger";
    [Tooltip("アニメーション再生後、UIを表示するまでの待機時間")]
    [SerializeField] private float waitBeforeUI = 3.0f;

    [Header("モンスター")]
    [SerializeField] private InvisibleMonster monster;


    [Header("プレイヤー制御の停止")]
    [Tooltip("ゲームオーバー時に停止させたいスクリプト（視点操作や移動など）をここに登録")]
    [SerializeField] private Behaviour[] scriptsToDisable;

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

        //monster.SetVisible(false);
        //monster.gameObject.layer = 0;
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

        ////先にカメラのUIを消す
        //if (cameraCanvas != null)
        //{
        //    cameraCanvas.SetActive(false);
        //}

        //if (gameOverText != null)
        //    gameOverText.text = gameOverMessage;

        //if (gameOverPanel != null)
        //    gameOverPanel.SetActive(true);

        //// ゲームを一時停止
        //Time.timeScale = 0f;

        //// カーソル解除
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;

        //Debug.Log("[GameOverManager] ゲームオーバー");

        // 演出の一連の流れ（コルーチン）を開始します
        StartCoroutine(GameOverRoutine());
    }

    // ───────────────────────────────────────────
    // 演出の進行（コルーチン）
    // ───────────────────────────────────────────
    private IEnumerator GameOverRoutine()
    {

        //プレイヤーの視点操作や移動スクリプトを停止
        if (scriptsToDisable != null)
        {
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                {
                    script.enabled = false;
                }
            }
        }

        // プレイ中のカメラUIを消す
        if (cameraCanvas != null)
        {
            cameraCanvas.SetActive(false);
        }

        // カメラの移動と回転処理
        if (mainCamera != null && cameraTargetPosition != null)
        {
            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            float elapsedTime = 0f;

            while (elapsedTime < cameraMoveDuration)
            {
                mainCamera.transform.position = Vector3.Lerp(startPos, cameraTargetPosition.position, elapsedTime / cameraMoveDuration);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, cameraTargetPosition.rotation, elapsedTime / cameraMoveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            mainCamera.transform.position = cameraTargetPosition.position;
            mainCamera.transform.rotation = cameraTargetPosition.rotation;
        }

        //敵のアニメーションを再生
        if (enemyAnimator != null)
        {
            Debug.Log("[GameOverManager] 敵のアニメーション再生");
            //monster.SetVisible(true);
            SetLayerRecursively(monster.gameObject, 0);
            enemyAnimator.SetTrigger(animationTriggerName);
        }

        // 待機時間
        yield return new WaitForSeconds(waitBeforeUI);

        // UIを表示してゲームを停止
        ShowGameOverUI();
    }

    /// <summary>
    /// 最終的なUI表示と時間停止を行いますわ
    /// </summary>
    private void ShowGameOverUI()
    {
        if (gameOverText != null)
            gameOverText.text = gameOverMessage;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // ここでゲーム内の時間を完全に停止
        Time.timeScale = 0f;

        // カーソルを表示して操作できるようにする
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[GameOverManager] ゲームオーバー演出完了、UI表示");
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

    /// <summary>
    /// 親も子もまとめてレイヤーを変更する
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}