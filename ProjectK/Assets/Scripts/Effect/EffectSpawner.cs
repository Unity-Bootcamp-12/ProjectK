using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EffectSpawner : NetworkBehaviour
{
    //�ش� ��ҿ� ȿ�� �߻�
    //Ǯ������ ����
    [SerializeField]
    private EffectObject effectPrefab;

    private Queue<EffectObject> readyPool;
    private int poolCount;

    private float playRate; //���� ����Ʈ���� �ð�
    private bool isReadyToPlay;

    private void Awake()
    {
        MakeEffectPool();
        playRate = 0.2f;
        isReadyToPlay = true;
    }

    private void MakeEffectPool()
    {
        poolCount = 10;
        readyPool = new();
        for (int i = 0; i < poolCount; i++)
        {
            EffectObject effectObject = Instantiate(effectPrefab, transform);
            effectObject.SetMaker(this);
            readyPool.Enqueue(effectObject);
        }
    }

    public void PlayEffect()
    {
        if(IsOwner == false)
        {
            return;
        }

        if(isReadyToPlay == false)
        {
           
            return;
        }

        PlayEffectRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void PlayEffectRpc()
    {
        if (readyPool.TryDequeue(out EffectObject effectObject))
        {
            effectObject.PlayEffect();
            effectObject.transform.position = transform.position;
            StartCoroutine(Timer());
            return;
        }
    }


    public void Recycle(EffectObject inEffectObject)
    {
        readyPool.Enqueue(inEffectObject);
    }

    IEnumerator Timer()
    {
        isReadyToPlay = false;
        yield return new WaitForSeconds(playRate);
        isReadyToPlay = true;

    }
}
