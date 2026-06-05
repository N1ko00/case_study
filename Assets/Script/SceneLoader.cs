using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("旧フェード用 (任意)")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    [Header("TVノイズ演出")]
    [SerializeField] private CanvasGroup noiseGroup;        // RawImageを含むCanvasGroup
    [SerializeField] private RawImage noiseImage;           // 砂嵐テクスチャを貼ったRawImage
    [Tooltip("ノイズが完全に出るまでの時間")]
    [SerializeField] private float noiseFadeInDuration = 0.8f;
    [Tooltip("ノイズ最大表示の維持時間")]
    [SerializeField] private float noiseHoldDuration = 0.4f;
    [Tooltip("シーン読み込み後にノイズを消す時間")]
    [SerializeField] private float noiseFadeOutDuration = 0.6f;
    [Tooltip("UVスクロールの速度 (X, Y)")]
    [SerializeField] private Vector2 uvScrollSpeed = new Vector2(2f, 1.5f);
    [Tooltip("UVのランダムジッター強度")]
    [SerializeField] private float uvJitterStrength = 0.15f;
    [Tooltip("UVスケール (大きいほど細かいノイズ)")]
    [SerializeField] private Vector2 uvTiling = new Vector2(2f, 2f);

    [Header("TVノイズSE")]
    [Tooltip("ノイズ演出中に流すAudioSource")]
    [SerializeField] private AudioSource noiseAudioSource;
    [Tooltip("ノイズのループSE。ループ再生される")]
    [SerializeField] private AudioClip noiseLoopSE;
    [Tooltip("ノイズが出る瞬間の一発SE (任意)")]
    [SerializeField] private AudioClip noiseStartSE;
    [Range(0f, 1f)]
    [SerializeField] private float noiseVolume = 0.7f;
    [Tooltip("ループSEのフェードイン/フェードアウトに合わせて音量も補間する")]
    [SerializeField] private bool fadeAudioWithAlpha = true;

    // シーン名の列挙型
    public enum SceneName
    {
        TitleScene,
        MainScene,
        MainScene_KimTest2,
        GameOverScene,
        GameClearScene,
        ResultScene
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // 起動時はノイズ非表示
        if (noiseGroup != null)
        {
            noiseGroup.alpha = 0f;
            noiseGroup.blocksRaycasts = false;
        }

        // AudioSource 未設定なら自動追加
        if (noiseAudioSource == null)
        {
            noiseAudioSource = gameObject.AddComponent<AudioSource>();
            noiseAudioSource.playOnAwake = false;
            noiseAudioSource.spatialBlend = 0f;     // 2D
            noiseAudioSource.loop = true;
            noiseAudioSource.ignoreListenerPause = true; // Time.timeScale=0 でも止まらない
        }
    }

    public void LoadScene(SceneName name)
    {
        StartCoroutine(LoadSceneCoroutine(name));
    }

    // ───────────────────────────────────────────
    // メインのシーン遷移コルーチン
    // ───────────────────────────────────────────
    IEnumerator LoadSceneCoroutine(SceneName name)
    {
        StartNoiseSE();
        // 1) ノイズで画面を覆う
        yield return NoiseFadeIn();

        // 2) ノイズ全開で少しキープ + UV揺らし
        yield return NoiseHold(noiseHoldDuration);

        // 3) シーン読み込み
        SceneManager.LoadScene(name.ToString());

        // 4) 1フレーム待って新シーンを安定化
        yield return null;

        // 5) ノイズをフェードアウト (この間もUVは動き続ける → NoiseFadeOut内で処理)
        yield return NoiseFadeOut();

        StopNoiseSE();
    }

    // ───────────────────────────────────────────
    // ノイズSE制御
    // ───────────────────────────────────────────
    private void StartNoiseSE()
    {
        if (noiseAudioSource == null) return;

        // 一発SE (スタート瞬間のバチッ音など)
        if (noiseStartSE != null)
            noiseAudioSource.PlayOneShot(noiseStartSE, noiseVolume);

        // ループSE (砂嵐のザーッ音)
        if (noiseLoopSE != null)
        {
            noiseAudioSource.clip = noiseLoopSE;
            noiseAudioSource.loop = true;
            noiseAudioSource.volume = fadeAudioWithAlpha ? 0f : noiseVolume;
            noiseAudioSource.Play();
        }
    }

    private void StopNoiseSE()
    {
        if (noiseAudioSource == null) return;
        noiseAudioSource.Stop();
        noiseAudioSource.clip = null;
    }

    /// <summary>
    /// CanvasGroup.alpha に合わせてSE音量も上下させる。
    /// </summary>
    private void SyncAudioVolumeToAlpha()
    {
        if (!fadeAudioWithAlpha) return;
        if (noiseAudioSource == null || noiseGroup == null) return;
        noiseAudioSource.volume = noiseGroup.alpha * noiseVolume;
    }

    // ───────────────────────────────────────────
    // ノイズ演出コルーチン
    // ───────────────────────────────────────────
    IEnumerator NoiseFadeIn()
    {
        if (noiseGroup == null) yield break;
        noiseGroup.blocksRaycasts = true;

        float t = 0f;
        while (t < noiseFadeInDuration)
        {
            t += Time.unscaledDeltaTime;       // Time.timeScale=0 でも動く
            noiseGroup.alpha = Mathf.Clamp01(t / noiseFadeInDuration);
            UpdateNoiseUV(Time.unscaledDeltaTime);
            SyncAudioVolumeToAlpha();
            yield return null;
        }
        noiseGroup.alpha = 1f;
        SyncAudioVolumeToAlpha();
    }

    IEnumerator NoiseHold(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            UpdateNoiseUV(Time.unscaledDeltaTime);
            yield return null;
        }
    }

    IEnumerator NoiseFadeOut()
    {
        if (noiseGroup == null) yield break;

        float t = 0f;
        while (t < noiseFadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            noiseGroup.alpha = 1f - Mathf.Clamp01(t / noiseFadeOutDuration);
            UpdateNoiseUV(Time.unscaledDeltaTime);
            SyncAudioVolumeToAlpha();
            yield return null;
        }
        noiseGroup.alpha = 0f;
        noiseGroup.blocksRaycasts = false;
        SyncAudioVolumeToAlpha();
    }

    /// <summary>
    /// RawImage.uvRect を毎フレーム動かして砂嵐感を作る。
    /// 連続スクロール + ランダムジッター + 縦方向の同期ズレ (水平ホールド乱れ風)
    /// </summary>
    private void UpdateNoiseUV(float dt)
    {
        if (noiseImage == null) return;

        Rect r = noiseImage.uvRect;

        // 連続スクロール
        r.x += uvScrollSpeed.x * dt;
        r.y += uvScrollSpeed.y * dt;

        // ランダムジッター (毎フレーム飛ばす → ザラついた感じ)
        r.x += Random.Range(-uvJitterStrength, uvJitterStrength);
        r.y += Random.Range(-uvJitterStrength, uvJitterStrength);

        // タイリング (細かさ)
        r.width = uvTiling.x;
        r.height = uvTiling.y;

        noiseImage.uvRect = r;
    }

    // ───────────────────────────────────────────
    // (旧) 黒フェード — 必要なら残しておく
    // ───────────────────────────────────────────
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