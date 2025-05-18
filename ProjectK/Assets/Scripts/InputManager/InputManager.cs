using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �÷��̾� �Է��� �ؼ��ϰ� ���� �÷��̾�Ը� �����ϴ� ���� �Է� ó����
/// </summary>

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private IPlayerInputReceiver localPlayerController;
    private PlayerState localPlayerState;
    private InGameUIManager inGameUIManager;
    public enum InputReceiver
    {
        None,
        PlayerOnly,
        InGameUIOnly,
        All
    }
    [SerializeField]
    private InputReceiver currentReceiver;

    #region
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        currentReceiver = InputReceiver.All;
    }

    private void OnEnable()
    {
        GameManager.LocalPlayerState += UpdateLocalPlayerStateChanged;
    }

    private void OnDisable()
    {
        GameManager.LocalPlayerState -= UpdateLocalPlayerStateChanged;
    }

    private void Update()
    {
        if (currentReceiver == InputReceiver.None)
        {
            return;
        }


        if (localPlayerState == PlayerState.Die)
            //|| currentPlayerState == PlayerState.Dodge
        {
            return;
        }

        if (currentReceiver == InputReceiver.All)
        {
            HandlePlayerInput();
        }

        
    }
    #endregion

    public void RegisterLocalPlayer(IPlayerInputReceiver inPlayer)
    {
        localPlayerController = inPlayer;

        currentReceiver = InputReceiver.All;
    }
    public void RegisterUIManager(InGameUIManager inManager)
    {
        inGameUIManager = inManager;
    }
    public void SetInputReceiver(InputReceiver inReceiver)
    {
        currentReceiver = inReceiver;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.y;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);
        return worldPos;
    }

    private void HandlePlayerInput()
    {
        // ȸ�� ó�� (���콺 ����)
        Vector3 lookDir = GetMouseWorldPosition();
        localPlayerController.RotateCharacterOnMousePosition(lookDir);

        // �̵� ó��
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        MoveType moveType = MoveType.Walk;
        bool isAim = false;

        // �޸���
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveType = MoveType.Run;
        }
        // ���ؽ�
        else if (Input.GetKey(KeyCode.Mouse1))
        {
            isAim = true;
            moveType = MoveType.Slow;
            localPlayerController.IsAim();
        }

        // �̵�
        localPlayerController.InputMove(moveType, h, v);

        // ����
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            localPlayerController.InputAttack();
        }

        // ������
        if (Input.GetKeyDown(KeyCode.R))
        {
            localPlayerController.InputReload();
        }

        // ��ӹڽ� ��ȣ�ۿ�
        if (Input.GetKeyDown(KeyCode.F))
        {
            localPlayerController.InteractDropBox();
        }

        // ������
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            localPlayerController.Dodge();
        }
    }
    private void UpdateLocalPlayerStateChanged(PlayerController inPlayerController, PlayerState inLocalPlayerState)
    {
        if (localPlayerController == null)
        {
            RegisterLocalPlayer(inPlayerController);
        }
        localPlayerState = inLocalPlayerState;
    }
}