using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    [Tooltip("0番目は必ずメインカメラにしてください")]
    public List<Camera> cameras = new List<Camera>();

    [Header("Monster")]
    [SerializeField] private InvisibleMonster monster;

    [Header("UI")]
    [SerializeField] private GameObject cameraCanvas;

    // 元のコードにあった変数もしっかり残しておきますわ
    private bool unique = true;

    public int CurrentCameraIndex { get; private set; } = 0;

    // 直前に見ていた監視カメラの番号を覚える
    private int lastSubCameraIndex = 1;

    void Start()
    {
        if (cameras.Count > 0)
        {
            SetCameraState(0); 
        }
    }

    void Update()
    {
        if (unique)
        {
            if (monster != null)
            {
                monster.SetVisible(false);
            }
            unique = false;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleCamera();
        }
    }

    //ボタン用の関数
    //インスペクターの「On Click()」で、この関数を選び、下の枠に 0 や 1 などの数字を入れてください
    public void SwitchToCamera(int index)
    {
        SetCameraState(index);
    }

    /// <summary>
    /// 状態を指定してカメラを切り替える関数です。
    /// 他のスクリプトから「switchScript.SetCameraState(CameraSwitcher.CameraState.Sub);」のように呼べます
    /// </summary>
    public void SetCameraState(int index)
    {
        if (index < 0 || index >= cameras.Count) return;

        // 監視カメラ（1番以降）を選択したなら、その番号を記憶しますわ
        if (index != 0)
        {
            lastSubCameraIndex = index;
        }

        CurrentCameraIndex = index;

        // 全カメラの有効・無効を一括管理いたしますわ
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] != null)
                cameras[i].gameObject.SetActive(i == CurrentCameraIndex);
        }

        // 0番目以外はすべて「サブカメラ」扱いですわ
        bool isSubCamera = (CurrentCameraIndex != 0);

        if (cameraCanvas != null) cameraCanvas.SetActive(isSubCamera);

        Cursor.visible = isSubCamera;
        Cursor.lockState = isSubCamera ? CursorLockMode.None : CursorLockMode.Locked;

        if (monster != null) monster.SetVisible(isSubCamera);
    }

    /// <summary>
    /// 現在の状態を判定して、もう一方のカメラに切り替える関数です。
    /// 他のスクリプトから「switchScript.SetCameraState(CameraSwitcher.CameraState.Sub);」のように呼べます
    /// </summary>
    public void ToggleCamera()
    {
        if (CurrentCameraIndex == 0)
        {
            // メインなら、記憶している監視カメラへ
            SetCameraState(lastSubCameraIndex);
        }
        else
        {
            // 監視カメラなら、メインへ
            SetCameraState(0);
        }
    }
}