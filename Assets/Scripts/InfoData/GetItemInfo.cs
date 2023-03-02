using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItemInfo 
{
    private int _attributeType = -1;
    private string _titleName = "";
    private string _itemName = "";
    private string _resultName = "";

    public GetItemInfo()
    {
    }

    public void SetSkillData(int attributeType,string itemName)
    {
        _attributeType = attributeType;
        _itemName = itemName;
    }
    public void SetTitleData(string titleName)
    {
        _titleName = titleName;
    }


    public void SetResultData(string resultName)
    {
        _resultName = resultName;
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
