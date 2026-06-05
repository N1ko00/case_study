using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Monster AI: Patrol → ChaseSound → Searching → Patrol
///        どんな状況でも → ChasePlayer (視野監視後)
///        どんな状況でも → Stunned (外部からOnStunned()呼び出し時)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class InvisibleMonster : MonoBehaviour
{
    // ───────────────────────────────────────────
    // 状態
    // ───────────────────────────────────────────
    private enum State
    {
        Patrol,
        ChaseSound,
        ChasePlayer,
        Searching,
        Stunned
    }

    // ───────────────────────────────────────────
    // Inspector
    // ───────────────────────────────────────────
    [Header("Patrol 設定")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waypointStopDistance = 0.5f;   // 到達とみなす距離
    [SerializeField] private float waypointWaitTime = 1.0f;       // ウェイポイントでの待機時間

    [Header("ChaseSound 設定")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float chaseStopDistance = 0.5f;

    [Header("ChasePlayer 設定")]
    [SerializeField] private float chasePlayerSpeed = 5f;
    [SerializeField] private float losePlayerDistance = 20f;       // viewDistance より大きく設定
    [Tooltip("視野を失っても追跡を続ける猶予時間 (秒)")]
    [SerializeField] private float loseSightGrace = 1.0f;
    [Tooltip("追跡中の視野角倍率")]
    [SerializeField] private float chaseViewAngleMultiplier = 1.5f;

    [Header("Searching 設定")]
    [SerializeField] private float searchDuration = 5f;            // 探す時間
    [SerializeField] private float searchRadius = 4f;              // ランダム移動の半径
    [SerializeField] private float searchSpeed = 2f;

    [Header("Stunned 設定")]
    [SerializeField] private float stunDuration = 3f;              // 気絶時間 (秒)

    [Header("FOV 視野設定")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float viewDistance = 10f;
    [SerializeField][Range(0f, 360f)] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [Tooltip("Raycast を発射する目の高さ")]
    [SerializeField] private float eyeHeight = 1.5f;
    [Tooltip("この距離以内なら角度・遮蔽を無視して必ず発見")]
    [SerializeField] private float closeDetectRadius = 3f;

    [Header("足音設定")]
    [SerializeField] private float footstepInterval = 0.7f;        // 通常時の足音間隔
    [SerializeField] private float footstepIntervalChase = 0.35f;  // 追跡時の足音間隔
    [SerializeField] private float footstepNoiseRadius = 5f;       // SoundManager に渡すノイズ半径

    // EnemyFootstepAudio から参照するためのプロパティ
    public float FootstepNoiseRadius => footstepNoiseRadius;

    private EnemyFootstepAudio _footstepAudio;
    private float _footstepTimer = 0f;

    // ───────────────────────────────────────────
    // 内部変数
    // ───────────────────────────────────────────
    private NavMeshAgent _agent;
    private State _currentState;

    // Patrol
    private int _waypointIndex = 0;
    private float _waypointWaitTimer = 0f;
    private bool _isWaitingAtWaypoint = false;

    // ChaseSound
    private Vector3 _soundPosition;

    // Searching
    private float _searchTimer = 0f;
    private bool _searchWaitingForPath = false;   // 経路計算直後の安定化待機
    private Vector3 _searchOrigin;   // 探索の中心点

    // ChasePlayer
    private bool _isPlayerInSight = false;
    private Vector3 _lastSeenPosition;            // 最後に視界から見た位置
    private bool _hasLastSeenPosition = false;
    private bool _chaseLostDestSet = false;       // 視界外で目的地を1回だけ設定するフラグ
    private float _loseSightTimer = 0f;           // 視野喪失からの経過時間 (grace 用)

    // Stunned
    private float _stunTimer = 0f;

    // SetVisible / SetStunnedColor 用
    private Renderer[] _renderers;
    private Color[] _originalColors;
    private bool unique = true; //最初に見えなくする用

    // ───────────────────────────────────────────
    // Unity ライフサイクル
    // ───────────────────────────────────────────
    private void Awake()
    {
        // 他からの参照用に Awake で初期化
        _agent = GetComponent<NavMeshAgent>();
        _renderers = GetComponentsInChildren<Renderer>();

        // 元のマテリアルカラーを保存 (スタン解除時に戻すため)
        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i].material.HasProperty("_Color"))
                _originalColors[i] = _renderers[i].material.color;
        }
    }

    private void Start()
    {
        // 足音コンポーネントを取得 (なければ自動追加)
        _footstepAudio = GetComponent<EnemyFootstepAudio>();
        if (_footstepAudio == null)
            _footstepAudio = gameObject.AddComponent<EnemyFootstepAudio>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning("[InvisibleMonster] Player タグのオブジェクトが見つかりません");
        }

        EnterState(State.Patrol);
    }

    private void Update()
    {
        //if(unique)
        //{
        //    SetVisible(false);
        //    unique = false;
        //    Debug.Log("[InvisibleMonster] 初期化: 見えなく設定");
        //}

        // スタン中は FOV も状態更新もスキップ
        if (_currentState == State.Stunned)
        {
            UpdateStunned();
            return;
        }

        CheckFieldOfView();
        UpdateFootstep();

        switch (_currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.ChaseSound: UpdateChaseSound(); break;
            case State.ChasePlayer: UpdateChasePlayer(); break;
            case State.Searching: UpdateSearching(); break;
        }
    }

    // ───────────────────────────────────────────
    // 足音システム
    // ───────────────────────────────────────────

    /// <summary>
    /// 移動中のみ一定間隔で足音ノイズを発生させる。
    /// </summary>
    private void UpdateFootstep()
    {
        // 停止中・待機中は足音なし
        if (_agent.isStopped || _agent.velocity.sqrMagnitude < 0.01f)
        {
            if (_footstepAudio != null) _footstepAudio.StopFootstep();
            return;
        }

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer > 0f) return;

        // ChasePlayer は速い間隔
        _footstepTimer = (_currentState == State.ChasePlayer)
            ? footstepIntervalChase
            : footstepInterval;

        if (SoundManager.Instance != null)
            SoundManager.Instance.EmitNoise(transform.position, footstepNoiseRadius, NoiseSourceType.Enemy);

        if (_footstepAudio != null)
            _footstepAudio.PlayFootstep();
    }

    // ───────────────────────────────────────────
    // FOV 視野監視
    // ───────────────────────────────────────────

    /// <summary>
    /// 毎フレーム視界内かどうかを判定する。
    /// - closeDetectRadius 内なら角度・遮蔽を無視して発見
    /// - 追跡中は視野角を chaseViewAngleMultiplier 倍に拡大
    /// - Raycast は eyeHeight オフセットから発射
    /// </summary>
    private void CheckFieldOfView()
    {
        if (playerTransform == null) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;

        // 近距離自動検知
        if (distance <= closeDetectRadius)
        {
            OnPlayerSpotted();
            return;
        }

        // 距離チェック
        if (distance > viewDistance)
        {
            _isPlayerInSight = false;
            return;
        }

        // 角度チェック (追跡中は視野拡大)
        float effectiveAngle = (_currentState == State.ChasePlayer)
            ? viewAngle * chaseViewAngleMultiplier
            : viewAngle;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > effectiveAngle * 0.5f)
        {
            _isPlayerInSight = false;
            return;
        }

        // 目の高さからプレイヤーの胴体へ Raycast
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = playerTransform.position + Vector3.up * 0.5f;
        Vector3 dir = (targetPos - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, targetPos);

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, dist, obstacleMask))
        {
            Debug.DrawRay(eyePos, dir * hit.distance, Color.red);
            _isPlayerInSight = false;
            return;
        }

        Debug.DrawRay(eyePos, dir * dist, Color.green);
        OnPlayerSpotted();
    }

    /// <summary>
    /// プレイヤー発見時の共通処理。必要なら ChasePlayer に遷移。
    /// </summary>
    private void OnPlayerSpotted()
    {
        _isPlayerInSight = true;
        _lastSeenPosition = playerTransform.position;
        _hasLastSeenPosition = true;
        _loseSightTimer = 0f;

        if (_currentState != State.ChasePlayer)
            EnterState(State.ChasePlayer);
    }

    // ───────────────────────────────────────────
    // 状態遷移
    // ───────────────────────────────────────────
    private void EnterState(State newState)
    {
        // 状態切替時はウェイポイント待機をリセット
        _isWaitingAtWaypoint = false;
        _currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                _agent.isStopped = false;
                _agent.speed = patrolSpeed;
                if (_footstepAudio != null) _footstepAudio.SetChaseMode(false);
                MoveToCurrentWaypoint();
                Debug.Log("[InvisibleMonster] 状態: Patrol");
                break;

            case State.ChaseSound:
                _agent.isStopped = false;
                _agent.speed = chaseSpeed;
                _agent.SetDestination(_soundPosition);
                Debug.Log("[InvisibleMonster] 状態: ChaseSound → " + _soundPosition);
                break;

            case State.ChasePlayer:
                _agent.isStopped = false;
                _agent.speed = chasePlayerSpeed;
                _chaseLostDestSet = false;
                _loseSightTimer = 0f;
                if (_footstepAudio != null) _footstepAudio.SetChaseMode(true);
                Debug.Log("[InvisibleMonster] 状態: ChasePlayer");
                break;

            case State.Searching:
                _agent.isStopped = false;
                _agent.speed = searchSpeed;
                _searchTimer = searchDuration;
                _searchWaitingForPath = true;
                // プレイヤーを見たことあればその位置、なければ今いる場所を基準にする
                _searchOrigin = _hasLastSeenPosition ? _lastSeenPosition : transform.position;
                _agent.SetDestination(_searchOrigin);
                if (_footstepAudio != null) _footstepAudio.SetChaseMode(false);
                Debug.Log("[InvisibleMonster] 状態: Searching");
                break;

            case State.Stunned:
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;     // 残留速度もリセット
                if (_footstepAudio != null) _footstepAudio.StopFootstep();
                SetStunnedColor(true);
                _stunTimer = stunDuration;
                Debug.Log($"[InvisibleMonster] 状態: Stunned ({stunDuration}s)");
                break;
        }
    }

    // ───────────────────────────────────────────
    // 状態更新
    // ───────────────────────────────────────────
    private void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // 待機中はタイマーカウント
        if (_isWaitingAtWaypoint)
        {
            _waypointWaitTimer -= Time.deltaTime;
            if (_waypointWaitTimer <= 0f)
            {
                _isWaitingAtWaypoint = false;
                _agent.isStopped = false;
                _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
                MoveToCurrentWaypoint();
            }
            return;
        }

        // 到着判定 → 待機開始
        if (!_agent.isStopped && !_agent.pathPending && _agent.remainingDistance <= waypointStopDistance)
        {
            _isWaitingAtWaypoint = true;
            _waypointWaitTimer = waypointWaitTime;
            _agent.isStopped = true;
        }
    }

    private void UpdateChaseSound()
    {
        if (!_agent.pathPending && _agent.remainingDistance <= chaseStopDistance)
            EnterState(State.Searching);
    }

    /// <summary>
    /// 視野内: プレイヤーをリアルタイム追跡。
    /// 視野外: loseSightGrace 秒だけ追跡継続、過ぎたら最後の位置 → Searching。
    /// </summary>
    private void UpdateChasePlayer()
    {
        if (playerTransform == null)
        {
            EnterState(State.Searching);
            return;
        }

        if (_isPlayerInSight)
        {
            _chaseLostDestSet = false;
            _loseSightTimer = 0f;

            // プレイヤーが一定距離以上動いた時だけ目的地更新
            float distFromDest = Vector3.Distance(_agent.destination, playerTransform.position);
            if (distFromDest > 0.5f)
                _agent.SetDestination(playerTransform.position);
        }
        else
        {
            // 視野喪失の猶予時間中はプレイヤーを追い続ける (一瞬の遮蔽に対応)
            _loseSightTimer += Time.deltaTime;

            if (_loseSightTimer < loseSightGrace)
            {
                _lastSeenPosition = playerTransform.position;
                _agent.SetDestination(playerTransform.position);
                return;
            }

            // 猶予終了 → 最後に見た位置を1回だけ設定
            if (!_chaseLostDestSet)
            {
                Vector3 destination = _lastSeenPosition;

                // 近すぎる場合は逃走方向に予測地点
                float distToLastSeen = Vector3.Distance(transform.position, _lastSeenPosition);
                if (distToLastSeen <= chaseStopDistance * 2f)
                {
                    Vector3 escapeDir = (playerTransform.position - transform.position).normalized;
                    Vector3 predicted = transform.position + escapeDir * searchRadius;

                    if (NavMesh.SamplePosition(predicted, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
                    {
                        destination = hit.position;
                        _lastSeenPosition = destination;
                        Debug.Log($"[InvisibleMonster] 近距離逃走検知 → 予測地点 {destination}");
                    }
                }

                _agent.SetDestination(destination);
                _chaseLostDestSet = true;
            }

            // 最後の位置に到達したら Searching へ
            if (!_agent.pathPending && _agent.remainingDistance <= chaseStopDistance)
            {
                Debug.Log("[InvisibleMonster] 最後の位置に到達 → Searching");
                EnterState(State.Searching);
            }
        }

        // 距離が離れすぎたら諦める
        if (Vector3.Distance(transform.position, playerTransform.position) > losePlayerDistance)
        {
            Debug.Log("[InvisibleMonster] 追跡放棄 → Searching");
            EnterState(State.Searching);
        }
    }

    /// <summary>
    /// 最後に見た位置を中心にランダム探索。searchDuration 経過で Patrol に戻る。
    /// </summary>
    private void UpdateSearching()
    {
        _searchTimer -= Time.deltaTime;

        if (_searchTimer <= 0f)
        {
            EnterState(State.Patrol);
            return;
        }

        if (_agent.pathPending)
        {
            _searchWaitingForPath = true;
            return;
        }

        // 経路計算直後の1フレームは remainingDistance が不安定なのでスキップ
        if (_searchWaitingForPath)
        {
            _searchWaitingForPath = false;
            return;
        }

        if (_agent.remainingDistance <= waypointStopDistance)
        {
            SetRandomSearchDestination();
            _searchWaitingForPath = true;
        }
    }

    /// <summary>
    /// スタンタイマーをカウントダウンし、時間切れで Patrol に復帰。
    /// </summary>
    private void UpdateStunned()
    {
        _stunTimer -= Time.deltaTime;

        if (_stunTimer <= 0f)
        {
            Debug.Log("[InvisibleMonster] スタン解除 → Patrol");
            SetStunnedColor(false);
            _agent.isStopped = false;
            EnterState(State.Patrol);
        }
    }

    /// <summary>
    /// スタン時に赤く、解除時に元の色に戻す。
    /// </summary>
    private void SetStunnedColor(bool stunned)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i].material.HasProperty("_Color"))
                _renderers[i].material.color = stunned ? Color.red : _originalColors[i];
        }
    }

    // ───────────────────────────────────────────
    // ヘルパーメソッド
    // ───────────────────────────────────────────
    private void MoveToCurrentWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        _agent.SetDestination(waypoints[_waypointIndex].position);
    }

    private void SetRandomSearchDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * searchRadius + _searchOrigin;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
        else
        {
            _agent.SetDestination(_searchOrigin);
            Debug.Log("[InvisibleMonster] NavMesh サンプリング失敗");
        }
    }

    // ───────────────────────────────────────────
    // 外部呼び出し API
    // ───────────────────────────────────────────

    /// <summary>
    /// 音の発生位置を受け取り ChaseSound に遷移。
    /// スタン中・プレイヤー追跡中は無視。
    /// </summary>
    public void HearSound(Vector3 position)
    {
        if (_currentState == State.ChasePlayer) return;
        if (_currentState == State.Stunned) return;

        _soundPosition = position;
        EnterState(State.ChaseSound);
    }

    /// <summary>
    /// Renderer の表示/非表示を切り替える。
    /// </summary>
    //public void SetVisible(bool visible)
    //{
    //    foreach (Renderer r in _renderers)
    //        r.enabled = visible;
    //}

    /// <summary>
    /// どの状態からでも気絶状態に遷移する。すでにスタン中なら時間延長。
    /// </summary>
    public void OnStunned()
    {
        if (_currentState == State.Stunned)
        {
            _stunTimer = stunDuration;
            Debug.Log("[InvisibleMonster] スタン延長");
            return;
        }

        EnterState(State.Stunned);
    }

    // ───────────────────────────────────────────
    // Gizmos
    // ───────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // ウェイポイント
        if (waypoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                Gizmos.DrawLine(
                    waypoints[i].position,
                    waypoints[(i + 1) % waypoints.Length].position
                );
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // 近距離自動検知範囲
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, closeDetectRadius);

        // 視野角の境界
        float halfAngle = viewAngle * 0.5f;
        Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * transform.forward * viewDistance;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);

        // プレイヤーへのライン
        if (playerTransform != null)
        {
            Vector3 toPlayer = playerTransform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toPlayer);
            bool inAngle = angle <= viewAngle * 0.5f && toPlayer.magnitude <= viewDistance;
            Gizmos.color = inAngle ? Color.red : Color.gray;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }

        // 最後に見た位置 (追跡中で視界外のとき)
        if (_currentState == State.ChasePlayer && !_isPlayerInSight)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_lastSeenPosition, 0.4f);
            Gizmos.DrawLine(transform.position, _lastSeenPosition);
        }

        // スタン中
        if (_currentState == State.Stunned)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }

        // 足音ノイズ半径
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, footstepNoiseRadius);
    }
#endif
}