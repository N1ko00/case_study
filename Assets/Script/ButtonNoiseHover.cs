using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ボタンホバー時にデジタルグリッチ効果を出す。
/// マウスホバー (IPointerEnter/Exit) と
/// コントローラー/キーボード選択 (ISelect/Deselect) の両方に反応。
/// </summary>
public class ButtonNoiseHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("グリッチ強度")]
    [Tooltip("水平方向の最大ずれ量")]
    [SerializeField] private float glitchShiftX = 15f;
    [Tooltip("垂直方向の最大ずれ量")]
    [SerializeField] private float glitchShiftY = 3f;
    [Tooltip("1秒間に何回グリッチするか")]
    [SerializeField] private float glitchFrequency = 10f;
    [Tooltip("1回のグリッチが続く時間 (秒)")]
    [SerializeField] private float glitchDuration = 1f;
    [Tooltip("アルファが0になる確率 (0~1)")]
    [SerializeField] private float flickerChance = 0.08f;

    [Header("クロマティックアベレーション")]
    [Tooltip("色ずれ残像を有効にする")]
    [SerializeField] private bool enableChromatic = true;
    [Tooltip("R残像のずれ量 (px)")]
    [SerializeField] private float chromaticOffset = 5f;
    [Tooltip("残像の透明度")]
    [SerializeField][Range(0f, 1f)] private float ghostAlpha = 0.4f;

    [Header("フェード")]
    [SerializeField] private float fadeInDuration = 0.06f;
    [SerializeField] private float fadeOutDuration = 0.1f;

    // ── 内部 ──
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Graphic _graphic;

    private Vector3 _originPos;
    private Vector3 _originScale;

    private float _effectStrength = 0f;
    private Coroutine _fadeCoroutine;
    private Coroutine _glitchCoroutine;

    // クロマティックアベレーション用の残像オブジェクト
    private RectTransform _ghostR;  // 赤残像
    private RectTransform _ghostB;  // 青残像

    // ホバーと選択を独立して扱うためのフラグ
    private bool _isPointerOver = false;
    private bool _isSelected = false;

    // ─────────────────────────────────────
    // 初期化
    // ─────────────────────────────────────
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _graphic = GetComponent<Graphic>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _originPos = _rectTransform.localPosition;
        _originScale = _rectTransform.localScale;

        if (enableChromatic)
            CreateGhosts();
    }

    private void CreateGhosts()
    {
        _ghostR = CreateGhost("Ghost_R", new Color(1f, 0.05f, 0.05f, ghostAlpha));
        _ghostB = CreateGhost("Ghost_B", new Color(0.1f, 0.3f, 1f, ghostAlpha));
    }

    private RectTransform CreateGhost(string goName, Color tint)
    {
        GameObject go = new GameObject(goName, typeof(RectTransform));
        go.transform.SetParent(transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image srcImage = GetComponent<Image>();
        if (srcImage != null)
        {
            Image ghost = go.AddComponent<Image>();
            ghost.sprite = srcImage.sprite;
            ghost.color = tint;
            ghost.material = srcImage.material;
            ghost.type = srcImage.type;
        }

        go.transform.SetAsFirstSibling();
        go.SetActive(false);
        return rt;
    }

    // ─────────────────────────────────────
    // マウスイベント
    // ─────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        StartEffect();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        TryStopEffect();
    }

    // ─────────────────────────────────────
    // コントローラー/キーボード選択イベント
    // ─────────────────────────────────────
    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
        StartEffect();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
        TryStopEffect();
    }

    // ─────────────────────────────────────
    // エフェクトON/OFF制御 (ホバー or 選択のどちらかで有効)
    // ─────────────────────────────────────
    private void StartEffect()
    {
        SetFade(1f, fadeInDuration);

        if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
        _glitchCoroutine = StartCoroutine(GlitchLoop());
    }

    private void TryStopEffect()
    {
        // マウスも乗っておらず、選択もされていない時だけ停止
        if (_isPointerOver || _isSelected) return;

        // フェードアウトをスキップして即座に全エフェクトを止める
        StopEffectImmediate();
    }

    /// <summary>
    /// 全エフェクトを即座に停止する。
    /// 別のボタンに移動したときに残像が残らないようにするための瞬間停止処理。
    /// </summary>
    private void StopEffectImmediate()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
        if (_glitchCoroutine != null)
        {
            StopCoroutine(_glitchCoroutine);
            _glitchCoroutine = null;
        }

        // 強度を即座に0にリセット
        _effectStrength = 0f;

        // 見た目を完全に元に戻す
        ResetToOrigin();
        SetGhostsActive(false);
    }

    // ─────────────────────────────────────
    // グリッチループ
    // ─────────────────────────────────────
    private IEnumerator GlitchLoop()
    {
        float interval = 1f / glitchFrequency;

        while (true)
        {
            yield return new WaitForSecondsRealtime(interval * Random.Range(0.5f, 1.5f));

            if (Random.value > _effectStrength) continue;

            yield return DoGlitch();
        }
    }

    private IEnumerator DoGlitch()
    {
        float elapsed = 0f;

        SetGhostsActive(true);

        while (elapsed < glitchDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float str = _effectStrength;

            float ox = Random.Range(-glitchShiftX, glitchShiftX) * str;
            float oy = Random.Range(-glitchShiftY, glitchShiftY) * str;
            _rectTransform.localPosition = _originPos + new Vector3(ox, oy, 0f);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = (Random.value < flickerChance * str) ? 0f : 1f;
            }

            if (enableChromatic)
            {
                float co = chromaticOffset * str;
                if (_ghostR != null)
                    _ghostR.localPosition = new Vector3(
                        Random.Range(-co, co),
                        Random.Range(-co * 0.3f, co * 0.3f), 0f);
                if (_ghostB != null)
                    _ghostB.localPosition = new Vector3(
                        Random.Range(-co, co),
                        Random.Range(-co * 0.3f, co * 0.3f), 0f);
            }

            yield return null;
        }

        ResetToOrigin();
        SetGhostsActive(false);
    }

    // ─────────────────────────────────────
    // フェード
    // ─────────────────────────────────────
    private void SetFade(float target, float duration)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeStrength(target, duration));
    }

    private IEnumerator FadeStrength(float target, float duration)
    {
        float start = _effectStrength;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            _effectStrength = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        _effectStrength = target;
        _fadeCoroutine = null;
    }

    // ─────────────────────────────────────
    // ヘルパー
    // ─────────────────────────────────────
    private void ResetToOrigin()
    {
        _rectTransform.localPosition = _originPos;
        _rectTransform.localScale = _originScale;
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
    }

    private void SetGhostsActive(bool active)
    {
        if (_ghostR != null) _ghostR.gameObject.SetActive(active);
        if (_ghostB != null) _ghostB.gameObject.SetActive(active);
    }

    private void OnDisable()
    {
        _isPointerOver = false;
        _isSelected = false;
        StopEffectImmediate();
    }
}