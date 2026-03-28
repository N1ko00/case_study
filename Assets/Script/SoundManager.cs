using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    //音発生関数(音発生座標、音の聞こえる半径、音を鳴らしたobject)
    public void EmitNoise(Vector3 soundPos, float radius, NoiseSourceType sourceType)
    {
        //ここに本実装

        //このデバックログはPlayer,Enemy側の確認用なので本実装の際は消して大丈夫です
        Debug.Log($"EmitNoise: pos={soundPos},radius={radius},source={sourceType}");
    }
}
