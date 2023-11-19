using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;
using Effekseer;

public class BaseModel
{
    public SavePlayInfo CurrentData => GameSystem.CurrentData;
    public StageInfo CurrentStage => CurrentData.CurrentStage;
    public AlcanaInfo CurrentAlcana => CurrentData.CurrentAlcana;

    public PartyInfo PartyInfo => CurrentData.Party;

    public int Currency => PartyInfo.Currency;

    public int Turns{get {return CurrentStage.Turns - (CurrentStage.CurrentTurn);}}

    public CancellationTokenSource _cancellationTokenSource;
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

    public List<ListData> CastActorInfos(List<ActorInfo> actorInfos)
    {
        var list = new List<ListData>();
        foreach (var actorInfo in actorInfos)
        {
            var listData = new ListData(actorInfo);
            list.Add(listData);
        }
        return list;
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
            var stageData = CurrentStage.Master;
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
        if (saveConfigInfo != null)
        {
            ChangeBGMValue(saveConfigInfo._bgmVolume);
            Ryneus.SoundManager.Instance._bgmMute = saveConfigInfo._bgmMute;
            ChangeSEValue(saveConfigInfo._seVolume);
            Ryneus.SoundManager.Instance._seMute = saveConfigInfo._seMute;
            ChangeGraphicIndex(saveConfigInfo._graphicIndex);
            ChangeEventSkipIndex(saveConfigInfo._eventSkipIndex);
            ChangeCommandEndCheck(saveConfigInfo._commandEndCheck);
            ChangeBattleWait(saveConfigInfo._battleWait);
            ChangeBattleAnimation(saveConfigInfo._battleAnimationSkip);
            ChangeInputType(saveConfigInfo._inputType);
            ChangeBattleAuto(saveConfigInfo._battleAuto);
        }
    }

    public float BGMVolume(){ return Ryneus.SoundManager.Instance._bgmVolume;}
    public bool BGMMute(){ return Ryneus.SoundManager.Instance._bgmMute;}
    public void ChangeBGMValue(float bgmVolume)
    {
        Ryneus.SoundManager.Instance._bgmVolume = bgmVolume;
        Ryneus.SoundManager.Instance.UpdateBgmVolume();
        if (bgmVolume > 0 && Ryneus.SoundManager.Instance._bgmMute == false)
        {
            ChangeBGMMute(false);
        }
        if (bgmVolume == 0 && Ryneus.SoundManager.Instance._bgmMute == true)
        {
            ChangeBGMMute(true);
        }
    }
    public void ChangeBGMMute(bool bgmMute)
    {
        Ryneus.SoundManager.Instance._bgmMute = bgmMute;
        Ryneus.SoundManager.Instance.UpdateBgmMute();
    }
    public float SEVolume(){ return Ryneus.SoundManager.Instance._seVolume;}
    public bool SEMute(){ return Ryneus.SoundManager.Instance._seMute;}
    
    public void ChangeSEValue(float seVolume)
    {
        Ryneus.SoundManager.Instance._seVolume = seVolume;
        Ryneus.SoundManager.Instance.UpdateSeVolume();
        if (seVolume > 0 && Ryneus.SoundManager.Instance._seMute == false)
        {
            ChangeSEMute(false);
        }
        if (seVolume == 0 && Ryneus.SoundManager.Instance._seMute == true)
        {
            ChangeSEMute(true);
        }
    }
    public void ChangeSEMute(bool seMute)
    {
        Ryneus.SoundManager.Instance._seMute = seMute;
    }

    public int GraphicIndex(){ return GameSystem.ConfigData._graphicIndex; }
    public void ChangeGraphicIndex(int graphicIndex)
    {
        GameSystem.ConfigData._graphicIndex = graphicIndex;
        QualitySettings.SetQualityLevel(graphicIndex);
    }

    public void ChangeEventSkipIndex(bool eventSkipIndex)
    {
        GameSystem.ConfigData._eventSkipIndex = eventSkipIndex;
    }

    public void ChangeCommandEndCheck(bool commandEndCheck)
    {
        GameSystem.ConfigData._commandEndCheck = !commandEndCheck;
    }

    public void ChangeBattleWait(bool battleWait)
    {
        GameSystem.ConfigData._battleWait = battleWait;
    }

    public void ChangeBattleAnimation(bool battleAnimation)
    {
        GameSystem.ConfigData._battleAnimationSkip = battleAnimation;
    }

