using System;

[Serializable]
public class ItemInfo
{
    private int _id;
    public int Id {get {return _id;}}
    private int _useCount;
    public int UseCount {get {return _useCount;}}


    public ItemInfo(ItemsData.ItemData itemInfo)
    {
        _id = itemInfo.Id;
        _useCount = itemInfo.UseCount;
    }
};