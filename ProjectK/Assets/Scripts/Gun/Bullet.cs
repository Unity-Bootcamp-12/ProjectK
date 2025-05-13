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

    public void Fire(Vector3 inDirect)
    {
        //�ѿ��� ���� ȣ��
        direction = inDirect;
    }

    private void Update()
    {
        Move();
        CountLifeTime();
    }

    private void Move()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void CountLifeTime()
    {
        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void Damage()
    {
        
    }

}
