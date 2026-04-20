using UnityEngine;

public class ItemUsePoint : MonoBehaviour
{
    public ItemType requiredType;
    public float useDistance = 3f;

    public bool CanUse(ItemData item, Transform player)
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist > useDistance) return false;

        return item.type == requiredType;
    }

    public void OnUse()
    {
        Debug.Log("g—p¬Œ÷I");
        gameObject.SetActive(false);
    }
}