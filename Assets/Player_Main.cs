using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Transform playerCamera;

    [Header("移動設定")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float gravity = -9.81f;

    [Header("アイテム取得")]
    public float itemPickupDistance = 3f;

    [Header("視点")]
    public float mouseSensitivity = 0.1f;
    public float minLookAngle = -75f;
    public float maxLookAngle = 75f;

    [Header("音・検知の当たり判定設定")]
    public float voiceDetectionRadius = 5f;
    public float actionSoundRadius = 8f;

    [Header("足音設定")]
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private float footstepRadius = 8f;

    private float footstepTimer;

    private SphereCollider voiceCollider;
    private SphereCollider actionCollider;

    private CharacterController controller;
    private float yVelocity;
    private float xRotation = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;

    private WorldItem currentlyHighlightingItem;

    // ===== 追加 =====
    [Header("プレイヤー動きカメラ止める用")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canLook = true;

    [Header("制限設定（タブレット用）")]
    [SerializeField] private bool canUseSpaceKey = false; // 初期状態はスペースキー無効

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        SetupDetectionColliders();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (UIInventory.Instance != null && UIInventory.Instance.IsOpen)
        {
            return;
        }

        if (!canUseSpaceKey && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("タブレットを見て左クリックで取得するまで、スペースキーは使えません。");
            return; // スペースキーの入力をここで遮断
        }

        // カメラ操作
        if (canLook)
        {
            Look();
        }

        // 移動処理
        if (canMove)
        {
            Move();
        }

        HandleItemPickup();
    }

    void SetupDetectionColliders()
    {
        // 1. 声の検知用
        GameObject voiceObj = new GameObject("VoiceDetectionArea");
        voiceObj.transform.SetParent(this.transform);
        voiceObj.transform.localPosition = Vector3.up * 1.5f;

        voiceCollider = voiceObj.AddComponent<SphereCollider>();
        voiceCollider.isTrigger = true;
        voiceCollider.radius = voiceDetectionRadius;

        var voiceDetector = voiceObj.AddComponent<DetectionTrigger>();
        voiceDetector.areaName = "声の届く範囲";

        // 2. アクション音用
        GameObject actionObj = new GameObject("ActionSoundArea");
        actionObj.transform.SetParent(this.transform);
        actionObj.transform.localPosition = Vector3.zero;

        actionCollider = actionObj.AddComponent<SphereCollider>();
        actionCollider.isTrigger = true;
        actionCollider.radius = actionSoundRadius;

        var actionDetector = actionObj.AddComponent<DetectionTrigger>();
        actionDetector.areaName = "アクション音の範囲";
    }

    public void OnMove(InputAction.CallbackContext context)
        => moveInput = context.ReadValue<Vector2>();

    public void OnLook(InputAction.CallbackContext context)
        => lookInput = context.ReadValue<Vector2>();

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed) isRunning = true;
        if (context.canceled) isRunning = false;
    }

    void Move()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        bool isMoving = move.magnitude > 0.1f && controller.isGrounded;

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * currentSpeed;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);

        // 足音処理
        HandleFootstep(isMoving);
    }

    void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        playerCamera.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleFootstep(bool isMoving)
    {
        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            footstepTimer =
                isRunning ? footstepInterval * 0.6f : footstepInterval;

            Debug.Log("足音発生！");

            SoundManager.Instance.EmitNoise(
                this.transform.position,
                footstepRadius,
                NoiseSourceType.Player
            );
        }
    }

    //void HandleItemPickup()
    //{
    //    if (Mouse.current.leftButton.wasPressedThisFrame)
    //    {
    //        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

    //        if (Physics.Raycast(ray, out RaycastHit hit, itemPickupDistance))
    //        {
    //            if (hit.collider.CompareTag("Item"))
    //            {
    //                WorldItem item =
    //                    hit.collider.GetComponent<WorldItem>();

    //                if (item != null)
    //                {
    //                    Debug.Log("拾った：" + item.itemData.itemName);

    //                    InventoryManager.Instance.AddItem(item.itemData);

    //                    Destroy(item.gameObject);
    //                }
    //            }
    //        }
    //    }
    //}
    void HandleItemPickup()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        WorldItem itemInView = null;

        // 1. アイテム専用レイヤー（Item）だけを狙い撃ちして、ロッカーの壁を貫通させる
        int itemLayerMask = LayerMask.GetMask("Item");

        if (Physics.Raycast(ray, out RaycastHit hit, itemPickupDistance, itemLayerMask))
        {
            if (hit.collider.CompareTag("Item"))
            {
                WorldItem foundItem = hit.collider.GetComponent<WorldItem>();

                if (foundItem != null)
                {
                    // 2. アイテムの親階層から LockerDoor スクリプトを取得
                    LockerDoor locker = foundItem.GetComponentInParent<LockerDoor>();

                    if (locker != null)
                    {
                        // 3. 【確実な方法】ロッカーのスクリプトにくっついている「回転対象（doorPivot）」を直接調べる
                        // リフレクション（System.Reflection）を使って、privateなフィールド「doorPivot」の中身を覗き見します
                        var field = typeof(LockerDoor).GetField("doorPivot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (field != null)
                        {
                            Transform doorPivotTransform = field.GetValue(locker) as Transform;

                            if (doorPivotTransform != null)
                            {
                                // 扉の初期角度（0,0,0）からのズレ（回転角）を計算
                                float angleDiff = Quaternion.Angle(doorPivotTransform.localRotation, Quaternion.identity);

                                // 【判定】ズレが 10度未満（＝まだほぼ閉まっている状態）なら、アイテムを検知させない！
                                if (angleDiff < 10f)
                                {
                                    foundItem = null;
                                }
                            }
                        }
                    }

                    // ドアが開いている、または元から外に置いてあるアイテムなら正常にセットされる
                    itemInView = foundItem;
                }
            }
        }

        // --- 以下、ハイライトやクリック処理はそのまま触らなくてOK！ ---
        if (itemInView != currentlyHighlightingItem)
        {
            if (currentlyHighlightingItem != null) currentlyHighlightingItem.SetHighlight(false);
            if (itemInView != null) itemInView.SetHighlight(true);
            currentlyHighlightingItem = itemInView;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && currentlyHighlightingItem != null)
        {
            Debug.Log("拾った：" + currentlyHighlightingItem.itemData.itemName);
            InventoryManager.Instance.AddItem(currentlyHighlightingItem.itemData);
            currentlyHighlightingItem = null;
            Destroy(hit.collider.gameObject);
        }
    }

    // ==================================================
    // 外部から呼べる処理
    // ==================================================

    // 移動ON/OFF
    public void SetMoveEnabled(bool value)
    {
        canMove = value;

        // 停止時に入力をリセット
        if (!value)
        {
            moveInput = Vector2.zero;
            isRunning = false;
        }
    }

    // カメラON/OFF
    public void SetLookEnabled(bool value)
    {
        canLook = value;

        // 停止時に入力をリセット
        if (!value)
        {
            lookInput = Vector2.zero;
        }
    }

    // 両方まとめて制御
    public void SetPlayerControl(bool move, bool look)
    {
        SetMoveEnabled(move);
        SetLookEnabled(look);
    }

    //外部（タブレット）から呼び出してスペースキーを解放する関数
    public void EnableSpaceKey()
    {
        canUseSpaceKey = true;
        Debug.Log("スペースキーが解放されました！");
    }
}

// 判定を検知するための小さなクラス
public class DetectionTrigger : MonoBehaviour
{
    public string areaName;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Debug.Log($"{areaName} に {other.name} が入りました！");
        }
    }
}


