using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public enum GunState
{
    None,
    Attack,
    Reload
}
public class Gun : NetworkBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform fireTransform;
    [SerializeField] private SoundSpawner fireSound;
    [SerializeField] private SoundSpawner reloadSound;

    public Animator animator;

    public GunState CurrentGunState { get; private set; }
    private bool isReloading;
    private float defaultReloadTime; //�⺻ źâ ä��� �ð�
    private float equiptReloadTime; //���� ����
    private float restReloadTime; //ä��� ���� ���� �ð�
   

    private int defaultBulletCount; //�⺻ �Ѿ� ��
    [SerializeField] private int equiptBulletCount; //���� ����
    [SerializeField] private int restBulletCount; //���� �Ѿ� ��
    [SerializeField] private int defaultRps; //���� ����
    [SerializeField] private int equiptRps; //1�ʴ� �Ѿ� �߻� ����

    [SerializeField] public bool canShoot = true; // �� �߻� ���� ���� (������ �� �ѱ� �߻� X �������� ����)

    [SerializeField] private bool isRating; //����ӵ����
    [SerializeField] private float rateTime;
    [SerializeField] float restRateTime;

    [SerializeField] private float defaultFocusRegion; //ź ������ : Ŭ���� ������.
    [SerializeField] public float equiptFocusRegion;

    public static event Action<int> OnChageAmmoUI;
    private bool isStateLock;

    [Header("Effect")]
    [SerializeField] private EffectSpawner fireEffectSpawner;
    [SerializeField] private EffectSpawner shellEffectSpawner;

    private void Awake()
    {
        defaultReloadTime = 2f;
        restReloadTime = 0f;

        defaultRps = 4;
        restRateTime = 0f;

        defaultBulletCount = 30;
        restBulletCount = defaultBulletCount;

        defaultFocusRegion = 1f; //���� �ݰ�
        isRating = false;
        isReloading = false;
        isStateLock = false;
        CurrentGunState = GunState.None;
        ResetEquiptValue();
    }

    private void Start()
    {
        FindTransform();
        OnChangeAmmo();
    }

    private void FindTransform()
    {
        fireTransform = transform.Find("fireTransform");
    }

    [ServerRpc]
    private void SpawnBulletServerRpc(Vector3 inDirection)
    {
        Bullet bullet = BulletPool.Instance.GetBullet();
        bullet.SetBulletInfo(fireTransform.position, inDirection);
    }

    public void Fire(Vector3 inDirection)
    {
        if (isReloading == true || isRating == true)
        {
            return;
        }
        if (HaveBullet() == false)
        {
            return;
        }
        //������ - �������� �ѹ��� Ȯ���ؼ� -> ����
        //�������� �Ѿ˸����

        fireEffectSpawner.PlayEffect();
        shellEffectSpawner.PlayEffect();
        SpawnBulletServerRpc(inDirection);
        fireSound.PlaySound();


        isRating = true;
        restRateTime = rateTime;
        restBulletCount -= 1;
        OnChangeAmmo();
    }

    public void Reload()
    {
        ReloadRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void ReloadRpc()
    {
        if (isReloading == true || IsFullBullet() == true)
        {
            return;
        }
        isReloading = true;
        restReloadTime = equiptReloadTime;
        reloadSound.PlaySound();
    }

    public void EquiptItems(ItemBase[] inEquiptItems)
    {
        ResetEquiptValue();
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
        if (inEquiptItem == null || inEquiptItem.itemType == ItemMainType.None)
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
        if (restBulletCount <= 0)
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
        if (isReloading == false)
        {
            return;
        }
        restReloadTime -= Time.deltaTime;
        if (restReloadTime <= 0)
        {
            isReloading = false;
            restBulletCount = equiptBulletCount;
            OnChangeAmmo();
            DoneRateTime();
        }
    }

    private void CountRateTime()
    {
        if (isReloading == true || isRating == false)
        {
            return;
        }
        restRateTime -= Time.deltaTime;
        if (restRateTime <= 0)
        {
            DoneRateTime();
        }
    }

    private void DoneRateTime()
    {
        isRating = false;
    }

    public void ChangeGunState(GunState inGunState)
    {
        if (isStateLock)
        {
            return;
        }
        if (CurrentGunState != inGunState)
        {
            Logger.Warning($"gunstateChanged :{CurrentGunState} -> {inGunState}");
            CurrentGunState = inGunState;
            switch (inGunState)
            {
                case GunState.None:
                    break;
                case GunState.Attack:
                    SetGunAnimatorTrigger(inGunState);
                    StartCoroutine(ChangeGunStateCoroutine(CurrentGunState, rateTime));
                    break;
                case GunState.Reload:
                    Reload();
                    SetGunAnimatorTrigger(inGunState);
                    StartCoroutine(ChangeGunStateCoroutine(CurrentGunState, equiptReloadTime));
                    break;
                default:
                    Logger.Log("FatalErreo :: Tried State Change is Not Allowed");
                    break;
            }
        }
    }

    public IEnumerator ChangeGunStateCoroutine(GunState inState, float inDelay)
    {
        isStateLock = true;
        yield return new WaitForSeconds(inDelay);
        isStateLock = false;

        if (CurrentGunState == inState)
        {
            CurrentGunState = GunState.None;
        }
    }
    public void SetGunAnimatorTrigger(GunState inState)
    {
        animator.SetTrigger(inState.ToString());
    }

    public void SetGunAnimatorBool(GunState inState, bool inBoolState)
    {
        animator.SetBool(inState.ToString(), inBoolState);
    }
    private void CalRateTime()
    {
        rateTime = 1f /equiptRps; //����ӵ�
    }

    private void OnChangeAmmo()
    {
        if (IsOwner)
        {
            OnChageAmmoUI?.Invoke(restBulletCount);
        }
        
    }
}
