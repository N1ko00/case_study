using UnityEngine;
using UnityEngine.InputSystem;

public class DoorSwitch : MonoBehaviour
{
    [SerializeField] private AutoDoor targetDoor;

    [SerializeField] private Transform player;

    [SerializeField] private float activeDistance = 1f;

    [Header("レバー")]
    [SerializeField] private Transform lever;

    [SerializeField] private Vector3 offRotation;

    [SerializeField] private Vector3 onRotation;

    private bool isOn = false;

    void Start()
    {
        lever.localEulerAngles = offRotation;
    }

    void Update()
    {
        float distance =
            Vector3.Distance(transform.position, player.position);

        if (distance <= activeDistance &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !isOn)
        {
            isOn = true;

            targetDoor.SetOpenDoor(true);

            // レバー倒す
            lever.localEulerAngles = onRotation;

            Debug.Log("スイッチON");
        }
    }
}