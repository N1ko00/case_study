using UnityEngine;
using UnityEngine.InputSystem;

// 깓긞긇?궻긤귺둎빧
// ?띿붝댪(interactZone)궴덨귢??깛(hideZone)궼긣깏긊?빁궚궲귡
// 빧귏궯궲귡렄궸 hideZone 볙궸긵깒귽깂?궕궋궫귞 Hidden 깒귽깂?궸먛귟뫶궑
public class LockerDoor : MonoBehaviour
{
    [Header("됷?뫮뤭 ")]
    [SerializeField] private Transform doorPivot;

    [Header("됷?먠믦")]
    [SerializeField] private float openAngle = -110f;   // 둎궋궫렄궻둷뱗
    [SerializeField] private float openSpeed = 200f;    // 됷귡긚긯?긤

    [Header("?띿먠믦")]
    [SerializeField] private Key interactKey = Key.E;
    [Tooltip("?띿궳궖귡붝댪갃깓긞긇?멟궇궫귟궸궭귛궯궴뛎귕궳")]
    [SerializeField] private LockerTriggerRelay interactZone;
    [Tooltip("덨귢귡붝댪갃깓긞긇?궻뭷궸귃궯궫귟뢁귏귡긖귽긛궳")]
    [SerializeField] private LockerTriggerRelay hideZone;

    [Header("덨귢먠믦")]
    [Tooltip("Project Settings 뫀궳먩궸띿궯궲궓궘")]
    [SerializeField] private string hiddenLayerName = "Hidden";

    [Header("돶먠믦")]
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;
    [SerializeField] private float noiseRadius = 6f;    // 긾깛긚??궸빓궞궑귡붝댪(덇돒갂띿궯궲궓궘)

    [Header("Door Collider먠믦")]
    [SerializeField] private MeshCollider doorMeshCollider;

    private AudioSource _audio;
    private bool _isOpen = false;
    private bool _isMoving = false;           // 긤귺궕됷?뭷궔 
    private bool _playerInInteract = false;   // ?띿붝댪볙궸궋귡궔
    private bool _playerInHide = false;       // 덨귢??깛볙궸궋귡궔
    private Quaternion _closedRot;
    private Quaternion _openRot;

    // 덨귢룉뿚뾭
    private GameObject _playerObj;
    private int _playerOriginalLayer;
    private int _hiddenLayer;
    private bool _isPlayerHidden = false;

    private void Awake()
    {
        // pivot 볺귢뻒귢뻞?
        if (doorPivot == null)
        {
            Debug.LogError("[LockerDoor] doorPivot 볺귢궲", this);
            enabled = false;
            return;
        }

        // 빧궣궫륉뫴귩딈?궸궢궲갂궩궞궔귞 openAngle 됷궢궫둷뱗귩걏둎궋궫륉뫴걐궸궥귡
        _closedRot = doorPivot.localRotation;
        _openRot = _closedRot * Quaternion.Euler(0f, openAngle, 0f);

        // AudioSource 궶궔궯궫귞룦롨궸븊궚귡
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.spatialBlend = 1f; // 3D 궳뼿귞궥

        // Hidden 깒귽깂?롦벦 (궶궔궯궫귞긄깋?)
        _hiddenLayer = LayerMask.NameToLayer(hiddenLayerName);
        if (_hiddenLayer < 0)
            Debug.LogError($"[LockerDoor] '{hiddenLayerName}' 깒귽깂?띿궯궲궶궋", this);

        // MeshCollider 궕뼟먠믦궶귞 doorPivot 댥돷궔귞렔벍롦벦
        if (doorMeshCollider == null)
            doorMeshCollider = doorPivot.GetComponentInChildren<MeshCollider>();

        if (doorMeshCollider == null)
            Debug.LogWarning("[LockerDoor] doorPivot 댥돷궸 MeshCollider 궕뙥궰궔귞궶궋", this);

        // 럔긣깏긊?궻귽긹깛긣귩뢉궎
        if (interactZone != null)
        {
            interactZone.OnEnter = c =>
            {
                if (!c.CompareTag("Player")) return;
                _playerInInteract = true;

                //「Press R2」表示
                if (InteractPromptUI.Instance != null)
                    InteractPromptUI.Instance.Show(this, "Press R2");
            };
            interactZone.OnExit = c =>
            {
                if (!c.CompareTag("Player")) return;
                _playerInInteract = false;

                // 表示をオフ
                if (InteractPromptUI.Instance != null)
                    InteractPromptUI.Instance.Hide(this);
            };
        }
        if (hideZone != null)
        {
            hideZone.OnEnter = c =>
            {
                if (!c.CompareTag("Player")) return;
                _playerInHide = true;
                _playerObj = c.gameObject;
            };
            hideZone.OnExit = c =>
            {
                if (!c.CompareTag("Player")) return;
                _playerInHide = false;
                // 뢯귡멟궸덨귢됶룣궢궴궔궶궋궴깒귽깂?뽣귞궶궋
                if (_isPlayerHidden) SetPlayerHidden(false);
                _playerObj = null;
            };
        }
    }

