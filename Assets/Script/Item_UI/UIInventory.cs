using UnityEngine;
using UnityEngine.InputSystem; // これが必要
using System.Collections.Generic;

public class UIInventory : MonoBehaviour
{
    public static UIInventory Instance;

    public GameObject slotPrefab;
    public Transform parent;
    public GameObject inventoryUI;

    // Input System Actions の参照
    [Header("Input Action Assets の 'Inventory' アクションをここに")]
    public InputActionProperty inventoryAction;

    public bool IsOpen => inventoryUI.activeSelf;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        // アクションを有効化し、関数を紐付ける
        inventoryAction.action.Enable();
        inventoryAction.action.performed += OnInventoryPressed;
    }

    void OnDisable()
    {
        inventoryAction.action.performed -= OnInventoryPressed;
    }

    // Tabキー（または設定したボタン）が押された時に実行
    private void OnInventoryPressed(InputAction.CallbackContext context)
    {
        if (IsOpen) CloseInventory();
        else OpenInventory();
    }

    public void OpenInventory()
    {
        inventoryUI.SetActive(true);
        Refresh();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    [Header("スロットのプレハブを登録（0=通常, 1=メモ用など）")]
    public List<GameObject> slotPrefabs = new List<GameObject>();
    public void CloseInventory()
    {
        inventoryUI.SetActive(false);

        // 確認パネルも閉じる
        if (ItemUseHandler.Instance != null && ItemUseHandler.Instance.confirmPanel != null)
        {
            ItemUseHandler.Instance.confirmPanel.SetActive(false);
        }

        // 視点を戻す
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("インベントリを閉じました");
    }

    public void Refresh()
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (var item in InventoryManager.Instance.items)
        {
            // アイテムが指定する「デザイン番号」を取得（リストの範囲外にならないようClamp）
            int index = Mathf.Clamp(item.slotTypeIndex, 0, slotPrefabs.Count - 1);

            // 選ばれたプレハブを生成
            GameObject slot = Instantiate(slotPrefabs[index], parent);
            slot.GetComponent<InventorySlot>().SetItem(item);
        }
    }
}
