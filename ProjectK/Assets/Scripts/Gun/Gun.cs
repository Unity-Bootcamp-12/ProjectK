using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform fireTransform;
  
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

    public void Fire()
    {
        Debug.Log("�� �߻�");
        Vector3 direction = fireTransform.position - playerTransform.position;
        GameObject bullet = Instantiate(bulletPrefab, fireTransform);
    }

}
