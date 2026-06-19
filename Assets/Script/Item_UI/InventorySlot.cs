//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems; // ★マウス検知（IPointerEnter, IPointerExit）に必須

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

//    // ★アイテムスロットにマウスカーソルが乗ったとき：ウィンドウを出して中身を表示
//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (item == null) return;

//        if (UIManager.Instance != null)
//        {
//            // カーソルが乗っている間は勝手に消えないモード（ShowPersistentItemMessage）でウィンドウをON
//            UIManager.Instance.ShowPersistentItemMessage(item.itemName, item.description);
//        }
//    }

//    // ★アイテムスロットからマウスカーソルが外れたとき：ウィンドウを完全に非表示にする
//    public void OnPointerExit(PointerEventData eventData)
//    {
//        // マウスが外れたら、即座にウィンドウとテキストを消す処理を実行します
//        ClearMessageAll();
//    }

//    // インベントリ自体を閉じた（スロットオブジェクトが非表示になった）とき
//    void OnDisable()
//    {
//        // ここでウィンドウごと消してしまうと、アイテム使用失敗時の「ここでは使えないようだ」まで
//        // 道連れで消してしまうため、インベントリが閉じるときはあえて文字データ（中身）のクリアだけに留めます。
//        ClearTextOnly();
//    }

//    // 【通常のマウスアウト用】文字もウィンドウも完全に非表示にする関数
//    private void ClearMessageAll()
//    {
//        if (UIManager.Instance != null)
//        {
//            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
//            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
//            if (UIManager.Instance.messageWindow != null) UIManager.Instance.messageWindow.SetActive(false); // ★ウィンドウをOFFにする
//        }
//    }

//    // 【インベントリを閉じた時用】ウィンドウは消さず、中の文字データだけをクリアする関数
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
using UnityEngine.EventSystems; // マウス検知（IPointerEnter, IPointerExit）に必須

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    private ItemData item;

    // 現在このスロットの上にマウスが乗っているかを記録するフラグ
    private bool isHovering = false;

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

        isHovering = true;

        if (UIManager.Instance != null)
        {
            // カーソルが乗っている間は勝手に消えないモード（ShowPersistentItemMessage）でウィンドウをON
            UIManager.Instance.ShowPersistentItemMessage(item.itemName, item.description);
        }
    }

    // ★アイテムスロットからマウスカーソルが外れたとき：ウィンドウを完全に非表示にする
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // マウスが外れたら、即座にウィンドウとテキストを消す処理を実行します
        ClearMessageAll();
    }

    // インベントリ自体を閉じた（スロットオブジェクトが非表示になった）とき
    void OnDisable()
    {
        // 自分がホバー中（マウスが乗ったまま）の状態でインベントリが閉じられた時だけ文字をクリアします。
        // アイテムを「使用」してインベントリが閉じた時は、すでにマウスが別のボタン等に移動していて
        // isHoveringはfalseになっているため、使用メッセージ（「〜を使用した」）の文字を消さずに残せます！
        if (isHovering)
        {
            ClearMessageAll();
            isHovering = false;
        }
    }

    // 文字もウィンドウも完全に非表示にする関数
    private void ClearMessageAll()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
            if (UIManager.Instance.messageWindow != null) UIManager.Instance.messageWindow.SetActive(false); // ウィンドウをOFFにする
        }
    }
}