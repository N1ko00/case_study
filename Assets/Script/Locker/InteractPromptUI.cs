using TMPro;
using UnityEngine;

/// <summary>
/// プレイヤーがインタラクト範囲に入った時に「Press E」などのヒントを表示するUI。
/// シングルトンでどこからでもShow/Hideを呼べる。
/// </summary>
public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    [Header("UI参照")]
    [Tooltip("表示・非表示の対象になるルートGameObject")]
    [SerializeField] private GameObject promptRoot;
    [Tooltip("ヒント文を表示するTextMeshPro")]
    [SerializeField] private TMP_Text promptText;

    [Header("デフォルト文")]
    [SerializeField] private string defaultMessage = "Press R2";

    // 現在ヒントを表示している呼び出し元 
    private Object _currentOwner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (promptRoot != null) promptRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(Object owner, string message = null)
    {
        _currentOwner = owner;

        if (promptText != null)
            promptText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;

        if (promptRoot != null) promptRoot.SetActive(true);
    }

    public void Hide(Object owner)
    {
        if (_currentOwner != owner) return;

        _currentOwner = null;
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    public void ForceHide()
    {
        _currentOwner = null;
        if (promptRoot != null) promptRoot.SetActive(false);
    }
}