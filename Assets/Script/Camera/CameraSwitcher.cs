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

    // 現在の状態を保持します
    public CameraState CurrentState { get; private set; }

    void Start()
    {
            // 初期状態をMainカメラに設定
            SetCameraState(CameraState.Main);
    }

    void Update()
    {
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

        switch (CurrentState)
        {
            case CameraState.Main:
                //GameObject自体のアクティブ状態を切り替え
                if (MainCamera != null) MainCamera.gameObject.SetActive(true);
                if (SubCamera != null) SubCamera.gameObject.SetActive(false);
                if (SubCamera2 != null) SubCamera2.gameObject.SetActive(false);

                //MainCameraの時は、OverlayのCanvasを非表示にして隠す
                if (cameraCanvas != null) cameraCanvas.SetActive(false);

                if (monster != null)
				{
					monster.SetVisible(false); // メインカメラのときはモンスターを見えないようにする
				}
				break;

            case CameraState.Sub1:
                //GameObject自体のアクティブ状態を切り替え
                if (MainCamera != null) MainCamera.gameObject.SetActive(false);
                if (SubCamera != null) SubCamera.gameObject.SetActive(true);
                if (SubCamera2 != null) SubCamera2.gameObject.SetActive(false);

                //Subの時はCanvasを表示
                if (cameraCanvas != null) cameraCanvas.SetActive(true);
                if (monster != null)
				{
					monster.SetVisible(true); // サブカメラのときはモンスターを見えるようにする
				}
				break;

            case CameraState.Sub2:
                //GameObject自体のアクティブ状態を切り替え
                if (MainCamera != null) MainCamera.gameObject.SetActive(false);
                if (SubCamera != null) SubCamera.gameObject.SetActive(false);
                if (SubCamera2 != null) SubCamera2.gameObject.SetActive(true);

                //Subの時はCanvasを表示
                if (cameraCanvas != null) cameraCanvas.SetActive(true);

                if (monster != null)
                {
                    monster.SetVisible(true); // サブカメラのときはモンスターを見えるようにする
                }
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
            SetCameraState(CameraState.Sub1);
        }
        else
        {
            SetCameraState(CameraState.Main);
        }
    }
}