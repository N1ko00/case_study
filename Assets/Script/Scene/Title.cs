using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Title : MonoBehaviour
{
    InputSystem_Actions inputAction;

    //めんどくなったのでインスタンス参照
    [SerializeField] SceneLoader sceneLoader;
    // Update is called once per frame
    void FixedUpdate()
    {
        inputAction.Scene.Move.performed += ctx => sceneLoader.LoadScene(SceneLoader.SceneName.MainScene);
    }

    void Awake()
    {
        inputAction = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputAction.Enable();
    }

    void OnDisable()
    {
        inputAction.Disable();
    }
}
