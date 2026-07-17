using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{

    // どこからでもカメラの状態を確認できるようにいたしますわ
    public static CameraSwitcher Instance;

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

    [Header("Camera Buttons UI")]
    [Tooltip("監視カメラ1, 2... に対応するボタンを順番に入れてくださいませ")]
    [SerializeField] private List<Button> cameraButtons = new List<Button>();
    [SerializeField] private Color normalColor = Color.white; // 通常時の色
    [SerializeField] private Color selectedColor = Color.green; // 選択中の色

    //停止対象プレイヤー
    [SerializeField] private FPSController player_Main;

    //初回メッセージ用の設定
    [Header("First Return Message")]
    [Tooltip("メインカメラに初めて戻った時のセリフ")]
    [SerializeField] private string firstReturnMessage = "入った時は誰もおらんかったけどな";
    private bool hasReturnedToMainCameraOnce = false; // 初回判定用フラグ
    private bool hasSwitchedToSubCamera = false;


    // 初回アップデート時の初期化用フラグ
    private bool unique = true;

    public int CurrentCameraIndex { get; private set; } = 0;

    // 直前に見ていた監視カメラの番号を記憶
    private int lastSubCameraIndex = 1;

    private bool cameraLocked = false;

    // 外部からロック状態を確認するためのプロパティ
    public bool IsLocked => cameraLocked;

    public bool IsToggled = true;

    [Header("キーパッド参照")]
    public GameObject keypadUI; // ← KeyPadTriggerと同じGameObjectを割り当て

    void Awake()
    {
        Instance = this;
    }

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
            // ★ 追加：キーパッドが表示中は操作を止める
            if (keypadUI != null && keypadUI.activeSelf)
                return;

            if (UIInventory.Instance != null && UIInventory.Instance.IsOpen)
                return;
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

        // インベントリが開いている時は、無効化
        if (UIInventory.Instance != null && UIInventory.Instance.IsOpen)
        {
            return;
        }

        // ボーズ中はカメラ切り替え無効
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsOpen)
        {
            return;
        }
        // ロック中は Space 入力自体を受け付けない
        if (cameraLocked) return;

        // スペースキー、またはゲームパッドのXボタン(Xbox基準)が押されたかを判定します
        bool isSpacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool isGamepadXPressed = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;

        if (isSpacePressed || isGamepadXPressed)
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

        // 監視カメラ（0番以外）を見たらフラグをオンにする
        if (index != 0)
        {
            hasSwitchedToSubCamera = true;
        }

        //監視カメラからメインカメラに戻った時の初回メッセージ表示
        if (index == 0 && hasSwitchedToSubCamera && !hasReturnedToMainCameraOnce)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowItemMessage("", firstReturnMessage);
            }
            hasReturnedToMainCameraOnce = true; // フラグを立てて二度と表示させない
        }


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
            {
                // gameObject.SetActive ではなく、Cameraコンポーネントの enabled を切り替えます
                cameras[i].enabled = (i == CurrentCameraIndex);
            }
        }

        // 0番目以外はすべて「サブカメラ（監視カメラ）」扱いとします
        bool isSubCamera = (CurrentCameraIndex != 0);

        //サブカメラに切り替わった場合、UIManagerに指示を出してテキストウィンドウを閉じます
        if (isSubCamera && UIManager.Instance != null)
        {
            UIManager.Instance.ForceCloseMessage();
        }

        // （メインカメラに戻った時は、!isSubCamera が true になるので自動的に再表示されます）
        if (InteractPromptUI.Instance != null)
        {
            InteractPromptUI.Instance.gameObject.SetActive(!isSubCamera);
        }

        if (cameraCanvas != null) cameraCanvas.SetActive(isSubCamera);

        Cursor.visible = isSubCamera;
        Cursor.lockState = isSubCamera ? CursorLockMode.None : CursorLockMode.Locked;

        // if (monster != null) monster.SetVisible(isSubCamera);

        UpdateButtonColors();

        // 監視カメラ画面が開いた時、現在見ているカメラのボタンに十字キーのフォーカスを合わせます
        if (isSubCamera && cameraButtons.Count > 0 && EventSystem.current != null)
        {
            int buttonIndex = CurrentCameraIndex - 1;
            if (buttonIndex >= 0 && buttonIndex < cameraButtons.Count && cameraButtons[buttonIndex] != null)
            {
                // コルーチンを呼び出して、選択を少し遅らせます
                StartCoroutine(SelectButtonNextFrame(cameraButtons[buttonIndex].gameObject));
            }
        }
    }

    private void UpdateButtonColors()
    {
        for (int i = 0; i < cameraButtons.Count; i++)
        {
            if (cameraButtons[i] == null) continue;

            int targetCameraIndex = i + 1;
            Button button = cameraButtons[i];

            // ボタンの画像コンポーネント（Image）を直接取得します
            Image buttonImage = button.GetComponent<Image>();

            if (buttonImage != null)
            {
                if (targetCameraIndex == CurrentCameraIndex)
                {
                    // 現在アクティブなカメラのボタンは、インスペクターで設定した「selectedColor（緑など）」にします
                    buttonImage.color = selectedColor;
                }
                else
                {
                    // それ以外のボタンは「normalColor（白など）」に戻します
                    buttonImage.color = normalColor;
                }
            }
        }
    }

    /// <summary>
    /// 現在の状態を判定して、メインカメラとサブカメラ（監視カメラ）を交互に切り替える関数です。
    /// </summary>
    public void ToggleCamera()
    {
        // カメラ切替がロックされている場合は、切替を行わないようにします
        if (cameraLocked) return;

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

    public void LockCamera()
    {
        cameraLocked = true;
        SetCameraState(0);
    }

    private IEnumerator SelectButtonNextFrame(GameObject targetButton)
    {
        yield return null; // ここで1フレームだけ待ちますのよ

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(targetButton);
    }
}
