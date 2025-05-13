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

    private void Update()
    {
        InputMove();
        InputAttack(); // �׽�Ʈ��
        InputReload(); // �׽�Ʈ��
        AniConrtrol();
    }

    private void InputReload()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            currentPlayerState = PlayerState.Reload;
        }
    }

    private void InputAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            currentPlayerState = PlayerState.Attack;
        }
    }

    /// <summary>
    /// ������ ���� ��ǲ �Լ�
    /// </summary>
    private void InputMove()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        if(inputHorizontal == 0 && inputVertical == 0)
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
}
