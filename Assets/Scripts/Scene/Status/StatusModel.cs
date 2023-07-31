using System.Collections;
using System.Collections.Generic;

public class StatusModel : BaseModel
{
    public string HelpText()
    {
        int textId = -1;
        if (CurrentStage == null)
        {
            textId = 11090;
        } else
        {
            textId = 11050;
        }

        if (textId >= 0)
        {
            return DataSystem.System.GetTextData(textId).Text;
        }
        return "";
    }

    private int _useNuminous = 0;
    private int _currentIndex = 0; 
    private AttributeType _currentAttributeType = AttributeType.Fire;
    public AttributeType CurrentAttributeType => _currentAttributeType;

    public ActorInfo CurrentActor
    {
        get {return StatusActors()[_currentIndex];}
    }

    public void ChangeActorIndex(int value){
        _currentIndex += value;
        if (_currentIndex > StatusActors().Count-1){
            _currentIndex = 0;
        } else
        if (_currentIndex < 0){
            _currentIndex = StatusActors().Count-1;
        }
        ChangeSkillAttributeType();
    }

    public void ChangeSkillAttributeType()
    {
        if (CurrentActor.LastSelectSkillId > 0)
        {
            _currentAttributeType = CurrentActor.Skills.Find(a => a.Id == CurrentActor.LastSelectSkillId).Attribute;
        }
    }
    
    public void SetActorLastSkillId(int selectSkillId)
    {
        CurrentActor.SetLastSelectSkillId(selectSkillId);
    }

    public List<ActorInfo> StatusActors(){
        List<int> StatusActorIds = PartyInfo.ActorIdList;
        var members = new List<ActorInfo>();
        for (int i = 0;i< StatusActorIds.Count;i++)
        {
            var temp = Actors().Find(a => a.ActorId == StatusActorIds[i]);
            if (temp != null)
            {
                members.Add(temp);
            }
        }
        return members;
    }

    private SkillInfo _learnSkillInfo = null;
    public SkillInfo LlearnSkillInfo
    {
        get {return _learnSkillInfo;}
    }

    public void SetLearnSkillInfo(SkillInfo skillInfo){
        _learnSkillInfo = skillInfo;
    }


    public void LearnSkillInfo(SkillInfo skillInfo)
    {
        if (_learnSkillInfo != null)
        {
            CurrentActor.ForgetSkill(_learnSkillInfo);
            if (skillInfo.Id == 2009)
            {
                CurrentActor.LearnSkillAttribute((int)_currentAttributeType + 2000,0,_currentAttributeType);
            } else{
                CurrentActor.LearnSkill(skillInfo.Id);
            }
            _learnSkillInfo = null;
        }
    }

    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        _currentAttributeType = attributeType;
        List<SkillInfo> skillInfos = CurrentActor.Skills.FindAll(a => a.Attribute == _currentAttributeType && a.Id > 50);
        skillInfos.ForEach(a => a.SetEnable(true));
        skillInfos.Sort((a,b) => {return a.Id - b.Id;});
        return skillInfos;
    }

    public List<SystemData.MenuCommandData> StatusCommand
    {
        get { return DataSystem.StatusCommand;}
    }
    
    public void SelectAddActor()
    {
        if (CurrentStage == null)
        {
            GameSystem.CurrentData.MakeStageData(CurrentActor.ActorId);
            GameSystem.CurrentData.SetResumeStage(true);
            GameSystem.CurrentData.Party.ClearData();
		    GameSystem.CurrentData.Party.ChangeCurrency(DataSystem.System.InitCurrency);
        } else{
            CurrentStage.AddSelectActorId(CurrentActor.ActorId);
        }
    }

    public void ForgetSkill()
    {
        SkillInfo skillInfo = _learnSkillInfo;
        CurrentActor.ForgetSkill(_learnSkillInfo);
    }

    public bool EnableParamUp(StatusParamType statusParamType)
    {
        int UseCost = CurrentActor.UsePointCost(statusParamType);
        if (CurrentAlcana.IsStatusCostDown(statusParamType))
        {
            UseCost -= 1;
        }
        int Numinous = Currency - _useNuminous;
        return (CurrentActor.Sp + Numinous) >= UseCost;
    }

    public bool EnableParamMinus(StatusParamType statusParamType)
    {
        return CurrentActor.TempStatus.GetParameter(statusParamType) > 0;
    }

    public void ChangeParameter(StatusParamType statusParamType,int value)
    {
        if (value > 0)
        {
            int Cost = CurrentActor.UsePointCost(statusParamType);
            if (CurrentAlcana.IsStatusCostDown(statusParamType))
            {
                Cost -= 1;
            }
            if (CurrentActor.Sp >= Cost)
            {
                CurrentActor.ChangeSp(CurrentActor.Sp - Cost);
            }else{
                int CostNuminous = Cost - CurrentActor.Sp;
                _useNuminous += CostNuminous;
                CurrentActor.ChangeSp(0);
            }
            CurrentActor.TempStatus.AddParameter(statusParamType,value);
        } else
        {
            CurrentActor.TempStatus.AddParameter(statusParamType,value);
            int BackPoint = CurrentActor.UsePointCost(statusParamType);
            if (CurrentAlcana.IsStatusCostDown(statusParamType))
            {
                BackPoint -= 1;
            }
            if (_useNuminous >= BackPoint)
            {
                _useNuminous -= BackPoint;
            } else
            {
                CurrentActor.ChangeSp((CurrentActor.Sp - _useNuminous) + BackPoint);
                _useNuminous = 0;
            }
        }
    }

    public void DecideStrength()
    {
        CurrentActor.DecideStrength(_useNuminous);
        PartyInfo.ChangeCurrency(Currency - _useNuminous);
        _useNuminous = 0;
    }

    public void StrengthReset()
    {
        int Numinous = CurrentActor.Numinous;
        PartyInfo.ChangeCurrency(Currency + Numinous);
        CurrentActor.StrengthReset();
        _useNuminous = 0;
    }

    public void ClearStrength()
    {
        CurrentActor.ClearStrength();
    }

    public int StrengthNuminous()
    {
        return Currency - _useNuminous;
    }

    public List<SkillInfo> LearningSkillList(AttributeType attributeType)
    {
        List<SkillInfo> skillInfos = new List<SkillInfo>();
        List<int> alchemyIds = PartyInfo.AlchemyIdList;
        foreach (var alchemyId in alchemyIds)
        {
            if (skillInfos.Find(a => a.Id == alchemyId) == null)
            {
                SkillInfo skillInfo = new SkillInfo(alchemyId);
                if (skillInfo.Master.Attribute == _currentAttributeType)
                {
                    skillInfo.SetLearningState(LearningState.SelectLearn);
                    skillInfo.SetEnable(true);
                    skillInfos.Add(skillInfo);
                }
            }
        }
        SkillInfo removeSkillInfo = new SkillInfo(2009);
        removeSkillInfo.SetLearningState(LearningState.SelectLearn);
        removeSkillInfo.SetEnable(true);
        skillInfos.Add(removeSkillInfo);
        return skillInfos;
    }
}
