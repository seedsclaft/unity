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



    public List<ListData> SkillActionList()
    {
        List<SkillInfo> skillInfos = CurrentActor.Skills.FindAll(a => a.Id > 100);

        skillInfos.ForEach(a => a.SetEnable(true));
        var sortList1 = new List<SkillInfo>();
        var sortList2 = new List<SkillInfo>();
        var sortList3 = new List<SkillInfo>();
        skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
        foreach (var skillInfo in skillInfos)
        {
            if (skillInfo.Master.IconIndex <= MagicIconType.Psionics)
            {
                sortList1.Add(skillInfo);
            } else
            if (skillInfo.Master.IconIndex >= MagicIconType.Demigod)
            {
                sortList2.Add(skillInfo);
            } else
            {
                sortList3.Add(skillInfo);
            }
        }
        skillInfos.Clear();
        skillInfos.AddRange(sortList1);
        skillInfos.AddRange(sortList2);
        skillInfos.AddRange(sortList3);
        var list = new List<ListData>();
        var idx = 0;
        foreach (var skillInfo in skillInfos)
        {
            var listData = new ListData(skillInfo,idx);
            list.Add(listData);
        }
        return list;
    }

    public List<ListData> StatusCommand()
    {
        var list = new List<ListData>();
        var idx = 0;
        foreach (var commandData in DataSystem.StatusCommand)
        {
            var listData = new ListData(commandData);
            list.Add(listData);
            idx++;
        }
        return list;
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

    public bool NeedReborn()
    {
        return CurrentStage.Master.Reborn;
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

}
