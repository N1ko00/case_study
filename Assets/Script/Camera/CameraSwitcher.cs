using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    // カメラの状態を管理する列挙型
    public enum CameraState
    {
        Main,
        Sub
    }

    [Header("Cameras")]
    public Camera MainCamera;
    public Camera SubCamera;

    // 現在の状態を保持します
    public CameraState CurrentState { get; private set; }

    void Start()
    {
        if (MainCamera != null && SubCamera != null)
        {
            // 初期状態をMainカメラに設定
            SetCameraState(CameraState.Main);
        }
        else
        {
            Debug.LogError("カメラが割り当てられていません!インスペクターを確認してください。");
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleCamera();
        }
    }

    /// <summary>
    /// 状態を指定してカメラを切り替える関数です。
    /// 他のスクリプトから「switchScript.SetCameraState(CameraSwitcher.CameraState.Sub);」のように呼べます
    /// </summary>
    public void SetCameraState(CameraState newState)
    {
        CurrentState = newState;
        Debug.Log($"現在のカメラの状態: {CurrentState}");

        switch (CurrentState)
        {
            case CameraState.Main:
                // GameObject自体のアクティブ状態を切り替え
                MainCamera.gameObject.SetActive(true);
                SubCamera.gameObject.SetActive(false);
                break;

            case CameraState.Sub:
                MainCamera.gameObject.SetActive(false);
                SubCamera.gameObject.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// 現在の状態を判定して、もう一方のカメラに切り替える関数です。
    /// 他のスクリプトから「switchScript.SetCameraState(CameraSwitcher.CameraState.Sub);」のように呼べます
    /// </summary>
    public void ToggleCamera()
    {
        if (CurrentState == CameraState.Main)
        {
            SetCameraState(CameraState.Sub);
        }
        else
        {
            SetCameraState(CameraState.Main);
        }
    }
}
