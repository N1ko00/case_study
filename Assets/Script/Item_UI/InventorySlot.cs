//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using UnityEngine.InputSystem; // ★新Input Systemを使用

//// マウスイベントのインターフェースを外し、EventSystemのフォーカス（Select）イベントだけを残す
//public class InventorySlot : MonoBehaviour, ISelectHandler, IDeselectHandler
//{
//    public Image icon;
//    private ItemData item;

//    public void SetItem(ItemData data)
//    {
//        item = data;
//        if (icon != null)
//        {
//            if (data != null)
//            {
//                icon.sprite = data.icon;
//                icon.enabled = true;
//            }
//            else
//            {
//                icon.sprite = null;
//                icon.enabled = false;
//            }
//        }
//    }

//    // ★ 十字キーでフォーカスがこのスロットに当たったとき
//    public void OnSelect(BaseEventData eventData)
//    {
//        if (item == null)
//        {
//            ClearMessageAll();
//            return;
//        }

//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.ShowPersistentItemMessage(item.itemName, item.description);
//        }
//    }

//    // フォーカスが隣に移動したとき
//    public void OnDeselect(BaseEventData eventData)
//    {
//        ClearMessageAll();
//    }

//    void Update()
//    {
//        // 現在このスロットがフォーカスされている場合
//        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
//        {
//            // ★ 新Input SystemでXboxコントローラーのBボタン(右ボタン)の沈み込みを検知
//            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
//            {
//                ExecuteUse();
//            }
//        }
//    }

//    private void ExecuteUse()
//    {
//        if (item == null) return;
//        ItemUseHandler.Instance.UseItem(item);
//    }

//    void OnDisable()
//    {
//        // 念のため非アクティブ化時にテキストを掃除
//        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
//        {
//            ClearMessageAll();
//        }
//    }

//    private void ClearMessageAll()
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
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // ★新Input Systemを使用

public class InventorySlot : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Image icon;
    private ItemData item;

    public void SetItem(ItemData data)
    {
        item = data;
        if (icon != null)
        {
            if (data != null)
            {
                icon.sprite = data.icon;
                icon.enabled = true;
            }
            else
            {
                icon.sprite = null;
                icon.enabled = false;
            }
        }
    }

    // 十字キーやLスティックでフォーカスがこのスロットに当たったとき
    public void OnSelect(BaseEventData eventData)
    {
        if (item == null)
        {
            ClearMessageAll();
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPersistentItemMessage(item.itemName, item.description);
        }
    }

    // フォーカスが隣に移動したとき
    public void OnDeselect(BaseEventData eventData)
    {
        ClearMessageAll();
    }

    void Update()
    {
        // 現在このスロットがフォーカスされている場合
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            // ★【ここを修正】XboxコントローラーのAボタン(buttonSouth)が押されたらアイテムを使用
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                ExecuteUse();
            }
        }
    }

    private void ExecuteUse()
    {
        if (item == null) return;
        ItemUseHandler.Instance.UseItem(item);
    }

    void OnDisable()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            ClearMessageAll();
        }
    }

    private void ClearMessageAll()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.itemNameText != null) UIManager.Instance.itemNameText.text = "";
            if (UIManager.Instance.messageContentText != null) UIManager.Instance.messageContentText.text = "";
            if (UIManager.Instance.messageWindow != null) UIManager.Instance.messageWindow.SetActive(false);
        }
    }
}

