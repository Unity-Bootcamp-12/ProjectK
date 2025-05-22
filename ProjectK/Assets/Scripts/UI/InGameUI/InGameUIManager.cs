using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    public PlayerController playerController;
    private Slider hpSlider;
    private RectTransform hpSliderRect;

    private Transform targetPlayerTransform; // ������ ��� (�÷��̾� Transform)
    private Vector3 offset; // �Ӹ� �� ��ġ ������

    private void Awake()
    {
        offset = new Vector3(0, 3f, 0);
    }

    private void Start()
    {
        PlayerController.OnChangeHpUI += UpdateHpIngameUI;
        InputManager.OnLocalPlayerRegistered += SetPlayerController;

        hpSlider = transform.Find("InGameHpSlider").GetComponent<Slider>();
        hpSlider.maxValue = 100f;
        hpSliderRect = hpSlider.GetComponent<RectTransform>();
    }

    private void Update()
    {
        //if (playerController != null)
        {
            TrackingPlayer();
        }
    }
    private void SetPlayerController(PlayerController inPlayerController)
    {
        playerController = inPlayerController;

        if (playerController != null)
        {
            targetPlayerTransform = playerController.transform;
        }
    }
    private void TrackingPlayer()
    {
        // ���� ��ġ + offset�� ȭ�� ��ǥ�� ��ȯ
        Vector3 worldPosition = targetPlayerTransform.position + offset;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        hpSliderRect.position = screenPosition;
    }

    private void UpdateHpIngameUI(float inHp)
    {
        hpSlider.value = inHp;

        if(hpSlider.value <= 0)
        {
            hpSlider.fillRect.GetComponent<Image>().color = Color.clear;
        }
        else
        {
            hpSlider.fillRect.GetComponent<Image>().color = Color.red;
        }
    }
}
