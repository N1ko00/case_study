//using UnityEngine;

//public class ItemUseHandler : MonoBehaviour
//{
//    public static ItemUseHandler Instance;
//    public Transform player;

//    [Header("確認用UIパネル")]
//    public GameObject confirmPanel;

//    private ItemData pendingItem;

//    void Awake()
//    {
//        Instance = this;
//        if (confirmPanel != null) confirmPanel.SetActive(false);
//    }

//    public void UseItem(ItemData item)
//    {
//        if (item == null) return;

//        pendingItem = item;
//        confirmPanel.SetActive(true);

//        Cursor.lockState = CursorLockMode.None;
//        Cursor.visible = true;
//    }

//    public void OnClickYes()
//    {
//        Debug.Log("Yesボタンが押されました");

//        if (confirmPanel != null)
//        {
//            confirmPanel.SetActive(false);
//        }

//        // アイテム処理
//        if (pendingItem != null)
//        {
//            ExecuteUseLogic(pendingItem);
//            pendingItem = null;
//        }

//        // インベントリ本体と背景を同時に閉じて視点を戻す
//        ResetCursor();
//    }

//    public void OnClickNo()
//    {
//        pendingItem = null;

//        // 「いいえ」の時もインベントリを閉じて視点を戻す
//        ResetCursor();
//    }

//    private void ResetCursor()
//    {
//        if (confirmPanel != null) confirmPanel.SetActive(false);

//        // ★重要：新しく作った InventoryToggle の閉じ処理を呼ぶことで、背景も確実に一緒に消します
//        if (InventoryToggle.Instance != null)
//        {
//            InventoryToggle.Instance.CloseInventory();
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

//        if (!usedSuccess && UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowItemMessage(item.itemName, "ここでは使えないよういだ");
//        }

//        if (UIInventory.Instance != null)
//        {
//            UIInventory.Instance.Refresh();
//        }
//    }
//}

using UnityEngine;

public class ItemUseHandler : MonoBehaviour
{
    public static ItemUseHandler Instance;
    public Transform player;

    [Header("確認用UIパネル")]
    public GameObject confirmPanel;

    private ItemData pendingItem;

    void Awake()
    {
        Instance = this;
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    public void UseItem(ItemData item)
    {
        if (item == null) return;

        pendingItem = item;
        confirmPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClickYes()
    {
        Debug.Log("Yesボタンが押されました");

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        // アイテム処理
        if (pendingItem != null)
        {
            ExecuteUseLogic(pendingItem);
            pendingItem = null;
        }
    }

    public void OnClickNo()
    {
        pendingItem = null;

        // 「いいえ」の時もインベントリを閉じて視点を戻す
        ResetCursor();
    }

    private void ResetCursor()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);

        // InventoryToggle の閉じ処理を呼ぶことで、背景も確実に一緒に消します
        if (InventoryToggle.Instance != null)
        {
            InventoryToggle.Instance.CloseInventory();
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

                // ① 使用に成功した場合のメッセージ表示
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
            // 使用成功時：インベントリと背景を閉じ、視点をロックする
            ResetCursor();
        }
        else
        {
            // ★【ここを修正】インベントリを閉じる（ResetCursor）を「先」に実行します！
            // これにより、インベントリ裏のUIリセットに邪魔されることなくメッセージが上書き表示されます。
            ResetCursor();

            // ② 使用に失敗した場合（ここでは使えない場合）のメッセージ表示を「後」から呼ぶ
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowItemMessage(item.itemName, "はここでは使えないようだ");
            }
        }

        if (UIInventory.Instance != null)
        {
            UIInventory.Instance.Refresh();
        }
    }
}