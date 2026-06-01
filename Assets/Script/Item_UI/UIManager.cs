using UnityEngine;
using UnityEngine.UI;
using System.Collections; // コルーチン（時間計測）を使うために必須です

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Text messageText;

    [Header("文字を表示しておく時間（秒）")]
    public float displayDuration = 3.0f;

    // 現在動いている文字消去タイマーを記憶する変数
    private Coroutine hideTextCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // ゲーム開始時はメッセージを空っぽ（非表示）にしておく
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    public void ShowMessage(string msg)
    {
        if (messageText == null) return;

        // もし、すでに前の文字のタイマー（3秒カウントなど）が動いていたら、一度止める
        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }

        // 新しい文字を表示
        messageText.text = msg;

        // 新しく文字消去タイマー（コルーチン）をスタートする
        hideTextCoroutine = StartCoroutine(HideTextAfterDelay());
    }

    // 指定された時間（秒）待ってから文字を消す処理
    IEnumerator HideTextAfterDelay()
    {
        // displayDuration（インスペクターで設定した秒数）だけ待つ
        yield return new WaitForSeconds(displayDuration);

        // 文字を空にする
        messageText.text = "";

        // タイマーの記憶をリセット
        hideTextCoroutine = null;
    }
}