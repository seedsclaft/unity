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
        SetTitleData(DataSystem.System.GetReplaceText(3000,name));
        var trainResult = DataSystem.System.GetReplaceText(3001,lv.ToString());
        if (isBonus)
        {
            trainResult += " " + DataSystem.System.GetTextData(3031).Text;
        }
        SetResultData(trainResult);
    }

    public void MakeAlchemyResult(string name,SkillsData.SkillData skillData)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3000,name));
        SetSkillElementId((int)skillData.Attribute);
        string magicAlchemy = skillData.Name;
        SetResultData(DataSystem.System.GetReplaceText(3002,magicAlchemy));    
    }

    public void MakeAlchemyBonusResult(SkillsData.SkillData skillData)
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

    public void MakeTrainCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3030,DataSystem.System.GetTextData(1).Text));
        SetResultData("Lv." + commandLv.ToString() + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeAlchemyCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3030,DataSystem.System.GetTextData(2).Text));
        SetResultData("Lv." + commandLv.ToString() + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeRecoveryCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3030,DataSystem.System.GetTextData(3).Text));
        SetResultData("Lv." + commandLv + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeResourceCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3030,DataSystem.System.GetTextData(5).Text));
        SetResultData("Lv." + commandLv.ToString() + " ⇒ " + (commandLv+1).ToString());
    }

    public void MakeBattleCommandResult(int commandLv)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3030,DataSystem.System.GetTextData(4).Text));
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

    public void MakeCommandRebornResult(TacticsComandType tacticsComandType,int bonus)
    {
        switch (tacticsComandType)
        {
            case TacticsComandType.Train:
            MakeTrainCommandResult(bonus);
            break;
            case TacticsComandType.Alchemy:
            MakeAlchemyCommandResult(bonus);
            break;
            case TacticsComandType.Recovery:
            MakeRecoveryCommandResult(bonus);
            break;
            case TacticsComandType.Battle:
            MakeBattleCommandResult(bonus);
            break;
            case TacticsComandType.Resource:
            MakeResourceCommandResult(bonus);
            break;
        }

    }

    public void MakeStatusRebornResult(string name, StatusParamType statusParamType,int bonus)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3070,name));
        var result = "";
        switch (statusParamType)
        {
            case StatusParamType.Hp:
            result += DataSystem.System.GetReplaceText(3071,DataSystem.System.GetTextData(300).Text);
            break;
            case StatusParamType.Mp:
            result += DataSystem.System.GetReplaceText(3071,DataSystem.System.GetTextData(301).Text);
            break;
            case StatusParamType.Atk:
            result += DataSystem.System.GetReplaceText(3071,DataSystem.System.GetTextData(302).Text);
            break;
            case StatusParamType.Def:
            result += DataSystem.System.GetReplaceText(3071,DataSystem.System.GetTextData(303).Text);
            break;
            case StatusParamType.Spd:
            result += DataSystem.System.GetReplaceText(3071,DataSystem.System.GetTextData(304).Text);
            break;
        }
        SetResultData(result + DataSystem.System.GetReplaceText(3072,bonus.ToString()));

    }
    
    public void MakeQuestRebornResult(string name, StatusParamType statusParamType,int bonus)
    {
        SetTitleData(DataSystem.System.GetReplaceText(3070,name));
        SetResultData(DataSystem.System.GetReplaceText(3073,bonus.ToString()));
    }

    
}
