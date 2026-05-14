using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BatteryManager : MonoBehaviour
{

    [Header("バッテリー設定")]
    public float maxBattery = 100f;                // 最大バッテリー
    public float decreaseRate = 5f;                // 減少スピード
    private float currentBattery;                  // 現在のバッテリー

    [Header("UIの参照")]
    public GameObject batteryUIPanel;
    public Image batteryFillImage;   // ゲージ用の画像(BatteryFill)
    public TextMeshProUGUI batteryText; // %表示用

    [Header("カメラ連携")]
    [Tooltip("CameraSwitcherがアタッチされているオブジェクトを入れてください")]
    public CameraSwitcher cameraSwitcher;

    [Header("カラー設定")]
    public Color normalColor;
    public Color warningColor;

    void Start()
    {
        // ゲーム開始時にバッテリーを最大値
        currentBattery = maxBattery;
        UpdateUI();
    }

    void Update()
    {
        // 0番目（メイン）以外のカメラを表示している時は「監視カメラ使用中」
        bool isSubCamera = (cameraSwitcher != null && cameraSwitcher.CurrentCameraIndex != 0);

        if (batteryUIPanel != null)
        {
            batteryUIPanel.SetActive(isSubCamera);
        }

        if (isSubCamera && currentBattery > 0)
        {
            currentBattery -= decreaseRate * Time.deltaTime;
            currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);
        }

        UpdateUI();

        // バッテリーが切れたら強制的にメインカメラに戻す
        if (currentBattery <= 0 && isSubCamera)
        {
            cameraSwitcher.SetCameraState(0);
        }
    }


    private void UpdateUI()
    {
        // 1. ゲージの長さを更新
        if (batteryFillImage != null)
        {
            batteryFillImage.fillAmount = currentBattery / maxBattery;

            //バッテリーが20%を切ったら
            if (currentBattery < 20f)
            {
                batteryFillImage.color = warningColor; // 赤色にする
            }
            else
            {
                batteryFillImage.color = normalColor;  // 通常の色に戻す（または維持する）
            }
        }

        // 2. テキストを更新
        if (batteryText != null)
        {
            int percentage = Mathf.FloorToInt(currentBattery);
            batteryText.text = percentage.ToString() + "%";
        }
    }
}
