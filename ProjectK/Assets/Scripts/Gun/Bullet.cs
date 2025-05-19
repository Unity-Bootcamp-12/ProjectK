using UnityEngine;

public class Bullet : MonoBehaviour
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
        speed = 10.0f;
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
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ITakeDamage takeDamageObj = other.GetComponent<ITakeDamage>();
        if (takeDamageObj != null)
        {
            takeDamageObj.TakeDamage(damage);
        }
    }
}
