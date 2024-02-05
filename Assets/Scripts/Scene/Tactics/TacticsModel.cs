using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TacticsModel : BaseModel
{
    
    private int _selectAlchemyActorId = 0;
    public void SetSelectAlchemyActorId(int actorId)
    {
        _selectAlchemyActorId = actorId;
    }    
    public ActorInfo SelectAlchemyActorInfo()
    {
        return StageMembers().Find(a => a.ActorId == _selectAlchemyActorId);
    }

    private int _selectSkillId = 0;
    public int SelectAlcanaSkillId => _selectSkillId;

    private TacticsCommandType _tacticsCommandType = TacticsCommandType.Train;
    public TacticsCommandType TacticsCommandType => _tacticsCommandType;
    public void SetTacticsCommandType(TacticsCommandType tacticsCommandType)
    {
        _tacticsCommandType = tacticsCommandType;
    }
    private Dictionary<TacticsCommandType,bool> _tacticsCommandEnables = new ();
    public void SetTacticsCommandEnables(TacticsCommandType tacticsCommand,bool isEnable)
    {
        _tacticsCommandEnables[tacticsCommand] = isEnable;
    }


    private int _stageSeekIndex = -1; 
    public int StageSeekIndex
    {
        get {return _stageSeekIndex;} set {_stageSeekIndex = value;}
    }
    
    private bool _needAllTacticsCommand = false;
    public bool NeedAllTacticsCommand => _needAllTacticsCommand;
    public void SetNeedAllTacticsCommand(bool isNeed)
    {
        _needAllTacticsCommand = isNeed;
    }

    public ActorInfo TacticsActor(int actorId)
    {
        return StageMembers().Find(a => a.ActorId == actorId);
    }

    public List<ListData> TacticsCommand()
    {
        if (CurrentStage.SurvivalMode)
        {
            SetTacticsCommandEnables(TacticsCommandType.Train,false);
            SetTacticsCommandEnables(TacticsCommandType.Alchemy,false);
            SetTacticsCommandEnables(TacticsCommandType.Recovery,false);
        }
        var commandListDates = MakeListData(DataSystem.TacticsCommand);
        foreach (var commandListData in commandListDates)
        {
            var commandData = (SystemData.CommandData)commandListData.Data;
            if (_tacticsCommandEnables.ContainsKey((TacticsCommandType)commandData.Id))
            {
                commandListData.SetEnable(_tacticsCommandEnables[(TacticsCommandType)commandData.Id]);
            }
        }
        return commandListDates;
    }

    public ListData ChangeEnableCommandData(int index,bool enable)
    {
        return new ListData(DataSystem.TacticsCommand[index],index,enable);
    }


    public void RefreshTacticsEnable()
    {
        for (int i = 0;i < StageMembers().Count;i++)
        {
            foreach(var tacticsCommandType in Enum.GetValues(typeof(TacticsCommandType)))
            {
                if ((int)tacticsCommandType != 0)
                {
                    StageMembers()[i].RefreshTacticsEnable((TacticsCommandType)tacticsCommandType,CanTacticsCommand((TacticsCommandType)tacticsCommandType,StageMembers()[i]));
                }       
            }
        }
    }

    private bool CanTacticsCommand(TacticsCommandType tacticsCommandType,ActorInfo actorInfo)
    {
        if (tacticsCommandType == TacticsCommandType.Paradigm)
        {
            return true;
        }
        if (tacticsCommandType == TacticsCommandType.Train)
        {
            return Currency >= TacticsUtility.TrainCost(actorInfo);
        }
        if (tacticsCommandType == TacticsCommandType.Alchemy)
        {
            return true;
        }
        if (tacticsCommandType == TacticsCommandType.Recovery)
        {
            return Currency > 0;
        }
        return false;
    }

    public void SelectActorTrain(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        var cost = TacticsUtility.TrainCost(actorInfo);
        if (Currency >= cost){
            var statusInfo = actorInfo.LevelUp(0);
            actorInfo.TempStatus.SetParameter(
                statusInfo.Hp,
                statusInfo.Mp,
                statusInfo.Atk,
                statusInfo.Def,
                statusInfo.Spd
            );
            actorInfo.DecideStrength(0);
            actorInfo.GainLevelUpCost(cost);
            PartyInfo.ChangeCurrency(Currency - cost);
        }
    }

    public bool SkillEquipmentActor()
    {
        return SelectAlchemyActorInfo().EquipmentSkillId != 0;
    }

    public void RemoveAlchemy()
    {
        var actorInfo = SelectAlchemyActorInfo();
        // 装備中
        if (actorInfo.EquipmentSkillId != 0)
        {
            actorInfo.RemoveEquipSkill();
        }
    }

    public void SetSkillEquip(int skillId)
    {
        var actorInfo = SelectAlchemyActorInfo();
        var skillInfo = new SkillInfo(skillId);
        skillInfo.SetLearningTurns(TacticsUtility.AlchemyTurns(actorInfo,skillInfo.Attribute,StageMembers()));
        actorInfo.EquipSkill(skillInfo);
    }

    public void SelectActorParadigm(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsCommandType == TacticsCommandType.Paradigm)
            {   
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsCommandType.Paradigm,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsCommandType.Paradigm,TacticsUtility.TrainCost(actorInfo));
                }
            }
        }
    }

    public void SetStageSeekIndex()
    {
        CurrentStage.SetSeekIndex(_stageSeekIndex);
    }

    public void SaveTempBattleMembers()
    {
        TempData.CashBattleActors(BattleMembers());
    }

    public void SetInBattle(bool inBattle)
    {
        foreach (var stageMember in StageMembers())
        {
            stageMember.SetInBattle(inBattle);
        }
    }

    public List<ListData> SelectActorAlchemy()
    {
        var skillInfos = new List<SkillInfo>();
        var actorInfo = SelectAlchemyActorInfo();
        if (actorInfo == null)
        {
        return MakeListData(skillInfos);
        }
        var otherActorSelected = new List<int>();
        foreach (var stageMember in StageMembers())
        {
            if (stageMember.EquipmentSkillId != 0)
            {
                otherActorSelected.Add(stageMember.EquipmentSkillId);
            }
        }
        
        foreach (var alchemyId in PartyInfo.AlchemyIdList)
        {
            if (actorInfo.IsLearnedSkill(alchemyId)) continue;
            var skillInfo = new SkillInfo(alchemyId);
            var cost = TacticsUtility.AlchemyTurns(actorInfo,skillInfo.Attribute,StageMembers());
            if (cost > PartyInfo.Currency)
            {
                //continue;
            }
            skillInfo.SetEnable(!otherActorSelected.Contains(alchemyId));
            skillInfos.Add(skillInfo);
        }
        return MakeListData(skillInfos);
    }

    public List<ListData> AlcanaMagicSkillInfos(List<GetItemInfo> getItemInfos)
    {
        var skillInfos = new List<SkillInfo>();
        foreach (var getItemInfo in getItemInfos)
        {
            var skillInfo = new SkillInfo(getItemInfo.Param1);
            var cost = getItemInfo.Param2;
            skillInfo.SetEnable(cost <= PartyInfo.Currency);
            skillInfos.Add(skillInfo);
        }
        return MakeListData(skillInfos);
    }
    
    public void SelectAlchemySkill(int skillId)
    {
        _selectSkillId = skillId;
    }

    public void SelectRecoveryPlus(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        var remain = TacticsUtility.RemainRecoveryCost(actorInfo);
        if (actorInfo != null && remain > 0){
            var cost = TacticsUtility.RecoveryCost(actorInfo);
            if (Currency >= cost){
                actorInfo.ChangeHp(actorInfo.CurrentHp + 10);
                actorInfo.ChangeMp(actorInfo.CurrentMp + 10);
                PartyInfo.ChangeCurrency(Currency - cost);
            }
        }
    }
    
    public AdvData StartTacticsAdvData()
    {
        if (CurrentStage.SurvivalMode)
        {
            var isSurvivalGameOver = CurrentStage.SelectActorIds.Count > 0 && !Actors().Exists(a => a.Lost == false);
            if (isSurvivalGameOver)
            {
                CurrentStage.SetEndingType(EndingType.C);
                return DataSystem.Adventures.Find(a => a.Id == 21);
            }
            var isSurvivalClear = Turns <= 0;
            if (isSurvivalClear)
            {
                CurrentStage.SetEndingType(EndingType.A);
                StageClear();
                return DataSystem.Adventures.Find(a => a.Id == 151);
            }
            return null;
        }
        var isGameOver = CurrentStage.SelectActorIds.Count > 0 && (Actors().Find(a => a.ActorId == CurrentStage.SelectActorIds[0])).Lost;
        if (isGameOver)
        {
            CurrentStage.SetEndingType(EndingType.C);
            return DataSystem.Adventures.Find(a => a.Id == 21);
        }
        var turnOver = Turns < 0;
        if (turnOver)
        {
        }
        var isAEndGameClear = CurrentStage.StageClear;
        if (isAEndGameClear)
        {
            CurrentStage.SetEndingType(EndingType.A);
            StageClear();
            return DataSystem.Adventures.Find(a => a.Id == 151);
        }
        var isBEndGameClear = CurrentStage.ClearTroopIds.Contains(4010);
        if (isBEndGameClear)
        {
            CurrentStage.SetEndingType(EndingType.B);
            StageClear();
            return DataSystem.Adventures.Find(a => a.Id == 152);
        }
        return null;
    }

    public List<ListData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var retire = new SystemData.CommandData();
        retire.Id = 1;
        retire.Name = DataSystem.GetTextData(704).Text;
        retire.Key = "Retire";
        list.Add(retire);
        var menuCommand = new SystemData.CommandData();
        menuCommand.Id = 2;
        menuCommand.Name = DataSystem.GetTextData(703).Text;
        menuCommand.Key = "Help";
        list.Add(menuCommand);
        if (CurrentStage.SurvivalMode == false)
        {
            var saveCommand = new SystemData.CommandData();
            saveCommand.Id = 3;
            saveCommand.Name = DataSystem.GetTextData(707).Text;
            saveCommand.Key = "Save";
            list.Add(saveCommand);
        }
        return MakeListData(list);
    }

    public List<ListData> TacticsCharacterData()
    {
        var list = new List<TacticsActorInfo>();
        foreach (var member in StageMembers())
        {
            var tacticsActorInfo = new TacticsActorInfo();
            tacticsActorInfo.TacticsCommandType = _tacticsCommandType;
            tacticsActorInfo.ActorInfo = member;
            tacticsActorInfo.ActorInfos = StageMembers();
            list.Add(tacticsActorInfo);
        }
        return MakeListData(list);
    }

    public string TacticsCommandInputInfo()
    {
        switch (_tacticsCommandType)
        {
            case TacticsCommandType.Train:
                return "TRAIN";
            case TacticsCommandType.Alchemy:
                return "ALCHEMY";
            case TacticsCommandType.Recovery:
                return "RECOVERY";
        }
        return "";
    }

    public TacticsCommandData TacticsCommandData()
    {
        var tacticsCommandData = new TacticsCommandData();
        tacticsCommandData.Title = CommandTitle();
        tacticsCommandData.Description = CommandDescription();
        tacticsCommandData.Rank = CommandRank();
        return tacticsCommandData;
    }

    private string CommandTitle()
    {
        return DataSystem.GetTextData((int)_tacticsCommandType).Text;
    }

    private int CommandRank()
    {
        return 0;
    }

    private string CommandDescription()
    {
        int rank = CommandRank();
        if (rank > 0)
        {
            // %の確率で
            var bonusParam = rank * 10;
            return DataSystem.GetReplaceText(10 + (int)_tacticsCommandType,bonusParam.ToString());
        }
        int count = 0;
        switch (_tacticsCommandType)
        {
            case TacticsCommandType.Train:
                count = DataSystem.System.TrainCount;
                break;
            case TacticsCommandType.Alchemy:
                count = DataSystem.System.AlchemyCount;
                break;
            case TacticsCommandType.Recovery:
                count = DataSystem.System.RecoveryCount;
                break;
        }
        return DataSystem.GetReplaceText(10,count.ToString());
    }

    private bool RecoveryCostZero()
    {
        return StageAlcana.CheckCommandCostZero(TacticsCommandType.Recovery);
    }

    public void ChangeBattleLineIndex(int actorId,bool isFront)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo.LineIndex == LineType.Front && isFront == false)
        {
            actorInfo.SetLineIndex(LineType.Back);
        } else
        if (actorInfo.LineIndex == LineType.Back && isFront == true)
        {
            actorInfo.SetLineIndex(LineType.Front);
        }
    }

    public void SetSurvivalMode()
    {
        CurrentStage.SetSurvivalMode();
    }

    public bool CheckTutorial(TacticsViewEvent viewEvent)
    {
        if (CurrentStageTutorialDates.Count == 0)
        {
            return false;
        }
        var tutorial = CurrentStageTutorialDates[0];
        switch (tutorial.Type)
        {
            case TutorialType.TacticsCommandTrain:
                return (viewEvent.commandType == Tactics.CommandType.TacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Train);
            case TutorialType.TacticsCommandAlchemy:
                return (viewEvent.commandType == Tactics.CommandType.TacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Alchemy);
            case TutorialType.TacticsCommandRecover:
                return (viewEvent.commandType == Tactics.CommandType.TacticsCommand && (TacticsCommandType)viewEvent.template == TacticsCommandType.Recovery);
            case TutorialType.TacticsSelectTacticsActor:
                return (viewEvent.commandType == Tactics.CommandType.SelectTacticsActor);
            case TutorialType.TacticsSelectTacticsDecide:
                return (viewEvent.commandType == Tactics.CommandType.TacticsCommandClose);
            case TutorialType.TacticsSelectEnemy:
                return (viewEvent.commandType == Tactics.CommandType.SelectSymbol);
            case TutorialType.TacticsSelectAlchemyMagic:
                return (viewEvent.commandType == Tactics.CommandType.SkillAlchemy && (SkillInfo)viewEvent.template != null);
            
        }
        return false;
    }
}

public class TacticsActorInfo{
    public ActorInfo ActorInfo;
    public List<ActorInfo> ActorInfos;
    public TacticsCommandType TacticsCommandType;
}

public class TacticsCommandData{
    public string Title;
    public int Rank;
    public string Description;
}