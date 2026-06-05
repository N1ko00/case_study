using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TVノイズ演出
/// シーン遷移、カメラきりかえなどで使用
/// </summary>
public class TVNoiseEffect : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private CanvasGroup noiseGroup;
    [SerializeField] private RawImage noiseImage;

    [Header("デフォルト時間")]
    [SerializeField] private float defaultFadeIn = 0.25f;
    [SerializeField] private float defaultHold = 0.1f;
    [SerializeField] private float defaultFadeOut = 0.25f;

    [Header("UV演出")]
    [SerializeField] private Vector2 uvScrollSpeed = new Vector2(2f, 0.5f);
    [SerializeField] private float uvJitterStrength = 0.5f; //震える強さ
    [SerializeField] private Vector2 uvTiling = new Vector2(2f, 2f);

    [Header("動作設定")]
    [SerializeField] private bool useUnscaledTime = true; //時間をスケールに影響させないか

    private Coroutine _running;
    private float Dt => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    private void Awake() => Hide();//最初は非表示

    private void Update()
    {
        if (noiseGroup != null && noiseGroup.alpha > 0f)
        {
            UpdateNoiseUV(Dt);
        }
    }
    // -----------------------------------------------------
    // 公開API
    // -----------------------------------------------------

    public Coroutine PlayFullSequence(Action onMid = null,
        float? fadeIn = null,
        float? hold = null,
        float? fadeOut = null)
    {
        StopIfRunning();
        _running = StartCoroutine(FullSequence(
            fadeIn ?? defaultFadeIn,
            hold ?? defaultHold,
            fadeOut ?? defaultFadeOut,
            onMid));
        return _running;
    }

    public Coroutine FadeIn(float? duration = null)
    {
        StopIfRunning();
        _running = StartCoroutine(FadeRoutine(0f, 1f, duration ?? defaultFadeIn, true));
        return _running;
    }

    public Coroutine FadeOut(float? duration = null)
    {
        StopIfRunning();
        _running = StartCoroutine(FadeRoutine(1f, 0f, duration ?? defaultFadeOut, false));
        return _running;
    }

    public void Hide()
    {
        if (noiseGroup == null) return;
        noiseGroup.alpha = 0f;
        noiseGroup.blocksRaycasts = false;
    }
    // -----------------------------------------------------
    // 内部処理
    // -----------------------------------------------------
    private IEnumerator FullSequence(float fadeIn, float hold, float fadeOut, Action onMid)
    {
        yield return FadeRoutine(0f, 1f, fadeIn, true);

        float t = 0f;
        while (t < hold)
        {
            t += Dt;
            yield return null;
        }

        onMid?.Invoke();
        yield return null;

        yield return FadeRoutine(1f, 0f, fadeOut, true);
        _running = null;
    }

    private IEnumerator FadeRoutine(float from, float to, float duration, bool block)
    {
        if (noiseGroup == null) yield break;
        noiseGroup.blocksRaycasts = block;

        float t = 0f;
        while (t < duration)
        {
            t += Dt;
            noiseGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        noiseGroup.alpha = to;
        if (Mathf.Approximately(to, 0f)) noiseGroup.blocksRaycasts = false;
    }

    private void UpdateNoiseUV(float dt)
    {
        if (noiseImage == null) return;

        Rect r = noiseImage.uvRect;
        r.x += uvScrollSpeed.x * dt;
        r.y += uvScrollSpeed.y * dt;
        r.x += UnityEngine.Random.Range(-uvJitterStrength, uvJitterStrength);
        r.y += UnityEngine.Random.Range(-uvJitterStrength, uvJitterStrength);
        r.width = uvTiling.x;
        r.height = uvTiling.y;
        noiseImage.uvRect = r;
    }

    private void StopIfRunning()
    {
        if (_running != null) { StopCoroutine(_running); _running = null; }
    }
}