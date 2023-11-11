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

    private TacticsComandType _tacticsCommandType = TacticsComandType.Train;
    public TacticsComandType TacticsCommandType => _tacticsCommandType;
    public void SetTacticsCommandType(TacticsComandType tacticsComandType)
    {
        _tacticsCommandType = tacticsComandType;
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
        var list = new List<ListData>();
        var idx = 0;
        foreach (var commandData in DataSystem.TacticsCommand)
        {
            var listData = new ListData(commandData,idx);
            list.Add(listData);
            idx++;
        }
        return list;
    }

    public ListData ChangeEnableCommandData(int index,bool enable)
    {
        return new ListData(DataSystem.TacticsCommand[index],index,enable);
    }

    public void SetTempData(TacticsComandType tacticsComandType)
    {
        if (tacticsComandType > TacticsComandType.Resource)
        {
            return;
        }
        _tempActorInfos.Clear();
        for (int i = 0;i < StageMembers().Count;i++)
        {
            ActorInfo stageMember = StageMembers()[i];
            ActorInfo actorInfo = new ActorInfo(stageMember.Master);
            actorInfo.SetTacticsCommand(stageMember.TacticsComandType,stageMember.TacticsCost);
            actorInfo.SetNextBattleEnemyIndex(stageMember.NextBattleEnemyIndex,stageMember.NextBattleEnemyId);
            actorInfo.SetNextLearnCost(stageMember.NextLearnCost);
            //actorInfo.SetNextLearnAttribute(stageMember.NextLearnAttribute);
            actorInfo.SetNextLearnSkillId(stageMember.NextLearnSkillId);
            _tempActorInfos.Add(actorInfo);
        }
    }

    public void ResetTempData(TacticsComandType tacticsComandType)
    {
        if (_tempActorInfos.Count > 0)
        {
            List<ActorInfo> removeActorInfos = new List<ActorInfo>();
            List<ActorInfo> _stageMembers = StageMembers();
            
            for (int i = 0;i < _stageMembers.Count;i++)
            {
                ActorInfo stageMember = _stageMembers[i];
                ActorInfo tempData = _tempActorInfos.Find(a => a.ActorId == stageMember.ActorId);
                if (tempData != null)
                {
                    if (stageMember.TacticsComandType != tempData.TacticsComandType)
                    {   
                        PartyInfo.ChangeCurrency(Currency + stageMember.TacticsCost);
                        if (tempData.TacticsComandType == TacticsComandType.None)
                        {
                            stageMember.ClearTacticsCommand();
                        } else{
                            stageMember.SetTacticsCommand(tempData.TacticsComandType,tempData.TacticsCost);
                            if (tempData.TacticsComandType != TacticsComandType.Resource)
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
            foreach(var tacticsComandType in Enum.GetValues(typeof(TacticsComandType)))
            {
                if ((int)tacticsComandType != 0)
                {
                    StageMembers()[i].RefreshTacticsEnable((TacticsComandType)tacticsComandType,CanTacticsCommand((TacticsComandType)tacticsComandType,StageMembers()[i]));
                }       
            }
        }
    }

    private bool CanTacticsCommand(TacticsComandType tacticsComandType,ActorInfo actorInfo)
    {
        if (tacticsComandType == TacticsComandType.Train)
        {
            return Currency >= TacticsUtility.TrainCost(actorInfo);
        }
        if (tacticsComandType == TacticsComandType.Alchemy)
        {
            return true;
        }
        if (tacticsComandType == TacticsComandType.Recovery)
        {
            return Currency > 0;
        }
        if (tacticsComandType == TacticsComandType.Battle)
        {
            return true;
        }
        if (tacticsComandType == TacticsComandType.Resource)
        {
            return true;
        }
        return false;
    }

    public bool IsOtherBusy(int actorId, TacticsComandType tacticsComandType)
    {
        ActorInfo actorInfo = TacticsActor(actorId);
        if (actorInfo.TacticsComandType == TacticsComandType.None)
        {
            return false;
        }
        if (actorInfo.TacticsComandType != tacticsComandType)
        {
            return true;
        }
        return false;
    }

    public bool CheckNonBusy()
    {
        return StageMembers().Find(a => a.TacticsComandType == TacticsComandType.None) != null;
    }
    
    public void ResetTacticsCost(int actorId)
    {
        ActorInfo actorInfo = TacticsActor(actorId);
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
        ActorInfo actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsComandType == TacticsComandType.Train)
            {   
                PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsComandType.Train,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsComandType.Train,TacticsUtility.TrainCost(actorInfo));
                    PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
                }
            }
        }
    }

    public bool IsCheckAlchemy(int actorId)
    {
        ActorInfo actorInfo = TacticsActor(actorId);
        if (actorInfo.TacticsComandType == TacticsComandType.Alchemy)
        {   
            PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
            actorInfo.ClearTacticsCommand();
            return true;
        }
        return false;
    }

    public List<ListData> SelectActorAlchemy(int actorId)
    {
        ActorInfo actorInfo = TacticsActor(actorId);
        List<SkillInfo> skillInfos = new List<SkillInfo>();
        
        for (int i = 0;i < PartyInfo.AlchemyIdList.Count;i++)
        {
            SkillInfo skillInfo = new SkillInfo(PartyInfo.AlchemyIdList[i]);
            if (actorInfo.IsLearnedSkill(skillInfo.Id)) continue;
            skillInfo.SetEnable(true);
            //skillInfo.SetLearningCost()
            skillInfos.Add(skillInfo);
        }
        var list = new List<ListData>();
        var idx = 0;
        foreach (var skillInfo in skillInfos)
        {
            var listData = new ListData(skillInfo,idx);
            list.Add(listData);
        }
        return list;
    }

    public bool CheckCanSelectAlchemy(AttributeType attributeType)
    {
        ActorInfo actorInfo = CurrentActor;
        return Currency >= TacticsUtility.AlchemyCost(actorInfo,attributeType,StageMembers());
    }
    
    public void SelectAlchemySkill(int skillId)
    {
        ActorInfo actorInfo = CurrentActor;
        SkillData skillData = DataSystem.Skills.Find(a => a.Id == skillId);
        if (actorInfo != null){
            actorInfo.SetTacticsCommand(TacticsComandType.Alchemy,TacticsUtility.AlchemyCost(actorInfo,skillData.Attribute,StageMembers()));
            PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
            actorInfo.SetNextLearnSkillId(skillId);
            actorInfo.SetNextLearnCost(TacticsUtility.AlchemyCost(actorInfo,skillData.Attribute,StageMembers()));
        }
    }

    public void SelectActorRecovery(int actorId)
    {
        ActorInfo actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsComandType == TacticsComandType.Recovery)
            {   
                PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsComandType.Recovery,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsComandType.Recovery,TacticsUtility.RecoveryCost(actorInfo));
                    PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
                }
            }
        }
    }

    public void SelectRecoveryPlus(int actorId)
    {
        ActorInfo actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (CanTacticsCommand(TacticsComandType.Recovery,actorInfo))
            {
                if (TacticsUtility.RecoveryCost(actorInfo) > actorInfo.TacticsCost)
                {
                    actorInfo.SetTacticsCommand(TacticsComandType.Recovery,actorInfo.TacticsCost + 1);
                    PartyInfo.ChangeCurrency(Currency - 1);
                }
            }
        }
    }

    public void SelectRecoveryMinus(int actorId)
    {
        ActorInfo actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (0 < actorInfo.TacticsCost)
            {
                if (actorInfo.TacticsComandType == TacticsComandType.Recovery)
                {   
                    PartyInfo.ChangeCurrency(Currency + 1);
                    actorInfo.SetTacticsCommand(TacticsComandType.Recovery,actorInfo.TacticsCost - 1);
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
        ActorInfo actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsComandType == TacticsComandType.Battle)
            {   
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsComandType.Battle,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsComandType.Battle,0);
                    actorInfo.SetNextBattleEnemyIndex(_currentEnemyIndex,TacticsTroops()[_currentEnemyIndex].BossEnemy.EnemyData.Id);
                }
            }
        }
    }

    public void SelectActorResource(int actorId)
    {
        ActorInfo actorInfo = TacticsActor(actorId);
        if (actorInfo != null){
            if (actorInfo.TacticsComandType == TacticsComandType.Resource)
            {   
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsComandType.Resource,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsComandType.Resource,TacticsUtility.ResourceCost(actorInfo));
                }
            }
        }
    }

    public bool IsBusyAll()
    {
        return StageMembers().Find(a => a.TacticsComandType == TacticsComandType.None) == null && GameSystem.ConfigData._commandEndCheck;
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
            return DataSystem.Advs.Find(a => a.Id == 203);
        }
        var isAendGameClear = CurrentStage.StageClaer;
        if (isAendGameClear)
        {
            CurrentStage.SetEndingType(EndingType.A);
            StageClaer(2);
            return DataSystem.Advs.Find(a => a.Id == 173);
        }
        var isBendGameClear = CurrentStage.RouteSelect == 1 && CurrentStage.IsBendGameClear();
        if (isBendGameClear)
        {
            CurrentStage.SetEndingType(EndingType.B);
            StageClaer(2);
            return DataSystem.Advs.Find(a => a.Id == 172);
        }
        var isEndStage = (CurrentStage.SubordinateValue <= 0);
        if (isEndStage)
        {
            ChangeSubordinate(false);
            ChangeRouteSelectStage(11);
            return DataSystem.Advs.Find(a => a.Id == 171);
        }
        var isTurnOver = (Turns < 0);
        if (isTurnOver)
        {
            CurrentStage.SetEndingType(EndingType.D);
            return DataSystem.Advs.Find(a => a.Id == 204);
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
        var menucommand = new SystemData.CommandData();
        menucommand.Id = 2;
        menucommand.Name = DataSystem.System.GetTextData(703).Text;
        menucommand.Key = "Help";
        list.Add(menucommand);
        return list;
    }

    public List<ListData> TacticsCharacterData()
    {
        var list = new List<ListData>();
        foreach (var member in StageMembers())
        {
            var tacticsActorInfo = new TacticsActorInfo();
            tacticsActorInfo.TacticsComandType = _tacticsCommandType;
            tacticsActorInfo.ActorInfo = member;
            var listData = new ListData(tacticsActorInfo);
            list.Add(listData);
        }
        return list;
    }

    public string TacticsCommandInputInfo()
    {
        switch (_tacticsCommandType)
        {
            case TacticsComandType.Train:
                return "TRAIN";
            case TacticsComandType.Alchemy:
                return "ALCHEMY";
            case TacticsComandType.Recovery:
                return "RECOVERY";
            case TacticsComandType.Battle:
                return "ENEMY_SELECT";
            case TacticsComandType.Resource:
                return "RESOURCE";
        }
        return "";
    }

    public TacticsCommandData TacticsCommandData()
    {
        var tacticsComandData = new TacticsCommandData();
        tacticsComandData.Title = CommandTitle();
        tacticsComandData.Description = CommandDescription();
        tacticsComandData.Rank = CommandRank();
        return tacticsComandData;
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
            case TacticsComandType.Train:
                count = DataSystem.System.TrainCount;
                break;
            case TacticsComandType.Alchemy:
                count = DataSystem.System.AlchemyCount;
                break;
            case TacticsComandType.Recovery:
                count = DataSystem.System.RecoveryCount;
                break;
            case TacticsComandType.Battle:
                count = DataSystem.System.BattleCount;
                break;
            case TacticsComandType.Resource:
                count = DataSystem.System.ResourceCount;
                break;
        }
        return DataSystem.System.GetReplaceText(10,count.ToString());
    }
}

public class TacticsActorInfo{
    public ActorInfo ActorInfo;
    public TacticsComandType TacticsComandType;
}

public class TacticsCommandData{
    public string Title;
    public int Rank;
    public string Description;
}