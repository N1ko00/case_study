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
        Stunned     // ← 追加: 気絶状態
    }

    // ───────────────────────────────────────────
    // Inspector
    // ───────────────────────────────────────────
    [Header("Patrol 設定")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;              // 巡回速度
    [SerializeField] private float waypointStopDistance = 0.5f;   // ウェイポイントに到達とみなす距離
    [SerializeField] private float waypointWaitTime = 1.0f;       // ウェイポイントでの待機時間

    [Header("ChaseSound 設定")]
    [SerializeField] private float chaseSpeed = 4f;             // 音を追跡する速度
    [SerializeField] private float chaseStopDistance = 0.5f;    // 音の位置に到達とみなす距離

    [Header("ChasePlayer 設定")]
    [SerializeField] private float chasePlayerSpeed = 5f;       // プレイヤー追跡速度
    [SerializeField] private float losePlayerDistance = 20f;    // viewDistanceより大きく設定（重要）

    [Header("Searching 設定")]
    [SerializeField] private float searchDuration = 5f;         // 探す時間
    [SerializeField] private float searchRadius = 4f;           // 最後に見た位置を中心にランダムに移動する半径
    [SerializeField] private float searchSpeed = 2f;            // 探索中の移動速度

    [Header("Stunned 設定")]
    [SerializeField] private float stunDuration = 3f;           // 気絶の持続時間（秒）

    [Header("FOV 視野設定")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float viewDistance = 10f;
    [SerializeField][Range(0f, 360f)] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;

    // ───────────────────────────────────────────
    // 内部変数
    // ───────────────────────────────────────────
    private NavMeshAgent _agent;
    private State _currentState;

    // Patrol
    private int _waypointIndex = 0;
    // Patrol 待機タイマー
    private float _waypointWaitTimer = 0f;
    private bool _isWaitingAtWaypoint = false;

    // ChaseSound
    private Vector3 _soundPosition;

    // Searching
    private float _searchTimer = 0f;

    // ChasePlayer
    private bool _isPlayerInSight = false;      // 現在のフレームが視界内にあるかどうか
    private Vector3 _lastSeenPosition;          // 最後に視界から見た位置

    // Stunned
    private float _stunTimer = 0f;              // 残りスタン時間

    // ───────────────────────────────────────────
    // Unity ライフサイクル
    // ───────────────────────────────────────────
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning("[InvisibleMonster] Playerタグを持つオブジェクトが見つかりませんでした");
        }

        EnterState(State.Patrol);
    }

    private void Update()
    {
        // スタン中はFOVチェックと状態更新を完全にスキップ
        if (_currentState == State.Stunned)
        {
            UpdateStunned();
            return;
        }

        CheckFieldOfView();

        switch (_currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.ChaseSound: UpdateChaseSound(); break;
            case State.ChasePlayer: UpdateChasePlayer(); break;
            case State.Searching: UpdateSearching(); break;
        }
    }

    // ───────────────────────────────────────────
    // FOV 視野監視
    // ───────────────────────────────────────────

    /// <summary>
    /// 毎フレーム、視界内にいるかどうかを判断します。
    /// - _isPlayerInSight : 今回のフレームが視界内にあるかどうか
    /// - _lastSeenPosition: 視界内にいる間は常に更新
    /// </summary>
    private void CheckFieldOfView()
    {
        if (playerTransform == null) return;

        Vector3 toPlayer = playerTransform.position - transform.position;

        // 距離チェック
        if (toPlayer.magnitude > viewDistance)
        {
            _isPlayerInSight = false;
            return;
        }

        // 角度チェック
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > viewAngle * 0.5f)
        {
            _isPlayerInSight = false;
            return;
        }

        // Raycast 障害物チェック
        if (Physics.Raycast(transform.position, toPlayer.normalized, out RaycastHit hit, viewDistance, obstacleMask))
        {
            Debug.DrawRay(transform.position, toPlayer.normalized * hit.distance, Color.red);
            _isPlayerInSight = false;
            return;
        }

        // すべての条件をクリア → 視界内にいる
        Debug.DrawRay(transform.position, toPlayer.normalized * viewDistance, Color.green);
        _isPlayerInSight = true;
        _lastSeenPosition = playerTransform.position; // 見るたびに最後の位置を更新

        // 他の状態で発見した場合、ChasePlayerに切り替える
        if (_currentState != State.ChasePlayer)
            EnterState(State.ChasePlayer);
    }

    // ───────────────────────────────────────────
    // 状態遷移
    // ───────────────────────────────────────────
    private void EnterState(State newState)
    {
        // 別の状態に切り替えると待機状態がリセットされる
        _isWaitingAtWaypoint = false;

        _currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                // 移動を再開してウェイポイントへ向かう
                _agent.isStopped = false;
                _agent.speed = patrolSpeed;
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
                Debug.Log("[InvisibleMonster] 状態: ChasePlayer → プレイヤー発見");
                break;

            case State.Searching:
                _agent.isStopped = false;
                _agent.speed = searchSpeed;
                _searchTimer = searchDuration;
                // 最後に見た位置にまず移動
                _agent.SetDestination(_lastSeenPosition);
                Debug.Log("[InvisibleMonster] 状態: Searching → 最後の位置に移動");
                break;

            case State.Stunned:
                // 移動を完全停止
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;  // 残留速度もリセット
                _stunTimer = stunDuration;
                Debug.Log($"[InvisibleMonster] 状態: Stunned → {stunDuration}秒間気絶");
                break;
        }
    }

    // ───────────────────────────────────────────
    // 状態更新
    // ───────────────────────────────────────────
    private void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // 待機中はタイマーでカウント
        if (_isWaitingAtWaypoint)
        {
            _waypointWaitTimer -= Time.deltaTime;
            if (_waypointWaitTimer <= 0f)
            {
                // 待機終了 → 次のウェイポイントへ移動
                _isWaitingAtWaypoint = false;
                _agent.isStopped = false;          //  移動再開を先に行う
                _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
                MoveToCurrentWaypoint();
            }
            return;                                //  待機中は到着判定をスキップ
        }

        // ウェイポイント到着判定 (isStopped=falseのときのみ実行)
        if (!_agent.isStopped && !_agent.pathPending && _agent.remainingDistance <= waypointStopDistance)
        {
            // 待機開始
            _isWaitingAtWaypoint = true;
            _waypointWaitTimer = waypointWaitTime;
            _agent.isStopped = true;
        }
    }

    private void UpdateChaseSound()
    {
        if (!_agent.pathPending && _agent.remainingDistance <= chaseStopDistance)
        {
            EnterState(State.Searching);
        }
    }

    /// <summary>
    /// 視野内：プレイヤーのリアルタイム追跡
    /// 視界外：最後に見た位置(_lastSeenPosition)に移動してからSearchingに切り替え
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
            // 視界内 → プレイヤーのリアルタイム追跡
            _agent.SetDestination(playerTransform.position);
        }
        else
        {
            // 視界外 → 最後に見た位置まで移動
            _agent.SetDestination(_lastSeenPosition);

            // 最後の位置に到達したらSearchingに切り替え
            if (!_agent.pathPending && _agent.remainingDistance <= chaseStopDistance)
            {
                Debug.Log("[InvisibleMonster] 最後の位置に到達 → Searching");
                EnterState(State.Searching);
            }
        }

        // あまりにも遠くなると諦める
        if (Vector3.Distance(transform.position, playerTransform.position) > losePlayerDistance)
        {
            Debug.Log("[InvisibleMonster] 追跡放棄 距離超過 → Searching");
            EnterState(State.Searching);
        }
    }

    /// <summary>
    /// 最後に見た位置に到達した後、周囲をランダムに探索。
    /// searchDuration が終了したら Patrol に戻る。
    /// </summary>
    private void UpdateSearching()
    {
        _searchTimer -= Time.deltaTime;

        if (_searchTimer <= 0f)
        {
            EnterState(State.Patrol);
            return;
        }

        // 最後の位置に到達した後、周囲をランダムに探索する
        if (!_agent.pathPending && _agent.remainingDistance <= waypointStopDistance)
        {
            SetRandomSearchDestination();
        }
    }

    /// <summary>
    /// スタンタイマーをカウントダウンし、時間が切れたらPatrolに復帰します。
    /// スタン中はFOVチェックも状態遷移も行いません。
    /// </summary>
    private void UpdateStunned()
    {
        _stunTimer -= Time.deltaTime;

        if (_stunTimer <= 0f)
        {
            Debug.Log("[InvisibleMonster] スタン解除 → Patrol に復帰");
            // 移動を再開してPatrolへ
            _agent.isStopped = false;
            EnterState(State.Patrol);
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
        Vector3 randomDirection = Random.insideUnitSphere * searchRadius + transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
    }

    // ───────────────────────────────────────────
    // 外部呼び出し API
    // ───────────────────────────────────────────

    /// <summary>
    /// 音が発生した位置を受け取り、ChaseSound状態に切り替えます。
    /// スタン中・プレイヤー追跡中は無視します。
    /// </summary>
    public void HearSound(Vector3 position)
    {
        // スタン中・プレイヤー追跡中は音を無視
        if (_currentState == State.ChasePlayer) return;
        if (_currentState == State.Stunned) return;

        _soundPosition = position;
        EnterState(State.ChaseSound);
    }

    /// <summary>
    /// 外部から呼び出すことで、どの状態からでも気絶状態に遷移します。
    /// 例: プレイヤーのアイテム使用、トラップ発動など
    /// </summary>
    public void OnStunned()
    {
        // すでにスタン中なら stunTimer をリセットするだけ（スタック可能）
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

        float halfAngle = viewAngle * 0.5f;
        Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * transform.forward * viewDistance;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);

        if (playerTransform != null)
        {
            Vector3 toPlayer = playerTransform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toPlayer);
            bool inAngle = angle <= viewAngle * 0.5f && toPlayer.magnitude <= viewDistance;
            Gizmos.color = inAngle ? Color.red : Color.gray;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }

        // 最後に見た位置の視覚化（追跡中に視界外のときに表示）
        if (_currentState == State.ChasePlayer && !_isPlayerInSight)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_lastSeenPosition, 0.4f);
            Gizmos.DrawLine(transform.position, _lastSeenPosition);
        }

        // スタン中はオレンジ色の円で表示
        if (_currentState == State.Stunned)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
#endif
}