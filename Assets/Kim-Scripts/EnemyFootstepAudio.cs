using UnityEngine;

/// <summary>
/// 敵の足音を 3D サウンドで再生するコンポーネント。
/// InvisibleMonster の footstepNoiseRadius を基準に
/// プレイヤーとの距離でリアルタイムボリューム調整します。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EnemyFootstepAudio : MonoBehaviour
{
    [Header("足音クリップ設定")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("ボリューム設定")]
    [SerializeField] private float maxVolume = 1.0f;  // 最大音量
    [SerializeField] private float chaseVolumeMultiplier = 1.5f; // 追跡時の音量倍率


    [Header("ピッチ設定")]
    // 通常時の再生速度 (1.0 = 等速)
    [SerializeField] private float normalPitch = 1f;
    // 追跡時の再生速度 (1.0より大きいほど速くなる)
    [SerializeField] private float chasePitch = 1.4f;

    // ───────────────────────────────────────────
    // 内部変数
    // ───────────────────────────────────────────
    private AudioSource _audioSource;
    private Transform _playerTransform;

    // InvisibleMonster の footstepNoiseRadius を参照
    private InvisibleMonster _monster;

    // footstepNoiseRadius の範囲をそのまま使う
    // 範囲の端 → 無音、中心に近いほど最大音量
    private float NoiseRadius => _monster != null ? _monster.FootstepNoiseRadius : 100f;

    // 現在追跡モードかどうか
    private bool _isChasing = false;

    // ───────────────────────────────────────────
    // Unity ライフサイクル
    // ───────────────────────────────────────────
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _monster = GetComponent<InvisibleMonster>();
        SetupAudioSource();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;
    }

    private void Update()
    {
        if (!_audioSource.isPlaying) return;

        // ピッチのみ更新 (ボリュームは Unity 3D オーディオに完全に任せる)
        _audioSource.pitch = _isChasing ? chasePitch : normalPitch;

        // 範囲外に出たら即停止
        if (_playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist >= NoiseRadius)
                _audioSource.Stop();
        }
    }

    // ───────────────────────────────────────────
    // AudioSource 初期設定
    // ───────────────────────────────────────────
    private void SetupAudioSource()
    {
        // 完全 3D サウンド → 方向・距離減衰を Unity に任せる
        _audioSource.spatialBlend = 1f;

        // 2次曲線カーブに近い Custom rolloff を使用
        _audioSource.rolloffMode = AudioRolloffMode.Custom;

        // 最大音量の距離 = 0付近、無音距離 = NoiseRadius
        _audioSource.minDistance = 0.5f;
        _audioSource.maxDistance = NoiseRadius;

        // 2次関数 のカスタムカーブを設定
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, 1f, 0f, 0f),    // t=0   → y=1.0   
            new Keyframe(0.25f, 0.5625f, -1f, -1f), // t=0.25→ y=0.5625
            new Keyframe(0.5f, 0.25f, -1f, -1f),  // t=0.5 → y=0.25
            new Keyframe(0.75f, 0.0625f, -0.5f, -0.5f), // t=0.75→ y=0.0625
            new Keyframe(1f, 0f, -0.5f, 0f)  // t=1   → y=0.0   (無音)
        );
        _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);

        _audioSource.volume = maxVolume;
        _audioSource.loop = false;
        _audioSource.playOnAwake = false;
        _audioSource.pitch = normalPitch;
    }

    // ───────────────────────────────────────────
    // 足音再生
    // ───────────────────────────────────────────
    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
        {
            Debug.LogWarning("[EnemyFootstepAudio] 足音クリップが設定されていません");
            return;
        }

        if (_audioSource.isPlaying) return;

        PlayClip();
    }

    /// <summary>
    /// 移動停止時に呼び出します。再生中のクリップを即座に停止します。
    /// </summary>
    public void StopFootstep()
    {
        if (_audioSource.isPlaying)
            _audioSource.Stop();
    }

    private void PlayClip()
    {
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        _audioSource.clip = clip;
        // 追跡中はボリュームを倍
        float volume = _isChasing ? maxVolume * chaseVolumeMultiplier : maxVolume;
        _audioSource.volume = Mathf.Clamp01(volume);
        _audioSource.Play();
    }

    /// <summary>
    /// 追跡状態に切り替わった瞬間に呼び出します。
    /// 再生中のクリップを即座に止めて新しいクリップを再生します。
    /// </summary>
    public void SetChaseMode(bool isChasing)
    {
        // 状態が変わっていなければ何もしない
        if (_isChasing == isChasing) return;

        _isChasing = isChasing;

        // 再生中ならピッチ＋ボリュームを即時反映
        if (_audioSource.isPlaying)
        {
            _audioSource.pitch = _isChasing ? chasePitch : normalPitch;
            float volume = _isChasing ? maxVolume * chaseVolumeMultiplier : maxVolume;
            _audioSource.volume = Mathf.Clamp01(volume);
        }
    }
    // ───────────────────────────────────────────
    // 距離ベースボリューム計算
    // ───────────────────────────────────────────

    /// <summary>
    /// footstepNoiseRadius 内での距離比率でボリュームを計算します。
    /// 距離 0  → maxVolume (最大)
    /// 距離 = NoiseRadius → 0 (無音)
    /// </summary>
    private float CalculateVolumeByDistance()
    {
        if (_playerTransform == null) return maxVolume;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        float radius = NoiseRadius;

        // 範囲外 → 無音
        if (dist >= radius) return 0f;

        // 範囲内 → 2次関数 (1-t)² で減衰
        float t = dist / radius;
        return maxVolume * (1f - t) * (1f - t);
    }

//    // ───────────────────────────────────────────
//    // Gizmos
//    // ───────────────────────────────────────────
//#if UNITY_EDITOR
//    private void OnDrawGizmosSelected()
//    {
//        float radius = NoiseRadius;

//        // 発音範囲 (footstepNoiseRadius と同じ円)
//        Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
//        Gizmos.DrawSphere(transform.position, radius);
//        Gizmos.color = new Color(0f, 1f, 1f, 0.8f);
//        Gizmos.DrawWireSphere(transform.position, radius);

//        // プレイヤーとの距離線 + リアルタイムボリューム表示
//        if (_playerTransform != null)
//        {
//            float dist = Vector3.Distance(transform.position, _playerTransform.position);
//            float t = Mathf.Clamp01(dist / radius);
//            Gizmos.color = Color.Lerp(Color.green, Color.red, t);
//            Gizmos.DrawLine(transform.position, _playerTransform.position);

//            float vol = CalculateVolumeByDistance();
//            UnityEditor.Handles.Label(
//                transform.position + Vector3.up * 2f,
//                $"Vol: {vol:F2}  Dist: {dist:F1}m"
//            );
//        }
//    }
//#endif
}