using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClearSequence : MonoBehaviour
{
    [Header("演出設定")]
    public Image whiteOverlay;            // 全画面白Image（alpha=0スタート）
    public float fadeInDuration = 1.2f;
    public float holdDuration = 0.4f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("スローモーション")]
    public bool useSlowMotion = true;
    public float slowTimeScale = 0.5f;

    [Header("遷移先")]
    [SerializeField] SceneLoader sceneLoader;
    public SceneLoader.SceneName resultScene = SceneLoader.SceneName.ResultScene;

    bool isPlaying = false;

    public void Play()
    {
        if (isPlaying) return; // 多重起動防止
        isPlaying = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        if (useSlowMotion)
            Time.timeScale = slowTimeScale;

        float t = 0f;
        Color c = whiteOverlay.color;
        c.a = 0f;
        whiteOverlay.color = c;

        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = fadeCurve.Evaluate(t / fadeInDuration);
            whiteOverlay.color = c;
            yield return null;
        }

        c.a = 1f;
        whiteOverlay.color = c;

        Time.timeScale = 1f; // スロー解除（シーン遷移前に必ず戻す）

        yield return new WaitForSecondsRealtime(holdDuration);

        sceneLoader.LoadScene(resultScene);
    }
}