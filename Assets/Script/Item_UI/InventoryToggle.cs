//using UnityEngine;
//using UnityEngine.InputSystem;

//public class InventoryToggle : MonoBehaviour
//{
//    public GameObject inventoryUI;

//    void Update()
//    {
//        if (Keyboard.current.tabKey.wasPressedThisFrame)
//        {
//            bool isOpen = !inventoryUI.activeSelf;
//            inventoryUI.SetActive(isOpen);

//            // ここが重要
//            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
//            Cursor.visible = isOpen;
//        }
//    }
//}

using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    // 外部のスクリプトから呼び出せるようにシングルトン化
    public static InventoryToggle Instance { get; private set; }

    [Header("インベントリ本体（InventoryPanel_1）を登録")]
    public GameObject inventoryUI;

    [Header("一緒に出したい背景（inventorybackground）を登録")]
    public GameObject inventoryBG;

    [Header("カメラ切替の参照")]
    [SerializeField] private CameraSwitcher cameraSwitcher;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // ゲーム開始時は本体も背景も確実に消しておく
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (inventoryBG != null) inventoryBG.SetActive(false);

        // 自動検索
        if (cameraSwitcher == null)
        {
            cameraSwitcher = FindAnyObjectByType<CameraSwitcher>(FindObjectsInactive.Include);
        }
    }

    void Update()
    {
        // Tabキーが押された瞬間を検知
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (inventoryUI == null) return;

            // ゲームオーバー中はインベントリを開かせない
            if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameOver)
                return;

            // 監視カメラ表示中はインベントリを開かせない
            if (cameraSwitcher != null && cameraSwitcher.CurrentCameraIndex != 0)
                return;

            // 現在の表示状態を反転
            bool isOpen = !inventoryUI.activeSelf;

            if (isOpen)
            {
                OpenInventory();
            }
            else
            {
                CloseInventory();
            }
        }
    }

    public void OpenInventory()
    {
        if (inventoryUI != null) inventoryUI.SetActive(true);
        if (inventoryBG != null) inventoryBG.SetActive(true);

        // マウスカーソルを表示して動かせるようにする
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // スロットを最新に更新
        if (UIInventory.Instance != null)
        {
            UIInventory.Instance.Refresh();
        }
    }

    public void CloseInventory()
    {
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (inventoryBG != null) inventoryBG.SetActive(false);

        // マウスカーソルを非表示にしてロックする
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("インベントリと背景を完全に閉じました");
    }
}