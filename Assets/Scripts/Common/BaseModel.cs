using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BaseModel
{
    public Scene CurrentScene {get { return _currentScene;} set {_currentScene = value;}}
    private Scene _currentScene = Scene.None;
    public SavePlayInfo CurrentData{get {return GameSystem.CurrentData;}}
    public TempInfo CurrentTempData{get {return GameSystem.CurrentTempData;}}

    public PartyInfo PartyInfo{get {return CurrentData.Party;}}

    public int Currency{get {return PartyInfo.Currency;}}

    public int Turns{get {return CurrentData.CurrentStage.Turns - CurrentData.CurrentStage.CurrentTurn - 1;}}
    
    public List<ActorInfo> Actors()
    {
        return GameSystem.CurrentData.Actors;
    }

    public async Task<List<AudioClip>> GetBgmData(string bgmKey){
        BGMData bGMData = DataSystem.Data.GetBGM(bgmKey);
        List<string> data = new List<string>();
        if (bGMData.Loop)
        {
            data.Add("BGM/" + bGMData.FileName + "_intro.ogg");
            data.Add("BGM/" + bGMData.FileName + "_loop.ogg");
        } else{
            data.Add("BGM/" + bGMData.FileName + ".ogg");
        }
        AudioClip result1 = null;
        AudioClip result2 = null;
        result1 = await ResourceSystem.LoadAsset<AudioClip>(data[0]);
        if (data.Count > 1)
        {
             result2 = await ResourceSystem.LoadAsset<AudioClip>(data[1]);
        }
        return new List<AudioClip>(){
            result1,result2
        };    
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

    public List<SystemData.MenuCommandData> NoChoiceConfirmCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = "確認";
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        return menuCommandDatas;
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

    
    public List<TroopInfo> TacticsTroops()
    {
        return CurrentData.CurrentStage.TacticsTroops();
    }

    public TroopInfo CurrentTroopInfo()
    {
        return CurrentData.CurrentStage.CurrentTroopInfo();
    }
}
