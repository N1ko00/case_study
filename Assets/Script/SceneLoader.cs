using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneLoader : MonoBehaviour
{
    public Image fadeImage; // フェード用のイメージ
    public float  fadeDuration = 0.5f; // フェードの時間
    // シーン名の列挙型
    public enum SceneName
    {
        TitleScene,
        MainScene,
        GameOverScene,
        GameClearScene,
        ResultScene 
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void LoadScene(SceneName name)
    {
        StartCoroutine(LoadSceneCoroutine(name));
    }

    IEnumerator LoadSceneCoroutine(SceneName name)
    {
        yield return FadeOut();
        SceneManager.LoadScene(name.ToString());
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = t / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
    }
}
