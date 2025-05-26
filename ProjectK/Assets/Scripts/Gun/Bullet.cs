using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour, ISpawnable
{
    private float defaultLifeTime;
    private float lifeTime;
    private float speed;
    private float damage;
    private Vector3 direction;
    private uint ownerNetworkId;

    private void Awake()
    {
        //���� �ʱⰪ 
        defaultLifeTime = 5.0f;
        lifeTime = defaultLifeTime;
        speed = 14.0f;
        damage = 30f;
    }

    private void Update()
    {
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
    public void SetDirection(Vector3 inDirection)
    {
        //�ѿ��� ���� ȣ��
        direction = inDirection;
    }
    private void CountLifeTime()
    {
        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
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
}
