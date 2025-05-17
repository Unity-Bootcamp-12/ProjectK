using System;
using UnityEngine;



public class Gun : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform fireTransform;

    public static event Action<Vector3> OnFire;
    private bool isReloading;
    private float defaultReloadTime; //�⺻ źâ ä��� �ð�
    private float restReloadTime; //ä��� ���� ���� �ð�

    private int defaultBulletCount; //�⺻ �Ѿ� ��
    private int restBulletCount; //���� �Ѿ� ��

    private int defaultRps; //1�ʴ� �Ѿ� �߻� ����

    private bool isRating; //����ӵ����
    private float defaultRateTime;
    private float restRateTime;

    private float focusRegion; //ź ������ : Ŭ���� ������.

    private void Awake()
    {
        defaultReloadTime = 2f; //�����ð�
        restReloadTime = 0f;

        defaultRps = 15; //�ʴ� �߻� ����
        CalRateTime();
        restRateTime = 0f;

        defaultBulletCount = 15; //źâ �뷮
        restBulletCount = defaultBulletCount;

        focusRegion = 1f; //���� �ݰ�

        isRating = false;
        isReloading = false;
    }

    private void Start()
    {
        FindTransform();
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
        //  Debug.Log("�� �߻�");
        Debug.Log(inDirection);
        Bullet bullet = Instantiate(bulletPrefab, fireTransform.position, Quaternion.identity);
        bullet.SetDirection(inDirection);

        if(OnFire != null)
        {
            OnFire.Invoke(transform.position);
        }

        isRating = true;
        restRateTime = defaultRateTime;
        restBulletCount -= 1;
    }

    public void Reload()
    {
        if(isReloading == true || IsFullBullet() == true)
        {
            //���� ���̰ų� Ǯźâ�̸� ���ε� ����
            return;
        }
        isReloading = true;
        restReloadTime = defaultReloadTime;
    }

    public void AttachEquiptment(ItemBase inEquiptItem)
    {
        if(inEquiptItem == null)
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
                focusRegion -= power;
                break;
            case Stat.AmmoSize:
                defaultBulletCount += power;
                break;
            case Stat.ReloadTime:
                defaultReloadTime -= power;
                break;
            case Stat.Rps:
                defaultRateTime += power;
                CalRateTime();
                break;
        }
    }

    public void DetachEquiptment(ItemBase inEquiptItem)
    {
        if(inEquiptItem == null)
        {
            return;
        }

        ItemMainType mainType = inEquiptItem.itemType;
        if (mainType != ItemMainType.AttachMent)
        {
            Debug.LogError("�������� �ƴմϴ�.");
            return;
        }
        Stat targetStat = inEquiptItem.stat;
        int power = inEquiptItem.power;
        switch (targetStat)
        {
            case Stat.Focus:
                focusRegion += power;
                break;
            case Stat.AmmoSize:
                defaultBulletCount -= power;
                break;
            case Stat.ReloadTime:
                defaultReloadTime += power;
                break;
            case Stat.Rps:
                defaultRateTime -= power;
                CalRateTime();
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
        return defaultBulletCount == restBulletCount;
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
            restBulletCount = defaultBulletCount;
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
        defaultRateTime = defaultRps / 60f; //����ӵ�
    }
}
