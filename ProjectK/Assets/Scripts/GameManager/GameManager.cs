using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private float maxPlayTime;
    private float currentTime;
    [SerializeField] 
    private float PlayTime;
    private int remainPlayerCount;

    private LayerMask playerLayer;

    public static event Action<int> PlayerCountChange;
    public static event Action<float> GamePlayTimeChange;
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

        PlayTime = 5f;
        maxPlayTime = 60 * PlayTime;  // �д���
        currentTime = maxPlayTime;

        playerLayer = 1 << 8;
    }

    private void OnEnable()
    {
        InitGame();
    }

    private void InitGame()
    {
        remainPlayerCount = CountRemainPlayer(playerLayer);
        PlayerCountChange?.Invoke(remainPlayerCount);
    }

    // ���� ���� UI ����
    private void ResetGame()
    {

    }

    private void Update()
    {
        if (currentTime < 0)
        {
            ResetGame();
        }

        if (remainPlayerCount <= 1)
        {
            ResetGame();
        }

        // �װ� �ƴϸ� ������ ��� �����Ѵ�
        currentTime -= Time.deltaTime;
        GamePlayTimeChange?.Invoke(currentTime);


    }

    [Obsolete]
    private int CountRemainPlayer(LayerMask playerLayer)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        int count = 0;
        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & playerLayer.value) != 0)
            {
                count++;
            }
        }

        return count;
    }

    public void DiscountPlayerCount()
    {
        remainPlayerCount--;    
        PlayerCountChange?.Invoke(remainPlayerCount);
    }
}
