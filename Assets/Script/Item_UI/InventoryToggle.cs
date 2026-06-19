//using UnityEngine;
//using UnityEngine.InputSystem;

//public class InventoryToggle : MonoBehaviour
//{
//    public GameObject inventoryUI;

//    void Update()
//    {
//        if (Keyboard.current.tabKey.wasPressedThisFrame)
//        {
//            bool isOpen = !inventoryUI.activeSelf;
//            inventoryUI.SetActive(isOpen);

//            // 궞궞궕뢣뾴
//            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
//            Cursor.visible = isOpen;
//        }
//    }
//}

using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    // 둖븫궻긚긏깏긵긣궔귞뚁귂뢯궧귡귝궎궸긘깛긐깑긣깛돸
    public static InventoryToggle Instance { get; private set; }

    [Header("귽깛긹깛긣깏?뫬걁InventoryPanel_1걂귩뱋?")]
    public GameObject inventoryUI;

    [Header("덇룒궸뢯궢궫궋봶똧걁inventorybackground걂귩뱋?")]
    public GameObject inventoryBG;

    [Header("긇긽깋먛뫶궻랷뤖")]
    [SerializeField] private CameraSwitcher cameraSwitcher;

    [Header("キーパッド参照")]
    public GameObject keypadUI; // ← KeyPadTriggerと同じGameObjectを割り当て

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 긒??둎럑렄궼?뫬귖봶똧귖둴렳궸뤑궢궲궓궘
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (inventoryBG != null) inventoryBG.SetActive(false);

        // 렔벍뙚랊
        if (cameraSwitcher == null)
        {
            cameraSwitcher = FindAnyObjectByType<CameraSwitcher>(FindObjectsInactive.Include);
        }
    }

    void Update()
    {
        // Tab긌?궕돓궠귢궫뢷듩귩뙚뭢
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            // ★ 追加：キーパッドが表示中は操作を止める
            if (keypadUI != null && keypadUI.activeSelf)
                return;

            if (UIInventory.Instance != null && UIInventory.Instance.IsOpen)
                return;
            if (inventoryUI == null) return;

            // 긒??긆?긫?뭷궼귽깛긹깛긣깏귩둎궔궧궶궋
            if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameOver)
                return;

            // ポーズ中はインベントリを開かない
            if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsOpen)
                return;

            // 듒럨긇긽깋?렑뭷궼귽깛긹깛긣깏귩둎궔궧궶궋
            if (cameraSwitcher != null && cameraSwitcher.CurrentCameraIndex != 0)
                return;

            // 뙸띪궻?렑륉뫴귩뵿?
            bool isOpen = !inventoryUI.activeSelf;

            if (isOpen)
            {
                OpenInventory();
            }
            else
            {
                CloseInventory();
            }
        }
    }

    public void OpenInventory()
    {
        if (inventoryUI != null) inventoryUI.SetActive(true);
        if (inventoryBG != null) inventoryBG.SetActive(true);

        // ?긂긚긇??깑귩?렑궢궲벍궔궧귡귝궎궸궥귡
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 긚깓긞긣귩띍륷궸뛛륷
        if (UIInventory.Instance != null)
        {
            UIInventory.Instance.Refresh();
        }
    }

    public void CloseInventory()
    {
        if (inventoryUI != null) inventoryUI.SetActive(false);
        if (inventoryBG != null) inventoryBG.SetActive(false);

        // ?긂긚긇??깑귩뷄?렑궸궢궲깓긞긏궥귡
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("귽깛긹깛긣깏궴봶똧귩뒶멣궸빧궣귏궢궫");
    }
}