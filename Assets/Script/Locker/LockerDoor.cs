using UnityEngine;
using UnityEngine.InputSystem;

// ロッカーのドア開閉
// 操作範囲(interactZone)と隠れゾーン(hideZone)はトリガー分けてる
// 閉まってる時に hideZone 内にプレイヤーがいたら Hidden レイヤーに切り替え
public class LockerDoor : MonoBehaviour
{
    [Header("回転対象 ")]
    [SerializeField] private Transform doorPivot;

    [Header("回転設定")]
    [SerializeField] private float openAngle = -110f;   // 開いた時の角度
    [SerializeField] private float openSpeed = 200f;    // 回るスピード

    [Header("操作設定")]
    [SerializeField] private Key interactKey = Key.E;
    [Tooltip("操作できる範囲。ロッカー前あたりにちょっと広めで")]
    [SerializeField] private LockerTriggerRelay interactZone;
    [Tooltip("隠れる範囲。ロッカーの中にぴったり収まるサイズで")]
    [SerializeField] private LockerTriggerRelay hideZone;

    [Header("隠れ設定")]
    [Tooltip("Project Settings 側で先に作っておく")]
    [SerializeField] private string hiddenLayerName = "Hidden";

    [Header("音設定")]
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;
    [SerializeField] private float noiseRadius = 6f;    // モンスターに聞こえる範囲(一応、作っておく)

    private AudioSource _audio;
    private bool _isOpen = false;
    private bool _playerInInteract = false;   // 操作範囲内にいるか
    private bool _playerInHide = false;       // 隠れゾーン内にいるか
    private Quaternion _closedRot;
    private Quaternion _openRot;

    // 隠れ処理用
    private GameObject _playerObj;
    private int _playerOriginalLayer;
    private int _hiddenLayer;
    private bool _isPlayerHidden = false;

    private void Awake()
    {
        // pivot 入れ忘れ防止
        if (doorPivot == null)
        {
            Debug.LogError("[LockerDoor] doorPivot 入れて", this);
            enabled = false;
            return;
        }

        // 閉じた状態を基準にして、そこから openAngle 回した角度を「開いた状態」にする
        _closedRot = doorPivot.localRotation;
        _openRot = _closedRot * Quaternion.Euler(0f, openAngle, 0f);

        // AudioSource なかったら勝手に付ける
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.spatialBlend = 1f; // 3D で鳴らす

        // Hidden レイヤー取得 (なかったらエラー)
        _hiddenLayer = LayerMask.NameToLayer(hiddenLayerName);
        if (_hiddenLayer < 0)
            Debug.LogError($"[LockerDoor] '{hiddenLayerName}' レイヤー作ってない", this);

        // 子トリガーのイベントを拾う
        if (interactZone != null)
        {
            interactZone.OnEnter = c => { if (c.CompareTag("Player")) _playerInInteract = true; };
            interactZone.OnExit = c => { if (c.CompareTag("Player")) _playerInInteract = false; };
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
                // 出る前に隠れ解除しとかないとレイヤー戻らない
                if (_isPlayerHidden) SetPlayerHidden(false);
                _playerObj = null;
            };
        }
    }

    private void Update()
    {
        // 操作範囲内 + E で開閉
        if (_playerInInteract
            && Keyboard.current != null
            && Keyboard.current[interactKey].wasPressedThisFrame)
        {
            Toggle();
        }

        // 目標角度に向けて少しずつ回す
        Quaternion target = _isOpen ? _openRot : _closedRot;
        doorPivot.localRotation = Quaternion.RotateTowards(
            doorPivot.localRotation,
            target,
            openSpeed * Time.deltaTime
        );

        // 閉まってて中にいる時だけ隠れる
        bool shouldHide = !_isOpen && _playerInHide;
        if (shouldHide && !_isPlayerHidden) SetPlayerHidden(true);
        else if (!shouldHide && _isPlayerHidden) SetPlayerHidden(false);
    }

    // プレイヤーのレイヤーを Hidden に切り替え (戻す時は元のレイヤーに)
    private void SetPlayerHidden(bool hide)
    {
        if (_playerObj == null || _hiddenLayer < 0) return;

        if (hide)
        {
            _playerOriginalLayer = _playerObj.layer;
            SetLayerRecursive(_playerObj, _hiddenLayer);
            _isPlayerHidden = true;
            Debug.Log("[LockerDoor] 隠れた");
        }
        else
        {
            SetLayerRecursive(_playerObj, _playerOriginalLayer);
            _isPlayerHidden = false;
            Debug.Log("[LockerDoor] 隠れ解除");
        }
    }

    // 子供までまとめてレイヤー変更
    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    // 開いてたら閉じる、閉じてたら開く
    public void Toggle()
    {
        _isOpen = !_isOpen;

        AudioClip clip = _isOpen ? openSE : closeSE;
        if (clip != null) _audio.PlayOneShot(clip);

        // ドアの音でモンスター呼ぶ
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitNoise(
                transform.position,
                noiseRadius,
                NoiseSourceType.Player
            );
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 音の届く範囲だけ表示。トリガー範囲は子の BoxCollider 側で見る
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
#endif
}