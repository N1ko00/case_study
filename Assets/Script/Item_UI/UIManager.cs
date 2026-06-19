//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class UIManager : MonoBehaviour
//{
//    public static UIManager Instance;

//    [Header("UIの参照")]
//    public GameObject messageWindow; // messagewindow.pngを付けたオブジェクト
//    public Text itemNameText;        // 【追加】上のタブに入れるアイテム名用テキスト
//    public Text messageContentText;  // 下の広いスペースに入れる「〜を見つけた」用テキスト

//    [Header("表示時間設定")]
//    public float displayDuration = 3.0f;

//    private Coroutine hideTextCoroutine;

//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        // 初期状態はすべて非表示
//        if (itemNameText != null) itemNameText.text = "";
//        if (messageContentText != null) messageContentText.text = "";
//        if (messageWindow != null) messageWindow.SetActive(false);
//    }

//    // アイテム名とメッセージを別々に受け取って表示する関数
//    public void ShowItemMessage(string itemName, string content)
//    {
//        if (messageWindow == null) return;

//        if (hideTextCoroutine != null)
//        {
//            StopCoroutine(hideTextCoroutine);
//        }

//        // テキストをそれぞれの場所にセット
//        if (itemNameText != null) itemNameText.text = itemName;
//        if (messageContentText != null) messageContentText.text = content;

//        // ウィンドウを表示
//        messageWindow.SetActive(true);

//        // タイマー開始
//        hideTextCoroutine = StartCoroutine(HideTextAfterDelay());
//    }

//    IEnumerator HideTextAfterDelay()
//    {
//        yield return new WaitForSeconds(displayDuration);

//        if (itemNameText != null) itemNameText.text = "";
//        if (messageContentText != null) messageContentText.text = "";
//        if (messageWindow != null) messageWindow.SetActive(false);

//        hideTextCoroutine = null;
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UIの参照")]
    public GameObject messageWindow; // messagewindow.pngを付けたオブジェクト
    public Text itemNameText;        // 上のタブに入れるアイテム名用テキスト
    public Text messageContentText;  // 下の広いスペースに入れるテキスト

    [Header("表示時間設定")]
    public float displayDuration = 3.0f;

    private Coroutine hideTextCoroutine;
    private Coroutine sequenceCoroutine; // 連続メッセージ用のコルーチン

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 初期状態はすべて非表示
        if (itemNameText != null) itemNameText.text = "";
        if (messageContentText != null) messageContentText.text = "";
        if (messageWindow != null) messageWindow.SetActive(false);
    }

    // 通常のアイテム拾った時・使用時用（3秒で自動で消える）
    public void ShowItemMessage(string itemName, string content)
    {
        if (messageWindow == null) return;

        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }

        if (itemNameText != null) itemNameText.text = itemName;
        if (messageContentText != null) messageContentText.text = content;

        messageWindow.SetActive(true);
        hideTextCoroutine = StartCoroutine(HideTextAfterDelay());
    }

    // ★【追加機能】複数のメッセージを順番に表示する機能
    // 使い方： UIManager.Instance.ShowSequentialMessages("鍵", new string[] { "鍵を見つけた。", "錆びついているな……。" });
    public void ShowSequentialMessages(string itemName, string[] contents)
    {
        if (messageWindow == null || contents == null || contents.Length == 0) return;

        StopAllMessageCoroutines(); // 実行中のメッセージ処理をリセットします

        sequenceCoroutine = StartCoroutine(ShowMessagesSequence(itemName, contents));
    }

    // ★【追加】順番に表示するためのコルーチン
    private IEnumerator ShowMessagesSequence(string itemName, string[] contents)
    {
        messageWindow.SetActive(true);

        foreach (string content in contents)
        {
            if (itemNameText != null) itemNameText.text = itemName;
            if (messageContentText != null) messageContentText.text = content;

            // 1つのメッセージにつき、displayDurationの秒数だけ待機します
            yield return new WaitForSeconds(displayDuration);
        }

        // すべて表示し終わったらウィンドウを閉じますわ
        if (itemNameText != null) itemNameText.text = "";
        if (messageContentText != null) messageContentText.text = "";
        if (messageWindow != null) messageWindow.SetActive(false);
    }

    // ★【追加機能】インベントリ用：自動で消えないメッセージ表示（カーソルが外れるまで出し続ける）
    public void ShowPersistentItemMessage(string itemName, string content)
    {
        if (messageWindow == null) return;

        // 3秒で消えるコルーチンが動いていたら即座に止める
        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }

        if (itemNameText != null) itemNameText.text = itemName;
        if (messageContentText != null) messageContentText.text = content;

        messageWindow.SetActive(true);
    }

    // ★【追加】コルーチンを安全に止めるためのまとめ関数
    private void StopAllMessageCoroutines()
    {
        if (hideTextCoroutine != null) StopCoroutine(hideTextCoroutine);
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
    }

    IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (itemNameText != null) itemNameText.text = "";
        if (messageContentText != null) messageContentText.text = "";
        if (messageWindow != null) messageWindow.SetActive(false);
    }
}