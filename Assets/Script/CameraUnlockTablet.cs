
//using System.Collections;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class CameraUnlockTablet : MonoBehaviour
//{
//    [Header("Linked Objects")]
//    [Tooltip("Connect the object with CameraSwitcher attached")]
//    public CameraSwitcher cameraSwitcher;

//    [Tooltip("Connect the object with BatteryManager attached")]
//    public BatteryManager batteryManager;

//    [Header("Interaction Settings")]
//    [SerializeField] private float interactDistance = 3.0f;
//    [SerializeField] private float dotThreshold = 0.95f;

//    [Header("Zoom Effect Settings")]
//    [SerializeField] private float zoomInFOV = 30f;
//    [SerializeField] private float zoomDuration = 0.8f;
//    [SerializeField] private float holdDuration = 2.0f;
//    [SerializeField] private Color tabletHighlightColor = Color.red;

//    [Header("Notification Settings")]
//    [SerializeField] private float notificationDuration = 4.0f;

//    private Transform playerCameraTransform;
//    private Camera playerCameraComponent;
//    private FPSController playerController;
//    private float defaultFOV = 60f;

//    private bool isLookingAtTablet = false;
//    private bool canClickTablet = false;
//    private bool isZoomingNow = false;
//    private GUIStyle guiStyle;

//    private Renderer tabletRenderer;
//    private Color originalColor = Color.white;

//    void Start()
//    {
//        tabletRenderer = GetComponent<Renderer>();
//        if (tabletRenderer == null)
//        {
//            tabletRenderer = GetComponentInChildren<Renderer>();
//        }

//        if (tabletRenderer != null)
//        {
//            originalColor = tabletRenderer.material.color;
//        }

//        if (cameraSwitcher != null)
//        {
//            cameraSwitcher.SetCameraState(0);
//            cameraSwitcher.enabled = false;
//        }

//        if (batteryManager != null)
//        {
//            batteryManager.enabled = false;
//        }

//        playerController = Object.FindFirstObjectByType<FPSController>();
//        if (playerController != null)
//        {
//            playerCameraTransform = playerController.playerCamera;

//            if (playerCameraTransform != null)
//            {
//                playerCameraComponent = playerCameraTransform.GetComponent<Camera>();
//                if (playerCameraComponent != null)
//                {
//                    defaultFOV = playerCameraComponent.fieldOfView;
//                }
//            }

//            LookAtTabletImmediately();
//            StartCoroutine(PlayZoomEffect());
//        }
//    }

//    void Update()
//    {
//        if (playerCameraTransform == null || playerController == null)
//        {
//            isLookingAtTablet = false;
//            canClickTablet = false;
//            return;
//        }

//        float distance = Vector3.Distance(playerCameraTransform.position, transform.position);
//        if (distance > interactDistance)
//        {
//            isLookingAtTablet = false;
//            canClickTablet = false;
//            return;
//        }

//        Vector3 directionToTablet = (transform.position - playerCameraTransform.position).normalized;
//        float dot = Vector3.Dot(playerCameraTransform.forward, directionToTablet);

//        if (dot >= dotThreshold)
//        {
//            isLookingAtTablet = true;
//            canClickTablet = true;

//            if (Mouse.current.leftButton.wasPressedThisFrame)
//            {
//                UnlockTabletFeatures();
//            }
//        }
//        else
//        {
//            isLookingAtTablet = false;
//            canClickTablet = false;
//        }
//    }

//    private void LookAtTabletImmediately()
//    {
//        if (playerController == null || playerCameraTransform == null) return;

//        // タブレットの方向を向かせる
//        Vector3 directionToTablet = (transform.position - playerCameraTransform.position).normalized;

//        Vector3 forwardOnXZ = new Vector3(directionToTablet.x, 0f, directionToTablet.z).normalized;
//        if (forwardOnXZ != Vector3.zero)
//        {
//            playerController.transform.rotation = Quaternion.LookRotation(forwardOnXZ);
//        }

