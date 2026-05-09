using UnityEngine;

public class DoorOpenKey : MonoBehaviour
{
    [SerializeField] private AutoDoor targetDoor;
    [SerializeField] private ItemUsePoint itemUsePoint;

    [Header("設定")]
    [SerializeField] private bool unlockPermanently = true;

    bool opened = false;

    void Update()
    {
        // ドアが開いている場合は何もしない
        if (opened)
            return;
        // アイテム使用ポイントとドアが設定されていない場合は何もしない
        if (itemUsePoint == null || targetDoor == null)
            return;
        // アイテム使用しない限りドアは開かない
        if (!itemUsePoint.IsOpened)
            return;
        // ドアを開ける
        if (unlockPermanently)
            targetDoor.SetUnlocked(true);
        targetDoor.OpenDoor();

        opened = true;
    }
}