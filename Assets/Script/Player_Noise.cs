using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Noise : MonoBehaviour
{
    [Header("音通知設定")]
    public Key noiseKey = Key.F;                  // 音を出すキー
    public float noiseRadius = 8f;                // 音の届く半径
    public NoiseSourceType noiseSourceType;       // 音の種類

    void Update()
    {
        CheckNoiseInput();
    }

    void CheckNoiseInput()
    {
        if (Keyboard.current != null && Keyboard.current[noiseKey].wasPressedThisFrame)
        {
            EmitNoise();
        }
    }

    void EmitNoise()
    {
        // 実装確認用ログ
        Debug.Log($"[Player_Noise] Fキー入力で音通知処理を実行します");

        if (SoundManager.Instance != null)
        {
            // 指定された音通知処理を呼ぶ
            //SoundManager.Instance.EmitNoise(transform.position, noiseRadius, noiseSourceType);

            // 呼び出し確認ログ
            Debug.Log("[Player_Noise]SoundManager.Instance.EmitNoise を呼び出しました");
        }
        else
        {
            Debug.LogWarning("[Player_Noise] SoundManager.Instance が見つかりません");
        }
    }
}