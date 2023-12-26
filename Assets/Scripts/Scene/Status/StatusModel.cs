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

    private int _currentIndex = 0;
    public void SelectActor(int actorId)
    {
        var index = StatusActors().FindIndex(a => a.ActorId == actorId);
        _currentIndex = index;
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
    
    public void SetActorLastSkillId(int selectSkillId)
    {
        CurrentActor.SetLastSelectSkillId(selectSkillId);
    }

    public List<ListData> SkillActionList()
    {
        var skillInfos = CurrentActor.Skills.FindAll(a => a.Id > 100);

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
        return MakeListData(skillInfos);
    }

    public void SelectAddActor()
    {
        CurrentStage.AddSelectActorId(CurrentActor.ActorId);
        if (CurrentStage == null)
        {
            CurrentStageData.SetResumeStage(true);
		    PartyInfo.ChangeCurrency(DataSystem.System.InitCurrency);
        }
    }

    public bool NeedReborn()
    {
        return CurrentStage.Master.Reborn;
    }

    public bool NeedAlcana()
    {
        return CurrentStage.Master.Alcana;
    }
}
