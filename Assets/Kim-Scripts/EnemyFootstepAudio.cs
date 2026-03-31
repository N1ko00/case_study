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
    [SerializeField] private float maxVolume = 1f;  // 最大音量


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
    private float NoiseRadius => _monster != null ? _monster.FootstepNoiseRadius : 10f;

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

        float volume = CalculateVolumeByDistance();
        _audioSource.volume = volume;

        // ピッチをリアルタイム更新 (再生中でも即反映される)
        _audioSource.pitch = _isChasing ? chasePitch : normalPitch;

        // 範囲外に出たら即停止
        if (volume <= 0f)
            _audioSource.Stop();
    }

    // ───────────────────────────────────────────
    // AudioSource 初期設定
    // ───────────────────────────────────────────
    private void SetupAudioSource()
    {
        _audioSource.spatialBlend = 1f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
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
        _audioSource.volume = CalculateVolumeByDistance();
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

        // 再生中なら pitch を即時反映 (Stop不要)
        if (_audioSource.isPlaying)
            _audioSource.pitch = _isChasing ? chasePitch : normalPitch;
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

        // 範囲内 → 距離比率で線形に減衰
        // dist=0 → 1.0, dist=radius → 0.0
        float t = dist / radius;
        return Mathf.Lerp(maxVolume, 0f, t);
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