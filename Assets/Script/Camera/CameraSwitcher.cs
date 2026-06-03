using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    [Tooltip("0�Ԗڂ͕K�����C���J�����ɂ��Ă�������")]
    public List<Camera> cameras = new List<Camera>();
    [FormerlySerializedAs("MainCamera")]
    [SerializeField] private Camera mainCamera;
    [FormerlySerializedAs("SubCamera")]
    [SerializeField] private Camera subCamera;
    [FormerlySerializedAs("SubCamera2")]
    [SerializeField] private Camera subCamera2;

    [Header("Monster")]
    [SerializeField] private InvisibleMonster monster;

    [Header("UI")]
    [SerializeField] private GameObject cameraCanvas;

    //停止対象プレイヤー
    [SerializeField] private FPSController player_Main;

    // ���̃R�[�h�ɂ������ϐ�����������c���Ă����܂���
    private bool unique = true;

    public int CurrentCameraIndex { get; private set; } = 0;

    // ���O�Ɍ��Ă����Ď��J�����̔ԍ����o����
    private int lastSubCameraIndex = 1;

    void Start()
    {
        EnsureCamerasInitialized();

        if (cameras.Count > 0)
        {
            SetCameraState(0); 
        }
    }

    private void EnsureCamerasInitialized()
    {
        if (cameras.Count > 0) return;

        AddLegacyCamera(mainCamera);
        AddLegacyCamera(subCamera);
        AddLegacyCamera(subCamera2);
    }

    private void AddLegacyCamera(Camera legacyCamera)
    {
        if (legacyCamera == null || cameras.Contains(legacyCamera)) return;
        cameras.Add(legacyCamera);
    }

    void Update()
    {
        if (unique)
        {
            // monster������ۂ���Ȃ����������s����悤�ɂ��܂���
            if (monster != null)
            {
                monster.SetVisible(false);
            }
            else
            {
                // �G���[���o�����ɁA�x�����b�Z�[�W�����R���\�[���ɂ��m�点���܂���
                Debug.LogWarning("���V����܁A�C���X�y�N�^�[�ł� monster �̐ݒ��Y��Ă���܂����I");
            }
            unique = false;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleCamera();
        }
    }

    //�{�^���p�̊֐�
    //�C���X�y�N�^�[�́uOn Click()�v�ŁA���̊֐���I�сA���̘g�� 0 �� 1 �Ȃǂ̐��������Ă�������
    public void SwitchToCamera(int index)
    {
        SetCameraState(index);
    }

    /// <summary>
    /// Switches to the camera at the specified list index.
    /// Call with an index managed in the `cameras` list (0 = main camera).
    /// </summary>
    public void SetCameraState(int index)
    {
        if (index < 0 || index >= cameras.Count) return;

        // �Ď��J�����i1�Ԉȍ~�j��I�������Ȃ�A���̔ԍ����L�����܂���
        if (index != 0)
        {
            lastSubCameraIndex = index;
        }

        CurrentCameraIndex = index;

        // �S�J�����̗L���E�������ꊇ�Ǘ��������܂���
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] != null)
                cameras[i].gameObject.SetActive(i == CurrentCameraIndex);
        }

        // 0�ԖڈȊO�͂��ׂāu�T�u�J�����v�����ł���
        bool isSubCamera = (CurrentCameraIndex != 0);

        if (cameraCanvas != null) cameraCanvas.SetActive(isSubCamera);

        Cursor.visible = isSubCamera;
        Cursor.lockState = isSubCamera ? CursorLockMode.None : CursorLockMode.Locked;

        if (monster != null) monster.SetVisible(isSubCamera);
    }

    /// <summary>
    /// ���݂̏�Ԃ𔻒肵�āA��������̃J�����ɐ؂�ւ���֐��ł��B
    /// ���̃X�N���v�g����uswitchScript.SetCameraState(CameraSwitcher.CameraState.Sub);�v�̂悤�ɌĂׂ܂�
    /// </summary>
    public void ToggleCamera()
    {
        if (CurrentCameraIndex == 0)
        {
            // ���C���Ȃ�A�L�����Ă���Ď��J������
            SetCameraState(lastSubCameraIndex);
            player_Main.SetMoveEnabled(false); // プレイヤーの移動を停止
        }
        else
        {
            // �Ď��J�����Ȃ�A���C����
            SetCameraState(0);
            player_Main.SetMoveEnabled(true); // プレイヤーの移動を再開
        }
    }
}
