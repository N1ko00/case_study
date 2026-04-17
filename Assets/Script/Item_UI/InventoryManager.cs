using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<ItemData> items = new List<ItemData>();

    void Awake()
    {
        Instance = this;
    }

    public void AddItem(ItemData item)
    {
        // 個数計算をせず、そのままリストに追加する
        items.Add(item);
        UIInventory.Instance.Refresh();
    }

    public void RemoveItem(ItemData item)
    {
        items.Remove(item);
        UIInventory.Instance.Refresh();
    }
}