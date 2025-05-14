using System;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Walk,
    Run,
    Attack,
    Reload,
    Die
}

public class PlayerController : MonoBehaviour
{
    [Header("PlayerMovement")]
    [SerializeField] private float currentMoveSpeed; // ���� ������ �ӵ�
    private float defaultSpeed; // �⺻ �ȴ� �ӵ�
    private float runSpeed; // �ٴ� �ӵ�
    private PlayerMove playerMove; // �÷��̾� ���� Ŭ����
    private float inputHorizontal; // AD ��ǲ ��
    private float inputVertical; // WS ��ǲ ��
    private Vector3 mousePosition;
    private Gun playerGun;

    [Header("PlayerAnimation")]
    private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerState currentPlayerState; // ���� �÷��̾��� ����

    private void Awake()
    {
        defaultSpeed = 5.0f;
        runSpeed = 8.0f;
        currentMoveSpeed = defaultSpeed;
        playerMove = GetComponent<PlayerMove>();
        playerAnimation = GetComponent<PlayerAnimation>();
        currentPlayerState = PlayerState.Idle;
    }

    private void Start()
    {
        playerGun = GetComponentInChildren<Gun>();
        mousePosition = new Vector3(0, 0, 0);
    }

    private void Update()
    {
        InputMove();
        InputAttack(); // �׽�Ʈ��
        InputReload(); // �׽�Ʈ��
        AniConrtrol();
        RotateCharacterOnMousePosition();
    }

    private void InputReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentPlayerState = PlayerState.Reload;
            playerGun.Reload();
        }
    }

    private void InputAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            currentPlayerState = PlayerState.Attack;

            playerGun.Fire(transform.forward);
        }
    }

    /// <summary>
    /// ������ ���� ��ǲ �Լ�
    /// </summary>
    private void InputMove()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        if (inputHorizontal == 0 && inputVertical == 0)
        {
            currentPlayerState = PlayerState.Idle;
            return;
        }
        else
        {
            currentPlayerState = PlayerState.Walk; // �����̴� �ִϸ��̼� // ���� �ٱ� �߰� ����
        }

        currentMoveSpeed = defaultSpeed; // �⺻ ������ �ӵ�
        if (Input.GetKey(KeyCode.LeftShift)) // �޸���
        {
            currentMoveSpeed = runSpeed;
        }

        playerMove.Move(inputHorizontal * Time.deltaTime * currentMoveSpeed, inputVertical * Time.deltaTime * currentMoveSpeed);
    }

    /// <summary>
    /// ���� �÷��̾� ���¸� �ִϸ��̼� ��ũ��Ʈ�� �ѱ�� �Լ�
    /// </summary>
    private void AniConrtrol()
    {
        playerAnimation.AnimationConrtrol(currentPlayerState);
    }

    private void RotateCharacterOnMousePosition()
    {
        mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.y; // Ȥ�� ĳ���ͱ����� �Ÿ�

        Vector3 MouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        MouseWorldPosition.y = 0f;

        transform.LookAt(MouseWorldPosition);
    }
}
