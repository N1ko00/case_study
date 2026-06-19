using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// CameraSwitcherと同じGameObjectに?ってあると、CameraSwitcherの切り替え時にノイズが入るフック
/// </summary>

public class CameraSwitchNoiseHook : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("未設定なら同じGameObjectから自動取得")]
    [SerializeField] private CameraSwitcher cameraSwitcher;
    [Tooltip("未設定ならシ?ンから自動検索")]
    [SerializeField] private TVNoiseEffect noiseEffect;

    [Header("演出時間")]
    [SerializeField] private float fadeIn = 0.08f;   // 切替後に被せるので短めが自然
    [SerializeField] private float hold = 0.05f;
    [SerializeField] private float fadeOut = 0.25f;

    [Header("動作設定")]
    [Tooltip("Spaceキ?での切替時もノイズを鳴らす")]
    [SerializeField] private bool reactToSpaceKey = true;
    [Tooltip("cameraCanvas配下のButtonクリックでもノイズを鳴らす")]
    [SerializeField] private bool reactToButtonClick = true;

    [Header("SE設定")]
    [Tooltip("ノイズ演出中に流すル?プSE (砂嵐音)")]
    [SerializeField] private AudioClip noiseSE;
    [Tooltip("未設定なら自動追加")]
    [SerializeField] private AudioSource audioSource;
    [Range(0f, 1f)]
    [SerializeField] private float seVolume = 0.8f;

    private Coroutine _audioCoroutine;

    private void Awake()
    {
        // 自動検索
        if (cameraSwitcher == null) cameraSwitcher = GetComponent<CameraSwitcher>();
        if (noiseEffect == null)
            noiseEffect = FindAnyObjectByType<TVNoiseEffect>(FindObjectsInactive.Include);

        if (cameraSwitcher == null)
        {
            Debug.LogWarning("[CameraSwitcherNoiseHook] CameraSwitcherが見つからん", this);
            enabled = false;
            return;
        }

        // AudioSource 未設定なら自動追加
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;  // 2D SE
            audioSource.loop = true;
            audioSource.volume = 0f;
        }
    }

    private void Start()
    {
        if (reactToButtonClick)
        {
            HookCameraCanvasButtons();
            PlayNoise();
        }
    }

    private void Update()
    {
        // Spaceキ? → CameraSwitcher.ToggleCamera() と同じ?イ?ングでノイズ
        if (!reactToSpaceKey) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;

        // インベントリが開いている間はノイズも鳴らさない
        if (UIInventory.Instance != null && UIInventory.Instance.IsOpen) return;

        // カメラがロックされている時もノイズを鳴らさない
        if (cameraSwitcher != null && cameraSwitcher.IsLocked) return;

        PlayNoiseWithSE();
    }

    /// <summary>
    /// CameraSwitcher の private な cameraCanvas を取得し、配下の全Buttonに
    /// PlayNoise を onClick リスナ?として追加する。
    /// </summary>
    private void HookCameraCanvasButtons()
    {
        FieldInfo field = typeof(CameraSwitcher).GetField(
            "cameraCanvas",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null) return;

        GameObject canvas = field.GetValue(cameraSwitcher) as GameObject;
        if (canvas == null) return;

        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button b in buttons)
        {
            b.onClick.AddListener(PlayNoiseWithSE);
        }

        Debug.Log($"[CameraSwitcherNoiseHook] {buttons.Length}個の??ンにノイズフックを追加");
    }

    private void PlayNoiseWithSE()
    {
        PlayNoise();
        PlaySESynced();
    }

    // ノイズ演出のみ (Spaceキ?用)
    private void PlayNoise()
    {
        if (noiseEffect == null) return;

        if (!noiseEffect.gameObject.activeInHierarchy)
            noiseEffect.gameObject.SetActive(true);

        noiseEffect.PlayFullSequence(
            onMid: null,
            fadeIn: fadeIn,
            hold: hold,
            fadeOut: fadeOut);
    }

    // ノイズ演出に合わせてSEをフェ?ドイン→フェ?ドアウト
    private void PlaySESynced()
    {
        if (audioSource == null || noiseSE == null) return;

        if (_audioCoroutine != null) StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(SyncedAudioCoroutine());
    }

    private IEnumerator SyncedAudioCoroutine()
    {
        // SE開始
        audioSource.clip = noiseSE;
        audioSource.volume = 0f;
        audioSource.Play();

        // フェ?ドイン (ノイズが出てくるのに合わせて)
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, seVolume, t / fadeIn);
            yield return null;
        }
        audioSource.volume = seVolume;

        // ホ?ルド
        t = 0f;
        while (t < hold)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // フェ?ドアウト (ノイズが消えるのに合わせて)
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(seVolume, 0f, t / fadeOut);
            yield return null;
        }

        // SE完全停?
        audioSource.Stop();
        audioSource.volume = 0f;
        _audioCoroutine = null;
    }
}