    private void Update()
    {
        // ?띿붝댪볙 + E (キーボード) または R2 (ゲームパッド右トリガー) で開閉
        bool keyboardPressed = Keyboard.current != null
            && Keyboard.current[interactKey].wasPressedThisFrame;
        bool gamepadPressed = Gamepad.current != null
            && Gamepad.current.rightTrigger.wasPressedThisFrame;

        if (_playerInInteract && (keyboardPressed || gamepadPressed))
        {
            Toggle();
        }

        Quaternion target = _isOpen ? _openRot : _closedRot;

        if (_isMoving)
        {
            doorPivot.localRotation = Quaternion.RotateTowards(
                doorPivot.localRotation,
                target,
                openSpeed * Time.deltaTime
            );

            // 뽞뷭둷뱗궸뱸묪궢궫귞긓깋귽??귩뽣궥
            if (Quaternion.Angle(doorPivot.localRotation, target) < 0.1f)
            {
                doorPivot.localRotation = target;
                _isMoving = false;
                SetDoorCollider(true);
            }
        }

        // 빧귏궯궲궲뭷궸궋귡렄궬궚덨귢귡
        bool shouldHide = !_isOpen && _playerInHide;
        if (shouldHide && !_isPlayerHidden) SetPlayerHidden(true);
        else if (!shouldHide && _isPlayerHidden) SetPlayerHidden(false);
    }

    private void SetDoorCollider(bool enabled)
    {
        if (doorMeshCollider != null)
        {
            doorMeshCollider.enabled = enabled;
        }
    }

    // 긵깒귽깂?궻깒귽깂?귩 Hidden 궸먛귟뫶궑 (뽣궥렄궼뙰궻깒귽깂?궸)
    private void SetPlayerHidden(bool hide)
    {
        if (_playerObj == null || _hiddenLayer < 0) return;

        if (hide)
        {
            _playerOriginalLayer = _playerObj.layer;
            SetLayerRecursive(_playerObj, _hiddenLayer);
            _isPlayerHidden = true;
            Debug.Log("[LockerDoor] 덨귢궫");
        }
        else
        {
            SetLayerRecursive(_playerObj, _playerOriginalLayer);
            _isPlayerHidden = false;
            Debug.Log("[LockerDoor] 덨귢됶룣");
        }
    }

    // 럔떉귏궳귏궴귕궲깒귽깂?빾뛛
    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    // 둎궋궲궫귞빧궣귡갂빧궣궲궫귞둎궘
    public void Toggle()
    {
        _isOpen = !_isOpen;
        _isMoving = true;

        SetDoorCollider(false);

        AudioClip clip = _isOpen ? openSE : closeSE;
        if (clip != null) _audio.PlayOneShot(clip);

        // 긤귺궻돶궳긾깛긚??뚁귆
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitNoise(
                transform.position,
                noiseRadius,
                NoiseSourceType.Player
            );
        }
    }

    private void OnDisable()
    {
        // Lockerが無効化/破棄された際にヒントUIが残らないようにする
        if (InteractPromptUI.Instance != null)
            InteractPromptUI.Instance.Hide(this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 돶궻벾궘붝댪궬궚?렑갃긣깏긊?붝댪궼럔궻 BoxCollider 뫀궳뙥귡
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
#endif
}