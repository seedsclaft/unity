using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Threading;
using System.Threading.Tasks;

public class StatusModel : BaseModel
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


    
    public List<ActorInfo> StatusActors(){
        List<int> StatusActorIds = PartyInfo.ActorIdList;
        return Actors().FindAll(a => StatusActorIds.Contains(a.ActorId));
    }

    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        _currentAttributeType = attributeType;
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

    public List<SystemData.MenuCommandData> StatusCommand
    {
        get { return DataSystem.StatusCommand;}
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
        BGMData bGMData = DataSystem.Data.GetBGM("Status");
        List<string> data = new List<string>();
        data.Add("BGM/" + bGMData.FileName + "_intro.ogg");
        data.Add("BGM/" + bGMData.FileName + "_loop.ogg");
        
        var result1 = await ResourceSystem.LoadAsset<AudioClip>(data[0]);
        var result2 = await ResourceSystem.LoadAsset<AudioClip>(data[1]);
        return new List<AudioClip>(){
            result1,result2
        };    
    }

    public void MakeStageData()
    {
        GameSystem.CurrentData.MakeStageData(CurrentActor.ActorId);
    }

    public bool EnableParamUp(StatusParamType statusParamType)
    {
        return CurrentActor.Sp >= CurrentActor.UsePoint.GetParameter(statusParamType);
    }

    public bool EnableParamMinus(StatusParamType statusParamType)
    {
        return CurrentActor.TempStatus.GetParameter(statusParamType) > 0;
    }

    public void ChangeParameter(StatusParamType statusParamType,int value)
    {
        if (value > 0)
        {
            CurrentActor.ChangeSp(CurrentActor.Sp - CurrentActor.UsePointCost(statusParamType));
            CurrentActor.TempStatus.AddParameter(statusParamType,value);
        } else
        {
            CurrentActor.TempStatus.AddParameter(statusParamType,value);
            CurrentActor.ChangeSp(CurrentActor.Sp + CurrentActor.UsePointCost(statusParamType));          
        }
    }

    public void DecideStrength()
    {
        CurrentActor.DecideStrength();
    }
}

namespace StatusModelData{
    public class CommandData{
        public string key = "";
        public string name;
    }
}
