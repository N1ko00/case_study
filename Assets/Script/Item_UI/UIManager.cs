using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UIの参照")]
    public GameObject messageWindow; // messagewindow.pngを付けたオブジェクト
    public Text itemNameText;        // 【追加】上のタブに入れるアイテム名用テキスト
    public Text messageContentText;  // 下の広いスペースに入れる「〜を見つけた」用テキスト

    [Header("表示時間設定")]
    public float displayDuration = 3.0f;

    private Coroutine hideTextCoroutine;

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

    // アイテム名とメッセージを別々に受け取って表示する関数
    public void ShowItemMessage(string itemName, string content)
    {
        if (messageWindow == null) return;

        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }

        // テキストをそれぞれの場所にセット
        if (itemNameText != null) itemNameText.text = itemName;
        if (messageContentText != null) messageContentText.text = content;

        // ウィンドウを表示
        messageWindow.SetActive(true);

        // タイマー開始
        hideTextCoroutine = StartCoroutine(HideTextAfterDelay());
    }

    IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (itemNameText != null) itemNameText.text = "";
        if (messageContentText != null) messageContentText.text = "";
        if (messageWindow != null) messageWindow.SetActive(false);

        hideTextCoroutine = null;
    }
}