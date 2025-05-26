using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;

public interface IPlayerInputReceiver
{
    void InputMove(MoveType inMoveType, float inInputHorizontal, float inInputVertical);
    void InputAttack();
    void InputReload();
    void InputMousePosition(Vector3 inMousePosition);
    void InteractDropBox();
    void Dodge();
    void IsAim(bool isAim);
    void UseItem(int number);
    void UseGranade();

}

public enum MoveType
{
    Walk,
    Run,
    Slow
}

public class PlayerController : NetworkBehaviour, IPlayerInputReceiver, ITakeDamage
{
    #region variable Scope
    private NetworkVariable<uint> myNetworkNumber;

    [Header("PlayerMovement")]
    private Vector3 lookDirection;
    [SerializeField] private float currentMoveSpeed; // ���� ������ �ӵ�
    private float defaultSpeed; // �⺻ �ȴ� �ӵ�
    private float slowSpeed; // ������ �ȴ� �ӵ�
    private float runSpeed; // �ٴ� �ӵ�
    private PlayerMove playerMove; // �÷��̾� ���� Ŭ����
    private Gun playerGun;
    private bool isAimed;
    public Vector3 mouseWorldPosition;
    public static event Action<Vector3> OnMousePositionUpdated;

    public MoveType currentMoveType;
    [Header("PlayerSight")]
    private PlayerSight playerSight;

    [Header("Crosshair")]
    [SerializeField] private float defaultCrosshairSize;
    [SerializeField] private float currentCrosshairSize;
    [SerializeField] private float gunCrosshairSize;
    [SerializeField] private float crosshairspreadRadius;
    [SerializeField] private float crosshairLerpSpeed;
    public static event Action<float> OnCrosshairSizeChanged;

    [Header("PlayerStateMachine")]
    private PlayerStateMachine playerStateMachine;
    public static event Action<PlayerController, PlayerState> OnPlayerStateChanged;
    private NetworkVariable<PlayerState> netCurrentPlayerState; 
    [SerializeField] private PlayerStat playerStat;
    private BoxDetector boxDetector;
    private PlayerInventory playerInventory;

    public static event Action<float> OnChangeHpUI;

    private float lastInputHorizontal = 0f;
    private float lastInputVertical = 0f;

    [Header("ItemPrefabs")]
    [SerializeField] private GameObject selcetedItem;
    //[SerializeField] private Granade granadePrefab;
    //[SerializeField] private Granade granadePrefab;
    [SerializeField] private WoodenBox woodenBoxPrefab;
    [SerializeField] private Granade granadePrefab;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        myNetworkNumber = new NetworkVariable<uint>();
        defaultSpeed = 5.0f;
        slowSpeed = 3.0f;
        runSpeed = 8.0f;
        currentMoveSpeed = defaultSpeed;
        mouseWorldPosition = Vector3.zero;
        lookDirection = Vector3.forward;
        playerMove = GetComponent<PlayerMove>();
        playerSight = GetComponent<PlayerSight>();
        playerStat = new PlayerStat();
        playerInventory = GetComponent<PlayerInventory>();
        boxDetector = GetComponentInChildren<BoxDetector>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
        defaultCrosshairSize = 30f;
        currentCrosshairSize = defaultCrosshairSize;
        gunCrosshairSize = 0f;
        crosshairspreadRadius = 10f;
        crosshairLerpSpeed = 5f;

