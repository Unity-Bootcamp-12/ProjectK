using System;
using UnityEngine;



public class Gun : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform fireTransform;

    public static event Action<Vector3> OnFire;
    private bool isReloading;
    private float defaultReloadTime; //�⺻ źâ ä��� �ð�
    private float equiptReloadTime; //���� ����
    private float restReloadTime; //ä��� ���� ���� �ð�

    private int defaultBulletCount; //�⺻ �Ѿ� ��
    private int equiptBulletCount; //���� ����
    private int restBulletCount; //���� �Ѿ� ��

    private int defaultRps; //���� ����
    private int equiptRps; //1�ʴ� �Ѿ� �߻� ����
    
    private bool isRating; //����ӵ����
    private float rateTime;
    private float restRateTime;

    private float defaultFocusRegion; //ź ������ : Ŭ���� ������.
    private float equiptFocusRegion;

    public static event Action<int> OnChageAmmoUI;


    private void Awake()
    {
        defaultReloadTime = 2f;
        restReloadTime = 0f;

        defaultRps = 15;
        restRateTime = 0f;

        defaultBulletCount = 30;
        restBulletCount = defaultBulletCount;

        defaultFocusRegion = 1f; //���� �ݰ�
  
        isRating = false;
        isReloading = false;
        ResetEquiptValue();
    }

    private void Start()
    {
        FindTransform();

        OnChageAmmoUI?.Invoke(restBulletCount);
    }
    
    private void FindTransform()
    {
        //�Ѿ� ������ ��ġ 
        fireTransform = transform.Find("fireTransform");
    }

    public void Fire(Vector3 inDirection)
    {
       // Debug.Log("�� �߻� ��û");
        if (isReloading == true || isRating == true )
        {
            return;
        }
        if(HaveBullet() == false)
        {
            return;
        }
        Bullet bullet = Instantiate(bulletPrefab, fireTransform.position, Quaternion.LookRotation(inDirection));
        bullet.SetDirection(inDirection);

        if(OnFire != null)
        {
            OnFire.Invoke(transform.position);
        }

        isRating = true;
        restRateTime = rateTime;
        restBulletCount -= 1;

        OnChageAmmoUI?.Invoke(restBulletCount);
    }

    public void Reload()
    {
        if(isReloading == true || IsFullBullet() == true)
        {
            //���� ���̰ų� Ǯźâ�̸� ���ε� ����
            return;
        }
        isReloading = true;
        restReloadTime = equiptReloadTime;
    }

    public void EquiptItems(ItemBase[] inEquiptItems)
    {
        ResetEquiptValue();
        //���� ���� ������ ����
        for (int i = 0; i < inEquiptItems.Length; i++)
        {
            AttachEquiptment(inEquiptItems[i]);
        }
    }

    private void ResetEquiptValue()
    {
        //��� ���� �� default�� �ʱ�ȭ
        equiptReloadTime = defaultReloadTime; //�����ð�
        equiptRps = defaultRps; //�ʴ� �߻� ����
        CalRateTime();
        equiptBulletCount = defaultBulletCount; //źâ �뷮
        equiptFocusRegion = defaultFocusRegion;
    }

    private void AttachEquiptment(ItemBase inEquiptItem)
    {
        if(inEquiptItem == null || inEquiptItem.itemType == ItemMainType.None)
        {
            return;
        }

        ItemMainType mainType = inEquiptItem.itemType;
        if(mainType != ItemMainType.AttachMent)
        {
            Debug.LogError("�������� �ƴմϴ�.");
            return;
        }

        Stat targetStat = inEquiptItem.stat;
        int power = inEquiptItem.power;
        switch (targetStat)
        {
            case Stat.Focus:
                equiptFocusRegion = defaultFocusRegion - power;
                break;
            case Stat.AmmoSize:
                equiptBulletCount = defaultBulletCount + power;
                break;
            case Stat.ReloadTime:
                equiptReloadTime = defaultReloadTime - power;
                break;
            case Stat.Rps:
                equiptRps = defaultRps + power;
                CalRateTime();
                break;
            default:
                Debug.LogWarning("���� ���� ���� ����");
                break;
        }
    }

    private void Update()
    {
        CountReloadTime();
        CountRateTime();
    }

    private bool HaveBullet()
    {
        if(restBulletCount <= 0)
        {
            return false;
        }
        return true;
    }

    private bool IsFullBullet()
    {
        return equiptBulletCount == restBulletCount;
    }

    private void CountReloadTime()
    {
        if(isReloading == false)
        {
            return;
        }
        restReloadTime -= Time.deltaTime;
        if(restReloadTime <= 0)
        {
            isReloading = false;
            restBulletCount = equiptBulletCount;

            OnChageAmmoUI?.Invoke(restBulletCount);

            DoneRateTime();
        }
    }

    private void CountRateTime()
    {
        //�������̰ų�, �����Ⱑ �ƴϸ� ����
        if(isReloading == true || isRating == false)
        {
            return;
        }
        restRateTime -= Time.deltaTime;
        if(restRateTime <= 0)
        {
            DoneRateTime();
        }
    }

    private void DoneRateTime()
    {
        isRating = false;
    }

    private void CalRateTime()
    {
        rateTime = 1f /equiptRps; //����ӵ�
    }

  
}
