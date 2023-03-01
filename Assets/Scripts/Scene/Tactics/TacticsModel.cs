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
    
    public List<ActorInfo> Actors()
    {
        return GameSystem.CurrentData.Actors;
    }

    public ActorInfo TacticsActor(int actorId)
    {
        return Actors().Find(a => a.ActorId == actorId);
    }

    public List<BattlerInfo> Enemies()
    {
        List<BattlerInfo> battlerInfos = new List<BattlerInfo>();
        EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == 1);
        BattlerInfo battlerInfo = new BattlerInfo(enemyData,1,0,0);
        battlerInfos.Add(battlerInfo);
        return battlerInfos;
    }


    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        if (attributeType != AttributeType.None)
        {
            _currentAttributeType = attributeType;
        }
        return CurrentActor.Skills.FindAll(a => a.Attribute == _currentAttributeType);
    }

    public List<AttributeType> AttributeTypes()
    {
        List<AttributeType> attributeTypes = new List<AttributeType>();
        foreach(var attribute in Enum.GetValues(typeof(AttributeType)))
        {
            if ((int)attribute != 0)
            {
                attributeTypes.Add((AttributeType)attribute);
            }
        } 
        return attributeTypes;
    }

    public List<SystemData.MenuCommandData> TacticsCommand
    {
        get { return DataSystem.TacticsCommand;}
    }

    public List<Sprite> ActorsImage(List<ActorInfo> actors){
        var sprites = new List<Sprite>();
        for (var i = 0;i < actors.Count;i++)
        {
            var actorData = DataSystem.Actors.Find(actor => actor.Id == actors[i].ActorId);
            var asset = Addressables.LoadAssetAsync<Sprite>(
                "Assets/Images/Actors/" + actorData.ImagePath + "/main.png"
            );
            asset.WaitForCompletion();
            sprites.Add(asset.Result);
            Addressables.Release(asset);
        }
        return sprites;
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
                    actorInfo.SetNextBattleEnemyIndex(Enemies()[_currentEnemyIndex].EnemyData.Id);
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

    }
}

namespace TacticsModelData{
    public class CommandData{
        public string key = "";
        public string name;
    }
}
