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
    
    public AttributeType CurrentAttributeType
    {
        get {return _currentAttributeType;}
    }
    public ActorInfo CurrentActor
    {
        get {return TacticsActor(_currentActorId);}
    }

    public string TacticsBgmFilename()
    {
        if (CurrentStage != null)
        {
            if (CurrentStage.CurrentTurn > 24)
            {        
                return "TACTICS3";
            }
            if (CurrentStage.CurrentTurn > 12)
            {        
                return "TACTICS2";
            }
            return "TACTICS1";
        }
        return "TACTICS1";
    }
    private TacticsComandType _commandType = TacticsComandType.Train;
    public TacticsComandType CommandType { get {return _commandType;} set {_commandType = value;}}
    private List<ActorInfo> _tempTacticsData = new List<ActorInfo>();

    private int _currentEnemyIndex = -1; 
    public int CurrentEnemyIndex
    {
        get {return _currentEnemyIndex;} set {_currentEnemyIndex = value;}
    }
    

    private bool _needAllTacticsCommand = false;
    public bool NeedAllTacticsCommand { get {return _needAllTacticsCommand;}}
    public void SetNeedAllTacticsCommand(bool isNeed)
    {
        _needAllTacticsCommand = isNeed;
    }

    public List<StagesData.StageEventData> StageEvents(EventTiming eventTiming)
    {
        int CurrentTurn = CurrentStage.CurrentTurn;
        List<string> eventKeys = CurrentStage.ReadEventKeys;
        return StageEventDatas.FindAll(a => a.Timing == eventTiming && a.Turns == CurrentTurn && !eventKeys.Contains(a.EventKey));
    }

    public void AddEventsReadFlag(List<StagesData.StageEventData> stageEventDatas)
    {
        foreach (var eventData in stageEventDatas)
        {
            AddEventReadFlag(eventData);
        }
    }

    public void AddEventReadFlag(StagesData.StageEventData stageEventDatas)
    {
        if (stageEventDatas.ReadFlag)
        {
            CurrentStage.AddEventReadFlag(stageEventDatas.EventKey);
        }
    }

    public ActorInfo TacticsActor(int actorId)
    {
        return StageMembers().Find(a => a.ActorId == actorId);
    }

    public List<TroopInfo> ResetTroopData()
    {
        return CurrentStage.MakeTutorialTroopData(CurrentStage.SelectActorIds[0]);
    }

    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        if (attributeType != AttributeType.None)
        {
            _currentAttributeType = attributeType;
        }
        return CurrentActor.Skills.FindAll(a => a.Attribute == _currentAttributeType);
    }

    public List<string> AttirbuteValues()
    {
        return CurrentActor.AttirbuteValues(StageMembers());
    }

    public List<int> AttirbutesLearingCosts(){
        List<int> LaerningCost = new List<int>();
        foreach (var attributeType in AttributeTypes())
        {
            LaerningCost.Add(TacticsUtility.AlchemyCost(CurrentActor,attributeType,StageMembers()));
        }
        return LaerningCost;
    }

    public List<SystemData.MenuCommandData> TacticsCommand
    {
        get { return DataSystem.TacticsCommand;}
    }

    public void SetTempData(TacticsComandType tacticsComandType)
    {
        _tempTacticsData = StageMembers().FindAll(a => a.TacticsComandType == tacticsComandType);
    }

    public void ResetTempData(TacticsComandType tacticsComandType)
    {
        if (_tempTacticsData.Count > 0)
        {
            List<ActorInfo> removeActors = new List<ActorInfo>();
            
            for (int i = 0;i < StageMembers().Count;i++)
            {
                if (_tempTacticsData.Find(a => a.ActorId == StageMembers()[i].ActorId) == null)
                {
                    if (StageMembers()[i].TacticsComandType == tacticsComandType)
                    {
                        PartyInfo.ChangeCurrency(Currency + StageMembers()[i].TacticsCost);
                        StageMembers()[i].ClearTacticsCommand();
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
            return Currency >= TacticsUtility.RecoveryCost(actorInfo);
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

    public int TacticsCost(TacticsComandType tacticsComandType)
    {
        int trainCost = 0;
        foreach (var actorInfo in StageMembers()) if (actorInfo.TacticsComandType == tacticsComandType) trainCost += actorInfo.TacticsCost;
        return trainCost;
    }
    
    public int TacticsTotalCost()
    {
        int totalCost = 0;
        foreach (var actorInfo in StageMembers()) totalCost += actorInfo.TacticsCost;
        return totalCost;
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

    public void SelectAlchemy(AttributeType attributeType)
    {
        ActorInfo actorInfo = CurrentActor;
        if (actorInfo != null){
            actorInfo.SetTacticsCommand(TacticsComandType.Alchemy,TacticsUtility.AlchemyCost(actorInfo,attributeType,StageMembers()));
            PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
            actorInfo.SetNextLearnAttribute(attributeType);
            actorInfo.SetNextLearnCost(TacticsUtility.AlchemyCost(actorInfo,attributeType,StageMembers()));
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
                if (CanTacticsCommand(TacticsComandType.Recovery,actorInfo))
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
                if (CanTacticsCommand(TacticsComandType.Recovery,actorInfo))
                {   
                    actorInfo.SetTacticsCommand(TacticsComandType.Resource,TacticsUtility.RecoveryCost(actorInfo));
                    //PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
                }
            }
        }
    }
    public void RefreshData()
    {
        
    }

    public void TurnEnd()
    {
        CurrentStage.SeekStage();
    }
}
