using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItemInfo 
{
    private int _attributeType = -1;
    private string _itemName = "";

    public GetItemInfo(int attributeType,string itemName)
    {
        _attributeType = attributeType;
        _itemName = itemName;
    }

    public bool IsSkill()
    {
        return _attributeType > 0;
    }

    public string GetItemName()
    {
        return _itemName;
    }
}
