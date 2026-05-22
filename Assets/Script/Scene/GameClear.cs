using UnityEngine;

public class GameClear : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーがゴールに触れたときの処理
        if (other.CompareTag("Player"))
        {
            Debug.Log("ゲームクリア！");
            //シーン遷移処理
            sceneLoader.LoadScene(SceneLoader.SceneName.ResultScene);
        }
    }
}
