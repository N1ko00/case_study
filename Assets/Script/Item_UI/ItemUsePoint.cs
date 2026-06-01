using UnityEngine;

public class ItemUsePoint : MonoBehaviour
{
    public ItemType requiredType;
    public float useDistance = 3f;

    // 追加：この場所が「使用済み」かどうかを判定するフラグ
    [SerializeField] private bool isOpened = false;
    // 外部から確認用
    public bool IsOpened
    {
        get { return isOpened; }
    }
    public bool CanUse(ItemData item, Transform player)
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist > useDistance) return false;

        return item.type == requiredType;
    }

    public void OnUse()
    {
        Debug.Log(gameObject.name + " の使用に成功しました！");
        // フラグを立てる（これで二度目は反応しなくなる）
        isOpened = true;

    }
}