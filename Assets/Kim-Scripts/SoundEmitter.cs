using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// 特定の位置で音を発生させ、半径内のInvisibleMonsterに知らせます。
/// プレイヤーの足音、ドアの開閉、物を落とすなど、どこからでも呼び出すことができます。
/// </summary>
public class SoundEmitter : MonoBehaviour
{
    // ───────────────────────────────────────────
    // Inspector 
    // ───────────────────────────────────────────
    [Header("音声検出設定")]
    [SerializeField] private float soundRadius = 10f;        // 音が伝わる半径
    [SerializeField] private LayerMask Enemy;          　　　// 敵オブジェクトのレイヤー

    [Header("自動生成設定（テスト用）")]
    [SerializeField] private bool emitOnStart = false;      // スタート時に自動で音が鳴る
    [SerializeField] private Key testKey = Key.F;           // このキーを押すと音が鳴ります（テスト用）

    // ───────────────────────────────────────────
    // Unity 생명주기
    // ───────────────────────────────────────────
    private void Start()
    {
        if (emitOnStart)
            EmitSound(transform.position, soundRadius);
    }

    private void Update()
    {
        // テスト用：キー入力で音を発生させる
        if (Keyboard.current != null && Keyboard.current[testKey].wasPressedThisFrame)
        {
            Debug.Log($"[SoundEmitter] テスト音が発生 at {transform.position}");
            EmitSound(transform.position, soundRadius);
        }
    }

    // ───────────────────────────────────────────
    // コアメソッド
    // ───────────────────────────────────────────

    /// <summary>
    /// 指定した位置で音を発生させます。
    /// Physics.OverlapSphereで半径内の敵を探索し、HearSound()を呼び出します。
    /// </summary>
    /// <param name="soundPosition">音が発生したワールドの位置</param>
    /// <param name="radius">音が伝わる半径</param>
    public void EmitSound(Vector3 soundPosition, float radius)
    {
        Collider[] hits=GetEnemyColliders(soundPosition,radius);
        /*// 半径内のすべてのColliderを探索（enemyLayer適用）
        Collider[] hits = Physics.OverlapSphere(soundPosition, radius, Enemy);
        */

        int notifiedCount = 0;

        foreach (Collider hit in hits)
        {
            // InvisibleMonster コンポーネントの探索
            InvisibleMonster monster = hit.GetComponent<InvisibleMonster>();

            if (monster != null)
            {
                // 音の発生位置を敵に伝える
                monster.HearSound(soundPosition);
                notifiedCount++;
                Debug.Log($"[SoundEmitter] '{hit.name}' に音声を送信完了");
            }
        }

        Debug.Log($"[SoundEmitter] 音発生 → 半径 {radius}m 中の敵 {notifiedCount}名感知");

        // エディタで音の範囲を一時的に可視化
#if UNITY_EDITOR
        _debugPosition = soundPosition;
        _debugRadius = radius;
        _debugTimer = 1f;
#endif
    }

    /// <summary>
    /// Inspector の soundRadius を使って、このオブジェクトの位置で音を発生させます。
    /// 外部スクリプトから最も簡単に呼び出す方法です。
    /// </summary>
    public void EmitSound() => EmitSound(transform.position, soundRadius);

    /// <summary>
    /// カスタム半径でこのオブジェクトの位置から音を発生させます。
    /// </summary>
    public void EmitSound(float customRadius) => EmitSound(transform.position, customRadius);

	/// <summary>
	/// Inspector の soundRadius を使って、このオブジェクト位置から敵をスタンさせます。
	/// </summary>
	public void StunEnemies() => StunEnemies(transform.position, soundRadius);

	/// <summary>
	/// 指定した位置と半径内の InvisibleMonster を重複なくスタンさせます。
	/// Enemy LayerMask が未設定ならレイヤー指定なしで探索します。
	/// </summary>
	public void StunEnemies(Vector3 center, float radius)
	{
		Collider[] hits = GetEnemyColliders(center, radius);
		HashSet<InvisibleMonster> stunnedMonsters = new HashSet<InvisibleMonster>();

		foreach (Collider hit in hits)
		{
			InvisibleMonster monster = hit.GetComponentInParent<InvisibleMonster>();

			if (monster != null)
				stunnedMonsters.Add(monster);
		}

		foreach (InvisibleMonster monster in stunnedMonsters)
			monster.OnStunned();

		Debug.Log($"[SoundEmitter] スタン発生 → 半径 {radius}m 中の敵 {stunnedMonsters.Count}体をスタン");
	}

	private Collider[] GetEnemyColliders(Vector3 center, float radius)
	{
		return Enemy.value == 0
			? Physics.OverlapSphere(center, radius)
			: Physics.OverlapSphere(center, radius, Enemy);
	}
	// ───────────────────────────────────────────
	// Gizmos / Debug 視覚化
	// ───────────────────────────────────────────
#if UNITY_EDITOR
    // 音が発生したときに一瞬表示されるデバッグ用の変数
    private Vector3 _debugPosition;
    private float _debugRadius;
    private float _debugTimer = 0f;

    private void OnDrawGizmosSelected()
    {
        // エディタで常に表示される音の半径 
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f); // オレンジ
        Gizmos.DrawSphere(transform.position, soundRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // オレンジ
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }

    private void OnDrawGizmos()
    {
        // 音が実際に発生したとき、1秒間赤色で強調表示
        if (_debugTimer > 0f)
        {
            _debugTimer -= Time.deltaTime;

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(_debugPosition, _debugRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_debugPosition, _debugRadius);
        }
    }
#endif
}