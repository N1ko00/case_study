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

        // インベントリが閉じていて、かつマウスがロックされていない場合、ロックし直す（念のため）
        //if (Cursor.lockState != CursorLockMode.Locked)
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;
        //    Cursor.visible = false;
        //}

        Look();
        Move();
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

        // 当たり判定を検知するための簡易コンポーネントを追加
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

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed) isRunning = true;
        if (context.canceled) isRunning = false;
    }

    void Move()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        bool isMoving = move.magnitude > 0.1f && controller.isGrounded;

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * currentSpeed;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);

        //足音処理
        HandleFootstep(isMoving);
    }

    void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

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
            footstepTimer = isRunning ? footstepInterval * 0.6f : footstepInterval;

            Debug.Log("足音発生！");

            SoundManager.Instance.EmitNoise(
                this.transform.position,
                footstepRadius,
                NoiseSourceType.Player
            );
        }
    }
    void HandleItemPickup()
    {
        // 左クリック（Input System）
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, itemPickupDistance))
            {
                if (hit.collider.CompareTag("Item"))
                {
                    WorldItem item = hit.collider.GetComponent<WorldItem>();

                    if (item != null)
                    {
                        Debug.Log("拾った：" + item.itemData.itemName);

                        InventoryManager.Instance.AddItem(item.itemData);

                        Destroy(item.gameObject);
                    }
                }
            }
        }
    }
}

// 判定を検知するための小さなクラス（同じファイル内でOK）
public class DetectionTrigger : MonoBehaviour
{
    public string areaName;
    private void OnTriggerEnter(Collider other)
    {
        // 自分自身（Player）以外が触れたらログを出す
        if (!other.CompareTag("Player"))
        {
            Debug.Log($"{areaName} に {other.name} が入りました！");
        }
    }
}

