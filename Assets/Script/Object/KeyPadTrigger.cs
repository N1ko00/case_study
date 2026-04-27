using UnityEngine;
using UnityEngine.InputSystem;

public class KeyPadTrigger : MonoBehaviour
{
    //InputSystem_Actions inputAction;

    public GameObject keypadUI;

    bool playerInRange = false;

    void Awake()
    {
        InputManager.Instance.inputActions.Password.ShowPass.performed += OnShowPassPerformed;
    }

    void OnDestroy()
    {
        InputManager.Instance.inputActions.Password.ShowPass.performed -= OnShowPassPerformed;
    }

    private void OnShowPassPerformed(InputAction.CallbackContext ctx)
    {
        if (playerInRange)
        {
            keypadUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Ç¶ÅHìÆÇ¢ÇƒÇ¢ÇÈÇÒÇ≈Ç∑Ç©ÅH");

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void OnEnable()
    {
        InputManager.Instance.inputActions.Password.Enable();
    }

    void OnDisable()
    {
        InputManager.Instance.inputActions.Password.Disable();
    }
}