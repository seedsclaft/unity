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

    public int Turns{get {return CurrentStage.Turns - (CurrentStage.CurrentTurn);}}
    
    public List<ActorInfo> Actors()
    {
        return GameSystem.CurrentData.Actors;
    }

    public void LostActors(List<ActorInfo> lostMembers)
    {
        lostMembers.ForEach(a => a.ChangeLost(true));
    }

    public List<ActorInfo> StageMembers()
    {
        List<int> SelectActorIds = CurrentStage.SelectActorIds;
        return Actors().FindAll(a => SelectActorIds.Contains(a.ActorId) && a.Lost == false);
    }

    public List<ActorInfo> PartyMembers()
    {
        List<int> PartyMembersIds = PartyInfo.ActorIdList;
        return Actors().FindAll(a => PartyMembersIds.Contains(a.ActorId));
    }

    public List<StagesData.StageEventData> StageEventDatas{ 
        get{ return DataSystem.Stages.Find(a => a.Id == CurrentStage.Id).StageEvents;}
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

    public async Task<List<AudioClip>> GetBgmData(string bgmKey){
        return await ResourceSystem.LoadBGMAsset(bgmKey);
    }

    public List<SystemData.MenuCommandData> ConfirmCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = DataSystem.System.GetTextData(3050).Text;
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        SystemData.MenuCommandData noCommand = new SystemData.MenuCommandData();
        noCommand.Key = "No";
        noCommand.Name = DataSystem.System.GetTextData(3051).Text;
        noCommand.Id = 1;
        menuCommandDatas.Add(noCommand);
        return menuCommandDatas;
    }

    public List<SystemData.MenuCommandData> NoChoiceConfirmCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = DataSystem.System.GetTextData(3052).Text;
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

    public SkillInfo BasicSkillInfo(int skillId)
    {
        SkillInfo skillInfo = new SkillInfo(skillId);
        skillInfo.SetEnable(true);
        return skillInfo;
    }

    
    public List<TroopInfo> TacticsTroops()
    {
        if (CurrentStage.TroopClearCount == 0 && CurrentStage.SelectActorIds.Count > 0)
        {
            return TutorialTroopData();
        }
        return CurrentStage.TacticsTroops();
    }

    public List<TroopInfo> TutorialTroopData()
    {
        return CurrentStage.MakeTutorialTroopData(CurrentStage.SelectActorIds[0]);
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
                            StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,-1,0,false);
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
                            StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,-1,0,false);
                            CurrentStage.ChengeCurrentTroopAddState(stateInfo);
                        }
                    }
                    if (featureData.FeatureType == FeatureType.Numinous)
                    {
                        PartyInfo.ChangeCurrency(Currency + featureData.Param1);
                    }
                    if (featureData.FeatureType == FeatureType.Subordinate)
                    {
                        CurrentStage.ChangeSubordinate(featureData.Param1);
                    }
                    if (featureData.FeatureType == FeatureType.Alcana)
                    {
                        CurrentAlcana.ChangeNextAlcana();
                    }
                    if (featureData.FeatureType == FeatureType.LineZeroErase)
                    {
                        CurrentStage.ChengeCurrentTroopLineZeroErase();
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

    public string GetAdvFile(int id)
    {
        return DataSystem.Advs.Find(a => a.Id == id).AdvName;
    }

    public void StageClaer()
    {
        CurrentData.StageClaer();
    }

    public Dictionary<TacticsComandType, int> CommandRankInfo()
    {
        return PartyInfo.CommandRankInfo;
    }
}
