using System.Collections.Generic;

[System.Serializable]
public class GetItemInfo 
{
    private GetItemType _getItemType = GetItemType.None;
    public GetItemType GetItemType => _getItemType;
    private int _param1 = -1;
    public int Param1 => _param1;
    private int _param2 = -1;
    public int Param2 => _param2;
    private int _skillElementId = -1;
    public int SkillElementId => _skillElementId;
    private string _titleName = "";
    public string TitleName => _titleName;
    private string _resultName = "";
    public string ResultName => _resultName;

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
        return _getItemType == GetItemType.Skill;
    }

    public bool IsAttributeSkill()
    {
        return (int)_getItemType >= (int)GetItemType.AttributeFire && (int)_getItemType <= (int)GetItemType.AttributeDark;
    }

    public void MakeTrainResult(string name,int lv,bool isBonus)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3000,name));
        var trainResult = DataSystem.System.GetReplaceText(3001,lv.ToString());
        if (isBonus)
        {
            trainResult += " " + DataSystem.System.GetTextData(3031).Text;
        }
        SetResultData(trainResult);
    }

    public void MakeAlchemyResult(string name,SkillData skillData)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3000,name));
        SetSkillElementId((int)skillData.Attribute);
        var magicAlchemy = skillData.Name;
        SetResultData(DataSystem.System.GetReplaceText(3002,magicAlchemy));    
    }

    public void MakeAlchemyBonusResult(SkillData skillData)
    {
        SetTitleData(DataSystem.System.GetTextData(14040).Text);
        SetResultData(skillData.Name);
        SetSkillElementId((int)skillData.Attribute);            
    }

    public void MakeRecoveryResult(string name)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3010,name));
        SetResultData(DataSystem.System.GetTextData(3011).Text);
    }

    public void MakeRecoveryBonusResult(string name)
    {
        SetTitleData(DataSystem.System.GetTextData(3012).Text);
        SetResultData(DataSystem.System.GetReplaceText(3013,name));
    }

    public void MakeCommandCountResult(int commandLv,TacticsCommandType tacticsCommandType)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3030,DataSystem.System.GetTextData((int)tacticsCommandType).Text));
        SetResultData(DataSystem.System.GetTextData(316).Text + commandLv.ToString() + DataSystem.System.GetTextData(3090).Text + (commandLv+1).ToString());
    }

    public void MakeCurrencyResult(int value)
    {
        SetTitleData(DataSystem.System.GetTextData(14041).Text);
        SetResultData("+" + value.ToString() + DataSystem.System.GetTextData(1000).Text);
    }

    public void MakeDemigodResult(int value)
    {
        SetTitleData(DataSystem.System.GetTextData(14042).Text);
        SetResultData("+" + value.ToString());
    }

    public void MakeStatusRebornResult(string name, StatusParamType statusParamType,int bonus)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3070,name));
        var result = "";
        var textId = 300 + (int)statusParamType;result += DataSystem.System.GetReplaceText(3071,DataSystem.System.GetTextData(textId).Text);
        SetResultData(result + DataSystem.System.GetReplaceText(3072,bonus.ToString()));
    }
    
    public void MakeQuestRebornResult(string name, StatusParamType statusParamType,int bonus)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3070,name));
        SetResultData(DataSystem.System.GetReplaceText(3073,bonus.ToString()));
    }

    
}
