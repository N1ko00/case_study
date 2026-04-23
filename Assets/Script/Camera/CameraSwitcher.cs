using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    // カメラの状態を管理する列挙型
    public enum CameraState
    {
        Main,
        Sub1,
        Sub2
    }

    [Header("Cameras")]
    public Camera MainCamera;
    public Camera SubCamera;
    public Camera SubCamera2;

    [Header("Monster")]
    [SerializeField] private InvisibleMonster monster; // モンスターのGameObjectをインスペクターで割り当

    [Header("UI")]
    [SerializeField] private GameObject cameraCanvas;


    //1回切り変数
    bool unique = true;

    // 現在の状態を保持します
    public CameraState CurrentState { get; private set; }

    void Start()
    {
        if (MainCamera != null && SubCamera != null && SubCamera2 != null)
        {
            // 初期状態をMainカメラに設定
            SetCameraState(CameraState.Main);
    }
        else
        {
            Debug.LogError("カメラが設定されていません");
        }
    }

    void Update()
    {
        if (unique)
        {
            monster.SetVisible(false);
            unique = false; 
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
           ToggleCamera();
        }
    }

    //ボタン用の関数
    public void SwitchToMain() => SetCameraState(CameraState.Main);
    public void SwitchToSub1() => SetCameraState(CameraState.Sub1);
    public void SwitchToSub2() => SetCameraState(CameraState.Sub2);

    /// <summary>
    /// 状態を指定してカメラを切り替える関数です。
    /// 他のスクリプトから「switchScript.SetCameraState(CameraSwitcher.CameraState.Sub);」のように呼べます
    /// </summary>
    public void SetCameraState(CameraState newState)
    {
        CurrentState = newState;
        Debug.Log($"現在のカメラの状態: {CurrentState}");

        // 各カメラの表示状態を、現在の状態と一致するかどうかで一元化
        if (MainCamera != null) MainCamera.gameObject.SetActive(CurrentState == CameraState.Main);
        if (SubCamera != null) SubCamera.gameObject.SetActive(CurrentState == CameraState.Sub1);
        if (SubCamera2 != null) SubCamera2.gameObject.SetActive(CurrentState == CameraState.Sub2);

        //サブカメラ（Main以外のカメラ）が選ばれているかどうかの判定
        bool isSubCamera = (CurrentState != CameraState.Main);

        //UIとモンスターの表示を、isSubCameraの判定を使って切り替え
        if (isSubCamera)
        {
            if (cameraCanvas != null) cameraCanvas.SetActive(isSubCamera);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (cameraCanvas != null) cameraCanvas.SetActive(isSubCamera);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }



        if (monster != null) monster.SetVisible(isSubCamera);
    }

    /// <summary>
    /// 現在の状態を判定して、もう一方のカメラに切り替える関数です。
    /// 他のスクリプトから「switchScript.SetCameraState(CameraSwitcher.CameraState.Sub);」のように呼べます
    /// </summary>
    public void ToggleCamera()
    {
        if (CurrentState == CameraState.Main)
        {
            SetCameraState(CameraState.Sub1);
        }
        else
        {
            SetCameraState(CameraState.Main);
        }
    }
}