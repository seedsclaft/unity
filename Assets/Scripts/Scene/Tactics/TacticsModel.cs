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
    private AttributeType _currentAttributeType = AttributeType.Fire;
    
    public AttributeType CurrentAttributeType => _currentAttributeType;
    public ActorInfo CurrentActor
    {
        get {return TacticsActor(_currentActorId);}
    }

    private TacticsComandType _commandType = TacticsComandType.Train;
    public TacticsComandType CommandType { get {return _commandType;} set {_commandType = value;}}
    private List<ActorInfo> _tempTacticsData = new();

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

    public List<string> AttirbuteValues()
    {
        return CurrentActor.AttirbuteValues(StageMembers());
    }

    public List<SkillData.SkillAttributeInfo> AttirbuteInfos()
    {
        var list = new List<SkillData.SkillAttributeInfo>();
        var attirbuteValues = AttirbuteValues();
        var idx = 0;
        foreach (var attributeType in AttributeTypes())
        {
            var info = new SkillData.SkillAttributeInfo();
            info.AttributeType = attributeType;
            info.LearningCost = TacticsUtility.AlchemyCost(CurrentActor,attributeType,StageMembers());
            info.ValueText = attirbuteValues[idx];
            info.LearningCount = DataSystem.System.GetTextData(1120).Text + SelectActorAlchemy(CurrentActor.ActorId,attributeType).Count.ToString();
            idx++;
            list.Add(info);
        }
        return list;
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
        _tempTacticsData.Clear();
        for (int i = 0;i < StageMembers().Count;i++)
        {
            ActorInfo stageMember = StageMembers()[i];
            ActorInfo actorInfo = new ActorInfo(stageMember.Master);
            actorInfo.SetTacticsCommand(stageMember.TacticsComandType,stageMember.TacticsCost);
            actorInfo.SetNextBattleEnemyIndex(stageMember.NextBattleEnemyIndex,stageMember.NextBattleEnemyId);
            actorInfo.SetNextLearnCost(stageMember.NextLearnCost);
            //actorInfo.SetNextLearnAttribute(stageMember.NextLearnAttribute);
            actorInfo.SetNextLearnSkillId(stageMember.NextLearnSkillId);
            _tempTacticsData.Add(actorInfo);
        }
        //_tempTacticsData = StageMembers().FindAll(a => a.TacticsComandType == tacticsComandType);
    }

    public void ResetTempData(TacticsComandType tacticsComandType)
    {
        if (_tempTacticsData.Count > 0)
        {
            List<ActorInfo> removeActors = new List<ActorInfo>();
            List<ActorInfo> _stageMembers = StageMembers();
            
            for (int i = 0;i < _stageMembers.Count;i++)
            {
                ActorInfo stageMember = _stageMembers[i];
                ActorInfo tempData = _tempTacticsData.Find(a => a.ActorId == stageMember.ActorId);
                if (tempData != null)
                {
                    if (stageMember.TacticsComandType != tempData.TacticsComandType)
                    {   
                        PartyInfo.ChangeCurrency(Currency + stageMember.TacticsCost);
                        //stageMember.ClearTacticsCommand();
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
            _tempTacticsData.Clear();
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

    public List<SkillInfo> SelectActorAlchemy(int actorId,AttributeType attributeType)
    {
        _currentAttributeType = attributeType;
        ActorInfo actorInfo = TacticsActor(actorId);
        List<SkillInfo> skillInfos = new List<SkillInfo>();
        
        for (int i = 0;i < PartyInfo.AlchemyIdList.Count;i++)
        {
            SkillInfo skillInfo = new SkillInfo(PartyInfo.AlchemyIdList[i]);
            if (skillInfo.Attribute != _currentAttributeType) continue;
            if (actorInfo.IsLearnedSkill(skillInfo.Id)) continue;
            skillInfo.SetEnable(true);
            skillInfos.Add(skillInfo);
        }
        return skillInfos;
    }

    public bool CheckCanSelectAlchemy(AttributeType attributeType)
    {
        ActorInfo actorInfo = CurrentActor;
        return Currency >= TacticsUtility.AlchemyCost(actorInfo,attributeType,StageMembers());
    }
    
    public void SelectAlchemySkill(int skillId)
    {
        ActorInfo actorInfo = CurrentActor;
        if (actorInfo != null){
            actorInfo.SetTacticsCommand(TacticsComandType.Alchemy,TacticsUtility.AlchemyCost(actorInfo,_currentAttributeType,StageMembers()));
            PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
            actorInfo.SetNextLearnSkillId(skillId);
            actorInfo.SetNextLearnCost(TacticsUtility.AlchemyCost(actorInfo,_currentAttributeType,StageMembers()));
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
                //PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
                actorInfo.ClearTacticsCommand();
            } else
            {
                if (CanTacticsCommand(TacticsComandType.Resource,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsComandType.Resource,TacticsUtility.ResourceCost(actorInfo));
                    //PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
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
            SetIsSubordinate(false);
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
}
