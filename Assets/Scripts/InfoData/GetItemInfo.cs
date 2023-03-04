using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItemInfo 
{
    private int _attributeType = 0;
    public int AttributeType{
        get {return _attributeType;}
    }
    private string _titleName = "";
    public string TitleName{
        get {return _titleName;}
    }
    private string _resultName = "";
    public string ResultName{
        get {return _resultName;}
    }

    public GetItemInfo()
    {
    }

    public void SetAttributeType(int attributeType)
    {
        _attributeType = attributeType;
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

}
