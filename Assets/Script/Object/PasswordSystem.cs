using UnityEngine;
using TMPro;

public class PasswordSystem : MonoBehaviour
{
    public string correctPassword = "1234";
    public TMP_Text displayText;

    public int maxLength = 4;

    [Header("âèúê›íË")]
    public bool unlockOnce = true;

    public GameObject keypadUI;
    public KeyPadTrigger keyPadTrigger;
    public AutoDoor autoDoor;

    string currentInput = "";
    bool unlocked = false;


    void Start()
    {
        UpdateDisplay();
    }

    public void InputNumber(string num)
    {
        if (currentInput.Length >= maxLength)
            return;

        currentInput += num;
        UpdateDisplay();
    }

    public void BackSpace()
    {
        if (currentInput.Length == 0)
            return;

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        UpdateDisplay();
    }

    public void Clear()
    {
        currentInput = "";
        UpdateDisplay();
    }

    public void Enter()
    {
        if (currentInput == correctPassword)
        {
            Debug.Log("ê¨å˜ÅI");

            if (unlockOnce)
                unlocked = true;

            autoDoor.SetUnlocked(true);
            autoDoor.OpenDoor();
            CloseKeyPad();
        }
        else
        {
            Debug.Log("é∏îsÅI");
            currentInput = "";
            UpdateDisplay();
        }
    }

    public bool IsUnlocked()
    {
        return unlockOnce && unlocked;
    }

    public void CloseKeyPad()
    {
        currentInput = "";
        UpdateDisplay();

        keypadUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdateDisplay()
    {
        displayText.text = currentInput;
    }
}