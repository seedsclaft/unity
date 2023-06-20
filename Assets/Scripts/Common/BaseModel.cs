using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

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
    public void InitSaveInfo()
    {
        var playInfo = new SavePlayInfo();
        playInfo.InitSaveData();
        GameSystem.CurrentData = playInfo;
    }

    public void InitConfigInfo()
    {
        GameSystem.ConfigData = new SaveConfigInfo();
    }

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
        var members = new List<ActorInfo>();
        for (int i = 0;i < SelectActorIds.Count ;i++)
        {
            var temp = Actors().Find(a => a.ActorId == SelectActorIds[i] && a.Lost == false);
            if (temp != null)
            {
                members.Add(temp);
            }
        }
        return members;
    }

    public List<ActorInfo> PartyMembers()
    {
        List<int> PartyMembersIds = PartyInfo.ActorIdList;
        var members = new List<ActorInfo>();
        for (int i = 0;i < PartyMembersIds.Count ;i++)
        {
            var temp = Actors().Find(a => a.ActorId == PartyMembersIds[i]);
            if (temp != null)
            {
                members.Add(temp);
            }
        }
        return members;
    }

    public List<ActorInfo> ResultMembers()
    {
        List<int> SelectActorIds = CurrentStage.SelectActorIds;
        var members = new List<ActorInfo>();
        for (int i = 0;i < SelectActorIds.Count ;i++)
        {
            var temp = Actors().Find(a => a.ActorId == SelectActorIds[i]);
            if (temp != null)
            {
                members.Add(temp);
            }
        }
        return members;
    }

    public string TacticsBgmFilename()
    {
        if (CurrentStage != null)
        {
            var stageData = DataSystem.Stages.Find(a => a.Id == CurrentStage.Id);
            if (CurrentStage.CurrentTurn >= 24)
            {        
                return DataSystem.Data.GetBGM(stageData.BGMId[2]).Key;
            }
            if (CurrentStage.CurrentTurn >= 12)
            {        
                return DataSystem.Data.GetBGM(stageData.BGMId[1]).Key;
            }
            return DataSystem.Data.GetBGM(stageData.BGMId[0]).Key;
        }
        return "TACTICS1";
    }

    public void ApllyConfigData()
    {
        SaveConfigInfo saveConfigInfo = GameSystem.ConfigData;
        ChangeBGMValue(saveConfigInfo._bgmVolume);
        Ryneus.SoundManager.Instance._bgmMute = saveConfigInfo._bgmMute;
        ChangeSEValue(saveConfigInfo._seVolume);
        Ryneus.SoundManager.Instance._seMute = saveConfigInfo._seMute;
        ChangeGraphicIndex(saveConfigInfo._graphicIndex);
    }

    public float BGMVolume(){ return Ryneus.SoundManager.Instance._bgmVolume;}
    public bool BGMMute() { return Ryneus.SoundManager.Instance._bgmMute;}
    public void ChangeBGMValue(float bgmVolume)
    {
        Ryneus.SoundManager.Instance._bgmVolume = bgmVolume;
        Ryneus.SoundManager.Instance.UpdateBgmVolume();
    }
    public void ChangeBGMMute(bool bgmMute)
    {
        Ryneus.SoundManager.Instance._bgmMute = bgmMute;
        Ryneus.SoundManager.Instance.UpdateBgmMute();
    }
    public float SEVolume(){ return Ryneus.SoundManager.Instance._seVolume;}
    public bool SEMute() { return Ryneus.SoundManager.Instance._seMute;}
    
    public void ChangeSEValue(float seVolume)
    {
        Ryneus.SoundManager.Instance._seVolume = seVolume;
        Ryneus.SoundManager.Instance.UpdateSeVolume();
    }
    public void ChangeSEMute(bool seMute)
    {
        Ryneus.SoundManager.Instance._seMute = seMute;
    }

    public int GraphicIndex() { return GameSystem.ConfigData._graphicIndex; }
    public void ChangeGraphicIndex(int graphicIndex)
    {
        GameSystem.ConfigData._graphicIndex = graphicIndex;
        QualitySettings.SetQualityLevel(graphicIndex);
    }

    public string PlayerName()
    {
        return CurrentData.PlayerInfo.PlayerName;
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

    public async UniTask<List<AudioClip>> GetBgmData(string bgmKey){
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

    public List<SkillInfo> BasicSkillInfos(GetItemInfo getItemInfo)
    {
        List<SkillInfo> skillInfos = new List<SkillInfo>();
        if (getItemInfo.IsSkill())
        {
            SkillInfo skillInfo = new SkillInfo(getItemInfo.Param1);
            skillInfo.SetEnable(true);
            skillInfos.Add(skillInfo);
        }
        if (getItemInfo.IsAttributeSkill())
        {
            List<SkillsData.SkillData> skillDatas = DataSystem.Skills.FindAll(a => a.Rank == getItemInfo.Param1 && a.Attribute == (AttributeType)((int)getItemInfo.GetItemType - 10));
            foreach (var skillData in skillDatas)
            {
                SkillInfo skillInfo = new SkillInfo(skillData.Id);
                skillInfo.SetEnable(true);
                skillInfos.Add(skillInfo);
            }
        }
        return skillInfos;
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

    public List<TroopInfo> RouteSelectTroopData()
    {
        return CurrentStage.MakeRouteSelectTroopData(CurrentStage.RouteSelect);
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
        PartyInfo.InitActors();
        foreach (var actorInfo in StageMembers())
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

    public void ChangeRouteSelectStage(int stageBaseId)
    {
        int stageId = stageBaseId + CurrentStage.RouteSelect;
		GameSystem.CurrentData.ChangeRouteSelectStage(stageId);
    }

    public Dictionary<TacticsComandType, int> CommandRankInfo()
    {
        return PartyInfo.CommandRankInfo;
    }

    public async UniTask LoadBattleResources(List<BattlerInfo> battlers)
    {
        var filePaths = BattleUtility.AnimationResourcePaths(battlers);
        int count = filePaths.Count;
        foreach (var filePath in filePaths)
        {
            await Resources.LoadAsync<Sprite>( filePath );
            count -= 1;
        }
        await UniTask.WaitUntil( () => count == 0 );
    }
}
