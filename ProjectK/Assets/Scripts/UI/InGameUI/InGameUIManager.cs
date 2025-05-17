using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    private PlayerStat playerStat;
    private Slider hpSlider;

    private void Start()
    {
        playerStat = GetComponentInParent<PlayerStat>();
        hpSlider = transform.Find("InGameHpSlider").GetComponent<Slider>();

        // hp�� �ʱ�ȭ
        hpSlider.maxValue = 100f;
        if (playerStat != null)
        {
            hpSlider.value = playerStat.GetHP();
        }

        // ī�޶� �������� �ٶ󺸰� �ϴ� �ڵ�
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
    }

    private void Update()
    {
        // ī�޶� �������� �ٶ󺸰� �ϴ� �ڵ�
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
    }
}
