using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryUI;

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            bool isOpen = !inventoryUI.activeSelf;
            inventoryUI.SetActive(isOpen);

            // Ç±Ç±Ç™èdóv
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;
        }
    }
}