    public void ChangeInputType(bool inputType)
    {
        GameSystem.ConfigData._inputType = inputType;
    }

    public void ChangeBattleAuto(bool battleAuto)
    {
        GameSystem.ConfigData._battleAuto = battleAuto;
    }

    public string PlayerName()
    {
        return CurrentData.PlayerInfo.PlayerName;
    }

    public List<StageEventData> StageEventDates{ 
        get{ return DataSystem.Stages.Find(a => a.Id == CurrentStage.Id).StageEvents;}
    }

    public List<StageEventData> StageEvents(EventTiming eventTiming)
    {
        int CurrentTurn = CurrentStage.CurrentTurn;
        List<string> eventKeys = CurrentStage.ReadEventKeys;
        return StageEventDates.FindAll(a => a.Timing == eventTiming && a.Turns == CurrentTurn && !eventKeys.Contains(a.EventKey));
    }
    
    public void AddEventsReadFlag(List<StageEventData> stageEventDates)
    {
        foreach (var eventData in stageEventDates)
        {
            AddEventReadFlag(eventData);
        }
    }

    public void AddEventReadFlag(StageEventData stageEventDates)
    {
        if (stageEventDates.ReadFlag)
        {
            CurrentStage.AddEventReadFlag(stageEventDates.EventKey);
        }
    }

    public List<EffekseerEffectAsset> BattleCursorEffects()
    {
        var list = new List<EffekseerEffectAsset>();
        var dates = DataSystem.Animations.FindAll(a => a.Id > 2000 && a.Id < 2100);
        foreach (var data in dates)
        {
            var path = "Animations/" + data.AnimationPath;
            var result = Resources.Load<EffekseerEffectAsset>(path);
            list.Add(result);
        }
        return list;
    }

    public async UniTask<List<AudioClip>> GetBgmData(string bgmKey){
        return await ResourceSystem.LoadBGMAsset(bgmKey);
    }

    public List<SystemData.CommandData> BaseConfirmCommand(int yesTextId,int noTextId = 0)
    {
        List<SystemData.CommandData> menuCommandDates = new List<SystemData.CommandData>();
        SystemData.CommandData yesCommand = new SystemData.CommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = DataSystem.System.GetTextData(yesTextId).Text;
        yesCommand.Id = 0;
        if (noTextId != 0)
        {
            SystemData.CommandData noCommand = new SystemData.CommandData();
            noCommand.Key = "No";
            noCommand.Name = DataSystem.System.GetTextData(noTextId).Text;
            noCommand.Id = 1;
            menuCommandDates.Add(noCommand);
        }
        menuCommandDates.Add(yesCommand);
        return menuCommandDates;
    }

    public List<ListData> ConfirmCommand()
    {
        var list = new List<ListData>();
        foreach (var commandData in BaseConfirmCommand(3050,3051))
        {
            var listData = new ListData(commandData);
            list.Add(listData);
        }
        return list;
    }

    public List<ListData> NoChoiceConfirmCommand()
    {
        var list = new List<ListData>(){};
        var listData = new ListData(BaseConfirmCommand(3052,0)[0]);
        list.Add(listData);
        return list;
    }
    
    public List<ListData> AttributeTypes()
    {
        List<AttributeType> attributeTypes = new List<AttributeType>();
        foreach(var attribute in Enum.GetValues(typeof(AttributeType)))
        {
            if ((int)attribute != 0)
            {
                attributeTypes.Add((AttributeType)attribute);
            }
        }
        var list = new List<ListData>();
        foreach (var attributeData in attributeTypes)
        {
            var attributeInfo = new SkillData.SkillAttributeInfo();
            attributeInfo.AttributeType = attributeData;
            var listData = new ListData(attributeInfo);
            list.Add(listData);
        }
        return list;
    }

