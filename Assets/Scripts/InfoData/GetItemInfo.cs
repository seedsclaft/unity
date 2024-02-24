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
    private int _skillId = -1;
    public int SkillId => _skillId;
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
            MakeTextData();
        }
    }

    public void CopyData(GetItemInfo getItemInfo)
    {
        _param1 = getItemInfo.Param1;
        _param2 = getItemInfo.Param2;
        _getItemType = getItemInfo.GetItemType;
        _skillId = getItemInfo.SkillId;
        _titleName = getItemInfo.TitleName;
        _resultName = getItemInfo.ResultName;
    }

    public void SetParam2(int param2)
    {
        _param2 = param2;
    }

    public void MakeTextData()
    {
        switch (_getItemType)
        {
            case GetItemType.Numinous:
                SetTitleData(DataSystem.GetTextData(14090).Text + "+" + _param1.ToString() + DataSystem.GetTextData(1000).Text);
                SetResultData(DataSystem.GetTextData(14090).Text + "+" + _param1.ToString() + DataSystem.GetTextData(1000).Text);
                break;
            case GetItemType.Skill:
                var skillData = DataSystem.FindSkill(_param1);
                SetTitleData(skillData.Name);
                SetResultData(DataSystem.GetReplaceText(14110,skillData.Name));
                SetSkillId(skillData.Id);
                break;
            case GetItemType.Demigod:
                SetTitleData(DataSystem.GetTextData(14042).Text + "+" + _param1.ToString());
                SetResultData(DataSystem.GetTextData(14042).Text + "+" + _param1.ToString());
                break;
            case GetItemType.Ending:
                SetResultData(DataSystem.GetTextData(14060).Text);
                break;
            case GetItemType.StatusUp:
                SetTitleData(DataSystem.GetReplaceText(14070,_param1.ToString()));
                SetResultData(DataSystem.GetReplaceText(14070,_param1.ToString()));
                break;
            case GetItemType.Regeneration:
                SetTitleData(DataSystem.GetReplaceText(3240,_param1.ToString()));
                SetResultData(DataSystem.GetReplaceText(3240,_param1.ToString()));
                break;
            case GetItemType.ReBirth:
                SetResultData("ロスト復活");
                break;
            case GetItemType.LearnSkill:
                SetTitleData(DataSystem.FindSkill(_param2).Name);
                //SetTitleData(DataSystem.GetReplaceText(3000,DataSystem.FindActor(_param1).Name));
                SetResultData(DataSystem.GetReplaceText(3002,DataSystem.FindSkill(_param2).Name));
                break;
            case GetItemType.AddActor:
                SetTitleData(DataSystem.FindActor(_param1).Name);
                //SetTitleData(DataSystem.GetReplaceText(3003,DataSystem.FindActor(_param1).Name));
                SetResultData(DataSystem.GetReplaceText(14120,DataSystem.FindActor(_param1).Name));
                break;
            case GetItemType.SaveHuman:
                SetTitleData(DataSystem.GetTextData(14100).Text + DataSystem.GetReplaceDecimalText(_param2) + "/" + DataSystem.GetReplaceDecimalText(_param1));
                SetResultData(DataSystem.GetTextData(14100).Text + DataSystem.GetReplaceDecimalText(_param2) + "/" + DataSystem.GetReplaceDecimalText(_param1));
                break;
        }
    }

    public void SetSkillId(int skillId)
    {
        _skillId = skillId;
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


    public void MakeAlchemyBonusResult(SkillData skillData)
    {
        _titleName = (DataSystem.GetTextData(14040).Text);
        _resultName = (skillData.Name);
        SetSkillId(skillData.Id);            
    }


    public void MakeCommandCountResult(int commandLv,int toLv,TacticsCommandType tacticsCommandType)
    {
        _titleName = (DataSystem.GetReplaceText(3030,DataSystem.GetTextData((int)tacticsCommandType).Text));
        _resultName = (DataSystem.GetTextData(316).Text + commandLv.ToString() + DataSystem.GetTextData(3090).Text + toLv.ToString());
    }


    public void MakeStatusRebornResult(string name, StatusParamType statusParamType,int bonus)
    {
        _titleName = (DataSystem.GetReplaceText(3070,name));
        var result = "";
        var textId = 300 + (int)statusParamType;result += DataSystem.GetReplaceText(3071,DataSystem.GetTextData(textId).Text);
        _resultName = (result + DataSystem.GetReplaceText(3072,bonus.ToString()));
    }
    
    public void MakeQuestRebornResult(string name, StatusParamType statusParamType,int bonus)
    {
        _titleName = (DataSystem.GetReplaceText(3070,name));
        _resultName = (DataSystem.GetReplaceText(3073,bonus.ToString()));
    }

    public void MakeGainTurnResult(string turn)
    {
        _titleName = (DataSystem.GetTextData(3210).Text);
        _resultName = (DataSystem.GetReplaceText(3211,turn));
    }

    public void MakeActorLvUpResult(string name,int lv)
    {
        _titleName = (DataSystem.GetReplaceText(3000,name));
       _resultName = DataSystem.GetReplaceText(3001,lv.ToString());
    }

    public void MakeAlchemyCostZeroResult(string attributeText)
    {
        _titleName = (DataSystem.GetTextData(3220).Text);
        _resultName = (DataSystem.GetReplaceText(3221,attributeText));
    }

    public void MakeNoBattleLostResult()
    {
        _titleName = (DataSystem.GetTextData(3220).Text);
        _resultName = (DataSystem.GetTextData(3222).Text);
    }

    public void MakeResourceBonusResult()
    {
        _titleName = (DataSystem.GetTextData(3220).Text);
        _resultName = (DataSystem.GetTextData(3223).Text);
    }
    
    public void MakeCommandCostZeroResult(string commandText)
    {
        _titleName = (DataSystem.GetTextData(3220).Text);
        _resultName = (DataSystem.GetReplaceText(3224,commandText));
    }    
    
    public void MakeAlchemyCostBonusResult()
    {
        _titleName = (DataSystem.GetTextData(3220).Text);
        _resultName = (DataSystem.GetTextData(3225).Text);
    }



    public void MakeAddSkillCurrencyResult(string skillName,int currency)
    {
        _titleName = (DataSystem.GetReplaceText(3020,skillName));
        _resultName = (DataSystem.GetReplaceText(3021,currency.ToString()));
    }
    
    public void MakeSkillLearnResult(string actorName,SkillData skillData)
    {
        _titleName = (DataSystem.GetReplaceText(3000,actorName));
        _resultName = (skillData.Name + DataSystem.GetTextData(3230).Text);
        SetSkillId(skillData.Id);            
    }
}