//        float lookAngleX = Mathf.Asin(directionToTablet.y) * Mathf.Rad2Deg;
//        float targetXRotation = -lookAngleX;

//        var xRotField = typeof(FPSController).GetField("xRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        if (xRotField != null)
//        {
//            xRotField.SetValue(playerController, targetXRotation);
//        }

//        playerCameraTransform.localRotation = Quaternion.Euler(targetXRotation, 0f, 0f);
//    }

//    private IEnumerator PlayZoomEffect()
//    {
//        if (playerCameraComponent == null || playerController == null) yield break;

//        playerController.SetLookEnabled(false);
//        playerController.SetMoveEnabled(false);
//        isZoomingNow = true;

//        if (tabletRenderer != null)
//        {
//            tabletRenderer.material.color = tabletHighlightColor;
//        }

//        // ズームイン開始
//        float elapsed = 0f;
//        while (elapsed < zoomDuration)
//        {
//            elapsed += Time.deltaTime;
//            playerCameraComponent.fieldOfView = Mathf.Lerp(defaultFOV, zoomInFOV, Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration));
//            yield return null;
//        }
//        playerCameraComponent.fieldOfView = zoomInFOV;

//        // ★ ズームが完了したタイミングで messagewindow を使って表示
//        // アイテム名は空（""）にして、下の広い枠にメッセージを出します。
//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowItemMessage("", "「……何だあれ？」");
//        }

//        // メッセージを読ませるために一定時間キープ
//        yield return new WaitForSeconds(holdDuration);

//        // ズームアウト開始
//        elapsed = 0f;
//        while (elapsed < zoomDuration)
//        {
//            elapsed += Time.deltaTime;
//            playerCameraComponent.fieldOfView = Mathf.Lerp(zoomInFOV, defaultFOV, Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration));
//            yield return null;
//        }
//        playerCameraComponent.fieldOfView = defaultFOV;

//        if (tabletRenderer != null)
//        {
//            tabletRenderer.material.color = originalColor;
//        }

//        isZoomingNow = false;

//        playerController.SetLookEnabled(true);
//        playerController.SetMoveEnabled(true);
//    }

//    private void OnGUI()
//    {
//        // 旧システムでの「……何だあれ？」の表示処理は削除しました（messagewindowに統合したため）

//        if (isZoomingNow || !canClickTablet) return;

//        float posX = Screen.width / 2f;
//        float posY = Screen.height / 2f;

//        if (guiStyle == null)
//        {
//            guiStyle = new GUIStyle();
//            guiStyle.alignment = TextAnchor.MiddleCenter;
//            guiStyle.fontSize = 24;
//            guiStyle.fontStyle = FontStyle.Bold;
//        }

//        guiStyle.normal.textColor = Color.green;
//        GUI.Label(new Rect(posX - 150, posY + 20, 300, 30), "左クリックでTABLETを拾う", guiStyle);
//    }

//    private void UnlockTabletFeatures()
//    {
//        if (playerController != null)
//        {
//            playerController.UnlockSpaceKey();
//        }

//        // タブレット取得時のUI表示
//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowItemMessage("タブレット", "を見つけた");
//        }

//        if (cameraSwitcher != null) cameraSwitcher.enabled = true;
//        if (batteryManager != null) batteryManager.enabled = true;

//        GameObject msgObj = new GameObject("UnlockNotificationUI");
//        var notifier = msgObj.AddComponent<UnlockNotifier>();
//        notifier.duration = notificationDuration;

//        Debug.Log("Tablet Unlocked");

//        Destroy(gameObject);
//    }
//}

//public class UnlockNotifier : MonoBehaviour
//{
//    public float duration = 4.0f;
//    private GUIStyle notificationStyle;

//    void Start()
//    {
//        Destroy(gameObject, duration);
//    }

