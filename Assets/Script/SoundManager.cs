using System;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    public enum NoiseSourceType
    {
        Player,
        Enemy
    }


    public static SoundManager Instance;

    [Header("Layer設定")]
    [SerializeField] private LayerMask wallLayer; // 壁レイヤー
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioClip footstepSE;

    private void Awake()
    {
        Instance = this;
    }

    //音発生関数
    public void EmitNoise(Vector3 soundPos, float radius, NoiseSourceType sourceType)
    {
        Debug.Log($"EmitNoise: pos={soundPos}, radius={radius}, source={sourceType}");

        if (sourceType == NoiseSourceType.Player)
        {
            // 🔹 Player → Enemy に通知
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(soundPos, enemy.transform.position);

                Debug.Log($"[Player->Enemy] distance to {enemy.name} = {distance}");

                if (distance <= radius)
                {
                    // 壁チェック
                    Vector3 dir = (enemy.transform.position - soundPos).normalized;

                    if (Physics.Raycast(soundPos, dir, out RaycastHit hit, distance, wallLayer))
                    {
                        Debug.Log($"[WallCheck] Enemyとの間に壁あり: {hit.collider.name}");
                        continue;
                    }
                    else
                    {
                        Debug.Log("[WallCheck] 壁なし → Enemyに通知");

                        // TODO: ここでEnemyに通知する

                        Debug.Log("※ここにEnemy通知処理を入れる");
                        PlaySEAtPosition(soundPos);
                    }
                }
            }
        }
        else if (sourceType == NoiseSourceType.Enemy)
        {
            // 🔹 Enemy → Player にSE再生
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null) return;

            float distance = Vector3.Distance(soundPos, player.transform.position);

            Debug.Log($"[Enemy->Player] distance = {distance}");

            if (distance <= radius)
            {
                // 壁チェック
                Vector3 dir = (player.transform.position - soundPos).normalized;

                if (Physics.Raycast(soundPos, dir, out RaycastHit hit, distance, wallLayer))
                {
                    Debug.Log($"[WallCheck] Playerとの間に壁あり: {hit.collider.name}");
                    return;
                }
                else
                {
                    Debug.Log("[WallCheck] 壁なし → SE再生");

                    //SE再生
                    PlaySEAtPosition(soundPos);
                }
            }
        }
    }

    private void PlaySEAtPosition(Vector3 pos)
    {
        Debug.Log($"SE再生 at {pos}");

        AudioSource temp = Instantiate(seSource, pos, Quaternion.identity);
        temp.PlayOneShot(footstepSE);
        Destroy(temp.gameObject, footstepSE.length);

    }
}