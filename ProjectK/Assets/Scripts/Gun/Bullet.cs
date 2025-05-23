using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private float defaultLifeTime;
    private float lifeTime;
    private float speed;
    private float damage;
    private Vector3 direction;
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>(false);

    private void Awake()
    {
        gameObject.SetActive(isActive.Value);
        defaultLifeTime = 5.0f;
        lifeTime = defaultLifeTime;
        speed = 14.0f;
        damage = 30f;



    }
    public override void OnNetworkSpawn()
    {
        isActive.OnValueChanged += OnActiveValueChange;
    }

    public void SetDirection(Vector3 inSpawnPosition, Vector3 inDirection)
    {
        //�ѿ��� ���� ȣ��
        transform.position = inSpawnPosition;
        direction = inDirection;
        lifeTime = defaultLifeTime;
        ActiveRpc(true);
    }

    [Rpc(SendTo.Everyone)]
    private void ActiveRpc(bool inActive)
    {
        gameObject.SetActive(inActive);
    }

    private void Update()
    {
        if(IsHost == false)
        {
            Debug.Log("ȣ��Ʈ�� �ƴϴ�");
        
        }

        if (IsServer == false)
        {
            Debug.Log("������ �ƴϴ�");
    
        }

        if(IsClient == false)
        {
            Debug.Log("Ŭ��ƴ�");
        }

        Move();
        CountLifeTime();
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
            InActive();
        }
    }

    private void OnActiveValueChange(bool inPreActive, bool inActive)
    {
        Debug.Log("�ٲ��");
        gameObject.SetActive(inActive);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsHost)
        {
            ITakeDamage takeDamageObj = other.GetComponent<ITakeDamage>();
            //ü���� ����ȭ�ϸ� �ٸ� Ŭ���̾�Ʈ�� ü�µ� ����ȭ�� �ȴ�. 
            //���ϴ°� �� Ÿ���� �ѱ�� ������ - �� ������Ʈ�� ���� ��
            if (takeDamageObj != null)
            {
                takeDamageObj.TakeDamage(damage);
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    public void InActive()
    {
        if(IsHost == false)
        {
            Debug.Log("��Ȱ��ȭ�� �����������ֱ�");
            return;
        }
        ActiveRpc(false);
        BulletPool.Instance.Recycle(this);
    }

}
