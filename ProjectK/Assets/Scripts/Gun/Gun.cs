using UnityEngine;



public class Gun : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform fireTransform;

    private bool isReloading;
    private float defaultReloadTime; //�⺻ źâ ä��� �ð�
    private float restReloadTime; //ä��� ���� ���� �ð�

    private int defaultBulletCount; //�⺻ �Ѿ� ��
    private int restBulletCount; //���� �Ѿ� ��

    private bool isRating; //����ӵ����
    private float defaultRateTime;
    private float restRateTime;

    private void Awake()
    {
        defaultReloadTime = 2f; //�����ð�
        restReloadTime = 0f;

        defaultRateTime = 0.2f; //����ӵ�
        restRateTime = 0f;

        defaultBulletCount = 15;
        restBulletCount = defaultBulletCount;

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

        //�÷��̾� ��ġ
        Transform findTransform = transform;
        while (findTransform != null)
        {
            if (findTransform.name == "Player")
            {
                playerTransform = findTransform;
                break;
            }

            findTransform = findTransform.parent;
        }
        if (playerTransform == null)
        {
            Debug.LogError("�÷��̾� Ʈ������ ã�� ������");
        }
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
}
