

using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    // これで ItemUsePoint.cs のエラーが消えます
    public ItemType type;

    [Header("スロットのデザイン番号 (0=通常, 1=メモ用など)")]
    public int slotTypeIndex = 0;

    public bool isConsumable = true; // Memoならfalseに設定すると消えません

    [TextArea]
    public string description;
}
public enum ItemType
{
    Key,
    Memo,
}