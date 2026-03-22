using UnityEngine;

public class Mike : MonoBehaviour
{
    [Header("判定設定")]
    [SerializeField] private int sampleWindow = 128;
    [SerializeField] private float volumeThreshold = 0.01f;

    private AudioClip micClip;
    private string micDevice;
    private float[] sampleBuffer;
    private SoundEmitter _soundEmitter;

    public bool IsMicActive { get; private set; }
    public float CurrentVolume { get; private set; }

    void Start()
    {
        // 利用可能なマイクがあるか確認
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("マイクが見つかりません。");
            return;
        }

        // デフォルトマイクを使う
        micDevice = Microphone.devices[0];

        _soundEmitter = GetComponent<SoundEmitter>();

        // 1秒のループ録音を開始
        micClip = Microphone.Start(micDevice, true, 1, 44100);
        sampleBuffer = new float[sampleWindow];

        Debug.Log("マイク録音開始: " + micDevice);
    }

    void Update()
    {
        if (micClip == null || string.IsNullOrEmpty(micDevice))
            return;

        int micPos = Microphone.GetPosition(micDevice);

        // まだ十分に録音されていない間は待つ
        if (micPos < sampleWindow)
            return;

        int startPos = micPos - sampleWindow;
        micClip.GetData(sampleBuffer, startPos);

        float sum = 0f;
        for (int i = 0; i < sampleBuffer.Length; i++)
        {
            float sample = sampleBuffer[i];
            sum += sample * sample;
        }

        // RMS音量
        CurrentVolume = Mathf.Sqrt(sum / sampleBuffer.Length);

        bool newState = CurrentVolume > volumeThreshold;

        if (newState != IsMicActive)
        {
            IsMicActive = newState;

            if (IsMicActive && _soundEmitter != null)
                _soundEmitter.StunEnemies();

            Debug.Log(IsMicActive
                ? $"マイク入力あり: {CurrentVolume:F4}"
                : "マイク入力なし");
        }
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(micDevice) && Microphone.IsRecording(micDevice))
        {
            Microphone.End(micDevice);
        }
    }
}