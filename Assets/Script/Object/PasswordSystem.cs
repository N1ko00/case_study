using UnityEngine;

public class PasswordSystem : MonoBehaviour
{
    public string correctPassword = "1234";

    string currentInput = "";

    public void InputNumber(string num)
    {
        currentInput += num;
        Debug.Log("入力: " + currentInput);
    }

    public void Clear()
    {
        currentInput = "";
        Debug.Log("クリア");
    }

    public void Enter()
    {
        if (currentInput == correctPassword)
        {
            Debug.Log("成功！");
        }
        else
        {
            Debug.Log("失敗！");
        }

        currentInput = "";
    }
}