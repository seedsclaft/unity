using System;

[Serializable]
public class ItemInfo
{
    private int _id;
    public int Id {get {return _id;}}


    public ItemInfo(ItemsData.ItemData itemInfo)
    {
        _id = itemInfo.Id;
    }
};