using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Bullet : NetworkBehaviour, ISpawnable
{
    private float defaultLifeTime;
    private float lifeTime;
    private float speed;
    private float damage;
    private Vector3 direction;
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>();
    private uint ownerNetworkId;

    private void Awake()
    {
        defaultLifeTime = 5.0f;
        lifeTime = defaultLifeTime;
        speed = 14.0f;
        damage = 30f;
    }


    protected override void InternalOnNetworkPostSpawn()
    {
        //Debug.Log("���� �Ǿ���.2");
        //OnNetworkSpawn - �����ɋ�
        //Internal(�����Լ�) - �����ǰ� ���� 
        if (IsServer == false)
        {
            //OnNetworkSpawn���� ��Ȱ��ȭ �ع����� NetWorkObejct�� Null�� ��
            gameObject.SetActive(isActive.Value);
        }
    }

    public void SetBulletInfo(Vector3 inSpawnPosition, Vector3 inDirection)
    {
        //�ѿ��� ���� ȣ��
        ControlActive(true); //�Ѿ� ����
        //transform.position = inSpawnPosition;
        var netTransform = GetComponent<NetworkTransform>();
        //��ġ�� ������ġ�� �����̵� ���Ѽ� ����ȭ
        netTransform.Teleport(inSpawnPosition, Quaternion.identity, transform.localScale);
        direction = inDirection;
        lifeTime = defaultLifeTime;
    }

    #region �Ѿ� �ൿ
    private void Update()
    {
        if (IsHost == false)
        {
            return;
        }

        Move();
        CountLifeTime();
    }

    public void SetOwner(uint inOwnerId)
    {
        ownerNetworkId = inOwnerId;
    }

    public uint GetOwner()
    {
        return ownerNetworkId;
    }

    private void Move()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }

    private void CountLifeTime()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            //Destroy(gameObject);
            BulletOff();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsHost)
        {
            // �������� �� �� �ִٸ�
            if (other.TryGetComponent<ITakeDamage>(out var target))
            {
                bool isSelfOwnedSpawnedObject = false;

                if (other.TryGetComponent<ISpawnable>(out var spawnObj))
                {
                    if (spawnObj.GetOwner() == ownerNetworkId)
                    {
                        isSelfOwnedSpawnedObject = true;
                    }
                }

                if (!isSelfOwnedSpawnedObject)
                {
                    target.TakeDamage(damage);
                }
            }

            GetComponent<NetworkObject>().Despawn();
        }
    }
    #endregion


    #region �Ѿ� Ȱ��ȭ ����ȭ
    private void BulletOff()
    {
        if (IsHost == false)
        {
           return;
        }
        ControlActive(false);
        BulletPool.Instance.Recycle(this);
    }

    public void ControlActive(bool inActive)
    {
        if (IsHost == false)
        {
            return;
        }
        isActive.Value = inActive;
        ActiveRpc(inActive);
    }

    [Rpc(SendTo.Everyone)]
    private void ActiveRpc(bool inActive)
    {
        gameObject.SetActive(inActive);
    }
    #endregion
}
