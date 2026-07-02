//using UnityEngine;
//using System.Collections.Generic;
//using UnityEngine.EventSystems;
//using UnityEngine.UI; // Navigationの制御に必要

//public class UIInventory : MonoBehaviour
//{
//    public static UIInventory Instance;

//    public Transform parent;
//    public GameObject inventoryUI;

//    public bool IsOpen => inventoryUI != null && inventoryUI.activeSelf;

//    [Header("スロットのプレハブを登録（0=通常, 1=メモ用など）")]
//    public List<GameObject> slotPrefabs = new List<GameObject>();

//    void Awake()
//    {
//        Instance = this;
//    }

//    public void OpenInventory()
//    {
//        if (inventoryUI != null) inventoryUI.SetActive(true);
//        Refresh();

//        Cursor.lockState = CursorLockMode.None;
//        Cursor.visible = true;

//        SelectFirstSlot();
//    }

//    public void CloseInventory()
//    {
//        if (inventoryUI != null) inventoryUI.SetActive(false);

//        if (ItemUseHandler.Instance != null && ItemUseHandler.Instance.confirmPanel != null)
//        {
//            ItemUseHandler.Instance.confirmPanel.SetActive(false);
//        }

//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }

//    public void Refresh()
//    {
//        if (parent == null) return;

//        // 古いスロットをすべて削除
//        foreach (Transform child in parent)
//            Destroy(child.gameObject);

//        if (InventoryManager.Instance == null) return;

//        List<Selectable> selectables = new List<Selectable>();

//        // 最新のアイテム一覧からスロットを再生成
//        foreach (var item in InventoryManager.Instance.items)
//        {
//            int index = Mathf.Clamp(item.slotTypeIndex, 0, slotPrefabs.Count - 1);
//            GameObject slot = Instantiate(slotPrefabs[index], parent);
//            slot.GetComponent<InventorySlot>().SetItem(item);

//            // ボタン（Selectable）コンポーネントを収集
//            Selectable sel = slot.GetComponent<Selectable>();
//            if (sel != null)
//            {
//                selectables.Add(sel);
//            }
//        }

//        // ★ 生成されたすべてのスロットのナビゲーションを「自動（Automatic）」に設定する
//        foreach (var sel in selectables)
//        {
//            Navigation nav = new Navigation();
//            nav.mode = Navigation.Mode.Automatic; // Unityに画面上の位置から自動で上下左右を繋いでもらう
//            sel.navigation = nav;
//        }
//    }

//    public void SelectFirstSlot()
//    {
//        if (parent == null) return;
//        StartCoroutine(SelectFirstSlotDelayed());
//    }

//    // オブジェクトの生成や削除が完全に終わるのを2フレーム待ってから確実に選択する
//    private System.Collections.IEnumerator SelectFirstSlotDelayed()
//    {
//        yield return null; // 1フレーム待つ（Destroyの完了待ち）
//        yield return null; // もう1フレーム待つ（Instantiateの反映待ち）

//        if (parent != null && parent.childCount > 0)
//        {
//            // 最初の子オブジェクト（一番古いアイテム）を取得
//            Transform firstChild = parent.GetChild(0);
//            if (firstChild != null)
//            {
//                // 一度選択をリセットしてから、強制的にフォーカスを当てる
//                EventSystem.current.SetSelectedGameObject(null);
//                EventSystem.current.SetSelectedGameObject(firstChild.gameObject);
//                Debug.Log("ゲームパッド用に最初のスロットを選択しました: " + firstChild.name);
//            }
//        }
//    }
//}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem; // ★新Input Systemを使用

public class UIInventory : MonoBehaviour
{
    public static UIInventory Instance;

    public Transform parent;
    public GameObject inventoryUI;

    public bool IsOpen => inventoryUI != null && inventoryUI.activeSelf;

    [Header("スロットのプレハブを登録")]
    public List<GameObject> slotPrefabs = new List<GameObject>();

    private List<GameObject> activeSlots = new List<GameObject>();
    private const int COLUMNS = 2; // ★2列グリッド

    // 連打防止用のクールダウン
    private bool isNavigating = false;

    void Awake()
    {
        Instance = this;
    }

    public void OpenInventory()
    {
        if (inventoryUI != null) inventoryUI.SetActive(true);
        Refresh();

        // マウスは完全に隠してロック
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SelectFirstSlot();
    }

