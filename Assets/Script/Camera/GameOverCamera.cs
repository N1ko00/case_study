using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSequence : MonoBehaviour
{
    [Header("カメラ移動の設定")]
    [Tooltip("現在プレイヤーを映しているメインカメラ")]
    public Camera mainCamera;
    [Tooltip("ゲームオーバー時にカメラが移動する目標地点（空のGameObject等）")]
    public Transform cameraTargetPosition;
    [Tooltip("カメラが目標地点に移動するまでの時間（秒）")]
    public float cameraMoveDuration = 0.5f;

    [Header("敵のアニメーション設定")]
    [Tooltip("敵のアニメーター")]
    public Animator enemyAnimator;
    [Tooltip("Animatorで設定したTriggerパラメーター名")]
    public string animationTriggerName = "GameOverTrigger";

    [Header("シーン遷移の設定")]
    [Tooltip("アニメーション再生後、リザルト画面に遷移するまでの待機時間")]
    public float waitBeforeResult = 3.0f;
    [Tooltip("遷移先のリザルト画面のシーン名")]
    public string resultSceneName = "ResultScene";

    private bool isGameOver = false;

    /// <summary>
    /// 敵の当たり判定処理からこのメソッドを呼び出してくださいませ。
    /// </summary>
    public void ExecuteGameOver()
    {
        // 既にゲームオーバー処理が始まっている場合は何もしません
        if (isGameOver) return;
        isGameOver = true;

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        // カメラ移動と回転処理
        if (mainCamera != null && cameraTargetPosition != null)
        {
            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            float elapsedTime = 0f;

            // cameraMoveDurationの時間をかけて、カメラを目標地点へ動かす
            while (elapsedTime < cameraMoveDuration)
            {
                mainCamera.transform.position = Vector3.Lerp(startPos, cameraTargetPosition.position, elapsedTime / cameraMoveDuration);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, cameraTargetPosition.rotation, elapsedTime / cameraMoveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // ズレを防ぐため、目標地点へ合わせます
            mainCamera.transform.position = cameraTargetPosition.position;
            mainCamera.transform.rotation = cameraTargetPosition.rotation;
        }

        // 敵のアニメーションを再生
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(animationTriggerName);
        }

        // 待機時間
        yield return new WaitForSeconds(waitBeforeResult);

        // リザルト画面遷移
        SceneManager.LoadScene(resultSceneName);
    }
}
