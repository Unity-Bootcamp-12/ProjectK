
using System.Collections.Generic;
using UnityEngine;


public class MasterDataManager :MonoBehaviour
{
    public static MasterDataManager Instance;
    public DropBoxTable DropBox;
    private Dictionary<int, ItemBase> masterItemDictionary;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            MakeMasterData();
            DropBox = new();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void MakeMasterData()
    {
        masterItemDictionary = new();
        ItemBase compensator = new ItemBase(1121, ItemMainType.AttachMent, ItemSubType.Muzzle, ItemUseType.Equipt, "보정기", Stat.Focus, 30);
        masterItemDictionary.Add(compensator.id, compensator);
        ItemBase treeBox = new ItemBase(2472, ItemMainType.Expendables, ItemSubType.Deploy, ItemUseType.Deploy, "나무상자", Stat.Hp, 100);
        masterItemDictionary.Add(treeBox.id, treeBox);
    }

    public Dictionary<int, ItemBase> GetMasterDataDic()
    {
        return masterItemDictionary;
    }

    public ItemBase GetMasterItemData(int inItemID)
    {
        return masterItemDictionary[inItemID];
    }
}

