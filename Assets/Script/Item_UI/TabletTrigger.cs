using UnityEngine;
using UnityEngine.InputSystem;

public class TabletTrigger : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float interactDistance = 3.0f; // 反応する距離（近づく必要がある範囲）
    [SerializeField] private float dotThreshold = 0.95f;   // 視線の一致度（1.0に近づくほどカメラのド正面にする必要あり）

    private Transform playerCameraTransform;
    private FPSController playerController;

    void Start()
    {
        // シーン内のプレイヤーを探して参照を取得
        playerController = Object.FindFirstObjectByType<FPSController>();
        if (playerController != null)
        {
            playerCameraTransform = playerController.playerCamera;
        }
    }

    void Update()
    {
        if (playerCameraTransform == null || playerController == null) return;

        // 1. プレイヤーとタブレットオブジェクトの距離を測る
        float distance = Vector3.Distance(playerCameraTransform.position, transform.position);
        if (distance > interactDistance) return; // 遠すぎる場合は処理を中断

        // 2. カメラの正面ベクトルと、タブレットへの方向ベクトルの内積（角度差）を計算
        Vector3 directionToTablet = (transform.position - playerCameraTransform.position).normalized;
        float dot = Vector3.Dot(playerCameraTransform.forward, directionToTablet);

        // 3. 視線が合っているか（画面中央付近に捉えているか）
        if (dot >= dotThreshold)
        {
            // 4. その状態で左クリックを押したらオブジェクトを削除して制限を解除
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                playerController.UnlockSpaceKey();
                Destroy(gameObject); // 自分自身（タブレットオブジェクト）を消去
            }
        }
    }
}