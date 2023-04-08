using System.Collections;
using System.Collections.Generic;

public class StatusModel : BaseModel
{
    private int _useNuminous = 0;
    private int _currentIndex = 0; 
    public int CurrentIndex
    {
        get {return _currentIndex;}
    }
    private AttributeType _currentAttributeType = AttributeType.Fire;
    
    public AttributeType CurrentAttributeType
    {
        get {return _currentAttributeType;}
    }

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
    }
    
    public List<ActorInfo> StatusActors(){
        List<int> StatusActorIds = PartyInfo.ActorIdList;
        return Actors().FindAll(a => StatusActorIds.Contains(a.ActorId));
    }

    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        _currentAttributeType = attributeType;
        List<SkillInfo> skillInfos = CurrentActor.Skills.FindAll(a => a.Attribute == _currentAttributeType && a.Id > 100);
        skillInfos.ForEach(a => a.SetEnable(true));
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
        } else{
            CurrentStage.AddSelectActorId(CurrentActor.ActorId);
        }
    }

    public void ForgetSkill(int skillId)
    {
        SkillInfo skillInfo = CurrentActor.Skills.Find(a => a.Id == skillId);
        if (skillInfo.LearingCost > 0)
        {
            int gainCurrency = CurrentActor.Skills.Find(a => a.Id == skillId).LearingCost;
            PartyInfo.ChangeCurrency(Currency + gainCurrency);
            CurrentActor.ForgetSkill(skillId);
        }
    }

    public bool EnableParamUp(StatusParamType statusParamType)
    {
        int Numinous = Currency - _useNuminous;
        return (CurrentActor.Sp + Numinous) >= CurrentActor.UsePoint.GetParameter(statusParamType);
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
}
