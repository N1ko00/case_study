using UnityEngine;
using UnityEngine.InputSystem;

public class KeyPadTrigger : MonoBehaviour
{
    InputSystem_Actions inputAction;

    public GameObject keypadUI;
    public PasswordSystem passwordSystem;
    public AutoDoor autoDoor;

    bool playerInRange = false;

    void Awake()
    {
        inputAction = new InputSystem_Actions();
        inputAction.Password.ShowPass.performed += OnShowPassPerformed;
    }

    void OnEnable()
    {
        inputAction.Enable();
    }

    void OnDisable()
    {
        inputAction.Disable();
    }

    private void OnShowPassPerformed(InputAction.CallbackContext ctx)
    {
        if (!playerInRange)
            return;

        if (passwordSystem.IsUnlocked())
            return;

        keypadUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (passwordSystem.IsUnlocked())
        {
            autoDoor.OpenDoor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}