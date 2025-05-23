using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private float defaultLifeTime;
    private float lifeTime;
    private float speed;
    private float damage;
    private Vector3 direction;

    private void Awake()
    {
        //���� �ʱⰪ 
        defaultLifeTime = 5.0f;
        lifeTime = defaultLifeTime;
        speed = 14.0f;
        damage = 30f;
    }

    public void SetDirection(Vector3 inDirection)
    {
        //�ѿ��� ���� ȣ��
        direction = inDirection;
    }

    private void Update()
    {
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
        if(lifeTime <= 0)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
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

}
