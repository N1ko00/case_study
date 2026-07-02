
//using UnityEngine;
//using UnityEngine.InputSystem; // ★追加
//using UnityEngine.EventSystems; // ★追加

//public class ItemUseHandler : MonoBehaviour
//{
//    public static ItemUseHandler Instance;
//    public Transform player;

//    [Header("確認用UIパネル")]
//    public GameObject confirmPanel;

//    [Header("ゲームパッドで最初に選択させるYesボタンのGameObject")]
//    public GameObject yesButtonObject;

//    private ItemData pendingItem;

//    void Awake()
//    {
//        Instance = this;
//        if (confirmPanel != null) confirmPanel.SetActive(false);
//    }

//    void Update()
//    {
//        // ★ 確認パネルが表示されている時、ゲームパッドのBボタン（buttonEast）でキャンセル（No）にする
//        if (confirmPanel != null && confirmPanel.activeSelf)
//        {
//            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
//            {
//                OnClickNo();
//            }
//        }
//    }

//    public void UseItem(ItemData item)
//    {
//        if (item == null) return;

//        pendingItem = item;
//        confirmPanel.SetActive(true);

//        Cursor.lockState = CursorLockMode.None;
//        Cursor.visible = true;

//        // ★ ゲームパッド用に確認パネルの「Yes」ボタンを自動選択する
//        if (yesButtonObject != null)
//        {
//            EventSystem.current.SetSelectedGameObject(yesButtonObject);
//        }
//    }

//    public void OnClickYes()
//    {
//        if (confirmPanel != null)
//        {
//            confirmPanel.SetActive(false);
//        }

//        if (pendingItem != null)
//        {
//            ExecuteUseLogic(pendingItem);
//            pendingItem = null;
//        }

//        ResetCursor();
//    }

//    public void OnClickNo()
//    {
//        pendingItem = null;

//        if (confirmPanel != null)
//        {
//            confirmPanel.SetActive(false);
//        }

//        // キャンセルした時はインベントリのスロットに選択を戻す
//        if (UIInventory.Instance != null)
//        {
//            UIInventory.Instance.SelectFirstSlot();
//        }
//    }

//    private void ResetCursor()
//    {
//        if (InventoryToggle.Instance != null)
//        {
//            InventoryToggle.Instance.CloseInventory();
//        }
//        else if (UIInventory.Instance != null)
//        {
//            UIInventory.Instance.CloseInventory();
//        }
//    }

//    private void ExecuteUseLogic(ItemData item)
//    {
//        if (player == null) return;

//        ItemUsePoint[] points = Object.FindObjectsByType<ItemUsePoint>(FindObjectsSortMode.None);
//        bool usedSuccess = false;

//        foreach (var point in points)
//        {
//            if (point.CanUse(item, player))
//            {
//                point.OnUse();

//                if (item.isConsumable)
//                {
//                    InventoryManager.Instance.RemoveItem(item);
//                }

//                if (UIManager.Instance != null)
//                {
//                    UIManager.Instance.ShowItemMessage(item.itemName, "を使用した");
//                }

//                usedSuccess = true;
//                break;
//            }
//        }

//        if (usedSuccess)
//        {
//            ResetCursor();
//        }
//        else
//        {
//            ResetCursor();
//            if (UIManager.Instance != null)
//            {
//                UIManager.Instance.ShowItemMessage(item.itemName, "ここでは使えない");
//            }
//        }
//    }
//}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ItemUseHandler : MonoBehaviour
{
    public static ItemUseHandler Instance;
    public Transform player;

    [Header("確認用UIパネル")]
    public GameObject confirmPanel;

    [Header("ゲームパッドで最初に選択させるYesボタンのGameObject")]
    public GameObject yesButtonObject;

    private ItemData pendingItem;

    void Awake()
    {
        Instance = this;
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void Update()
    {
        // ★確認パネルが表示されている時、ゲームパッドのBボタン（buttonEast）でキャンセル（No）にする
        if (confirmPanel != null && confirmPanel.activeSelf)
        {
            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                OnClickNo();
            }
        }
    }

    public void UseItem(ItemData item)
    {
        if (item == null) return;

        pendingItem = item;
        confirmPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ★確認パネルが開いた瞬間、「Yes」ボタンに自動でフォーカスを合わせる（十字キーでNoに移動可能）
        if (yesButtonObject != null)
        {
            EventSystem.current.SetSelectedGameObject(yesButtonObject);
        }
    }

    public void OnClickYes()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if (pendingItem != null)
        {
            ExecuteUseLogic(pendingItem);
            pendingItem = null;
        }

        ResetCursor();
    }

    public void OnClickNo()
    {
        pendingItem = null;

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        // ★キャンセルして確認ウィンドウを閉じたら、インベントリの最初のスロットにフォーカスを戻す
        if (UIInventory.Instance != null)
        {
            UIInventory.Instance.SelectFirstSlot();
        }
    }

    private void ResetCursor()
    {
        if (InventoryToggle.Instance != null)
        {
            InventoryToggle.Instance.CloseInventory();
        }
        else if (UIInventory.Instance != null)
        {
            UIInventory.Instance.CloseInventory();
        }
    }

    private void ExecuteUseLogic(ItemData item)
    {
        if (player == null) return;

        ItemUsePoint[] points = Object.FindObjectsByType<ItemUsePoint>(FindObjectsSortMode.None);
        bool usedSuccess = false;

        foreach (var point in points)
        {
            if (point.CanUse(item, player))
            {
                point.OnUse();

                if (item.isConsumable)
                {
                    InventoryManager.Instance.RemoveItem(item);
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowItemMessage(item.itemName, "を使用した");
                }

                usedSuccess = true;
                break;
            }
        }

        if (usedSuccess)
        {
            ResetCursor();
        }
        else
        {
            ResetCursor();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowItemMessage(item.itemName, "ここでは使えない");
            }
        }
    }
}