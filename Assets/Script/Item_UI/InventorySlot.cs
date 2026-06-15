//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems; // ★マウスの「乗った」「外れた」を検知するために必須

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

//    // ★アイテムスロットにマウスカーソルが乗った瞬間に自動で実行される関数
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (item == null) return;

//        // UIManagerを使って、メッセージウィンドウを表示し、アイテム名と説明文をセットする
//        if (UIManager.Instance != null)
//        {
//            // ShowPersistentItemMessageを呼び出して、タイマーで勝手に消えないように表示します
//            UIManager.Instance.ShowPersistentItemMessage(item.itemName, item.description);
//        }
//    }

//    // ★アイテムスロットからマウスカーソルが外れた瞬間に自動で実行される関数
//    public void OnPointerExit(PointerEventData eventData)
//    {
//        // カーソルが外れたら、メッセージウィンドウの文字を消してウィンドウも閉じます
//        ClearMessageAll();
//    }

//    // インベントリ自体を閉じた（スロットオブジェクトが非表示になった）時の安全処理
//    void OnDisable()
//    {
//        // ウィンドウ（messageWindow）ごと消してしまうと、
//        // アイテム使用失敗時の「ここでは使えないようだ」の表示まで道連れで消してしまうため、
//        // ここではテキスト（文字データ）だけを安全にクリアします。
//        ClearTextOnly();
//    }

//    // 文字もウィンドウも完全に非表示にする（通常のマウスアウト用）
//    private void ClearMessageAll()
//    {
//        if (UIManager.Instance != null)
//        {
//            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
//            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
//            if (UIManager.Instance.messageWindow != null) UIManager.Instance.messageWindow.SetActive(false);
//        }
//    }

//    // ウィンドウは消さず、中の文字データだけをクリアする（インベントリを閉じた時用）
//    private void ClearTextOnly()
//    {
//        if (UIManager.Instance != null)
//        {
//            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
//            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
//        }
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ★マウス検知（IPointerEnter, IPointerExit）に必須

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

    // ★アイテムスロットにマウスカーソルが乗ったとき：ウィンドウを出して中身を表示
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null) return;

        if (UIManager.Instance != null)
        {
            // カーソルが乗っている間は勝手に消えないモード（ShowPersistentItemMessage）でウィンドウをON
            UIManager.Instance.ShowPersistentItemMessage(item.itemName, item.description);
        }
    }

    // ★アイテムスロットからマウスカーソルが外れたとき：ウィンドウを完全に非表示にする
    public void OnPointerExit(PointerEventData eventData)
    {
        // マウスが外れたら、即座にウィンドウとテキストを消す処理を実行します
        ClearMessageAll();
    }

    // インベントリ自体を閉じた（スロットオブジェクトが非表示になった）とき
    void OnDisable()
    {
        // ここでウィンドウごと消してしまうと、アイテム使用失敗時の「ここでは使えないようだ」まで
        // 道連れで消してしまうため、インベントリが閉じるときはあえて文字データ（中身）のクリアだけに留めます。
        ClearTextOnly();
    }

    // 【通常のマウスアウト用】文字もウィンドウも完全に非表示にする関数
    private void ClearMessageAll()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
            if (UIManager.Instance.messageWindow != null) UIManager.Instance.messageWindow.SetActive(false); // ★ウィンドウをOFFにする
        }
    }

    // 【インベントリを閉じた時用】ウィンドウは消さず、中の文字データだけをクリアする関数
    private void ClearTextOnly()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
        }
    }
}