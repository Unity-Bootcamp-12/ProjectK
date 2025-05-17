using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DropBoxTable
{
    public ItemSubType[] GunTypes;
    public ItemSubType[] ExpendType;
    public Dictionary<ItemSubType, List<int>> rollTableDictionary;
    private Random random;
    public DropBoxTable()
    {
        random = new();
        GunTypes = new ItemSubType[]{ItemSubType.Bullet, ItemSubType.Muzzle,
                                     ItemSubType.Rail, ItemSubType.Magazine};
        ExpendType = new ItemSubType[]{ItemSubType.Recovery, ItemSubType.Stamina,
                                       ItemSubType.Deploy, ItemSubType.Throw};
        rollTableDictionary = new();
        foreach(KeyValuePair<int, ItemBase> pair in MasterDataManager.Instance.GetMasterDataDic()) 
        {
            ItemSubType subType = pair.Value.subType;
            if (rollTableDictionary.ContainsKey(subType) == false)
            {
                rollTableDictionary[subType] = new List<int>();
            }
            rollTableDictionary[subType].Add(pair.Key);
        }
    }

    public ItemBase RollDropBox(ItemMainType inItemMainType)
    {
        if(inItemMainType == ItemMainType.AttachMent)
        {
            return Roll(GunTypes);
        }
        else if(inItemMainType == ItemMainType.Expendables)
        {
            return Roll(ExpendType);
        }
        return null;
    }

    private ItemBase Roll(ItemSubType[] typeTable)
    {
        ItemBase rollItem = null;
        ItemSubType rollSubType = typeTable[ random.Next(0, typeTable.Length)];
        if(rollTableDictionary.TryGetValue(rollSubType, out List<int> idList))
        {
            int itemID = idList[random.Next(0, idList.Count)];
            rollItem = MasterDataManager.Instance.GetMasterItemData(itemID);
        }
        return rollItem;
    }

}

