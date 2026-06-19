//using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(CharacterController))]
//public class FPSController : MonoBehaviour
//{
//    public Transform playerCamera;

//    [Header("移動設定")]
//    public float walkSpeed = 5f;
//    public float runSpeed = 10f;
//    public float gravity = -9.81f;

//    [Header("アイテム取得")]
//    public float itemPickupDistance = 3f;

//    [Header("視点")]
//    public float mouseSensitivity = 0.1f;
//    public float minLookAngle = -75f;
//    public float maxLookAngle = 75f;

//    [Header("音・検知の当たり判定設定")]
//    public float voiceDetectionRadius = 5f;
//    public float actionSoundRadius = 8f;

//    [Header("足音設定")]
//    [SerializeField] private float footstepInterval = 0.5f;
//    [SerializeField] private float footstepRadius = 8f;

//    private float footstepTimer;

//    private SphereCollider voiceCollider;
//    private SphereCollider actionCollider;

//    private CharacterController controller;
//    private float yVelocity;
//    private float xRotation = 0f;

//    private Vector2 moveInput;
//    private Vector2 lookInput;
//    private bool isRunning;

//    private WorldItem currentlyHighlightingItem;

//    [Header("プレイヤー動きカメラ止める用")]
//    [SerializeField] private bool canMove = true;
//    [SerializeField] private bool canLook = true;

//    [Header("制限設定")]
//    [SerializeField] private bool hasTablet = false;

//    void Awake()
//    {
//        controller = GetComponent<CharacterController>();
//        SetupDetectionColliders();
//    }

//    void Start()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }

//    void Update()
//    {
//        if (UIInventory.Instance != null && UIInventory.Instance.IsOpen)
//        {
//            return;
//        }

//        // スペースキーの入力制限（タブレットがないときは弾く）
//        if (!hasTablet && Keyboard.current.spaceKey.wasPressedThisFrame)
//        {
//            Debug.Log("タブレットを持っていないため、スペースキーは無効です。");
//            return; // これ以降の処理をさせない
//        }

//        // カメラ操作
//        if (canLook)
//        {
//            Look();
//        }

//        // 移動処理
//        if (canMove)
//        {
//            Move();
//        }

//        HandleItemPickup();
//    }

//    void SetupDetectionColliders()
//    {
//        GameObject voiceObj = new GameObject("VoiceDetectionArea");
//        voiceObj.transform.SetParent(this.transform);
//        voiceObj.transform.localPosition = Vector3.up * 1.5f;

//        voiceCollider = voiceObj.AddComponent<SphereCollider>();
//        voiceCollider.isTrigger = true;
//        voiceCollider.radius = voiceDetectionRadius;

//        var voiceDetector = voiceObj.AddComponent<DetectionTrigger>();
//        voiceDetector.areaName = "声の届く範囲";

//        GameObject actionObj = new GameObject("ActionSoundArea");
//        actionObj.transform.SetParent(this.transform);
//        actionObj.transform.localPosition = Vector3.zero;

//        actionCollider = actionObj.AddComponent<SphereCollider>();
//        actionCollider.isTrigger = true;
//        actionCollider.radius = actionSoundRadius;

//        var actionDetector = actionObj.AddComponent<DetectionTrigger>();
//        actionDetector.areaName = "アクション音の範囲";
//    }

//    public void OnMove(InputAction.CallbackContext context)
//        => moveInput = context.ReadValue<Vector2>();

//    public void OnLook(InputAction.CallbackContext context)
//        => lookInput = context.ReadValue<Vector2>();

//    public void OnRun(InputAction.CallbackContext context)
//    {
//        if (context.performed) isRunning = true;
//        if (context.canceled) isRunning = false;
//    }

//    void Move()
//    {
//        float currentSpeed = isRunning ? runSpeed : walkSpeed;

//        Vector3 move =
//            transform.right * moveInput.x +
//            transform.forward * moveInput.y;

//        bool isMoving = move.magnitude > 0.1f && controller.isGrounded;

//        if (controller.isGrounded && yVelocity < 0)
//            yVelocity = -2f;

//        yVelocity += gravity * Time.deltaTime;

//        Vector3 velocity = move * currentSpeed;
//        velocity.y = yVelocity;

//        controller.Move(velocity * Time.deltaTime);

//        HandleFootstep(isMoving);
//    }

