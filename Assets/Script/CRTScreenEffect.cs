using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TV 画面のノイズを常時ループ再生します。
/// </summary>
public class CRTScreenEffect : MonoBehaviour
{
    [Header("ノイズ設定")]
    [Tooltip("ノイズテクスチャを貼った RawImage")]
    [SerializeField] private RawImage noiseImage;

    [Tooltip("UV スクロール速度")]
    [SerializeField] private Vector2 uvScrollSpeed = new Vector2(1.5f, 2.5f);

    [Tooltip("毎フレームのブレ強度")]
    [SerializeField][Range(0f, 0.2f)] private float uvJitter = 0.04f;

    [SerializeField] private Vector2 uvTiling = new Vector2(2f, 2f);

    //[Tooltip("ノイズの透明度")]
    //[SerializeField][Range(0f, 1f)] private float noiseAlpha = 0.35f;

    //private void Awake()
    //{
    //    if (noiseImage == null) return;
    //    Color c = noiseImage.color;
    //    c.a = noiseAlpha;
    //    noiseImage.color = c;
    //}

    private void Update()
    {
        if (noiseImage == null) return;

        Rect r = noiseImage.uvRect;
        r.x += uvScrollSpeed.x * Time.deltaTime + Random.Range(-uvJitter, uvJitter);
        r.y += uvScrollSpeed.y * Time.deltaTime + Random.Range(-uvJitter, uvJitter);
        r.width = uvTiling.x;
        r.height = uvTiling.y;
        noiseImage.uvRect = r;
    }
}