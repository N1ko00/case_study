//using UnityEngine;
//using UnityEngine.InputSystem;

//public class DoorSwitch : MonoBehaviour
//{
//    [SerializeField] private AutoDoor targetDoor;
//    [SerializeField] private Transform player;
//    [SerializeField] private float activeDistance = 1f;

//    [Header("アニメーション設定")]
//    [SerializeField] private Animator modelAnimator;
//    [SerializeField] private string animationTriggerName = "PlayAnimation";

//    [Header("マテリアル差し替えの設定")]
//    [SerializeField] private MeshRenderer cylinder36Renderer;
//    [SerializeField] private MeshRenderer cylinder37Renderer;

//    // ★ ここに「ONになったときにはめ込みたいマテリアル」を登録します
//    [SerializeField] private Material onMaterial;

//    private bool isOn = false;

//    void Start()
//    {
//    }

//    void Update()
//    {
//        float distance = Vector3.Distance(transform.position, player.position);

//        if (distance <= activeDistance &&
//            Keyboard.current.eKey.wasPressedThisFrame &&
//            !isOn)
//        {
//            isOn = true;

//            targetDoor.SetUnlocked(true);

//            // アニメーション再生
//            if (modelAnimator != null)
//            {
//                modelAnimator.SetTrigger(animationTriggerName);
//            }

//            // ★ スイッチON時にマテリアルをはめ込む（差し替える）
//            ChangeCylinderMaterial();

//            Debug.Log("スイッチON (アニメーション再生 & マテリアル差し替え)");
//        }
//    }

//    // マテリアルを差し替えるメソッド
//    private void ChangeCylinderMaterial()
//    {
//        if (onMaterial == null)
//        {
//            Debug.LogWarning("ON用のマテリアル(On Material)が設定されていません。");
//            return;
//        }

//        // .material に新しいマテリアルを直接代入してはめ込みます
//        if (cylinder36Renderer != null)
//        {
//            cylinder36Renderer.material = onMaterial;
//        }

//        if (cylinder37Renderer != null)
//        {
//            cylinder37Renderer.material = onMaterial;
//        }
//    }
//}

using UnityEngine;
using UnityEngine.InputSystem;

public class DoorSwitch : MonoBehaviour
{
    [SerializeField] private AutoDoor targetDoor;
    [SerializeField] private Transform player;
    [SerializeField] private float activeDistance = 1f;

    [Header("アニメーション設定")]
    [SerializeField] private Animator modelAnimator;
    [SerializeField] private string animationTriggerName = "PlayAnimation";

    [Header("マテリアル差し替えの設定")]
    [SerializeField] private MeshRenderer cylinder36Renderer;
    [SerializeField] private MeshRenderer cylinder37Renderer;

    // ★ ここに「ゲーム開始時（OFF）のマテリアル」を登録します
    [SerializeField] private Material offMaterial;

    // ★ ここに「スイッチONになったときのマテリアル」を登録します
    [SerializeField] private Material onMaterial;

    private bool isOn = false;

    void Start()
    {
        // ★ ゲーム開始時に、シリンダーをOFF用のマテリアルにする
        SetCylinderMaterial(offMaterial);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= activeDistance &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !isOn)
        {
            isOn = true;

            targetDoor.SetUnlocked(true);

            // アニメーション再生
            if (modelAnimator != null)
            {
                modelAnimator.SetTrigger(animationTriggerName);
            }

            // ★ スイッチON時にマテリアルをON用に差し替える
            SetCylinderMaterial(onMaterial);

            Debug.Log("スイッチON (アニメーション再生 & マテリアルONに差し替え)");
        }
    }

    // マテリアルを一括で適用するメソッド
    private void SetCylinderMaterial(Material targetMaterial)
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("マテリアルが設定されていません。");
            return;
        }

        if (cylinder36Renderer != null)
        {
            cylinder36Renderer.material = targetMaterial;
        }

        if (cylinder37Renderer != null)
        {
            cylinder37Renderer.material = targetMaterial;
        }
    }
}