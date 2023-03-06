using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Threading;
using System.Threading.Tasks;

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

    private TacticsComandType _commandType = TacticsComandType.Train;
    public TacticsComandType CommandType { get {return _commandType;} set {_commandType = value;}}
    private List<ActorInfo> _tempTacticsData = new List<ActorInfo>();

    private int _currentEnemyIndex = -1; 
    public int CurrentEnemyIndex
    {
        get {return _currentEnemyIndex;} set {_currentEnemyIndex = value;}
    }
    
    public List<StagesData.StageEventData> StageEvents(EventTiming eventTiming)
    {
        int CurrentTurn = GameSystem.CurrentData.CurrentStage.CurrentTurn;
        return GameSystem.CurrentData.CurrentStage.StageEvents.FindAll(a => a.Timing == eventTiming && a.Turns == CurrentTurn);
    }

    public ActorInfo TacticsActor(int actorId)
    {
        return Actors().Find(a => a.ActorId == actorId);
    }

    private List<BattlerInfo> _tacticalEnemies = new List<BattlerInfo>();
    public List<BattlerInfo> TacticsEnemies()
    {
        if (_tacticalEnemies.Count > 0) return _tacticalEnemies;
        List<BattlerInfo> battlerInfos = new List<BattlerInfo>();
        List<TroopsData.TroopData> tacticsEnemyDatas = GameSystem.CurrentData.CurrentStage.TacticsEnemies();
        for (int i = 0;i < tacticsEnemyDatas.Count;i++)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == tacticsEnemyDatas[i].EnemyId);
            BattlerInfo battlerInfo = new BattlerInfo(enemyData,tacticsEnemyDatas[i].Lv,0,0);
            battlerInfos.Add(battlerInfo);
        }
        _tacticalEnemies = battlerInfos;
        return battlerInfos;
    }

    public List<List<GetItemInfo>> TacticsGetItemInfos()
    {
        List<List<GetItemInfo>> getItemDataLists = new List<List<GetItemInfo>>();
        List<TroopsData.TroopData> tacticsEnemyDatas = GameSystem.CurrentData.CurrentStage.TacticsEnemies();
        for (int i = 0;i < tacticsEnemyDatas.Count;i++)
        {
            List<GetItemInfo> getItemInfos = new List<GetItemInfo>();
            List<GetItemData> getItemDatas = tacticsEnemyDatas[i].GetItemDatas;
            for (int j = 0;j < getItemDatas.Count;j++)
            {
                GetItemInfo getItemInfo = new GetItemInfo();
                if (getItemDatas[j].Type == GetItemType.Skill)
                {
                    getItemInfo.SetAttributeType((int)getItemDatas[j].Type);
                    SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == getItemDatas[j].Param1);
                    getItemInfo.SetResultData(skillData.Name);
                }
                getItemInfos.Add(getItemInfo);
            }
            getItemDataLists.Add(getItemInfos);
        }
        return getItemDataLists;
    }

    public List<BattlerInfo> TacticsTutorialEnemies()
    {
        List<BattlerInfo> battlerInfos = new List<BattlerInfo>();
        TroopsData.TroopData troopData = DataSystem.Troops.Find(a => a.TroopId == CurrentData.CurrentStage.SelectActorIds[0] * 10 && a.Line == 1);
        List<TroopsData.TroopData> tacticsEnemyDatas = new List<TroopsData.TroopData>();
        tacticsEnemyDatas.Add(troopData);
        for (int i = 0;i < tacticsEnemyDatas.Count;i++)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == tacticsEnemyDatas[i].EnemyId);
            BattlerInfo battlerInfo = new BattlerInfo(enemyData,tacticsEnemyDatas[i].Lv,0,0);
            battlerInfos.Add(battlerInfo);
        }
        _tacticalEnemies = battlerInfos;
        return battlerInfos;
    }

    public List<List<GetItemInfo>> TacticsTutorialGetItemInfos()
    {
        List<List<GetItemInfo>> getItemDataLists = new List<List<GetItemInfo>>();
        
        List<TroopsData.TroopData> tacticsEnemyDatas = new List<TroopsData.TroopData>();
        TroopsData.TroopData troopData = DataSystem.Troops.Find(a => a.TroopId == CurrentData.CurrentStage.SelectActorIds[0] * 10 && a.Line == 1);
        tacticsEnemyDatas.Add(troopData);
        for (int i = 0;i < tacticsEnemyDatas.Count;i++)
        {
            List<GetItemInfo> getItemInfos = new List<GetItemInfo>();
            List<GetItemData> getItemDatas = tacticsEnemyDatas[i].GetItemDatas;
            for (int j = 0;j < getItemDatas.Count;j++)
            {
                GetItemInfo getItemInfo = new GetItemInfo();
                if (getItemDatas[j].Type == GetItemType.Skill)
                {
                    getItemInfo.SetAttributeType((int)getItemDatas[j].Type);
                    SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == getItemDatas[j].Param1);
                    getItemInfo.SetResultData(skillData.Name);
                }
                getItemInfos.Add(getItemInfo);
            }
            getItemDataLists.Add(getItemInfos);
        }
        return getItemDataLists;
    }

    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        if (attributeType != AttributeType.None)
        {
            _currentAttributeType = attributeType;
        }
        return CurrentActor.Skills.FindAll(a => a.Attribute == _currentAttributeType);
    }

    public List<SystemData.MenuCommandData> TacticsCommand
    {
        get { return DataSystem.TacticsCommand;}
    }
    
    public async Task<List<AudioClip>> BgmData(){
        BGMData bGMData = DataSystem.Data.GetBGM("TACTICS1");
        List<string> data = new List<string>();
        data.Add("BGM/" + bGMData.FileName + ".ogg");
        
        var result1 = await ResourceSystem.LoadAsset<AudioClip>(data[0]);
        return new List<AudioClip>(){
            result1,null
        };    
    }

    public void SetTempData(TacticsComandType tacticsComandType)
    {
        _tempTacticsData = Actors().FindAll(a => a.TacticsComandType == tacticsComandType);
    }

    public void ResetTempData(TacticsComandType tacticsComandType)
    {
        if (_tempTacticsData != null)
        {
            List<ActorInfo> removeActors = new List<ActorInfo>();
            
            for (int i = 0;i < Actors().Count;i++)
            {
                if (_tempTacticsData.Find(a => a.ActorId == Actors()[i].ActorId) == null)
                {
                    PartyInfo.ChangeCurrency(Currency + Actors()[i].TacticsCost);
                    Actors()[i].ClearTacticsCommand();
                }
            }
            _tempTacticsData.Clear();
        }
    }

    public void RefreshTacticsEnable()
    {
        for (int i = 0;i < Actors().Count;i++)
        {
            foreach(var tacticsComandType in Enum.GetValues(typeof(TacticsComandType)))
            {
                if ((int)tacticsComandType != 0)
                {
                    Actors()[i].RefreshTacticsEnable((TacticsComandType)tacticsComandType,CanTacticsCommand((TacticsComandType)tacticsComandType,Actors()[i]));
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
        return Actors().Find(a => a.TacticsComandType == TacticsComandType.None) != null;
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
        foreach (var actorInfo in Actors()) if (actorInfo.TacticsComandType == tacticsComandType) trainCost += actorInfo.TacticsCost;
        return trainCost;
    }
    
    public int TacticsTotalCost()
    {
        int totalCost = 0;
        foreach (var actorInfo in Actors()) totalCost += actorInfo.TacticsCost;
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
        
        {
            for (int i = 0;i < PartyInfo.AlchemyIdList.Count;i++)
            {
                SkillInfo skillInfo = new SkillInfo(PartyInfo.AlchemyIdList[i]);
                if (skillInfo.Attribute != _currentAttributeType) continue;
                skillInfo.LearingCost = TacticsUtility.AlchemyCost(actorInfo,PartyInfo.AlchemyIdList[i]);
                if (skillInfo.LearingCost > Currency)
                {
                    skillInfo.SetDisable();
                } else{
                    skillInfo.SetEnable();
                }
                skillInfos.Add(skillInfo);
            }
        }
        return skillInfos;
    }

    public void SelectAlchemy(int skillId)
    {
        ActorInfo actorInfo = CurrentActor;
        if (actorInfo != null){
            actorInfo.SetTacticsCommand(TacticsComandType.Alchemy,TacticsUtility.AlchemyCost(actorInfo,skillId));
            PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
            actorInfo.SetNextLearnSkillId(skillId);
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
                    actorInfo.SetNextBattleEnemyIndex(TacticsEnemies()[_currentEnemyIndex].EnemyData.Id);
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
        GameSystem.CurrentData.CurrentStage.SeekStage();
    }
}

namespace TacticsModelData{
    public class CommandData{
        public string key = "";
        public string name;
    }
}
