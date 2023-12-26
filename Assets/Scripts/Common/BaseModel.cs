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
    public SaveInfo CurrentData => GameSystem.CurrentData;
    public SaveStageInfo CurrentStageData => GameSystem.CurrentStageData;
    public TempInfo TempData => GameSystem.TempData;
    public StageInfo CurrentStage => CurrentStageData.CurrentStage;
    public AlcanaInfo CurrentAlcana => CurrentStageData.CurrentAlcana;

    public PartyInfo PartyInfo => CurrentStageData.Party;

    public int Currency => PartyInfo.Currency;

    public int Turns{get {return CurrentStage.Turns - (CurrentStage.CurrentTurn);}}
    public int DisplayTurns{get {return CurrentStage.DisplayTurns - (CurrentStage.CurrentTurn);}}

    public CancellationTokenSource _cancellationTokenSource;
    private List<StageTutorialData> _currentStageTutorialDates = new ();
    public List<StageTutorialData> CurrentStageTutorialDates => _currentStageTutorialDates;
    public void InitSaveInfo()
    {
        GameSystem.CurrentData = new SaveInfo();;
    }

    public void InitSaveStageInfo()
    {
        var saveStageInfo = new SaveStageInfo();
        saveStageInfo.Initialize();
        GameSystem.CurrentStageData = saveStageInfo;
    }

    public void InitConfigInfo()
    {
        GameSystem.ConfigData = new SaveConfigInfo();
    }

    public List<ActorInfo> Actors()
    {
        return CurrentStageData.Actors;
    }

    public void LostActors(List<ActorInfo> lostMembers)
    {
        lostMembers.ForEach(a => a.ChangeLost(true));
    }

    public List<ActorInfo> StageMembers()
    {
        var SelectActorIds = CurrentStage.SelectActorIds;
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
        var PartyMembersIds = PartyInfo.ActorIdList;
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
        var SelectActorIds = CurrentStage.SelectActorIds;
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

    public List<ActorInfo> StatusActors(){
        var StatusActorIds = PartyInfo.ActorIdList;
        var members = new List<ActorInfo>();
        for (int i = 0;i< StatusActorIds.Count;i++)
        {
            var temp = Actors().Find(a => a.ActorId == StatusActorIds[i]);
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
            return DataSystem.Data.GetBGM(stageData.BGMId).Key;
        }
        return "TACTICS1";
    }

    public void ApplyConfigData()
    {
        var saveConfigInfo = GameSystem.ConfigData;
        if (saveConfigInfo != null)
        {
            ChangeBGMValue(saveConfigInfo.BgmVolume);
            Ryneus.SoundManager.Instance.BGMMute = saveConfigInfo.BgmMute;
            ChangeSEValue(saveConfigInfo.SeVolume);
            Ryneus.SoundManager.Instance.SeMute = saveConfigInfo.SeMute;
            ChangeGraphicIndex(saveConfigInfo.GraphicIndex);
            ChangeEventSkipIndex(saveConfigInfo.EventSkipIndex);
            ChangeCommandEndCheck(saveConfigInfo.CommandEndCheck);
            ChangeBattleWait(saveConfigInfo.BattleWait);
            ChangeBattleAnimation(saveConfigInfo.BattleAnimationSkip);
            ChangeInputType(saveConfigInfo.InputType);
            ChangeBattleAuto(saveConfigInfo.BattleAuto);
        }
    }

    public void ChangeBGMValue(float bgmVolume)
    {
        Ryneus.SoundManager.Instance.BGMVolume = bgmVolume;
        Ryneus.SoundManager.Instance.UpdateBgmVolume();
        if (bgmVolume > 0 && Ryneus.SoundManager.Instance.BGMMute == false)
        {
            ChangeBGMMute(false);
        }
        if (bgmVolume == 0 && Ryneus.SoundManager.Instance.BGMMute == true)
        {
            ChangeBGMMute(true);
        }
    }

    public void ChangeBGMMute(bool bgmMute)
    {
        Ryneus.SoundManager.Instance.BGMMute = bgmMute;
        Ryneus.SoundManager.Instance.UpdateBgmMute();
    }
    
    public void ChangeSEValue(float seVolume)
    {
        Ryneus.SoundManager.Instance.SeVolume = seVolume;
        Ryneus.SoundManager.Instance.UpdateSeVolume();
        if (seVolume > 0 && Ryneus.SoundManager.Instance.SeMute == false)
        {
            ChangeSEMute(false);
        }
        if (seVolume == 0 && Ryneus.SoundManager.Instance.SeMute == true)
        {
            ChangeSEMute(true);
        }
    }

    public void ChangeSEMute(bool seMute)
    {
        Ryneus.SoundManager.Instance.SeMute = seMute;
    }

    public int GraphicIndex(){ return GameSystem.ConfigData.GraphicIndex; }

    public void ChangeGraphicIndex(int graphicIndex)
    {
        GameSystem.ConfigData.GraphicIndex = graphicIndex;
        QualitySettings.SetQualityLevel(graphicIndex);
    }

    public void ChangeEventSkipIndex(bool eventSkipIndex)
    {
        GameSystem.ConfigData.EventSkipIndex = eventSkipIndex;
    }

    public void ChangeCommandEndCheck(bool commandEndCheck)
    {
        GameSystem.ConfigData.CommandEndCheck = commandEndCheck;
    }

    public void ChangeBattleWait(bool battleWait)
    {
        GameSystem.ConfigData.BattleWait = battleWait;
    }

    public void ChangeBattleAnimation(bool battleAnimation)
    {
        GameSystem.ConfigData.BattleAnimationSkip = battleAnimation;
    }

    public void ChangeInputType(bool inputType)
    {
        GameSystem.ConfigData.InputType = inputType;
    }

    public void ChangeBattleAuto(bool battleAuto)
    {
        GameSystem.ConfigData.BattleAuto = battleAuto;
    }

    public string PlayerName()
    {
        return CurrentData.PlayerInfo.PlayerName;
    }

    public List<StageEventData> StageEventDates{ 
        get{ return DataSystem.Stages.Find(a => a.Id == CurrentStage.Id).StageEvents;}
    }

    public List<StageTutorialData> StageTutorialDates{ 
        get{ return DataSystem.Stages.Find(a => a.Id == CurrentStage.Id).Tutorials;}
    }

    public List<StageEventData> StageEvents(EventTiming eventTiming)
    {
        int CurrentTurn = CurrentStage.CurrentTurn;
        var eventKeys = CurrentStage.ReadEventKeys;
        return StageEventDates.FindAll(a => a.Timing == eventTiming && a.Turns == CurrentTurn && !eventKeys.Contains(a.EventKey));
    }
    
    public bool SetStageTutorials(EventTiming eventTiming)
    {
        int CurrentTurn = CurrentStage.CurrentTurn;
        _currentStageTutorialDates = StageTutorialDates.FindAll(a => a.Timing == eventTiming && a.Turns == CurrentTurn);
        return _currentStageTutorialDates.Count > 0;
    }

    public void SeekTutorial()
    {
        if (_currentStageTutorialDates.Count == 0) return;
        _currentStageTutorialDates.RemoveAt(0);
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
            var result = ResourceSystem.LoadResourceEffect(data.AnimationPath);
            list.Add(result);
        }
        return list;
    }

    public async UniTask<List<AudioClip>> GetBgmData(string bgmKey){
        return await ResourceSystem.LoadBGMAsset(bgmKey);
    }

    public List<SystemData.CommandData> BaseConfirmCommand(int yesTextId,int noTextId = 0)
    {
        var menuCommandDates = new List<SystemData.CommandData>();
        var yesCommand = new SystemData.CommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = DataSystem.System.GetTextData(yesTextId).Text;
        yesCommand.Id = 0;
        if (noTextId != 0)
        {
            var noCommand = new SystemData.CommandData();
            noCommand.Key = "No";
            noCommand.Name = DataSystem.System.GetTextData(noTextId).Text;
            noCommand.Id = 1;
            menuCommandDates.Add(noCommand);
        }
        menuCommandDates.Add(yesCommand);
        return menuCommandDates;
    }

    public List<ListData> MakeListData<T>(List<T> dataList)
    {
        return ListData.MakeListData(dataList);
    }

    public List<ListData> ConfirmCommand()
    {
        return MakeListData(BaseConfirmCommand(3050,3051));
    }

    public List<ListData> NoChoiceConfirmCommand()
    {
        return MakeListData(new List<SystemData.CommandData>(){BaseConfirmCommand(3052,0)[0]});
    }

    public List<SkillInfo> BasicSkillInfos(GetItemInfo getItemInfo)
    {
        var skillInfos = new List<SkillInfo>();
        if (getItemInfo.IsSkill())
        {
            var skillInfo = new SkillInfo(getItemInfo.Param1);
            skillInfo.SetEnable(true);
            skillInfos.Add(skillInfo);
        }
        if (getItemInfo.IsAttributeSkill())
        {
            var skillDates = DataSystem.Skills.FindAll(a => a.Rank == getItemInfo.Param1 && a.Attribute == (AttributeType)((int)getItemInfo.GetItemType - 10));
            foreach (var skillData in skillDates)
            {
                var skillInfo = new SkillInfo(skillData.Id);
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

    public List<SkillInfo> CheckAlcanaSkillInfos(TriggerTiming triggerTiming)
    {
        var skillInfos = CurrentAlcana.CheckAlcanaSkillInfo(triggerTiming);
        return skillInfos.FindAll(a => a.Master.TriggerDates.Find(b => b.Param1 == DisplayTurns) != null);
    }

    public void SetAlcanaSkillInfo(List<SkillInfo> skillInfos)
    {
        TempData.SetAlcanaSkillInfo(skillInfos);
    }

    public List<GetItemInfo> GetAlcanaResults(SkillInfo skillInfo)
    {
        var list = new List<GetItemInfo>();
        if (CurrentAlcana.IsAlcana)
        {
            foreach (var featureData in skillInfo.Master.FeatureDates)
            {
                var getItemInfo = new GetItemInfo(null);
                switch (featureData.FeatureType)
                {
                    case FeatureType.GainTurn:
                        getItemInfo.MakeGainTurnResult((CurrentStageData.CurrentStage.DisplayTurns).ToString());
                        CurrentStageData.CurrentStage.DeSeekStage();
                        break;
                }
                list.Add(getItemInfo);
            }
        }
        return list;
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
        var selectActorIds = Actors().FindAll(a => !StageMembers().Contains(a));
        
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
        CurrentStage.SetDefineBossIndex(index);
    }

    public string GetAdvFile(int id)
    {
        return DataSystem.Adventures.Find(a => a.Id == id).AdvName;
    }

    public void StageClear()
    {
        CurrentData.PlayerInfo.StageClear(CurrentStage.BaseStageId);
    }
    
    public bool IsSuccessStage()
    {
        return (CurrentStage.SubordinateValue >= CurrentStage.Master.SubordinateValue);
    }

    public void ChangeRouteSelectStage(int stageId)
    {
        // stageId + RouteSelect
        int route = GameSystem.CurrentStageData.CurrentStage.RouteSelect;
        CurrentStageData.ChangeRouteSelectStage(stageId + route);
    }

    public void SetDisplayTurns()
    {
        CurrentStage.SetDisplayTurns();
    }

    public void MoveStage(int stageId)
    {
		CurrentStageData.MoveStage(stageId);
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
        CurrentStageData.SetResumeStage(resumeStage);
    }
    
    public void SavePlayerData()
    {
        SaveSystem.SavePlayerInfo(GameSystem.CurrentData);
    }

    public void SavePlayerStageData(bool isResumeStage)
    {
        SetResumeStage(isResumeStage);
        SaveSystem.SaveStageInfo(GameSystem.CurrentStageData);
    }
    
    public List<ListData> RebornSkillInfos(ActorInfo actorInfo)
    {
        var skillInfos = actorInfo.RebornSkillInfos;

        skillInfos.ForEach(a => a.SetEnable(true));
        var sortList1 = new List<SkillInfo>();
        var sortList2 = new List<SkillInfo>();
        var sortList3 = new List<SkillInfo>();
        skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
        foreach (var skillInfo in skillInfos)
        {
            if (skillInfo.Master.IconIndex <= MagicIconType.Psionics)
            {
                sortList1.Add(skillInfo);
            } else
            if (skillInfo.Master.IconIndex >= MagicIconType.Demigod)
            {
                sortList2.Add(skillInfo);
            } else
            {
                sortList3.Add(skillInfo);
            }
        }
        skillInfos.Clear();
        skillInfos.AddRange(sortList1);
        skillInfos.AddRange(sortList2);
        skillInfos.AddRange(sortList3);
        return MakeListData(skillInfos);
    }

    public List<ActorInfo> EvaluateMembers()
    {
        var selectActorIds = CurrentStageData.CurrentStage.SelectActorIds;
        var members = new List<ActorInfo>();
        for (int i = 0;i < selectActorIds.Count ;i++)
        {
            var temp = CurrentStageData.Actors.Find(a => a.ActorId == selectActorIds[i]);
            if (temp != null)
            {
                members.Add(temp);
            }
        }
        return members;
    }

#if UNITY_ANDROID
    public List<RankingActorData> RankingActorDates()
    {
        var list = new List<RankingActorData>();
        foreach (var actorInfo in EvaluateMembers())
        {
            var skillIds = new List<int>();
            foreach (var skill in actorInfo.Skills)
            {
                skillIds.Add(skill.Id);
            }
            var rankingActorData = new RankingActorData()
            {
                ActorId = actorInfo.ActorId,
                Level = actorInfo.Level,
                Hp = actorInfo.CurrentParameter(StatusParamType.Hp),
                Mp = actorInfo.CurrentParameter(StatusParamType.Mp),
                Atk = actorInfo.CurrentParameter(StatusParamType.Atk),
                Def = actorInfo.CurrentParameter(StatusParamType.Def),
                Spd = actorInfo.CurrentParameter(StatusParamType.Spd),
                SkillIds = skillIds,
                DemigodParam = actorInfo.DemigodParam,
                Lost = actorInfo.Lost
            };
            list.Add(rankingActorData);
        }
        return list;
    }
#endif
    public int TotalEvaluate()
    {        
        var evaluate = 0;
        foreach (var actorInfo in EvaluateMembers())
        {
            evaluate += actorInfo.Evaluate();
        }
        if (CurrentStage.EndingType == global::EndingType.A)
        {
            evaluate += 1000;
        }
        if (CurrentStage.EndingType == global::EndingType.B)
        {
            evaluate += 500;
        }
        return evaluate;
    }

    public List<int> SelectIdxList()
    {
        var selectIdx = new List<int>();
        foreach (var actorInfo in EvaluateMembers())
        {
            selectIdx.Add(actorInfo.ActorId);
        }
        return selectIdx;
    }

    public List<int> SelectRankList()
    {
        var selectIdRank = new List<int>();
        foreach (var actorInfo in EvaluateMembers())
        {
            selectIdRank.Add(actorInfo.Evaluate());
        }
        return selectIdRank;
    }

    public async void CurrentRankingData(System.Action<string> endEvent)
    {
        var userId = CurrentData.PlayerInfo.UserId.ToString();
        var rankingText = "";
#if (UNITY_WEBGL || UNITY_ANDROID)// && !UNITY_EDITOR
        FirebaseController.Instance.CurrentRankingData(userId);
        await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
        var currentScore = FirebaseController.CurrentScore;
        var evaluate = TotalEvaluate();

        if (evaluate > currentScore)
        {
            FirebaseController.Instance.WriteRankingData(
                userId,
                evaluate,
                CurrentData.PlayerInfo.PlayerName,
                SelectIdxList(),
                SelectRankList(),
                RankingActorDates()
            );
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);

            FirebaseController.Instance.ReadRankingData();
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
            var results = FirebaseController.RankingInfos;
            var rank = 1;
            var include = false;
            foreach (var result in results)
            {
                if (result.Score == evaluate)
                {
                    include = true;
                }
                if (result.Score > evaluate)
                {
                    rank++;
                }
            }

            if (include == true)
            {
                // 〇位
                rankingText = rank.ToString() + DataSystem.System.GetTextData(16070).Text;
            } else
            {
                // 圏外
                rankingText = DataSystem.System.GetTextData(16071).Text;
            }
        } else
        {          
            // 記録更新なし  
            rankingText = DataSystem.System.GetTextData(16072).Text;
        }
#endif
        endEvent(rankingText);
    }

    public string SavePopupTitle()
    {
        var baseText = DataSystem.System.GetTextData(11080).Text;
        var subText = DataSystem.System.GetReplaceText(11081,DataSystem.System.LimitSaveCount.ToString());
        var savedCount = DataSystem.System.GetReplaceText(11083,(CurrentStage.SavedCount+1).ToString());
        return baseText + savedCount + "\n" + subText;
    }

    public string FailedSavePopupTitle()
    {
        var baseText = DataSystem.System.GetTextData(11082).Text;
        return baseText;
    }

    public bool NeedAdsSave()
    {
        var needAds = false;
#if UNITY_ANDROID
        needAds = (CurrentStage.SavedCount + 1) >= DataSystem.System.LimitSaveCount;
#endif
        return needAds;
    }

    public void GainSaveCount()
    {
        CurrentStage.GainSaveCount();
    }

    public string ContinuePopupTitle()
    {
        var baseText = DataSystem.System.GetTextData(3061).Text;
        var subText = DataSystem.System.GetReplaceText(3062,DataSystem.System.LimitContinueCount.ToString());
        var continueCount = DataSystem.System.GetReplaceText(3063,(CurrentStage.ContinueCount+1).ToString());
        return baseText + continueCount + "\n" + subText;
    }

    public bool NeedAdsContinue()
    {
        var needAds = false;
#if UNITY_ANDROID
        needAds = (CurrentStage.ContinueCount + 1) >= DataSystem.System.LimitContinueCount;
#endif
        return needAds;
    }

    public void GainContinueCount()
    {
        CurrentStage.GainContinueCount();
    }

    public List<int> SaveAdsCommandTextIds()
    {
        return new List<int>(){3053,3051};
    }
}
