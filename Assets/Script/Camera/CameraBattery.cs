using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BatteryManager : MonoBehaviour
{

    // カメラごとのバッテリー情報をまとめるクラス
    [System.Serializable]
    public class BatteryData
    {
        public CameraSwitcher.CameraState cameraState; // どのカメラか
        public float maxBattery = 100f;                // 最大バッテリー
        public float decreaseRate = 5f;                // 減少スピード
        [HideInInspector] public float currentBattery; // 現在のバッテリー
    }

    [Header("UIの参照")]
    public Image batteryFillImage;   // ゲージ用の画像(BatteryFill)
    public TextMeshProUGUI batteryText; // %表示用

    [Header("カメラ連携")]
    [Tooltip("CameraSwitcherがアタッチされているオブジェクトを入れてください")]
    public CameraSwitcher cameraSwitcher;

    [Header("各カメラのバッテリー設定")]
    public List<BatteryData> batterySettings = new List<BatteryData>();

    [Header("カラー設定")]
    public Color normalColor;
    public Color warningColor;

    // 現在表示しているカメラのバッテリーデータを保持
    private BatteryData activeBatteryData;

    void Start()
    {
        // ゲーム開始時にバッテリーを最大値
        foreach (var data in batterySettings)
        {
            data.currentBattery = data.maxBattery;
        }

        if (cameraSwitcher == null)
        {
            UpdateActiveBatteryData();
        }
    }

    void Update()
    {
        if (cameraSwitcher == null) return;

        // カメラが切り替わったかチェック
        if (activeBatteryData == null || activeBatteryData.cameraState != cameraSwitcher.CurrentState)
        {
            UpdateActiveBatteryData();
        }

        // 現在表示しているカメラのデータが設定されていれば処理
        if (activeBatteryData != null)
        {
            // そのカメラを見ている間だけ、バッテリーを消費
            if (activeBatteryData.currentBattery > 0)
            {
                activeBatteryData.currentBattery -= activeBatteryData.decreaseRate * Time.deltaTime;
                activeBatteryData.currentBattery = Mathf.Clamp(activeBatteryData.currentBattery, 0, activeBatteryData.maxBattery);
            }

            // 毎フレームUIを更新
            UpdateUI(activeBatteryData);
        }
    }

    private void UpdateActiveBatteryData()
    {
        // 現在のカメラ状態に対応するバッテリーデータを探す
        activeBatteryData = batterySettings.Find(b => b.cameraState == cameraSwitcher.CurrentState);
        if (activeBatteryData == null)
        {
            Debug.LogWarning("現在のカメラ状態に対応するバッテリーデータが見つかりません！ カメラ状態: " + cameraSwitcher.CurrentState);
        }
    }

    private void UpdateUI(BatteryData data)
    {
        // 1. ゲージの長さを更新
        if (batteryFillImage != null)
        {
            batteryFillImage.fillAmount = data.currentBattery / data.maxBattery;

            //バッテリーが20%を切ったら
            if (data.currentBattery < 20f)
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
            int percentage = Mathf.FloorToInt(data.currentBattery);
            batteryText.text = percentage.ToString() + "%";
        }
    }
}