//    private void OnGUI()
//    {
//        if (notificationStyle == null)
//        {
//            notificationStyle = new GUIStyle();
//            notificationStyle.alignment = TextAnchor.MiddleCenter;
//            notificationStyle.fontSize = 20;
//            notificationStyle.fontStyle = FontStyle.Bold;
//            notificationStyle.normal.textColor = Color.yellow;
//        }

//        float posX = Screen.width / 2f;
//        GUI.Label(new Rect(posX - 300, 60, 600, 40), "スペースキーでカメラが使用可能になった", notificationStyle);
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraUnlockTablet : MonoBehaviour
{
    [Header("Linked Objects")]
    [Tooltip("Connect the object with CameraSwitcher attached")]
    public CameraSwitcher cameraSwitcher;

    [Tooltip("Connect the object with BatteryManager attached")]
    public BatteryManager batteryManager;

    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3.0f;
    [SerializeField] private float dotThreshold = 0.95f;

    [Header("Zoom Effect Settings")]
    [SerializeField] private float zoomInFOV = 30f;
    [SerializeField] private float zoomDuration = 0.8f;
    [SerializeField] private float holdDuration = 2.0f;
    [SerializeField] private Color tabletHighlightColor = Color.red;

    [Header("Notification Settings")]
    [SerializeField] private float notificationDuration = 4.0f;

    private Transform playerCameraTransform;
    private Camera playerCameraComponent;
    private FPSController playerController;
    private float defaultFOV = 60f;

    private bool isLookingAtTablet = false;
    private bool canClickTablet = false;
    private bool isZoomingNow = false;
    private GUIStyle guiStyle;

    private Renderer tabletRenderer;
    private Color originalColor = Color.white;

    void Start()
    {
        tabletRenderer = GetComponent<Renderer>();
        if (tabletRenderer == null)
        {
            tabletRenderer = GetComponentInChildren<Renderer>();
        }

        if (tabletRenderer != null)
        {
            originalColor = tabletRenderer.material.color;
        }

        if (cameraSwitcher != null)
        {
            cameraSwitcher.SetCameraState(0);
            cameraSwitcher.enabled = false;
        }

        if (batteryManager != null)
        {
            batteryManager.enabled = false;
        }

        playerController = Object.FindFirstObjectByType<FPSController>();
        if (playerController != null)
        {
            playerCameraTransform = playerController.playerCamera;

            if (playerCameraTransform != null)
            {
                playerCameraComponent = playerCameraTransform.GetComponent<Camera>();
                if (playerCameraComponent != null)
                {
                    defaultFOV = playerCameraComponent.fieldOfView;
                }
            }

            LookAtTabletImmediately();
            StartCoroutine(PlayZoomEffect());
        }
    }

    void Update()
    {
        if (playerCameraTransform == null || playerController == null)
        {
            isLookingAtTablet = false;
            canClickTablet = false;
            return;
        }

        float distance = Vector3.Distance(playerCameraTransform.position, transform.position);
        if (distance > interactDistance)
        {
            isLookingAtTablet = false;
            canClickTablet = false;
            return;
        }

        Vector3 directionToTablet = (transform.position - playerCameraTransform.position).normalized;
        float dot = Vector3.Dot(playerCameraTransform.forward, directionToTablet);

        if (dot >= dotThreshold)
        {
            isLookingAtTablet = true;
            canClickTablet = true;

            // マウスの左クリック、またはコントローラーのAボタン（buttonSouth）を検知
            bool mouseInteract = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool gamepadInteract = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

            if (mouseInteract || gamepadInteract)
            {
                UnlockTabletFeatures();
            }
        }
        else
        {
            isLookingAtTablet = false;
            canClickTablet = false;
        }
    }

    private void LookAtTabletImmediately()
    {
        if (playerController == null || playerCameraTransform == null) return;

        // タブレットの方向を向かせる
        Vector3 directionToTablet = (transform.position - playerCameraTransform.position).normalized;

        Vector3 forwardOnXZ = new Vector3(directionToTablet.x, 0f, directionToTablet.z).normalized;
        if (forwardOnXZ != Vector3.zero)
        {
            playerController.transform.rotation = Quaternion.LookRotation(forwardOnXZ);
        }

        float lookAngleX = Mathf.Asin(directionToTablet.y) * Mathf.Rad2Deg;
        float targetXRotation = -lookAngleX;

        var xRotField = typeof(FPSController).GetField("xRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (xRotField != null)
        {
            xRotField.SetValue(playerController, targetXRotation);
        }

        playerCameraTransform.localRotation = Quaternion.Euler(targetXRotation, 0f, 0f);
    }

    private IEnumerator PlayZoomEffect()
    {
        if (playerCameraComponent == null || playerController == null) yield break;

        playerController.SetLookEnabled(false);
        playerController.SetMoveEnabled(false);
        isZoomingNow = true;

        if (tabletRenderer != null)
        {
            tabletRenderer.material.color = tabletHighlightColor;
        }

        // ズームイン開始
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            playerCameraComponent.fieldOfView = Mathf.Lerp(defaultFOV, zoomInFOV, Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration));
            yield return null;
        }
        playerCameraComponent.fieldOfView = zoomInFOV;

        // ★ ズームが完了したタイミングで messagewindow を使って表示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowItemMessage("", "「……何だあれ？」");
        }

        // メッセージを読ませるために一定時間キープ
        yield return new WaitForSeconds(holdDuration);

        // ズームアウト開始
        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            playerCameraComponent.fieldOfView = Mathf.Lerp(zoomInFOV, defaultFOV, Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration));
            yield return null;
        }
        playerCameraComponent.fieldOfView = defaultFOV;

        if (tabletRenderer != null)
        {
            tabletRenderer.material.color = originalColor;
        }

        isZoomingNow = false;

        playerController.SetLookEnabled(true);
        playerController.SetMoveEnabled(true);
    }

    private void OnGUI()
    {
        if (isZoomingNow || !canClickTablet) return;

        float posX = Screen.width / 2f;
        float posY = Screen.height / 2f;

        if (guiStyle == null)
        {
            guiStyle = new GUIStyle();
            guiStyle.alignment = TextAnchor.MiddleCenter;
            guiStyle.fontSize = 24;
            guiStyle.fontStyle = FontStyle.Bold;
        }

        guiStyle.normal.textColor = Color.green;
        // テキストを「Aボタン」に修正
        GUI.Label(new Rect(posX - 250, posY + 20, 500, 30), "左クリック または AボタンでTABLETを拾う", guiStyle);
    }

    private void UnlockTabletFeatures()
    {
        if (playerController != null)
        {
            playerController.UnlockSpaceKey();
        }

        // タブレット取得時のUI表示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowItemMessage("タブレット", "を見つけた");
        }

        if (cameraSwitcher != null) cameraSwitcher.enabled = true;
        if (batteryManager != null) batteryManager.enabled = true;

        GameObject msgObj = new GameObject("UnlockNotificationUI");
        var notifier = msgObj.AddComponent<UnlockNotifier>();
        notifier.duration = notificationDuration;

        Debug.Log("Tablet Unlocked");

        Destroy(gameObject);
    }
}

public class UnlockNotifier : MonoBehaviour
{
    public float duration = 4.0f;
    private GUIStyle notificationStyle;

    void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnGUI()
    {
        if (notificationStyle == null)
        {
            notificationStyle = new GUIStyle();
            notificationStyle.alignment = TextAnchor.MiddleCenter;
            notificationStyle.fontSize = 20;
            notificationStyle.fontStyle = FontStyle.Bold;
            notificationStyle.normal.textColor = Color.yellow;
        }

        float posX = Screen.width / 2f;
        GUI.Label(new Rect(posX - 300, 60, 600, 40), "スペースキーでカメラが使用可能になった", notificationStyle);
    }
}