using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        None,
        Ready,
        Play,
        End
    }
    
    public static GameManager Instance { get; private set; }

    [Header("GamePlayTime")]
    [SerializeField] private float PlayTime;
    private float maxPlayTime;
    private float currentTime;
    private float timeAccumulator;
    public static event Action<float> GamePlayTimeChange;

    [Header("PlayerCount")]
    private readonly Dictionary<PlayerController, PlayerState> players = new Dictionary<PlayerController, PlayerState>();

    public static event Action<int> PlayerCountChange;

    [Header("GameState")]
    [SerializeField] private GameState currentGameState;
    private string gameWinner;  
    public static event Action<string> GameEnd;

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
        currentGameState = GameState.None;
        PlayTime = 5f;
        maxPlayTime = 60 * PlayTime;  // �д���
    }

    private void OnEnable()
    {
        ResetGame();
    }

    private void Update()
    {
        // ���� ���� ����
        if (currentGameState == GameState.Ready)
        {
            return;
        }
        
        // ���� �÷�����
        else if (currentGameState == GameState.Play)
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
        else if(currentGameState == GameState.End)
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
        gameWinner = null;

        // ���� ���� �ʱ�ȭ
        currentGameState = GameState.Ready;
    }

    // ���� ���� UI ����
    private void EndGame()
    {
        gameWinner = null;
        GameEnd?.Invoke(gameWinner);
        currentGameState = GameState.End;
    }
    private void EndGame(PlayerController inWinner)
    {
        gameWinner = inWinner.name;
        GameEnd?.Invoke(gameWinner);
        currentGameState = GameState.End;
    }
    public void RegisterAlivePlayer(PlayerController inPlayerController, PlayerState inPlayerStat)
    {
        if (!players.ContainsKey(inPlayerController))
        {
            players.Add(inPlayerController, inPlayerStat);
            //if (isLocal)
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
        currentGameState = GameState.Play;
        GamePlayTimeChange?.Invoke(currentTime);
        PlayerCountChange?.Invoke(players.Count);
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
}