        netCurrentPlayerState = new NetworkVariable<PlayerState>(PlayerState.Idle);
        
    }

    private void Start()
    {
        playerGun = GetComponentInChildren<Gun>();
        GameManager.Instance.RegisterAlivePlayer(this, netCurrentPlayerState.Value, IsOwner);

        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return null;
        OnChangeHpUI?.Invoke(playerStat.GetHP());
    }

    private void Update()
    {
        lookDirection = CalculateDirectionFromMouseWorldPosition();
        //if (currentPlayerState == PlayerState.Dodge)
        //{
        //    lookDirection = new Vector3(inInputHorizontal, 0, inInputVertical);
        //}
        playerMove.RotateCharacter(lookDirection);
        UpdateCrosshairSize();
    }

    #endregion

    #region Input Methods
    public void InputReload()
    {
        ChangeGunStateServerRpc(GunState.Reload);
    }

    public void InputAttack()
    {
        ChangeGunStateServerRpc(GunState.Attack);

        Vector3 direction = playerSight.GetRandomSpreadDirection();
        playerGun.Fire(direction);
    }


    [ServerRpc]
    private void ChangeGunStateServerRpc(GunState inState)
    {
        if (netCurrentPlayerState.Value == PlayerState.Die)
        {
            return;
        }
        playerGun.ChangeGunState(inState);
    }


    public void InputMove(MoveType inMoveType, float inInputHorizontal, float inInputVertical)
    {
        lastInputHorizontal = inInputHorizontal;
        lastInputVertical = inInputVertical;
        
        if (inInputHorizontal == 0 && inInputVertical == 0)
        {
            ChangeStateServerRpc(PlayerState.Idle, NetworkManager.Singleton.LocalClientId);
            return;
        }

        if (inMoveType == MoveType.Slow)
        {
            currentMoveSpeed = slowSpeed;
        }
        else if (inMoveType == MoveType.Run)
        {
            currentMoveSpeed = runSpeed;
            currentMoveType = MoveType.Run;
        }
        else
        {
            currentMoveSpeed = defaultSpeed; // �⺻ ������ �ӵ�
            currentMoveType = MoveType.Walk;
        }

        ChangeStateServerRpc(PlayerState.Walk, NetworkManager.Singleton.LocalClientId);
        // inState

        playerMove.Move(inInputHorizontal * Time.deltaTime * currentMoveSpeed, inInputVertical * Time.deltaTime * currentMoveSpeed);
    }

    [Rpc(SendTo.Server)]
    private void ChangeStateServerRpc(PlayerState inState, ulong inId)
    {
        if (netCurrentPlayerState.Value == PlayerState.Die)
        {
            return;
        }
        if (netCurrentPlayerState.Value == inState)
        {
            return;
        }
        PlayerState changedState = playerStateMachine.ChangePlayerState(inState);
        netCurrentPlayerState.Value = changedState;
        GameManager.Instance?.UpdatePlayerState(this, changedState);
    }

    public void InputMousePosition(Vector3 inMousePosition)
    {
        mouseWorldPosition = inMousePosition;
        OnMousePositionUpdated?.Invoke(mouseWorldPosition);
    }

    public void InteractDropBox()
    {
        if (IsOwner)
        {
            DropBox box = boxDetector.GetNearestBox();
            if (box == null)
            {
                return;
            }
            box.OpenBox(myNetworkNumber.Value);
        }
    }

    public void Dodge()
    {
        ChangeStateServerRpc(PlayerState.Dodge, NetworkManager.Singleton.LocalClientId);
        //2. ������ ���� ����
        Vector3 dodgeDirection = new Vector3(lastInputHorizontal, 0, lastInputVertical).normalized;
        if(dodgeDirection == Vector3.zero)
        {
            dodgeDirection = transform.forward; // �Է��� ������ ����
        }
        float dodgeDistance = 5.0f; // ������ �Ÿ�
        playerMove.Move(dodgeDirection.x * dodgeDistance, dodgeDirection.z * dodgeDistance);
        //4. ���⵵ �����ְ� ������
        playerMove.RotateCharacter(dodgeDirection);
    }

    public void IsAim(bool isAim)
    {
        isAimed = isAim;
        currentMoveType = MoveType.Slow;
    }

    #endregion

    /// <summary>
    /// ���� �÷��̾� ���¸� �ִϸ��̼� ��ũ��Ʈ�� �ѱ�� �Լ�
    /// </summary>
    private Vector3 CalculateDirectionFromMouseWorldPosition()
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = mouseWorldPosition - currentPosition;
        direction.y = 0f;
        return direction;
    }

    private void UpdateCrosshairSize()
    {
        float previousSize = currentCrosshairSize;
        float targetCrosshairSize = defaultCrosshairSize;
        // ���¿� ���� ��ǥ ũ�� ����
        if (currentMoveType == MoveType.Run)
        {
            targetCrosshairSize += crosshairspreadRadius;
        }

        if (playerGun != null)
        {
            targetCrosshairSize += playerGun.equiptFocusRegion;

            if (isAimed)
            {
                targetCrosshairSize -= crosshairspreadRadius;
            }

        }

        currentCrosshairSize = Mathf.Lerp(currentCrosshairSize, targetCrosshairSize, Time.deltaTime * crosshairLerpSpeed);

        if (Mathf.Abs(currentCrosshairSize - targetCrosshairSize) < 0.01f)
        {
            currentCrosshairSize = targetCrosshairSize;
        }

        //if (Mathf.Abs(currentCrosshairSize - previousSize) > 0.01f)
        //{
        OnCrosshairSizeChanged?.Invoke(currentCrosshairSize);
        //}
    }

    public void TakeDamage(float inBulletDamage)
    {
        playerStat.ApplyHp(-inBulletDamage);
        float hp = playerStat.GetHP();

        if (hp <= 0f)
        {
            ChangeStateServerRpc(PlayerState.Die, OwnerClientId);
        }

        UpdateHpClientRpc(hp); 
    }

    [Rpc(SendTo.Server)]
    private void ApplyDamageServerRpc(float inBulletDamage)
    {
        playerStat.ApplyHp(-inBulletDamage);
        float hp = playerStat.GetHP();

        if (hp <= 0f)
        {
            ChangeStateServerRpc(PlayerState.Die, OwnerClientId); // ��� ���� ��ȯ
        }

        // Ŭ���̾�Ʈ���� UI ���� ��û
        UpdateHpClientRpc(hp);
    }

    [Rpc(SendTo.Owner)]
    private void UpdateHpClientRpc(float newHp)
    {
        OnChangeHpUI?.Invoke(newHp);
    }

    [Rpc(SendTo.Everyone)]
    private void TakeDamageRpc(float inBulletDamage)
    {
        playerStat.ApplyHp(-inBulletDamage);
        float hp = playerStat.GetHP();

        OnChangeHpUI?.Invoke(hp);
        if (hp <= 0)
        {
            ChangeStateServerRpc(PlayerState.Die, NetworkManager.Singleton.LocalClientId);
            return;
        }
    }

    
    //���������ϴ°�
    public ItemBase PickItem(ItemBase inPickItem)
    {
        uint number = myNetworkNumber.Value;
        //Debug.Log(NetworkManager.Singleton.LocalClientId + "�� " + number + " �κ��丮�� ����");
        //�κ��丮 ����
        ItemBase previousItem = playerInventory.TryAddOrReturnPreviousItem(inPickItem, playerGun);
        PickItemRpc(inPickItem.id, inPickItem.amount);
        return previousItem; //�ִ� ���Կ��� ��ü�ɰ��� ��ȯ
    }


    [Rpc(SendTo.NotMe)]
    public void PickItemRpc(int inItemId, int itemAmount)
    {
        //�κ��丮 ����
        uint number = myNetworkNumber.Value;
        //Debug.Log(NetworkManager.Singleton.LocalClientId + "�� " + number + " �κ��丮�� ����");
        ItemBase pickItem = new ItemBase(MasterDataManager.Instance.GetMasterItemData(inItemId), itemAmount);
        ItemBase previousItem = playerInventory.TryAddOrReturnPreviousItem(pickItem, playerGun);
    }


    public void SetNetworkNumber(uint inNumber)
    {
        myNetworkNumber.Value = inNumber;
    }

    public uint GetNetworkNumber()
    {
        return myNetworkNumber.Value;
    }

    [Rpc(SendTo.Everyone)]
    public void SetSpawnPositionRpc(Vector3 inPosition)
    {
        if (IsOwner)
        {
            transform.position = inPosition;
        }
    }

    // ���� �۾� �ʿ�
    public void UseItem(int inIndex)
    {
        if(playerInventory.HasUseItem(inIndex))
        {
            switch (inIndex)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    SpawnDeployableRpc();
                    break;
                default:
                    Logger.Log("There is not Allowed Input Key Event");
                    break;
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnDeployableRpc()
    {
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        Quaternion finalRotation = lookRotation;
        WoodenBox go = Instantiate(woodenBoxPrefab, transform.position + Vector3.forward * 7f + Vector3.up * 10f, finalRotation);
        go.GetComponent<NetworkObject>().Spawn();
        go.SetOwner(GetNetworkNumber());
    }

    public void UseGranade()
    {
        if (playerInventory.HasUseGranade())
        {
            SpawnGranadeRpc();
        }
    }
    [Rpc(SendTo.Server)]
    private void SpawnGranadeRpc()
    {
        Granade granade = Instantiate(granadePrefab, transform.position, Quaternion.LookRotation(lookDirection));
        granade.GetComponent<NetworkObject>().Spawn();
        granade.SetOwner(GetNetworkNumber());
        Vector3 start = transform.position + Vector3.up;
        granade.Launch(start, mouseWorldPosition);
    }
}
