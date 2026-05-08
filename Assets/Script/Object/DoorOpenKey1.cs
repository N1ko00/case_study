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
        if (opened)
            return;
        Debug.Log("ドアを開ける1");
        if (itemUsePoint == null || targetDoor == null)
            return;
        Debug.Log("ドアを開ける2");
        if (!itemUsePoint.IsOpened)
            return;
        Debug.Log("ドアを開ける3");
        if (unlockPermanently)
            targetDoor.SetUnlocked(true);


        targetDoor.OpenDoor();

        opened = true;
    }
}