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

    public void SetIsSubordinate(bool isSubordinate)
    {
        CurrentData.CurrentStage.SetIsSubordinate(isSubordinate);
    }

    public void SetIsAlcana(bool isAlcana)
    {
        CurrentData.CurrentStage.SetIsAlcana(isAlcana);
    }

    public bool CheckIsAlcana()
    {
        var stage = CurrentData.CurrentStage;
        return stage.IsAlcana && stage.AlcanaIds.Count > 0 && stage.UsedAlcana == false;
    }

    public void MakeAlcana()
    {
        int alcanaId = CurrentData.CurrentStage.AlcanaIds[0];
        CurrentData.CurrentStage.AlcanaIds.RemoveAt(0);
        CurrentData.CurrentStage.AddAlcanaId(alcanaId);
    }

    public void OpenAlcana()
    {
        CurrentData.CurrentStage.OpenAlcana();
    }

    public AlcanaData.Alcana CurrentAlcana()
    {
        return CurrentData.CurrentStage.CurrentAlcana();
    }

    public void UseAlcana()
    {
        CurrentData.CurrentStage.UseAlcana(true);
        var skill = new SkillInfo(1003);
        if (skill.Master.SkillType == SkillType.UseAlcana)
        {
            // 基本的に味方全員
            List<int> targetIndexs = new List<int>();
            if (skill.Master.TargetType == TargetType.Friend)
            {
                foreach (var item in Actors())
                {
                    targetIndexs.Add(item.ActorId);
                }
            }
            ActionInfo actionInfo = new ActionInfo(skill.Master.Id,0,-1,targetIndexs);
        
            List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
            for (int i = 0; i < targetIndexs.Count;i++)
            {
                ActorInfo Target = Actors().Find(a => a.ActorId == targetIndexs[i]);
                ActionResultInfo actionResultInfo = new ActionResultInfo(0,Target.ActorId,actionInfo);
                actionResultInfo.MakeResultData(null,null);
                actionResultInfos.Add(actionResultInfo);
            }
            actionInfo.SetActionResult(actionResultInfos);

            
            for (int i = 0; i < actionResultInfos.Count; i++)
            {
                ActorInfo target = Actors().Find(a => a.ActorId == actionResultInfos[i].TargetIndex);
                if (actionResultInfos[i].HpHeal != 0)
                {
                    target.ChangeHp(actionResultInfos[i].HpHeal);
                }
                if (actionResultInfos[i].MpHeal != 0)
                {
                    target.ChangeMp(actionResultInfos[i].MpHeal);
                }
            }
        }
        
    }

    public void DeleteAlcana()
    {
        CurrentData.CurrentStage.DeleteAlcana();
    }
}
