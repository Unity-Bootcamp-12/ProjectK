using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
public enum GameState
{
    None,
    Ready,
    Play,
    End
}

public class GameManager : NetworkBehaviour
{
    public SpawnAssginer spawnAssigner;
    public static GameManager Instance { get; private set; }

    [Header("GamePlayTime")]
    [SerializeField] private float PlayTime;
    private float maxPlayTime;
    public static NetworkVariable<int> alivePlayCount = new NetworkVariable<int>(0);
    public static NetworkVariable<float> currentTime = new NetworkVariable<float>(0);
    private float timeAccumulator;

    [Header("PlayerCount")]
    private const uint INVALID_PLAYER_NUMBER = 99999999;
    private readonly Dictionary<PlayerController, PlayerState> players = new Dictionary<PlayerController, PlayerState>();

    [Header("GameState")]
    [SerializeField] private NetworkVariable<GameState> currentGameState = new NetworkVariable<GameState>(GameState.None);

    private uint gameWinner;
    public static event Action<uint> OnWinnerChanged;
    public static event Action<GameState> OnGameStateChanged;

    public static event Action<PlayerController, PlayerState> LocalPlayerState;

    public static event Action OnHideLobbyUIRequested;

    [Header("DropBox")]
    [SerializeField] private GameObject dropboxPrefab;
    public List<Transform> dropboxSpawnTransform;


    #region Unity Methods
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

        PlayTime = 5f;
        maxPlayTime = 60 * PlayTime;  // �д���
    }

    private void Update()
    {
        // ���� �÷�����
         if (currentGameState.Value == GameState.Play)
        {
            if (currentTime.Value <= 0)
            {
                EndGame();
                return;
            }

            // �װ� �ƴϸ� ������ ��� �����Ѵ�
            if (IsServer)
            {
                timeAccumulator += Time.deltaTime;
                while (timeAccumulator >= 1f)
                {
                    // 1�� ������ UI�� ������Ʈ
                    currentTime.Value -= 1f;
                    timeAccumulator -= 1f;
                }
            }
     
        }
    }
    #endregion
    private void ChangeGameState(GameState inGameState)
    {
        if (!IsHost)
        {
            return;
        }

        if (currentGameState.Value == inGameState)
        {
            return;
        }
        currentGameState.Value = inGameState;
        NotifyClientsGameStateChangedRpc(currentGameState.Value);
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyClientsGameStateChangedRpc(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ResetGame();
        }
    }
    
    // ���� ���� UI ����
    private void ResetGame()
    {
        //������ ����
        // �÷��� Ÿ�� �ʱ�ȭ
        currentTime.Value = maxPlayTime;
        timeAccumulator = 0f;

        // �÷��̾� �� �ʱ�ȭ
        players.Clear();
        alivePlayCount.Value = players.Count;

        // ���� �ʱ�ȭ
        gameWinner = INVALID_PLAYER_NUMBER;

        // ���� ���� �ʱ�ȭ
        ChangeGameState(GameState.Ready);
    }

    // ���� ���� UI ����
    private void EndGame()
    {
        OnWinnerChanged?.Invoke(gameWinner);
        ChangeGameState(GameState.End);
    }
    private void EndGame(PlayerController inWinner)
    {
        gameWinner = inWinner.GetNetworkNumber();
        OnWinnerChanged?.Invoke(gameWinner);
        ChangeGameState(GameState.End);
        StartCoroutine(GameEnd(10f));
    }

    private IEnumerator GameEnd(float inTime)
    {
        yield return new WaitForSeconds(inTime);
        ResetGame();
    }
    public void RegisterAlivePlayer(PlayerController inPlayerController, PlayerState inPlayerState, bool inIsOwner = true)
    {
        if (!players.ContainsKey(inPlayerController))
        {
            players.Add(inPlayerController, inPlayerState);
            
            if (inIsOwner)
            {
                LocalPlayerState?.Invoke(inPlayerController, inPlayerState);
            }
            if (IsHost)
            {
                alivePlayCount.Value = players.Count;
            }
        }
    }
    public void UpdatePlayerState(PlayerController inPlayerController, PlayerState inPlayerState)
    {
        //������ ����
        if (players.ContainsKey(inPlayerController))
        {
            players[inPlayerController] = inPlayerState;

            //if (isLocal)
            {
                LocalPlayerState?.Invoke(inPlayerController, inPlayerState);
            }

            if (inPlayerState == PlayerState.Die)
            {
                alivePlayCount.Value -= GetAlivePlayerCount();
                CheckGameOver();
            }
        }
    }

    // UI�󿡼� �Է� �޾Ƽ� ���� ����
    public void StartGame()
    {
        // ���� ����
        AllocatePlayerNomber();
        SpawnDropBox();
        AssignPlayerPosition();
        ChangeGameState(GameState.Play);
        ApplyStartUIRpc();
    }

    public void RequestStartGame()
    {
        if (IsHost)
        {
            StartGame();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ApplyStartUIRpc()
    {
        OnHideLobbyUIRequested?.Invoke();
    }

    private void SpawnDropBox()
    {
        for (int i = 0; i < dropboxSpawnTransform.Count; i++)
        {
            GameObject dropBox = Instantiate(dropboxPrefab, dropboxSpawnTransform[i].position, Quaternion.identity);
            dropBox.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void AssignPlayerPosition()
    {
        int i = 0;
        List<Transform> spawnTransformList = spawnAssigner.GetSpawnPositionList();
        foreach (var item in players)
        {
            //�÷��̾� ���� ��ġ ����Ʈ�� ������ �Դٰ� ����
            item.Key.SetSpawnPositionRpc(spawnTransformList[i].position);
            i++;
        }
    }

    public int GetAlivePlayerCount()
    {
        return players.Count(pair => pair.Value != PlayerState.Die);
    }

    private void CheckGameOver()
    {
        int aliveCount = 0;
        PlayerController lastAlive = null;

        foreach (var pair in players)
        {
            if (pair.Value != PlayerState.Die)
            {
                aliveCount++;
                lastAlive = pair.Key;

                if (aliveCount > 1)
                {
                    return;
                }
            }
        }

        if (aliveCount == 1)
        {
            EndGame(lastAlive);
        }
        else if (aliveCount == 0)
        {
            EndGame();
        }
    }

    private void AllocatePlayerNomber()
    {
        uint number = 1;
        foreach (var player in players.Keys.OrderBy(p => p.OwnerClientId))
        {
            player.SetNetworkNumber(number++);
        }
    }

    public PlayerController GetPlayer(uint inPlayerNumber)
    {
        foreach(var player in players)
        {
            if(player.Key.GetNetworkNumber() == inPlayerNumber)
            {
                return player.Key;
            }
        }
        return null;
    }
}
