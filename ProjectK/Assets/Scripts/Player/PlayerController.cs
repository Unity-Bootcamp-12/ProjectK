using System;
using UnityEngine;

public interface IPlayerInputReceiver
{
    void InputMove(float inInputHorizontal, float inInputVertical);
    void InputAttack();
    void InputReload();
    void RotateCharacterOnMousePosition(Vector3 inDirection);
}
public enum PlayerState
{
    Idle,
    Walk,
    Run,
    Attack,
    Reload,
    Die
}

public class PlayerController : MonoBehaviour, IPlayerInputReceiver
{
    [Header("PlayerMovement")]
    [SerializeField] private float currentMoveSpeed; // ���� ������ �ӵ�
    private float defaultSpeed; // �⺻ �ȴ� �ӵ�
    private float runSpeed; // �ٴ� �ӵ�
    private PlayerMove playerMove; // �÷��̾� ���� Ŭ����
    private Gun playerGun;

    [Header("PlayerAnimation")]
    private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerState currentPlayerState; // ���� �÷��̾��� ����

    #region Unity Methods
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
        GameManager.Instance.RegisterAlivePlayer(this, currentPlayerState);
    }

    private void Update()
    {
        AniConrtrol();
    }
    #endregion

    #region Input Methods
    public void InputReload()
    {
        currentPlayerState = PlayerState.Reload;
        playerGun.Reload();
    }

    public void InputAttack()
    {
        currentPlayerState = PlayerState.Attack;
        playerGun.Fire(transform.forward);
    }

    public void InputMove(float inInputHorizontal, float inInputVertical)
    {
        if (inInputHorizontal == 0 && inInputVertical == 0)
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

        playerMove.Move(inInputHorizontal * Time.deltaTime * currentMoveSpeed, inInputVertical * Time.deltaTime * currentMoveSpeed);
    }
    public void RotateCharacterOnMousePosition(Vector3 inMouseWorldPosition)
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = inMouseWorldPosition - currentPosition;
        direction.y = 0f;

        transform.LookAt(transform.position + direction);
    }
    #endregion

    /// <summary>
    /// ���� �÷��̾� ���¸� �ִϸ��̼� ��ũ��Ʈ�� �ѱ�� �Լ�
    /// </summary>
    private void AniConrtrol()
    {
        playerAnimation.AnimationConrtrol(currentPlayerState);
    }
}
