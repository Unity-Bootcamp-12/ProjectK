using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor.Networking.PlayerConnection;
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
    public NetworkVariable<int> alivePlayCount = new NetworkVariable<int>(0);
    public NetworkVariable<float> currentTime = new NetworkVariable<float>(0);
    private float timeAccumulator;

    [Header("PlayerCount")]
    private const uint INVALID_PLAYER_NUMBER = 99999999;
    private readonly Dictionary<PlayerController, PlayerState> players = new Dictionary<PlayerController, PlayerState>();

    [Header("GameState")]
    [SerializeField] private NetworkVariable<GameState> currentGameState = new NetworkVariable<GameState>(GameState.None);

    private PlayerController myPlayer;
    private uint gameWinner;
    public static event Action<uint> OnWinnerChanged;
    public static event Action<GameState> OnGameStateChanged;

    public static event Action<PlayerController, PlayerState> LocalPlayerState;

    public static event Action OnHideLobbyUIRequested;

    [Header("DropBox")]
    [SerializeField] private GameObject dropboxPrefab;
    public List<Transform> dropboxSpawnTransform;

    private void SpawnBots()
    {
        if (!IsHost) return;

        foreach (var pair in players)
        {
            PlayerController pc = pair.Key;
            if (pc == myPlayer) continue;

            Vector2 random = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(8f, 15f);
            Vector3 pos = myPlayer.transform.position + new Vector3(random.x, 0f, random.y);
            pc.SetSpawnPositionRpc(pos);

            BotInputProvider bot = pc.gameObject.AddComponent<BotInputProvider>();
            bot.target = myPlayer.transform;
        }
    }
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
        maxPlayTime = 60 * PlayTime;  // 분단위
        myPlayer = null;
    }

    private void Update()
    {
        // 게임 플레이중
        if (currentGameState.Value == GameState.Play)
        {
            if (currentTime.Value <= 0)
            {
                EndGame(gameWinner);
                return;
            }

            // 그거 아니면 게임을 계속 진행한다
            if (IsServer)
            {
                timeAccumulator += Time.deltaTime;
                while (timeAccumulator >= 1f)
                {
                    // 1초 단위로 UI에 업데이트
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
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
            ResetGame();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }

    // 게임 종료 UI 띄우기
    private void ResetGame()
    {
        //서버만 들어옴
        // 플레이 타임 초기화
        currentTime.Value = maxPlayTime;
        timeAccumulator = 0f;

        // 플레이어 수 초기화
        players.Clear();
        alivePlayCount.Value = players.Count;

        // 승자 초기화
        gameWinner = INVALID_PLAYER_NUMBER;

        // 게임 상태 초기화
        ChangeGameState(GameState.Ready);
    }

    // 게임 종료 UI 띄우기
    [Rpc(SendTo.Everyone)]
    private void NotifyEndGameRpc(uint lastPlayerId)
    {
        OnWinnerChanged?.Invoke(lastPlayerId);
    }

    private void EndGame(uint lastPlayerId)
    {
        NotifyEndGameRpc(lastPlayerId);
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
                myPlayer = inPlayerController;
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
        //서버만 들어옴
        if (players.ContainsKey(inPlayerController))
        {
            players[inPlayerController] = inPlayerState;

            NotifyPlayerStateChangedRpc(inPlayerController.GetNetworkNumber(), inPlayerState);

            if (inPlayerState == PlayerState.Die)
            {
                alivePlayCount.Value = GetAlivePlayerCount();
                CheckGameOver();
            }
        }
    }
    [Rpc(SendTo.Everyone)]
    private void NotifyPlayerStateChangedRpc(uint inPlayerNumber, PlayerState inPlayerState)
    {
        PlayerController playerController = GetPlayer(inPlayerNumber);
        if (inPlayerNumber == GetMyPlayerNumber())
        {
            LocalPlayerState?.Invoke(playerController, inPlayerState);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        PlayerController disconnectedPlayer = null;

        foreach (var kvp in players)
        {
            if (kvp.Key.OwnerClientId == clientId)
            {
                disconnectedPlayer = kvp.Key;
                break;
            }
        }

        if (disconnectedPlayer != null)
        {
            players.Remove(disconnectedPlayer);
            alivePlayCount.Value = GetAlivePlayerCount();
            Logger.Warning($"Client {clientId} disconnected. Remaining: {players.Count}");
            CheckGameOver();
        }
    }

    // UI상에서 입력 받아서 게임 시작
    public void StartGame()
    {
        // 게임 시작
        AllocatePlayerNomber();
        SpawnDropBox();
        AssignPlayerPosition();
        ChangeGameState(GameState.Play);
        ApplyStartUIRpc();
        SpawnBots();
    }

    public void RequestStartGame()
    {
        if (IsHost)
        {
            StartGame();
        }
    }

    public void RequestRestartGame()
    {
        if (IsHost)
        {
            ResetGame();
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
            //플레이어 스폰 위치 리스트가 섞여서 왔다고 가정
            item.Key.SetSpawnPositionRpc(spawnTransformList[i].position);
            i++;
        }
    }

    public int GetAlivePlayerCount()
    {
        return players.Count(pair => pair.Value != PlayerState.Die);
    }

    public uint GetMyPlayerNumber()
    {
        return myPlayer.GetNetworkNumber();
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
            gameWinner = lastAlive.GetNetworkNumber();
        }
        EndGame(gameWinner);
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
        foreach (var player in players)
        {
            if (player.Key.GetNetworkNumber() == inPlayerNumber)
            {
                return player.Key;
            }
        }
        return null;
    }
}
