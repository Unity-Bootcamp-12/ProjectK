using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        None,
        Ready,
        Play,
        End
    }

    public static GameManager instance;

    [Header("GamePlayTime")]
    [SerializeField] 
    private float PlayTime;
    private float maxPlayTime;
    private float currentTime;
    private float timeAccumulator;
    public static event Action<float> GamePlayTimeChange;

    [Header("PlayerCount")]
    private readonly List<PlayerController> players = new List<PlayerController>();

    public static event Action<int> PlayerCountChange;

    [Header("GameState")]
    private GameState currentGameState;
    private string gameWinner;  
    public static event Action<string> GameEnd;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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

    private void Update()
    {
        // ���� ���� ����
        if (currentGameState == GameState.Ready)
        {
            // ���� ������ ���� �ƹ��͵� ���Ұ���
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

    public void RegisterAlivePlayer(PlayerController inPlayerController)
    {
        if (!players.Contains(inPlayerController))
        {
            players.Add(inPlayerController);
            PlayerCountChange?.Invoke(players.Count);
        }
    }

    // -> �÷��̾� ����� �κ�ũ�� ���� ȣ��� ���� ����
    public void UnregisterAlivePlayer(PlayerController inPlayerController)
    {
        if (!players.Contains(inPlayerController))
        {
            Debug.LogError("PlayerController is not registered");
            return;
        }    
        else
        {
            players.Remove(inPlayerController);
            PlayerCountChange?.Invoke(players.Count);
            if (players.Count == 1)
            {
                EndGame(players[0]);
            }
            else if (players.Count == 0)
            {
                EndGame();
            }
        }
    }

    // UI�󿡼� �Է� �޾Ƽ� ���� ����
    public void StartGame()
    {
        // ���� ����
        currentGameState = GameState.Play;
        GamePlayTimeChange?.Invoke(currentTime);
    }
}