//    void Look()
//    {
//        float mouseX = lookInput.x * mouseSensitivity;
//        float mouseY = lookInput.y * mouseSensitivity;

//        xRotation -= mouseY;
//        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

//        playerCamera.localRotation =
//            Quaternion.Euler(xRotation, 0f, 0f);

//        transform.Rotate(Vector3.up * mouseX);
//    }

//    void HandleFootstep(bool isMoving)
//    {
//        if (!isMoving)
//        {
//            footstepTimer = 0f;
//            return;
//        }

//        footstepTimer -= Time.deltaTime;

//        if (footstepTimer <= 0f)
//        {
//            footstepTimer =
//                isRunning ? footstepInterval * 0.6f : footstepInterval;

//            //Debug.Log("足音発生！");

//            SoundManager.Instance.EmitNoise(
//                this.transform.position,
//                footstepRadius,
//                NoiseSourceType.Player
//            );
//        }
//    }

//    void HandleItemPickup()
//    {
//        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
//        WorldItem itemInView = null;

//        int itemLayerMask = LayerMask.GetMask("Item");

//        if (Physics.Raycast(ray, out RaycastHit hit, itemPickupDistance, itemLayerMask))
//        {
//            if (hit.collider.CompareTag("Item"))
//            {
//                WorldItem foundItem = hit.collider.GetComponent<WorldItem>();

//                if (foundItem != null)
//                {
//                    LockerDoor locker = foundItem.GetComponentInParent<LockerDoor>();

//                    if (locker != null)
//                    {
//                        var field = typeof(LockerDoor).GetField("doorPivot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//                        if (field != null)
//                        {
//                            Transform doorPivotTransform = field.GetValue(locker) as Transform;

//                            if (doorPivotTransform != null)
//                            {
//                                float angleDiff = Quaternion.Angle(doorPivotTransform.localRotation, Quaternion.identity);

//                                if (angleDiff < 10f)
//                                {
//                                    foundItem = null;
//                                }
//                            }
//                        }
//                    }

//                    itemInView = foundItem;
//                }
//            }
//        }

//        if (itemInView != currentlyHighlightingItem)
//        {
//            if (currentlyHighlightingItem != null) currentlyHighlightingItem.SetHighlight(false);
//            if (itemInView != null) itemInView.SetHighlight(true);
//            currentlyHighlightingItem = itemInView;
//        }

//        if (Mouse.current.leftButton.wasPressedThisFrame && currentlyHighlightingItem != null)
//        {
//            Debug.Log("拾った：" + currentlyHighlightingItem.itemData.itemName);

//            if (currentlyHighlightingItem.itemData.itemName == "Tablet")
//            {
//                hasTablet = true;
//                Debug.Log("タブレットを入手！スペースキーが解放されました。");
//            }

//            InventoryManager.Instance.AddItem(currentlyHighlightingItem.itemData);

//            GameObject objectToDelete = currentlyHighlightingItem.gameObject;
//            currentlyHighlightingItem = null;
//            Destroy(objectToDelete);
//        }
//    }

//    // ★ 外部（独立タブレットスクリプト）から呼び出してロック解除する関数
//    public void UnlockSpaceKey()
//    {
//        hasTablet = true;
//        Debug.Log("タブレットが視認されクリックされました。スペースキーを解放します。");
//    }

//    public void SetMoveEnabled(bool value)
//    {
//        canMove = value;

//        if (!value)
//        {
//            moveInput = Vector2.zero;
//            isRunning = false;
//        }
//    }

//    public void SetLookEnabled(bool value)
//    {
//        canLook = value;

//        if (!value)
//        {
//            lookInput = Vector2.zero;
//        }
//    }

//    public void SetPlayerControl(bool move, bool look)
//    {
//        SetMoveEnabled(move);
//        SetLookEnabled(look);
//    }
//}

