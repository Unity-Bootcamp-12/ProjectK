
using System;
using System.Collections.Generic;
using UnityEngine;

public class DropBox : MonoBehaviour
{
    [SerializeField] private List<ItemBase> haveItems;
    private float startChance;
    private float stackRate; //찬스가 중첩 될수록 변하는 비율
    private int maxRollCount;
    public static event Action<DropBox, PlayerController> OnOpenBox;
    public static event Action OnCloseBox;
    public static event Action<DropBox> OnChangeBox;

    private void Awake()
    {
        haveItems = new();
        startChance = 100;
        stackRate = 0.8f;
        maxRollCount = 5;
    }

    private void Start()
    {
        DiceItem();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            DiceItem();
        }
    }

    private void DiceItem()
    {
        haveItems.Clear();
        startChance = 100;
        for (int i = 1; i <= maxRollCount; i++)
        {
            int rollNum = UnityEngine.Random.Range(1, 101);
            if (startChance < rollNum)
            {
                //아이템 뽑기 실패 했으면 종료
                break;
            }
            int mainTypeNum = UnityEngine.Random.Range(1, 3);
            ItemMainType mainType = (ItemMainType)mainTypeNum;
            ItemBase rollItem = MasterDataManager.Instance.DropBox.RollDropBox(mainType);
            if (rollItem == null)
            {
                Debug.Log("뽑기 실패");
            }
            else
            {
                Debug.Log(rollItem.name + "을 뽑았다.");
                haveItems.Add(rollItem);
            }
            //뽑았으면 확률 조정
            startChance *= stackRate;

        }
    }

    public void OpenBox(PlayerController inOpenCharactor)
    {
        Debug.Log("박스를 열었다.");
        OnOpenBox.Invoke(this, inOpenCharactor);
    }

    public void CloseBox()
    {
        Debug.Log("박스를 닫았다.");
        OnCloseBox.Invoke();
    }

    public List<ItemBase> GetBoxItemList()
    {
        return haveItems;
    }

    public void SelectItem(int inSlotIndex, PlayerInventory inPlayerInventory)
    {
        if (haveItems.Count <= inSlotIndex)
        {
            return;
        }

        ItemBase returnItem = inPlayerInventory.PickItem(haveItems[inSlotIndex]); //
        if (returnItem == null)
        {
            haveItems.RemoveAt(inSlotIndex);
        }
        else
        {
            haveItems[inSlotIndex] = returnItem;
        }

        OnChangeBox.Invoke(this);
    }

}

