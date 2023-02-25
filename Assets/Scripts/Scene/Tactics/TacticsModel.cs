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
        get {return Actors()[_currentIndex];}
    }

    public void ChangeActorIndex(int value){
        _currentIndex += value;
        if (_currentIndex > Actors().Count-1){
            _currentIndex = 0;
        } else
        if (_currentIndex < 0){
            _currentIndex = Actors().Count-1;
        }
    }

    private TacticsComandType _commandType = TacticsComandType.Train;
    public TacticsComandType CommandType { get {return _commandType;} set {_commandType = value;}}
    private List<ActorInfo> _tempTacticsData = new List<ActorInfo>();

    
    public List<ActorInfo> Actors(){
        return GameSystem.CurrentData.Actors;
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

    public List<SystemData.MenuCommandData> ConfirmCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = "決定";
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        SystemData.MenuCommandData noCommand = new SystemData.MenuCommandData();
        noCommand.Key = "No";
        noCommand.Name = "中止";
        noCommand.Id = 1;
        menuCommandDatas.Add(noCommand);
        return menuCommandDatas;
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
                    if (tacticsComandType == TacticsComandType.Train)
                    {
                        PartyInfo.ChangeCurrency(Currency + Actors()[i].TacticsCost);
                    }
                    if (tacticsComandType == TacticsComandType.Alchemy)
                    {
                        PartyInfo.ChangeCurrency(Currency + Actors()[i].TacticsCost);
                        Actors()[i].SetNextLearnSkillId(0);
                    }
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
        if (actorInfo.TacticsComandType != TacticsComandType.None && actorInfo.TacticsComandType != tacticsComandType)
        {
            return false;
        }
        if (tacticsComandType == TacticsComandType.Train)
        {
            return Currency >= TacticsUtility.TrainCost(actorInfo);
        }
        if (tacticsComandType == TacticsComandType.Alchemy)
        {
            return true;
        }
        return false;
    }

    public void SelectActorTrain(int actorId)
    {
        ActorInfo actorInfo = Actors().Find(a => a.ActorId == actorId);
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
        ActorInfo actorInfo = Actors().Find(a => a.ActorId == actorId);
        if (actorInfo.TacticsComandType == TacticsComandType.Alchemy)
        {   
            PartyInfo.ChangeCurrency(Currency + actorInfo.TacticsCost);
            actorInfo.ClearTacticsCommand();
            return true;
        }
        return false;
    }

    public List<SkillInfo> SelectActorAlchemy(int actorId)
    {
        _currentIndex = actorId;
        ActorInfo actorInfo = Actors().Find(a => a.ActorId == actorId);
        
        List<SkillInfo> skillInfos = new List<SkillInfo>();
        
        {
            for (int i = 0;i < PartyInfo.AlchemyIdList.Count;i++)
            {
                SkillInfo skillInfo = new SkillInfo(PartyInfo.AlchemyIdList[i]);
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
        ActorInfo actorInfo = Actors().Find(a => a.ActorId == _currentIndex);
        if (actorInfo != null){
            actorInfo.SetTacticsCommand(TacticsComandType.Alchemy,TacticsUtility.AlchemyCost(actorInfo,skillId));
            PartyInfo.ChangeCurrency(Currency - actorInfo.TacticsCost);
            actorInfo.SetNextLearnSkillId(skillId);
        }
    }

    public void RefreshData()
    {
        int trainCost = 0;
        
    }
}

namespace TacticsModelData{
    public class CommandData{
        public string key = "";
        public string name;
    }
}
