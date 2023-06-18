using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        SetTitleData(DataSystem.System.GetTextData(3000).Text.Replace("\\d",name));
        var trainResult = DataSystem.System.GetTextData(3001).Text.Replace("\\d",lv.ToString());
        if (isBonus)
        {
            trainResult += " " + DataSystem.System.GetTextData(3031).Text;
        }
        SetResultData(trainResult);
    }

    public void MakeAlchemyResult(string name,SkillsData.SkillData skillData)
    {
        SetTitleData(DataSystem.System.GetTextData(3000).Text.Replace("\\d",name));
        SetSkillElementId((int)skillData.Attribute);
        string magicAlchemy = skillData.Name;
        SetResultData(DataSystem.System.GetTextData(3002).Text.Replace("\\d",magicAlchemy));    
    }

    public void MakeAlchemyBonusResult(SkillsData.SkillData skillData)
    {
        SetTitleData(DataSystem.System.GetTextData(14040).Text);
        SetResultData(skillData.Name);
        SetSkillElementId((int)skillData.Attribute);            
    }

    public void MakeRecoveryResult(string name)
    {
        SetTitleData(DataSystem.System.GetTextData(3010).Text.Replace("\\d",name));
        SetResultData(DataSystem.System.GetTextData(3011).Text);
    }

    public void MakeRecoveryBonusResult(string name)
    {
        SetTitleData(DataSystem.System.GetTextData(3012).Text);
        SetResultData(DataSystem.System.GetTextData(3013).Text.Replace("\\d",name));
    }

    public void MakeTrainCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(1).Text));
        SetResultData("Lv." + commandLv.ToString() + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeAlchemyCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(2).Text));
        SetResultData("Lv." + commandLv.ToString() + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeRecoveryCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(3).Text));
        SetResultData("Lv." + commandLv + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeResourceCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(5).Text));
        SetResultData("Lv." + commandLv.ToString() + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeBattleCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetTextData(3030).Text.Replace("\\d",DataSystem.System.GetTextData(4).Text));
        SetResultData("Lv." + commandLv + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeNuminosResult(int value)
    {
        SetTitleData(DataSystem.System.GetTextData(14041).Text);
        SetResultData("+" + value.ToString() + DataSystem.System.GetTextData(1000).Text);
    }

    public void MakeDemigodResult(int value)
    {
        SetTitleData(DataSystem.System.GetTextData(14042).Text);
        SetResultData("+" + value.ToString());
    }
}
