using UnityEngine;
using Unity.Netcode;

public class Camerafollow : NetworkBehaviour
{
    private Vector3 offSet; //ī�޶� �⺻ ��ġ

    private void Awake()
    {
        offSet = Camera.main.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
#if MULTI
        if(IsOwner == true){
        Camera.main.transform.position = transform.position + offSet;
        }
#else
        Camera.main.transform.position = transform.position + offSet;
#endif


    }
}