//// 💡 括弧の不整合を防ぐため、DetectionTriggerはクラス外の末尾に完全独立して配置
//public class DetectionTrigger : MonoBehaviour
//{
//    public string areaName;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (!other.CompareTag("Player"))
//        {
//            //Debug.Log($"{areaName} に {other.name} が入りました！");
//        }
//    }
//}

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

    [Header("プレイヤー動きカメラ止める用")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canLook = true;

    [Header("制限設定")]
    [SerializeField] private bool hasTablet = false;

    [Header("キーパッド参照")]
    public GameObject keypadUI; // ← KeyPadTriggerと同じGameObjectを割り当て

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
        // ポーズメニュー表示中は操作を止める
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsOpen)
            return;

        // キーパッドが表示中は操作を止める
        if (keypadUI != null && keypadUI.activeSelf)
            return;

        if (UIInventory.Instance != null && UIInventory.Instance.IsOpen)
        {
            return;
        }

        // スペースキーの入力制限（タブレットがないときは弾く）
        if (!hasTablet && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("タブレットを持っていないため、スペースキーは無効です。");
            return; // これ以降の処理をさせない
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
        GameObject voiceObj = new GameObject("VoiceDetectionArea");
        voiceObj.transform.SetParent(this.transform);
        voiceObj.transform.localPosition = Vector3.up * 1.5f;

        voiceCollider = voiceObj.AddComponent<SphereCollider>();
        voiceCollider.isTrigger = true;
        voiceCollider.radius = voiceDetectionRadius;

        var voiceDetector = voiceObj.AddComponent<DetectionTrigger>();
        voiceDetector.areaName = "声の届く範囲";

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

            //Debug.Log("足音発生！");

            SoundManager.Instance.EmitNoise(
                this.transform.position,
                footstepRadius,
                NoiseSourceType.Player
            );
        }
    }

    void HandleItemPickup()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        WorldItem itemInView = null;

        int itemLayerMask = LayerMask.GetMask("Item");

        if (Physics.Raycast(ray, out RaycastHit hit, itemPickupDistance, itemLayerMask))
        {
            if (hit.collider.CompareTag("Item"))
            {
                WorldItem foundItem = hit.collider.GetComponent<WorldItem>();

                if (foundItem != null)
                {
                    LockerDoor locker = foundItem.GetComponentInParent<LockerDoor>();

                    if (locker != null)
                    {
                        var field = typeof(LockerDoor).GetField("doorPivot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (field != null)
                        {
                            Transform doorPivotTransform = field.GetValue(locker) as Transform;

                            if (doorPivotTransform != null)
                            {
                                float angleDiff = Quaternion.Angle(doorPivotTransform.localRotation, Quaternion.identity);

                                if (angleDiff < 10f)
                                {
                                    foundItem = null;
                                }
                            }
                        }
                    }

                    itemInView = foundItem;
                }
            }
        }

        if (itemInView != currentlyHighlightingItem)
        {
            if (currentlyHighlightingItem != null) currentlyHighlightingItem.SetHighlight(false);
            if (itemInView != null) itemInView.SetHighlight(true);
            currentlyHighlightingItem = itemInView;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && currentlyHighlightingItem != null)
        {
            Debug.Log("拾った：" + currentlyHighlightingItem.itemData.itemName);

            // ★ 追加：新しい背景付きメッセージウィンドウにアイテム名とメッセージを送る
            if (UIManager.Instance != null && currentlyHighlightingItem.itemData != null)
            {
                UIManager.Instance.ShowItemMessage(currentlyHighlightingItem.itemData.itemName, "を見つけた");
            }

            if (currentlyHighlightingItem.itemData.itemName == "Tablet")
            {
                hasTablet = true;
                Debug.Log("タブレットを入手！スペースキーが解放されました。");
            }

            InventoryManager.Instance.AddItem(currentlyHighlightingItem.itemData);

            GameObject objectToDelete = currentlyHighlightingItem.gameObject;
            currentlyHighlightingItem = null;
            Destroy(objectToDelete);
        }
    }

    // ★ 外部（独立タブレットスクリプト）から呼び出してロック解除する関数
    public void UnlockSpaceKey()
    {
        hasTablet = true;
        Debug.Log("タブレットが視認されクリックされました。スペースキーを解放します。");
    }

    public void SetMoveEnabled(bool value)
    {
        canMove = value;

        if (!value)
        {
            moveInput = Vector2.zero;
            isRunning = false;
        }
    }

    public void SetLookEnabled(bool value)
    {
        canLook = value;

        if (!value)
        {
            lookInput = Vector2.zero;
        }
    }

    public void SetPlayerControl(bool move, bool look)
    {
        SetMoveEnabled(move);
        SetLookEnabled(look);
    }
}

public class DetectionTrigger : MonoBehaviour
{
    public string areaName;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            //Debug.Log($"{areaName} に {other.name} が入りました！");
        }
    }
}