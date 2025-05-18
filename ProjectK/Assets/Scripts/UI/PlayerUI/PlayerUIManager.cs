using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    private PlayerController playerController;
    public PlayerStat playerStat;
    [SerializeField] private Transform crosshairTransform;
    private Slider hpSlider;
    private Slider staminaSlider;

    private void OnEnable()
    {
        InputManager.OnLocalPlayerRegistered += SetPlayerController;
    }

    private void OnDisable()
    {
        InputManager.OnLocalPlayerRegistered -= SetPlayerController;
    }

    private void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }
    private void Start()
    {
        playerStat = playerController.GetComponent<PlayerStat>();
        crosshairTransform = transform.Find("Crosshair");
        hpSlider = transform.Find("PlayerHUDPanel/HpSlider").GetComponent<Slider>();
        staminaSlider = transform.Find("PlayerHUDPanel/StaminaSlider").GetComponent<Slider>();

        // hp��, ���׹̳��� �ʱ�ȭ
        hpSlider.maxValue = 100f;
        staminaSlider.maxValue = 100f;
        if (playerStat != null)
        {
            hpSlider.value = playerStat.GetHP();
            staminaSlider.value = playerStat.GetStamina();
        }
        else
        {
            Debug.LogError("playerStat�� ������ �־����� �ν����Ϳ��� Ȯ�� �ʿ�!");
        }
    }

    void Update()
    {
        crosshairTransform.position = Camera.main.WorldToScreenPoint(playerController.mouseWorldPosition);
    }
}
