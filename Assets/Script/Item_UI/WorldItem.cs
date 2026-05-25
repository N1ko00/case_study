//using UnityEngine;

//public class WorldItem : MonoBehaviour
//{
//    public ItemData itemData;
//}

using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;

    // Unity標準のメッシュレンダラーと、元の色を記憶する変数
    private MeshRenderer meshRenderer;
    private Color originalColor;

    void Awake()
    {
        // 自分についているMeshRendererを取得して記憶する
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            // ゲーム開始時のアイテムの元の色を保存しておく
            originalColor = meshRenderer.material.color;
        }
    }

    // Player_Mainから呼ばれる関数（色を切り替えます）
    public void SetHighlight(bool value)
    {
        if (meshRenderer != null)
        {
            if (value)
            {
                // カーソルが当たったら、わかりやすく「黄色」に光らせる
                meshRenderer.material.color = Color.yellow;
            }
            else
            {
                // カーソルが外れたら、元の色に戻す
                meshRenderer.material.color = originalColor;
            }
        }
    }
}