using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public PlayerStat playerStat;

    [SerializeField] private Transform crosshairTransform;
    [SerializeField] private Button[] DropboxSlotButtons;
    private Slider hpSlider;
    private Slider staminaSlider;
    private GameObject dropBoxPanelObj;
    private bool isOnDropBoxPanel;

    private void Awake()
    {
        isOnDropBoxPanel = false;
    }

    private void Start()
    {
        crosshairTransform = transform.Find("Crosshair");

        StartSettingHUDUI();

        StartSettingDropBoxUI();
    }

    private void Update()
    {
        crosshairTransform.position = Input.mousePosition;
    }

    private void StartSettingHUDUI()
    {
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

    private void StartSettingDropBoxUI()
    {
        // ��ư�� ������ ���
        for (int i = 0; i < DropboxSlotButtons.Length; i++)
        {
            int index = i;
            Button button = DropboxSlotButtons[i];
            button.onClick.AddListener(delegate {
                DropBoxSlotClick(index, button);
            });
        }

        dropBoxPanelObj = transform.Find("DropBoxPanel").gameObject;
        dropBoxPanelObj.SetActive(false);
    }

    private void OnOffDorpBoxUI()
    {
        if(isOnDropBoxPanel == true)
        {
            dropBoxPanelObj?.SetActive(false);
            isOnDropBoxPanel = false;
        }
        else
        {
            dropBoxPanelObj?.SetActive(true);
            isOnDropBoxPanel = true;
        }
    }

    private void DropBoxSlotClick(int inIndex, Button inDropBoxIndex)
    {
        Debug.Log($"[{inIndex}]�� ���� Ŭ����");
        Debug.Log("Ŭ���� ������Ʈ �̸�: " + inDropBoxIndex.gameObject.name);
    }
}
