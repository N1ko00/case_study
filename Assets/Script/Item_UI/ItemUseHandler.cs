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
        Debug.Log("Yesボタンが押されました"); // ログが出るか確認


        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
        // 2. インベントリ本体を閉じる（ここでマウスロックも行われる）
        if (UIInventory.Instance != null)
        {
            UIInventory.Instance.CloseInventory();
        }
        // 2. アイテム処理
        if (pendingItem != null)
        {
            ExecuteUseLogic(pendingItem);
            pendingItem = null;
        }

        // 3. インベントリを閉じて視点を戻す
        ResetCursor();
    }

    public void OnClickNo()
    {
        pendingItem = null;

        // 「いいえ」の時もインベントリを閉じて視点を戻したいならここでもResetCursor
        ResetCursor();
    }

    private void ResetCursor()
    {
        // 1. 確認パネルを消す
        if (confirmPanel != null) confirmPanel.SetActive(false);

        // 2. インベントリ本体を閉じる（ここでマウスロックも行われる）
        if (UIInventory.Instance != null)
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

                // --- 重要：UIManagerが無くても止まらないように修正 ---
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage("使用しました");
                }

                usedSuccess = true;
                break;
            }
        }

        if (!usedSuccess && UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("ここでは使えないようです");
        }

        if (UIInventory.Instance != null)
        {
            UIInventory.Instance.Refresh();
        }
    }
}