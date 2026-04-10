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
    private Vector3 dodgeDirection;
    [SerializeField] private float currentMoveSpeed; // 현재 움직임 속도
    private float defaultSpeed; // 기본 걷는 속도
    private float slowSpeed; // 느리게 걷는 속도
    private float runSpeed; // 뛰는 속도
    private PlayerMove playerMove; // 플레이어 무브 클래스
    public Gun playerGun;
    private bool isAimed;
    public Vector3 mouseWorldPosition;
    public static event Action<Vector3> OnMousePositionUpdated;
    private bool isDodging;

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
    public static event Action<float> OnChangeStaminaUI;

    private float lastInputHorizontal = 0f;
    private float lastInputVertical = 0f;

    [Header("ItemPrefabs")]
    [SerializeField] private GameObject selcetedItem;
    //[SerializeField] private Granade granadePrefab;
    //[SerializeField] private Granade granadePrefab;
    [SerializeField] private WoodenBox woodenBoxPrefab;
    [SerializeField] private Granade granadePrefab;

    [Header("Effect")]
    [SerializeField] private EffectSpawner hitEffectSpawner;
    [SerializeField] private EffectSpawner spawnEffectSpawner;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        myNetworkNumber = new NetworkVariable<uint>();
        defaultSpeed = 8.0f;
        slowSpeed = 5.0f;
        runSpeed = 12.0f;
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
        dodgeDirection = lookDirection;
        netCurrentPlayerState = new NetworkVariable<PlayerState>(PlayerState.Idle);
        isDodging = false;
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
        OnChangeStaminaUI?.Invoke(playerStat.GetStamina());
    }

    private void Update()
    {
        lookDirection = CalculateDirectionFromMouseWorldPosition();
        if (isDodging)
        {
            playerMove.RotateCharacter(dodgeDirection);
        }
        else
        {
            playerMove.RotateCharacter(lookDirection);
        }
        if (IsOwner)
        {
            playerSight.SetSightDirection(mouseWorldPosition);
            UpdateCrosshairSize();
        }
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
        if (TryGetComponent<BotInputProvider>(out var ip))
        {
            Debug.Log("Bot Fire!");
        }

        playerGun.Fire(direction);
        Debug.Log(direction);
    }


    [ServerRpc(RequireOwnership = false)]
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
            currentMoveSpeed = defaultSpeed; // 기본 움직임 속도
            currentMoveType = MoveType.Walk;
        }

        ChangeStateServerRpc(PlayerState.Walk, NetworkManager.Singleton.LocalClientId);

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
        PlayerState before = playerStateMachine.GetCurrentPlayerState();
        playerStateMachine.ChangePlayerState(inState);

        PlayerState after = playerStateMachine.GetCurrentPlayerState();
        if (before != after)
        {
            netCurrentPlayerState.Value = after;
            GameManager.Instance?.UpdatePlayerState(this, after);
        }
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
        if (playerStateMachine.GetCurrentPlayerState() == PlayerState.Dodge)
        {
            return;
        }
        if (playerStat.GetStamina() < 30)
        {
            return;
        }

        StartCoroutine(LockLookDirecion(0.8f));
        dodgeDirection = new Vector3(lastInputHorizontal, 0, lastInputVertical).normalized;
        if(dodgeDirection == Vector3.zero)
        {
            dodgeDirection = transform.forward; // 입력이 없으면 정면
        }
        ApplyStaminaRpc(-30f);
        ChangeStateServerRpc(PlayerState.Dodge, NetworkManager.Singleton.LocalClientId);
    }

    private IEnumerator LockLookDirecion(float delay)
    {
        isDodging = true;
        yield return new WaitForSeconds(delay);
        isDodging = false;
    }
    public void IsAim(bool isAim)
    {
        isAimed = isAim;
        currentMoveType = MoveType.Slow;
    }

    #endregion

    /// <summary>
    /// 현재 플레이어 상태를 애니메이션 스크립트에 넘기는 함수
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
        // 상태에 따라 목표 크기 설정
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

    [Rpc(SendTo.Everyone)]
    public void ApplyHpRpc(float inHpStat)
    {
        playerStat.ApplyHp(inHpStat);

        float hp = playerStat.GetHP();

        if (IsOwner)
        {
            OnChangeHpUI?.Invoke(hp);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ApplyStaminaRpc(float inStaminaStat)
    {
        playerStat.ApplyStamina(inStaminaStat);
        Logger.Info(playerStat.GetStamina().ToString());
        float stamina = playerStat.GetStamina();

        if (IsOwner)
        {
            OnChangeStaminaUI?.Invoke(stamina);
        }
    }
    public void TakeDamage(float inBulletDamage)
    {
        //서버에서 피격 충돌 판정, 피격 함수를 Rpc로 동기화
        TakeDamageRpc(inBulletDamage);
    }

    [Rpc(SendTo.Everyone)]
    private void TakeDamageRpc(float inBulletDamage)
    {
        if (netCurrentPlayerState.Value == PlayerState.Dodge)
        {
            return;
        }
        playerStat.ApplyHp(-inBulletDamage);

        hitEffectSpawner.PlayEffect();

        float hp = playerStat.GetHP();

        if (IsOwner)
        {
            OnChangeHpUI?.Invoke(hp);
        }

        if (hp <= 0)
        {
            ChangeStateServerRpc(PlayerState.Die, NetworkManager.Singleton.LocalClientId);
            return;
        }
    }

    
    //아이템픽하는거
    public ItemBase PickItem(ItemBase inPickItem)
    {
        uint number = myNetworkNumber.Value;
        //인벤토리 적용
        ItemBase previousItem = playerInventory.TryAddOrReturnPreviousItem(inPickItem, playerGun);
        PickItemRpc(inPickItem.id, inPickItem.amount);
        return previousItem; //있던 슬롯에서 교체될것은 반환
    }


    [Rpc(SendTo.NotMe)]
    public void PickItemRpc(int inItemId, int itemAmount)
    {
        //인벤토리 적용
        uint number = myNetworkNumber.Value;
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
            spawnEffectSpawner.PlayEffect();
        }
    }

    // 여기 작업 필요
    public void UseItem(int inIndex)
    {
        if(playerInventory.HasItem(inIndex))
        {
            ItemBase useItem = playerInventory.GetItem(inIndex);
            switch (inIndex)
            {
                case 1:
                    ApplyHpRpc(useItem.power);
                    break;
                case 2:
                    ApplyHpRpc(useItem.power);
                    break;
                case 3:
                    ApplyStaminaRpc(useItem.power);
                    break;
                case 4:
                    SpawnDeployableRpc(GetNetworkNumber());
                    break;
                default:
                    Logger.Log("There is not Allowed Input Key Event");
                    break;
            }
            playerInventory.UseItem(inIndex);
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnDeployableRpc(uint ownerNumber)
    {
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        Quaternion finalRotation = lookRotation;
        WoodenBox go = Instantiate(woodenBoxPrefab, transform.position + transform.forward * 7f + Vector3.up * 10f, finalRotation);
        go.GetComponent<NetworkObject>().Spawn();
        go.SetOwner(ownerNumber);
    }

    public void UseGranade()
    {
        if (playerInventory.HasItem(5))
        {
            SpawnGranadeRpc(mouseWorldPosition, GetNetworkNumber());
            playerInventory.UseItem(5);
        }
        
    }

    [Rpc(SendTo.Server)]
    private void SpawnGranadeRpc(Vector3 mouseWorldPosition, uint ownerNumber)
    {
        Granade granade = Instantiate(granadePrefab, transform.position, Quaternion.LookRotation(lookDirection));
        granade.GetComponent<NetworkObject>().Spawn();
        granade.SetOwner(ownerNumber);
        Vector3 start = transform.position + Vector3.up;
        granade.Launch(start, mouseWorldPosition);
    }
}
