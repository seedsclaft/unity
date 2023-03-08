using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GetItemInfo 
{
    private GetItemType _getItemType = GetItemType.None;
    public GetItemType GetItemType{ get {return _getItemType;}}
    private int _param1 = -1;
    public int Param1{ get {return _param1;}}
    private int _param2 = -1;
    public int Param2{ get {return _param2;}}
    private int _skillElementId = -1;
    public int SkillElementId{ get {return _skillElementId;}}
    private string _titleName = "";
    public string TitleName{
        get {return _titleName;}
    }
    private string _resultName = "";
    public string ResultName{
        get {return _resultName;}
    }

    public GetItemInfo(GetItemData getItemData)
    {
        if (getItemData != null)
        {
            _param1 = getItemData.Param1;
            _param2 = getItemData.Param2;
            _getItemType = getItemData.Type;
        }
    }

    public void SetSkillElementId(int skillElementId)
    {
        _skillElementId = skillElementId;
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
        return _skillElementId > 0;
    }
}
