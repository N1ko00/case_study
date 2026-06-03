using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyCameraSpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyObject;
    public Vector3 positionOffset = new Vector3(0f, -0.5f, 2.0f);

    [Header("State Settings")]
    [SerializeField] private bool forceChaseModeOnSpawn = true;

    private CameraSwitcher cameraSwitcher;
    private bool isFirstTimeOpening = true;
    private bool isCameraActive = false;

    // Flags to track the very first return to player view
    private bool hasOpenedCameraAtLeastOnce = false;
    private bool hasTriggeredReturnMessage = false;

    // Message Display System
    private bool showReturnMessage = false;
    private GUIStyle messageStyle;
    [Header("Message Settings")]
    [SerializeField] private float messageDuration = 3.0f;

    void Start()
    {
        cameraSwitcher = GetComponent<CameraSwitcher>();
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (cameraSwitcher != null && cameraSwitcher.enabled)
            {
                isCameraActive = !isCameraActive;

                if (isCameraActive)
                {
                    // --- CAMERA OPENED ---
                    hasOpenedCameraAtLeastOnce = true; // Mark that player has entered camera mode

                    if (isFirstTimeOpening)
                    {
                        cameraSwitcher.SetCameraState(2);
                        isFirstTimeOpening = false;
                    }

                    Camera currentCamera = GetActiveCamera();
                    if (currentCamera != null && enemyObject != null)
                    {
                        MoveEnemyToCamera(currentCamera);
                    }
                }
                else
                {
                    // --- CAMERA CLOSED (Returned to Player View) ---
                    // Triggers only the very first time player returns from camera mode
                    if (hasOpenedCameraAtLeastOnce && !hasTriggeredReturnMessage)
                    {
                        StartCoroutine(DisplayReturnMessage());
                        hasTriggeredReturnMessage = true; // Block future triggers
                    }
                }
            }
        }
    }

    private IEnumerator DisplayReturnMessage()
    {
        showReturnMessage = true;
        yield return new WaitForSeconds(messageDuration);
        showReturnMessage = false;
    }

    private void OnGUI()
    {
        if (showReturnMessage)
        {
            if (messageStyle == null)
            {
                messageStyle = new GUIStyle();
                messageStyle.alignment = TextAnchor.MiddleCenter;
                messageStyle.fontSize = 22;
                messageStyle.fontStyle = FontStyle.Italic;
                messageStyle.normal.textColor = Color.white;
            }

            float posX = Screen.width / 2f;
            float posY = Screen.height / 2f;

            GUI.Label(new Rect(posX - 200, posY + 80, 400, 30), "「……何だあれは？」", messageStyle);
        }
    }

    private void MoveEnemyToCamera(Camera targetCamera)
    {
        Transform camTransform = targetCamera.transform;
        Vector3 spawnPosition = camTransform.TransformPoint(positionOffset);

        var rb = enemyObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        enemyObject.transform.position = spawnPosition;

        Vector3 lookRotation = camTransform.forward;
        lookRotation.y = 0f;
        if (lookRotation != Vector3.zero)
        {
            enemyObject.transform.rotation = Quaternion.LookRotation(lookRotation);
        }

        if (forceChaseModeOnSpawn)
        {
            EnemyFootstepAudio footstepAudio = enemyObject.GetComponent<EnemyFootstepAudio>();
            if (footstepAudio != null)
            {
                footstepAudio.SetChaseMode(true);
                footstepAudio.PlayFootstep();
            }
        }

        Debug.Log("Enemy moved to active camera position.");
    }

    private Camera GetActiveCamera()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.enabled && cam.gameObject.activeInHierarchy)
            {
                if (!cam.CompareTag("MainCamera"))
                {
                    return cam;
                }
            }
        }
        return Camera.main;
    }
}