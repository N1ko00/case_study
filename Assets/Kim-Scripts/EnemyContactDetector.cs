using UnityEngine;

/// <summary>
/// 敵とプレイヤーの物理的な衝突を検知し、ゲームオーバーを発生させます。
/// InvisibleMonster のような GameObject に貼り付けてください。
/// 適切なオブジェクトには Collider が必要です。
/// </summary>
public class EnemyContactDetector : MonoBehaviour
{
    [Header("衝突設定")]
    // playerTag
    [SerializeField] private string playerTag = "Player";

    // ───────────────────────────────────────────
    // 衝突検知
    // ───────────────────────────────────────────

    /// <summary>
    /// Collider方式：敵のColliderがプレイヤーのColliderに触れたとき
    /// CharacterController を使用するプレーヤーに対応しています。
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // この方式はプレイヤー側で検知されるため、ここでは使用しません
    }

    /// <summary>
    /// 敵の Collider が isTrigger = false のとき、プレイヤーとの接触を検知
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag)) return;
        TriggerGameOver();
    }

    /// <summary>
    /// 敵の Collider が isTrigger = true のとき、プレイヤーとの接触を検知
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        TriggerGameOver();
    }

    // ───────────────────────────────────────────
    // GameOver
    // ───────────────────────────────────────────
    private void TriggerGameOver()
    {
        if (GameOverManager.Instance == null)
        {
            Debug.LogWarning("[EnemyContactDetector] GameOverManager が見つかりません");
            return;
        }

        Debug.Log($"[EnemyContactDetector] プレイヤーが {gameObject.name} に触れた → ゲームオーバー");
        GameOverManager.Instance.TriggerGameOver();
    }
}