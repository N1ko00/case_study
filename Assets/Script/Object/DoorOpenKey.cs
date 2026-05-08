//using UnityEngine;

//public class DoorOpenKey : MonoBehaviour
//{
//    [SerializeField] private AutoDoor targetDoor;

//    [Header("ê›íË")]
//    [SerializeField] private bool unlockPermanently = true;

//    bool used = false;

//    public void OnUse()
//    {
//        if (used && unlockPermanently)
//            return;

//        if (targetDoor == null)
//        {
//            Debug.LogWarning("DoorOpenKey : targetDoor Ç™ñ¢ê›íËÇ≈Ç∑");
//            return;
//        }

//        if (unlockPermanently)
//        {
//            targetDoor.SetUnlocked(true);
//            used = true;
//        }

//        targetDoor.OpenDoor();
//    }
//}