using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BoxDetector : NetworkBehaviour
{
    private List<DropBox> inRangeBoxList; //���� �ȿ� ���� ���ڵ�

    private void Awake()
    {
        inRangeBoxList = new();
    }

    public DropBox GetNearestBox()
    {
        DropBox detectedBox = null;
        float minDisstance = float.MaxValue;
        //���ڸ���Ʈ�� �� ���� �����ָ� ��ȯ
        for (int i = 0; i < inRangeBoxList.Count; i++)
        {
            float curDistance = Vector3.Distance(inRangeBoxList[i].transform.position, transform.position);
            if (curDistance < minDisstance)
            {
                detectedBox = inRangeBoxList[i];
                minDisstance = curDistance;
            }
        }
        return detectedBox;
    }

    private void OnTriggerEnter(Collider other)
    {
       // Debug.Log("�ڽ������Ϳ��� ����" + other.gameObject.name);
        DropBox box = other.gameObject.GetComponent<DropBox>();
        if(box == null)
        {
            return;
        }
        inRangeBoxList.Add(box);
    }

 
    private void OnTriggerExit(Collider other)
    {
      //  Debug.Log("�ڽ������Ϳ��� ����" + other.gameObject.name);
        DropBox box = other.gameObject.GetComponent<DropBox>();
        if (box == null)
        {
            return;
        }
        if (IsOwner)
        {
            box.CloseBox();
        }
        inRangeBoxList.Remove(box);
    }
}
