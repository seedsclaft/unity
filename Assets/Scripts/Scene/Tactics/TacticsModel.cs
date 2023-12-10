using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TacticsModel : BaseModel
{
    private int _currentActorId = 0;
    public int CurrentActorId {
        get {return _currentActorId;} set{_currentActorId = value;}
    }
    public ActorInfo CurrentActor
    {
        get {return TacticsActor(_currentActorId);}
    }

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

    private List<ActorInfo> _tempActorInfos = new();

    private int _currentEnemyIndex = -1; 
    public int CurrentEnemyIndex
    {
        get {return _currentEnemyIndex;} set {_currentEnemyIndex = value;}
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

    public void SetTempData(TacticsCommandType tacticsCommandType)
    {
        if (tacticsCommandType > TacticsCommandType.Resource)
        {
            return;
        }
        _tempActorInfos.Clear();
        for (int i = 0;i < StageMembers().Count;i++)
        {
            var stageMember = StageMembers()[i];
            var actorInfo = new ActorInfo(stageMember.Master);
            actorInfo.SetTacticsCommand(stageMember.TacticsCommandType,stageMember.TacticsCost);
            actorInfo.SetNextBattleEnemyIndex(stageMember.NextBattleEnemyIndex,stageMember.NextBattleEnemyId);
            actorInfo.SetNextLearnCost(stageMember.NextLearnCost);
            //actorInfo.SetNextLearnAttribute(stageMember.NextLearnAttribute);
            actorInfo.SetNextLearnSkillId(stageMember.NextLearnSkillId);
            _tempActorInfos.Add(actorInfo);
        }
    }

    public void ResetTempData(TacticsCommandType tacticsCommandType)
    {
        if (_tempActorInfos.Count > 0)
        {
            var removeActorInfos = new List<ActorInfo>();
            var _stageMembers = StageMembers();
            
            for (int i = 0;i < _stageMembers.Count;i++)
            {
                var stageMember = _stageMembers[i];
                var tempData = _tempActorInfos.Find(a => a.ActorId == stageMember.ActorId);
                if (tempData != null)
                {
                    if (stageMember.TacticsCommandType != tempData.TacticsCommandType)
                    {   
                        PartyInfo.ChangeCurrency(Currency + stageMember.TacticsCost);
                        if (tempData.TacticsCommandType == TacticsCommandType.None)
                        {
                            stageMember.ClearTacticsCommand();
                        } else{
                            stageMember.SetTacticsCommand(tempData.TacticsCommandType,tempData.TacticsCost);
                            if (tempData.TacticsCommandType != TacticsCommandType.Resource)
                            {
                                PartyInfo.ChangeCurrency(Currency - tempData.TacticsCost);
                            }
                            stageMember.SetNextBattleEnemyIndex(tempData.NextBattleEnemyIndex,tempData.NextBattleEnemyId);
                            stageMember.SetNextLearnCost(tempData.NextLearnCost);
                            //stageMember.SetNextLearnAttribute(tempData.NextLearnAttribute);
                            stageMember.SetNextLearnSkillId(tempData.NextLearnSkillId);
                        }
                    }
                }
            }
            _tempActorInfos.Clear();
        }
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
        if (tacticsCommandType == TacticsCommandType.Battle)
        {
            return true;
        }
        if (tacticsCommandType == TacticsCommandType.Resource)
        {
            return true;
        }
        return false;
    }

    public bool IsOtherBusy(int actorId, TacticsCommandType tacticsCommandType)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo.TacticsCommandType == TacticsCommandType.None)
        {
            return false;
        }
        if (actorInfo.TacticsCommandType != tacticsCommandType)
        {
            return true;
        }
        return false;
    }

    public bool CheckNonBusy()
    {
        return StageMembers().Find(a => a.TacticsCommandType == TacticsCommandType.None) != null;
    }
    
    public void ResetTacticsCost(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
        actorInfo.ClearTacticsCommand();
    }

    public void ResetTacticsCostAll()
    {
        foreach (var member in StageMembers())
        {
            ResetTacticsCost(member.ActorId);
        }
    }

    public void SelectActorTrain(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsCommandType == TacticsCommandType.Train)
            {   
                PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsCommandType.Train,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsCommandType.Train,TacticsUtility.TrainCost(actorInfo));
                    PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
                }
            }
        }
    }

    public bool IsCheckAlchemy(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo.TacticsCommandType == TacticsCommandType.Alchemy)
        {   
            PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
            actorInfo.ClearTacticsCommand();
            return true;
        }
        return false;
    }

    public List<ListData> SelectActorAlchemy(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        var skillInfos = new List<SkillInfo>();
        
        for (int i = 0;i < PartyInfo.AlchemyIdList.Count;i++)
        {
            var skillInfo = new SkillInfo(PartyInfo.AlchemyIdList[i]);
            if (actorInfo.IsLearnedSkill(skillInfo.Id)) continue;
            skillInfo.SetEnable(true);
            skillInfo.SetLearningCost(TacticsUtility.AlchemyCost(actorInfo,skillInfo.Attribute,StageMembers()));
            skillInfos.Add(skillInfo);
        }
        return MakeListData(skillInfos);
    }

    public bool CheckCanSelectAlchemy(AttributeType attributeType)
    {
        var actorInfo = CurrentActor;
        return Currency >= TacticsUtility.AlchemyCost(actorInfo,attributeType,StageMembers());
    }
    
    public void SelectAlchemySkill(int skillId)
    {
        var actorInfo = CurrentActor;
        var skillData = DataSystem.Skills.Find(a => a.Id == skillId);
        if (actorInfo != null){
            actorInfo.SetTacticsCommand(TacticsCommandType.Alchemy,TacticsUtility.AlchemyCost(actorInfo,skillData.Attribute,StageMembers()));
            PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
            actorInfo.SetNextLearnSkillId(skillId);
            actorInfo.SetNextLearnCost(TacticsUtility.AlchemyCost(actorInfo,skillData.Attribute,StageMembers()));
        }
    }

    public void SelectActorRecovery(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsCommandType == TacticsCommandType.Recovery)
            {   
                PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsCommandType.Recovery,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsCommandType.Recovery,TacticsUtility.RecoveryCost(actorInfo));
                    PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
                }
            }
        }
    }

    public void SelectRecoveryPlus(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (CanTacticsCommand(TacticsCommandType.Recovery,actorInfo))
            {
                if (TacticsUtility.RecoveryCost(actorInfo) > actorInfo.TacticsCost)
                {
                    actorInfo.SetTacticsCommand(TacticsCommandType.Recovery,actorInfo.TacticsCost + 1);
                    PartyInfo.ChangeCurrency(Currency - 1);
                }
            }
        }
    }

    public void SelectRecoveryMinus(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (0 < actorInfo.TacticsCost)
            {
                if (actorInfo.TacticsCommandType == TacticsCommandType.Recovery)
                {   
                    PartyInfo.ChangeCurrency(Currency + 1);
                    actorInfo.SetTacticsCommand(TacticsCommandType.Recovery,actorInfo.TacticsCost - 1);
                    if (actorInfo.TacticsCost == 0)
                    {
                        actorInfo.ClearTacticsCommand();
                    }
                }
            }
        }
    }
    
    public void SelectActorBattle(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsCommandType == TacticsCommandType.Battle)
            {   
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsCommandType.Battle,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsCommandType.Battle,0);
                    actorInfo.SetNextBattleEnemyIndex(_currentEnemyIndex,TacticsTroops()[_currentEnemyIndex].BossEnemy.EnemyData.Id);
                }
            }
        }
    }

    public void SelectActorResource(int actorId)
    {
        var actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsCommandType == TacticsCommandType.Resource)
            {   
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsCommandType.Resource,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsCommandType.Resource,TacticsUtility.ResourceCost(actorInfo));
                }
            }
        }
    }

    public bool IsBusyAll()
    {
        return StageMembers().Find(a => a.TacticsCommandType == TacticsCommandType.None) == null && GameSystem.ConfigData.CommandEndCheck;
    }

    public void InitInBattle()
    {
        StageMembers().ForEach(a => a.SetInBattle(false));
    }

    public void TurnEnd()
    {
    }
    
    public AdvData StartTacticsAdvData()
    {
        var isGameOver = (Actors().Find(a => a.ActorId == CurrentStage.SelectActorIds[0])).Lost;
        if (isGameOver)
        {
            CurrentStage.SetEndingType(EndingType.D);
            return DataSystem.Adventures.Find(a => a.Id == 203);
        }
        var isAEndGameClear = CurrentStage.StageClear;
        if (isAEndGameClear)
        {
            CurrentStage.SetEndingType(EndingType.A);
            StageClear(CurrentStage.Master.Id);
            return DataSystem.Adventures.Find(a => a.Id == 173);
        }
        var isBEndGameClear = CurrentStage.RouteSelect == 1 && CurrentStage.IsBendGameClear();
        if (isBEndGameClear)
        {
            CurrentStage.SetEndingType(EndingType.B);
            StageClear(CurrentStage.Master.Id);
            return DataSystem.Adventures.Find(a => a.Id == 172);
        }
        var isEndStage = (CurrentStage.SubordinateValue <= 0);
        if (isEndStage)
        {
            ChangeSubordinate(false);
            ChangeRouteSelectStage(11);
            return DataSystem.Adventures.Find(a => a.Id == 171);
        }
        var isTurnOver = (Turns < 0);
        if (isTurnOver)
        {
            CurrentStage.SetEndingType(EndingType.D);
            return DataSystem.Adventures.Find(a => a.Id == 204);
        }
        return null;
    }

    public List<SystemData.CommandData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var retire = new SystemData.CommandData();
        retire.Id = 1;
        retire.Name = DataSystem.System.GetTextData(704).Text;
        retire.Key = "Retire";
        list.Add(retire);
        var menuCommand = new SystemData.CommandData();
        menuCommand.Id = 2;
        menuCommand.Name = DataSystem.System.GetTextData(703).Text;
        menuCommand.Key = "Help";
        list.Add(menuCommand);
        return list;
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
            case TacticsCommandType.Battle:
                return "ENEMY_SELECT";
            case TacticsCommandType.Resource:
                return "RESOURCE";
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
        return DataSystem.System.GetTextData((int)_tacticsCommandType).Text;
    }

    private int CommandRank()
    {
        return CommandRankInfo()[_tacticsCommandType];
    }

    private string CommandDescription()
    {
        int rank = CommandRank();
        if (rank > 0)
        {
            return DataSystem.System.GetReplaceText(10 + (int)_tacticsCommandType,(rank * 10).ToString());
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
            case TacticsCommandType.Battle:
                count = DataSystem.System.BattleCount;
                break;
            case TacticsCommandType.Resource:
                count = DataSystem.System.ResourceCount;
                break;
        }
        return DataSystem.System.GetReplaceText(10,count.ToString());
    }

    public void SetDefineBoss(int index)
    {
        var defineTroopId = CurrentStage.DefineTroopId(false);
        if (defineTroopId != 0 && !CurrentStage.ClearedTroopId(defineTroopId))
        {
            CurrentStage.SetDefineBossOnly(index);
            SetNeedAllTacticsCommand(true);
            SetTacticsCommandEnables(TacticsCommandType.Train,false);
            SetTacticsCommandEnables(TacticsCommandType.Alchemy,false);
            SetTacticsCommandEnables(TacticsCommandType.Recovery,false);
            SetTacticsCommandEnables(TacticsCommandType.Resource,false);
        }
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