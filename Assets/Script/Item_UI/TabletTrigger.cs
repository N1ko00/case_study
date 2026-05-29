using UnityEngine;
using UnityEngine.InputSystem;

public class TabletTrigger : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float interactDistance = 3.0f; // 反応する距離
    [SerializeField] private float dotThreshold = 0.95f;   // 視線が合っているかの判定（1.0に近づくほどシビア）

    private Transform playerCameraTransform;
    private FPSController playerController;

    void Start()
    {
        // シーン内からプレイヤーを探してコンポーネントを取得
        playerController = Object.FindFirstObjectByType<FPSController>();
        if (playerController != null)
        {
            // プレイヤーのカメラ（FPSControllerで設定されているもの）を取得
            playerCameraTransform = playerController.playerCamera;
        }
    }

    void Update()
    {
        if (playerCameraTransform == null || playerController == null) return;

        // 1. プレイヤーとの距離を計算
        float distance = Vector3.Distance(playerCameraTransform.position, transform.position);
        if (distance > interactDistance) return; // 離れすぎていたら何もしない

        // 2. プレイヤーのカメラがタブレットの方向を向いているか（視線チェック）
        Vector3 directionToTablet = (transform.position - playerCameraTransform.position).normalized;
        float dot = Vector3.Dot(playerCameraTransform.forward, directionToTablet);

        // 視線が一定以上合っているか判定
        if (dot >= dotThreshold)
        {
            // 3. その状態で左クリックが押されたか
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // プレイヤーのスペースキーを解放
                playerController.EnableSpaceKey();

                // タブレットオブジェクト自体を消去
                Destroy(gameObject);
            }
        }
    }
}