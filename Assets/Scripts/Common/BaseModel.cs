﻿using System;
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
    public StageInfo CurrentStage{get {return GameSystem.CurrentData.CurrentStage;}}
    public AlcanaInfo CurrentAlcana{get {return GameSystem.CurrentData.CurrentAlcana;}}

    public PartyInfo PartyInfo{get {return CurrentData.Party;}}

    public int Currency{get {return PartyInfo.Currency;}}

    public int Turns{get {return CurrentStage.Turns - CurrentStage.CurrentTurn - 1;}}
    
    public List<ActorInfo> Actors()
    {
        return GameSystem.CurrentData.Actors;
    }

    public List<ActorInfo> StageMembers()
    {
        List<int> SelectActorIds = CurrentStage.SelectActorIds;
        return Actors().FindAll(a => SelectActorIds.Contains(a.ActorId));
    }

    public List<ActorInfo> PartyMembers()
    {
        List<int> PartyMembersIds = PartyInfo.ActorIdList;
        return Actors().FindAll(a => PartyMembersIds.Contains(a.ActorId));
    }

    public List<StagesData.StageEventData> StageEventDatas{ 
        get{ return DataSystem.Stages.Find(a => a.Id == CurrentStage.Id).StageEvents;}
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
        return CurrentStage.TacticsTroops();
    }

    public TroopInfo CurrentTroopInfo()
    {
        return CurrentStage.CurrentTroopInfo();
    }

    public void SetIsSubordinate(bool isSubordinate)
    {
        CurrentStage.SetIsSubordinate(isSubordinate);
    }

    public void SetIsAlcana(bool isAlcana)
    {
        CurrentAlcana.SetIsAlcana(isAlcana);
    }

    public bool CheckIsAlcana()
    {
        return CurrentAlcana.IsAlcana && CurrentAlcana.AlcanaIds.Count > 0 && CurrentAlcana.UsedAlcana == false;
    }

    public void MakeAlcana()
    {
        CurrentAlcana.AddAlcanaId();
    }

    public void OpenAlcana()
    {
        CurrentAlcana.OpenAlcana();
    }

    public void CurrentSelectAlcana()
    {
        CurrentAlcana.OpenAlcana();
    }

    public void UseAlcana()
    {
        CurrentAlcana.UseAlcana(true);
        SkillInfo skill = new SkillInfo(CurrentAlcana.CurrentSelectAlcana().SkillId);
        if (skill.Master.SkillType == SkillType.UseAlcana)
        {
            // 基本的に味方全員
            if (skill.Master.TargetType == TargetType.Friend)
            {
                List<int> targetIndexs = new List<int>();
                if (skill.Master.Scope == ScopeType.All)
                {
                    foreach (var item in StageMembers())
                    {
                        targetIndexs.Add(item.ActorId);
                    }
                }
                
                for (int i = 0; i < targetIndexs.Count; i++)
                {
                    foreach (var featureData in skill.Master.FeatureDatas)
                    {
                        ActorInfo target = StageMembers()[i];
                        if (featureData.FeatureType == FeatureType.AddState)
                        {
                            StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,-1,0);
                            CurrentAlcana.SetAlacanaState(stateInfo);
                        }
                        if (featureData.FeatureType == FeatureType.HpHeal)
                        {
                            target.ChangeHp(featureData.Param1);
                        }
                        if (featureData.FeatureType == FeatureType.MpHeal)
                        {
                            target.ChangeMp(featureData.Param1);
                        }
                        if (featureData.FeatureType == FeatureType.RemoveState)
                        {
                            if (featureData.Param1 == (int)StateType.Death)
                            {
                                target.ChangeLost(false);
                            }
                        }
                        if (featureData.FeatureType == FeatureType.TacticsCost)
                        {
                            target.ChangeTacticsCostRate((int)Mathf.Floor(featureData.Param1 * 0.01f));
                        }
                        if (featureData.FeatureType == FeatureType.AddSp)
                        {
                            target.ChangeSp(target.Sp + featureData.Param1);
                        }
                    }
                }

            }

            // パーティに影響
            if (skill.Master.TargetType == TargetType.Party)
            {
                foreach (var featureData in skill.Master.FeatureDatas)
                {
                    if (featureData.FeatureType == FeatureType.AddState)
                    {
                        if (skill.Master.TargetType == TargetType.Opponent)
                        {
                            StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,-1,0);
                            CurrentStage.ChengeCurrentTroopAddState(stateInfo);
                        }
                    }
                    if (featureData.FeatureType == FeatureType.Numinous)
                    {
                        PartyInfo.ChangeCurrency(Currency + featureData.Param1);
                    }
                    if (featureData.FeatureType == FeatureType.EnemyLv)
                    {
                        CurrentStage.ChengeCurrentTroopLevel(featureData.Param1 * 0.01f);
                    }
                    if (featureData.FeatureType == FeatureType.Subordinate)
                    {
                        CurrentStage.ChangeSubordinate(featureData.Param1);
                    }
                    if (featureData.FeatureType == FeatureType.Alcana)
                    {
                        CurrentAlcana.ChangeNextAlcana();
                    }
                    if (featureData.FeatureType == FeatureType.LineChange)
                    {
                        CurrentStage.ChengeCurrentTroopLine();
                    }
                    if (featureData.FeatureType == FeatureType.LineZeroErase)
                    {
                        CurrentStage.ChengeCurrentTroopLineZeroErase();
                    }
                    if (featureData.FeatureType == FeatureType.EnemyHp)
                    {
                        CurrentStage.ChengeCurrentTroopHp(featureData.Param1 * 0.01f);
                    }
                }
            }
        }
        
    }

    public void DeleteAlcana()
    {
        CurrentData.CurrentAlcana.DeleteAlcana();
    }

    public string SelectAddActorConfirmText(string actorName)
    {
        int textId = 0;
        if (CurrentStage == null)
        {
            textId = 11060;
        } else
        {
            textId = 11070;
        }
        return DataSystem.System.GetTextData(textId).Text.Replace("\\d",actorName);
    }

    public void SetStageActor()
    {
        //　加入しているパーティを生成
        List<ActorInfo> selectActorIds = Actors().FindAll(a => StageMembers().Contains(a));
        
        PartyInfo.InitActors();
        foreach (var actorInfo in selectActorIds)
        {
            PartyInfo.AddActor(actorInfo.ActorId);
        }
    }

    public void SetSelectAddActor()
    {
        //　加入していないパーティを生成
        List<ActorInfo> selectActorIds = Actors().FindAll(a => !StageMembers().Contains(a));
        
        PartyInfo.InitActors();
        foreach (var actorInfo in selectActorIds)
        {
            PartyInfo.AddActor(actorInfo.ActorId);
        }
    }

    public void SetDefineBossIndex(int index)
    {
        CurrentStage.SetDefineBossIndex(index);
    }
}
