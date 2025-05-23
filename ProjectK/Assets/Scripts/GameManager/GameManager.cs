using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode;
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
    private float currentTime;
    private float timeAccumulator;
    public static event Action<float> GamePlayTimeChange;

    [Header("PlayerCount")]
    private const uint INVALID_PLAYER_NUMBER = 99999999;
    private readonly Dictionary<PlayerController, PlayerState> players = new Dictionary<PlayerController, PlayerState>();

    public static event Action<int> PlayerCountChange;

    [Header("GameState")]
    [SerializeField] private NetworkVariable<GameState> currentGameState;

    private uint gameWinner;
    public static event Action<uint> OnWinnerChanged;
    public static event Action<GameState> OnGameStateChanged;

    public static event Action<PlayerController, PlayerState> LocalPlayerState;

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
        currentGameState.Value = GameState.None;
        PlayTime = 5f;
        maxPlayTime = 60 * PlayTime;  // �д���
    }

    private void OnEnable()
    {
        ResetGame();
    }

    private void TestAssignPlayerPosition()
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

    private void Update()
    {
        // ���� ���� ���¿��� �����ϱ�
        if (currentGameState == GameState.Ready && IsHost && Input.GetKeyDown(KeyCode.L))
        {
            TestAssignPlayerPosition();
            return;
        }

        // ���� �÷�����
        else if (currentGameState.Value == GameState.Play)
        {
            if (currentTime <= 0)
            {
                EndGame();
                return;
            }

            // �װ� �ƴϸ� ������ ��� �����Ѵ�
            timeAccumulator += Time.deltaTime;
            while (timeAccumulator >= 1f)
            {
                // 1�� ������ UI�� ������Ʈ
                currentTime -= 1f;
                timeAccumulator -= 1f;

                GamePlayTimeChange?.Invoke(currentTime);
            }
        }

        // ���� ���� ��
        else if (currentGameState.Value == GameState.End)
        {
            // ���� �����϶��� �ƹ��͵� ���Ұ���   
        }
    }
    #endregion

    // ���� ���� UI ����
    private void ResetGame()
    {
        // �÷��� Ÿ�� �ʱ�ȭ
        currentTime = maxPlayTime;
        timeAccumulator = 0f;

        // �÷��̾� �� �ʱ�ȭ
        players.Clear();
        PlayerCountChange?.Invoke(players.Count);

        // ���� �ʱ�ȭ
        gameWinner = INVALID_PLAYER_NUMBER;

        // ���� ���� �ʱ�ȭ
        currentGameState.Value = GameState.Ready;
        OnGameStateChanged?.Invoke(currentGameState.Value);
    }

    // ���� ���� UI ����
    private void EndGame()
    {
        OnWinnerChanged?.Invoke(gameWinner);
        currentGameState.Value = GameState.End;
        OnGameStateChanged?.Invoke(currentGameState.Value);
    }
    private void EndGame(PlayerController inWinner)
    {
        gameWinner = inWinner.GetNetworkNumber();
        OnWinnerChanged?.Invoke(gameWinner);
        currentGameState.Value = GameState.End;
        OnGameStateChanged?.Invoke(currentGameState.Value);

        GameEnd(10f);
    }

    private IEnumerator GameEnd(float inTime)
    {
        yield return new WaitForSeconds(inTime);
        ResetGame();
    }
    public void RegisterAlivePlayer(PlayerController inPlayerController, PlayerState inPlayerStat, bool inIsOwner = true)
    {
        if (!players.ContainsKey(inPlayerController))
        {
            players.Add(inPlayerController, inPlayerStat);
            if (inIsOwner) //�ɺ��� Multi �� ���ʰ��� ����� ����
            {
                LocalPlayerState?.Invoke(inPlayerController, inPlayerStat);
            }
            PlayerCountChange?.Invoke(players.Count);
        }
    }
    public void UpdatePlayerState(PlayerController inPlayerController, PlayerState inPlayerStat)
    {
        if (players.ContainsKey(inPlayerController))
        {
            players[inPlayerController] = inPlayerStat;

            //if (isLocal)
            {
                LocalPlayerState?.Invoke(inPlayerController, inPlayerStat);
            }

            if (inPlayerStat == PlayerState.Die)
            {
                PlayerCountChange?.Invoke(GetAlivePlayerCount());
                CheckGameOver();
            }
        }
    }

    // -> �÷��̾� ����� �κ�ũ�� ���� ȣ��� ���� ����
    public void UnregisterAlivePlayer(PlayerController inPlayerController)
    {
        if (!players.ContainsKey(inPlayerController))
        {
            Debug.LogError("PlayerController is not registered");
            return;
        }
        else
        {
            players.Remove(inPlayerController);
            PlayerCountChange?.Invoke(players.Count);
        }
    }

    // UI�󿡼� �Է� �޾Ƽ� ���� ����
    public void StartGame()
    {
        // ���� ����
        GamePlayTimeChange?.Invoke(currentTime);
        PlayerCountChange?.Invoke(players.Count);
        AllocatePlayerNomber();
        currentGameState.Value = GameState.Play;
        OnGameStateChanged?.Invoke(currentGameState.Value);

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
}
