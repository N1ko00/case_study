using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class Result : MonoBehaviour
{
    InputSystem_Actions inputAction;

    [SerializeField] SceneLoader sceneLoader;

    [Header("ボタン参照")]
    [SerializeField] private Button ReturnTitle;

    void OnEnable()
    {
        if (ReturnTitle != null)
        {
            ReturnTitle.onClick.AddListener(OnReturnTitleClicked);
        }
    }
    
    void OnDisable()
    {
        if (ReturnTitle != null)
        {
            ReturnTitle.onClick.RemoveListener(OnReturnTitleClicked);
        }
    }

    private void OnquitClicked()
    {
        sceneLoader.LoadScene(SceneLoader.SceneName.TitleScene);
    }

    private void OnReturnTitleClicked()
    {
        Debug.Log("[Result] 終了");
        sceneLoader.LoadScene(SceneLoader.SceneName.TitleScene);

    }
}
