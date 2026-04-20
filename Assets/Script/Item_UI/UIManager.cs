using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Text messageText;

    void Awake()
    {
        Instance = this;
    }

    public void ShowMessage(string msg)
    {
        messageText.text = msg;
    }
}