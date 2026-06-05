using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    [Tooltip("0番目は必ずメインカメラにしてください")]
    public List<Camera> cameras = new List<Camera>();
    [FormerlySerializedAs("MainCamera")]
    [SerializeField] private Camera mainCamera;
    [FormerlySerializedAs("SubCamera")]
    [SerializeField] private Camera subCamera;
    [FormerlySerializedAs("SubCamera2")]
    [SerializeField] private Camera subCamera2;

    [Header("Monster")]
    [SerializeField] private InvisibleMonster monster;

    [Header("UI")]
    [SerializeField] private GameObject cameraCanvas;

    //停止対象プレイヤー
    [SerializeField] private FPSController player_Main;

    // 初回アップデート時の初期化用フラグ
    private bool unique = true;

    public int CurrentCameraIndex { get; private set; } = 0;

    // 直前に見ていた監視カメラの番号を記憶
    private int lastSubCameraIndex = 1;

    void Start()
    {
        EnsureCamerasInitialized();

        if (cameras.Count > 0)
        {
            SetCameraState(0); 
        }
    }

    private void EnsureCamerasInitialized()
    {
        if (cameras.Count > 0) return;

        AddLegacyCamera(mainCamera);
        AddLegacyCamera(subCamera);
        AddLegacyCamera(subCamera2);
    }

    private void AddLegacyCamera(Camera legacyCamera)
    {
        if (legacyCamera == null || cameras.Contains(legacyCamera)) return;
        cameras.Add(legacyCamera);
    }

    void Update()
    {
        if (unique)
        {
            // monsterが存在する場合、最初は非表示にする処理を行います
            if (monster != null)
            {
                //monster.SetVisible(false);
            }
            else
            {
                // エラーを防ぐために、警告メッセージをコンソールにお知らせします
                Debug.LogWarning("モンスターが見つかりません。インスペクターで monster の設定を忘れていませんか？");
            }
            unique = false;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleCamera();
        }
    }

    // ボタン用の関数
    // インスペクターの「On Click()」でこの関数を選び、引数の枠に 0 や 1 などの数値を設定してください
    public void SwitchToCamera(int index)
    {
        SetCameraState(index);
    }

    /// <summary>
    /// Switches to the camera at the specified list index.
    /// Call with an index managed in the `cameras` list (0 = main camera).
    /// </summary>
    public void SetCameraState(int index)
    {
        if (index < 0 || index >= cameras.Count) return;

        // 監視カメラ（1番目以降）を選択しているなら、その番号を記録します
        if (index != 0)
        {
            lastSubCameraIndex = index;
        }

        CurrentCameraIndex = index;

        // 全カメラの有効・無効を、カレントインデックスに応じて一括管理します
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] != null)
                cameras[i].gameObject.SetActive(i == CurrentCameraIndex);
        }

        // 0番目以外はすべて「サブカメラ（監視カメラ）」扱いとします
        bool isSubCamera = (CurrentCameraIndex != 0);

        if (cameraCanvas != null) cameraCanvas.SetActive(isSubCamera);

        Cursor.visible = isSubCamera;
        Cursor.lockState = isSubCamera ? CursorLockMode.None : CursorLockMode.Locked;

       // if (monster != null) monster.SetVisible(isSubCamera);
    }

    /// <summary>
    /// 現在の状態を判定して、メインカメラとサブカメラ（監視カメラ）を交互に切り替える関数です。
    /// </summary>
    public void ToggleCamera()
    {
        if (CurrentCameraIndex == 0)
        {
            // メインカメラなら、記憶している監視カメラへ切り替え
            SetCameraState(lastSubCameraIndex);
            player_Main.SetMoveEnabled(false); // プレイヤーの移動を停止
        }
        else
        {
            // 監視カメラなら、メインカメラへ切り替え
            SetCameraState(0);
            player_Main.SetMoveEnabled(true); // プレイヤーの移動を再開
        }
    }
}
