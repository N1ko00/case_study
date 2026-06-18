using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LocationUIController : MonoBehaviour
{
    public static LocationUIController Instance;

    [Header("場所名テキスト")]
    [SerializeField] private TextMeshProUGUI locationText;

    [Header("UI全体の親")]
    [SerializeField] private GameObject locationUIRoot;

    [Header("一人称時のみ表示")]
    [SerializeField] private bool isFirstPersonOnly = true;

    [Header("ゲームオーバー中は非表示")]
    [SerializeField] private bool ishideonGameOver = true;

    [Header("プレイヤー操作中のみ表示")]
    [SerializeField] private bool showOnlyWhenPlayerControllable = true;

    [Header("インベントリ中は非表示")]
    [SerializeField] private bool hideWhenInventoryOpen = true;

    [Header("一人称判定用")]
    [SerializeField] private CameraSwitcher cameraSwitcher;

    [Header("ゲームオーバー判定用")]
    [SerializeField] private GameOverManager gameOverManager;

    [Header("プレイヤー操作スクリプト")]
    [SerializeField] private MonoBehaviour playerController;

    private string currentLocationName = "";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshText();
        UpdateVisibility();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    public void SetLocation(string locationName)
    {
        Debug.Log("場所が変更された: " + locationName);
        currentLocationName = locationName;
        RefreshText();
    }

    private void RefreshText()
    {
        if (locationText != null)
        {
            locationText.text = currentLocationName;
        }
    }

    private void UpdateVisibility()
    {
        if (locationUIRoot == null) return;

        bool shouldShow = !string.IsNullOrEmpty(currentLocationName);
        Debug.Log("currentLocationName=" + currentLocationName + " / shouldShow=" + shouldShow);

        if (isFirstPersonOnly && cameraSwitcher != null)
        {
            shouldShow &= (cameraSwitcher.CurrentCameraIndex == 0);
        }

        if (ishideonGameOver && gameOverManager != null)
        {
            shouldShow &= !gameOverManager.IsGameOver;
        }

        if (showOnlyWhenPlayerControllable && playerController != null)
        {
            shouldShow &= playerController.enabled;
        }

        if (hideWhenInventoryOpen && UIInventory.Instance != null)
        {
            shouldShow &= !UIInventory.Instance.IsOpen;
        }

        Debug.Log("UIの表示状態: " + shouldShow);
        locationUIRoot.SetActive(shouldShow);
    }
}
