using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
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
}