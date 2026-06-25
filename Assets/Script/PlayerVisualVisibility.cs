using UnityEngine;

//プレイヤーの描画制御用
public class PlayerVisualVisibility : MonoBehaviour
{
    [Header("カメラ切り替え")]
    [SerializeField] private CameraSwitcher cameraSwitcher;

    [Header("見た目モデルの親")]
    [SerializeField] private GameObject visualRoot;

    private Renderer[] cachedRenderer;
    private bool lastVisible = true;

    private void Awake()
    {
        if (visualRoot != null)
        {
            cachedRenderer = visualRoot.GetComponentsInChildren<Renderer>(true);
        }
    }

    private void Start()
    {
        ApplyVisibility();
    }

    private void Update()
    {
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (cameraSwitcher == null || cachedRenderer == null) return;

        //0:一人称 1:その他視点
        bool shouldShow = cameraSwitcher.CurrentCameraIndex != 0;

        if(shouldShow== lastVisible) return;

        lastVisible = shouldShow;

        foreach (var r in cachedRenderer)
        {
            if (r != null)
            {
                r.enabled = shouldShow;
            }
        }
    }
}