    public List<ListData> AttributeAllTypes(ActorInfo actorInfo = null,int selectedIndex = -1)
    {
        var list = new List<ListData>();
        var idx = 0;
        List<AttributeType> attributeTypes = new List<AttributeType>();
        foreach(var attribute in Enum.GetValues(typeof(AttributeType)))
        {
            var attributeInfo = new SkillData.SkillAttributeInfo();
            attributeInfo.AttributeType = (AttributeType)attribute;
            attributeInfo.ValueText = "";
            var listData = new ListData(attributeInfo,idx);
            listData.SetSelected(selectedIndex == idx);
            list.Add(listData);
            idx++;
        } 
        return list;
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
            List<SkillData> skillDates = DataSystem.Skills.FindAll(a => a.Rank == getItemInfo.Param1 && a.Attribute == (AttributeType)((int)getItemInfo.GetItemType - 10));
            foreach (var skillData in skillDates)
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
        return CurrentStage.TacticsTroops(CurrentStage.CurrentTurn);
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

    public void ChangeSubordinate(bool isSubordinate)
    {
        CurrentStage.ChangeSubordinate(isSubordinate);
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
                List<int> targetIndexes = new List<int>();
                if (skill.Master.Scope == ScopeType.All)
                {
                    foreach (var item in StageMembers())
                    {
                        targetIndexes.Add(item.ActorId);
                    }
                }
                
                for (int i = 0; i < targetIndexes.Count; i++)
                {
                    foreach (var featureData in skill.Master.FeatureDates)
                    {
                        ActorInfo target = StageMembers()[i];
                        if (featureData.FeatureType == FeatureType.AddState)
                        {
                            StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,-1,0,skill.Id);
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
                foreach (var featureData in skill.Master.FeatureDates)
                {
                    if (featureData.FeatureType == FeatureType.AddState)
                    {
                        if (skill.Master.TargetType == TargetType.Opponent)
                        {
                            StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,-1,0,skill.Id);
                            CurrentStage.ChangeCurrentTroopAddState(stateInfo);
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
                        CurrentStage.ChangeCurrentTroopLineZeroErase();
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
        return DataSystem.System.GetReplaceText(textId,actorName);
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
        //　加入しているパーティ
        var stageMembers = StageMembers();
        //　加入していないパーティを生成
        List<ActorInfo> selectActorIds = Actors().FindAll(a => !StageMembers().Contains(a));
        
        PartyInfo.InitActors();
        foreach (var actorInfo in selectActorIds)
        {
            if (actorInfo.Lost == false)
            {
                if (stageMembers.Find(a => a.Master.ClassId == actorInfo.Master.ClassId) == null)
                {
                    PartyInfo.AddActor(actorInfo.ActorId);
                }
            }
        }
    }

    public void SetDefineBossIndex(int index)
    {
        CurrentStage.SetDefineBossIndex(index,CurrentStage.CurrentTurn);
    }

    public string GetAdvFile(int id)
    {
        return DataSystem.Advs.Find(a => a.Id == id).AdvName;
    }

    public void StageClear()
    {
        CurrentData.PlayerInfo.StageClear(CurrentStage.Id);
    }

    public void StageClear(int stageId)
    {
        CurrentData.PlayerInfo.StageClear(stageId);
    }

    public void ChangeRouteSelectStage(int stageBaseId)
    {
        int stageId = stageBaseId + CurrentStage.RouteSelect;
		GameSystem.CurrentData.ChangeRouteSelectStage(stageId);
    }

    public Dictionary<TacticsCommandType, int> CommandRankInfo()
    {
        return PartyInfo.CommandRankInfo;
    }

    public async UniTask LoadBattleResources(List<BattlerInfo> battlers)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var filePaths = BattleUtility.AnimationResourcePaths(battlers);
        int count = filePaths.Count;
        foreach (var filePath in filePaths)
        {
            await Resources.LoadAsync<Sprite>( filePath );
            count -= 1;
        }
        try {
            await UniTask.WaitUntil( () => count == 0 ,PlayerLoopTiming.Update,_cancellationTokenSource.Token);
        } catch (OperationCanceledException e)
        {
            Debug.Log(e);
        }
    }

    public void SetResumeStage(bool resumeStage)
    {
        CurrentData.SetResumeStage(resumeStage);
    }
    
    public void SetResumeStageTrue()
    {
        SetResumeStage(true);
        SaveSystem.SaveStart(GameSystem.CurrentData);
    }

    public void SetResumeStageFalse()
    {
        SetResumeStage(false);
        SaveSystem.SaveStart(GameSystem.CurrentData);
    }

    public ListData CastData(object data)
    {
        return new ListData(data);
    }

    public List<ListData> TroopInfoListDates(TroopInfo data)
    {
        var list = new List<ListData>();
        list.Add(CastData(data));
        return list;
    }

    public List<ListData> TroopInfoListDates(List<TroopInfo> dataList)
    {
        var list = new List<ListData>();
        foreach (var data in dataList)
        {
            list.Add(CastData(data));
        }
        return list;
    }
}
