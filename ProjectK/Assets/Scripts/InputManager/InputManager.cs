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
    private InGameUIManager inGameUIManager;
    public enum InputReceiver
    {
        None,
        PlayerOnly,
        InGameUIOnly,
        All
    }

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

    private void Start()
    {

    }

    private void Update()
    {
        if (currentReceiver == InputReceiver.None || localPlayerController == null)
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

        localPlayerController.InputMove(h, v);

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
    }
}