    public void CloseInventory()
    {
        if (inventoryUI != null) inventoryUI.SetActive(false);

        if (ItemUseHandler.Instance != null && ItemUseHandler.Instance.confirmPanel != null)
        {
            ItemUseHandler.Instance.confirmPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Refresh()
    {
        if (parent == null) return;

        // 現在選択中の位置を記憶
        int currentSelectedIndex = GetCurrentSelectedIndex();

        // 古いスロットを削除してリストをクリア
        foreach (Transform child in parent) Destroy(child.gameObject);
        activeSlots.Clear();

        if (InventoryManager.Instance == null) return;

        // スロットを再生成
        foreach (var item in InventoryManager.Instance.items)
        {
            int index = Mathf.Clamp(item.slotTypeIndex, 0, slotPrefabs.Count - 1);
            GameObject slot = Instantiate(slotPrefabs[index], parent);
            slot.GetComponent<InventorySlot>().SetItem(item);

            // ★標準のナビゲーションはすべて無効化（干渉を防ぐ）
            Selectable sel = slot.GetComponent<Selectable>();
            if (sel != null)
            {
                Navigation nav = new Navigation { mode = Navigation.Mode.None };
                sel.navigation = nav;
            }

            activeSlots.Add(slot);
        }

        // フォーカスの復元
        if (currentSelectedIndex >= 0 && activeSlots.Count > 0)
        {
            int targetIndex = Mathf.Clamp(currentSelectedIndex, 0, activeSlots.Count - 1);
            StartCoroutine(SelectSlotDelayed(targetIndex));
        }
    }

    public void SelectFirstSlot()
    {
        if (parent == null) return;
        StartCoroutine(SelectSlotDelayed(0));
    }

    private System.Collections.IEnumerator SelectSlotDelayed(int index)
    {
        yield return null; // Destroy完了待ち
        yield return null; // Instantiate反映待ち

        if (activeSlots.Count > index)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(activeSlots[index]);
        }
    }

    // 現在選択されているオブジェクトが何番目のスロットか取得する
    private int GetCurrentSelectedIndex()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null) return -1;
        return activeSlots.IndexOf(EventSystem.current.currentSelectedGameObject);
    }

    void Update()
    {
        if (!IsOpen || Gamepad.current == null || activeSlots.Count == 0) return;

        // ★新Input Systemによる十字キーとLスティックの入力取得
        Vector2 dpad = Gamepad.current.dpad.ReadValue();
        Vector2 stick = Gamepad.current.leftStick.ReadValue();
        Vector2 input = dpad.magnitude > 0 ? dpad : stick;

        // 入力がない時はクールダウンをリセットして終了
        if (input.magnitude < 0.5f)
        {
            isNavigating = false;
            return;
        }

        // クールダウン中（押しっぱなしで高速移動しすぎるのを防ぐ）なら処理しない
        if (isNavigating) return;

        int currentIndex = GetCurrentSelectedIndex();
        if (currentIndex == -1)
        {
            // もしフォーカスが外れていたら1番目のスロットを選択して復帰
            SelectFirstSlot();
            isNavigating = true;
            return;
        }

        int targetIndex = currentIndex;

        // 入力の方向を解析（閾値 0.5f）
        if (input.x > 0.5f)       // 右
        {
            if (currentIndex % COLUMNS < COLUMNS - 1 && currentIndex + 1 < activeSlots.Count)
                targetIndex = currentIndex + 1;
        }
        else if (input.x < -0.5f) // 左
        {
            if (currentIndex % COLUMNS > 0)
                targetIndex = currentIndex - 1;
        }
        else if (input.y < -0.5f) // 下
        {
            if (currentIndex + COLUMNS < activeSlots.Count)
                targetIndex = currentIndex + COLUMNS;
        }
        else if (input.y > 0.5f)  // 上
        {
            if (currentIndex - COLUMNS >= 0)
                targetIndex = currentIndex - COLUMNS;
        }

        // フォーカスに対象が変化した場合のみ反映
        if (targetIndex != currentIndex)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(activeSlots[targetIndex]);
            isNavigating = true; // クールダウン開始（指を離すか、ニュートラルに戻るまでロック）
        }
    }
}

