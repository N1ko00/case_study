//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems; // ★マウス検知に必須

//public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    public Image icon;
//    private ItemData item;

//    public void SetItem(ItemData data)
//    {
//        item = data;
//        icon.sprite = data.icon;
//    }

//    public void OnClick()
//    {
//        ItemUseHandler.Instance.UseItem(item);
//    }

//    // ★スロットにマウスカーソルが乗ったときに実行
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (item == null) return;

//        // UIManagerを使って、アイテム名と説明文（description）をメッセージウィンドウに常時表示
//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowPersistentItemMessage(item.itemName, item.description);
//        }
//    }

//    // ★スロットからマウスカーソルが外れたときに実行
//    public void OnPointerExit(PointerEventData eventData)
//    {
//        // カーソルが外れたらメッセージウィンドウの表示をクリアする
//        ClearMessage();
//    }

//    // スロット自体が非表示になった（インベントリを閉じた）時も文字を消す
//    void OnDisable()
//    {
//        ClearMessage();
//    }

//    private void ClearMessage()
//    {
//        if (UIManager.Instance != null)
//        {
//            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
//            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
//            if (UIManager.Instance.messageWindow != null) UIManager.Instance.messageWindow.SetActive(false);
//        }
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ★マウスの「乗った」「外れた」を検知するために必須

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    private ItemData item;

    public void SetItem(ItemData data)
    {
        item = data;
        icon.sprite = data.icon;
    }

    public void OnClick()
    {
        ItemUseHandler.Instance.UseItem(item);
    }

    // ★アイテムスロットにマウスカーソルが乗った瞬間に自動で実行される関数
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null) return;

        // UIManagerを使って、メッセージウィンドウを表示し、アイテム名と説明文をセットする
        if (UIManager.Instance != null)
        {
            // ShowPersistentItemMessageを呼び出して、タイマーで勝手に消えないように表示します
            UIManager.Instance.ShowPersistentItemMessage(item.itemName, item.description);
        }
    }

    // ★アイテムスロットからマウスカーソルが外れた瞬間に自動で実行される関数
    public void OnPointerExit(PointerEventData eventData)
    {
        // カーソルが外れたら、メッセージウィンドウの文字を消してウィンドウも閉じます
        ClearMessageAll();
    }

    // インベントリ自体を閉じた（スロットオブジェクトが非表示になった）時の安全処理
    void OnDisable()
    {
        // ウィンドウ（messageWindow）ごと消してしまうと、
        // アイテム使用失敗時の「ここでは使えないようだ」の表示まで道連れで消してしまうため、
        // ここではテキスト（文字データ）だけを安全にクリアします。
        ClearTextOnly();
    }

    // 文字もウィンドウも完全に非表示にする（通常のマウスアウト用）
    private void ClearMessageAll()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
            if (UIManager.Instance.messageWindow != null) UIManager.Instance.messageWindow.SetActive(false);
        }
    }

    // ウィンドウは消さず、中の文字データだけをクリアする（インベントリを閉じた時用）
    private void ClearTextOnly()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
        }
    }
}