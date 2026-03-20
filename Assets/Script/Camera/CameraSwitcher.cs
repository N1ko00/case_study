using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    //2つのカメラをインスペクターで割り当て
    public Camera MainCamera;
    public Camera SubCamera;

    //Main Cameraを使っているかどうかのフラグ
    private bool usingMain = true;

    void Start()
    {
        if (MainCamera != null && SubCamera != null)
        {
            MainCamera.enabled = true;
            SubCamera.enabled = false;
        }
        else
        {
            Debug.LogError("カメラが割り当てられていません。インスペクターを確認してください。");
        }
    }

    void Update()
    {
        // スペースキーが押されたら
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            usingMain = !usingMain;

            if (usingMain)
            {
                MainCamera.enabled = true;
                SubCamera.enabled = false;
            }
            else
            {
                MainCamera.enabled = false;
                SubCamera.enabled = true;
            }
        }
    }
}
