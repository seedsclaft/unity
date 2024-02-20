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
    public SaveStageInfo CurrentSaveData => GameSystem.CurrentStageData;
    public TempInfo TempInfo => GameSystem.TempData;
    public StageInfo CurrentStage => CurrentSaveData.CurrentStage;
    public AlcanaInfo StageAlcana => CurrentSaveData.Party.AlcanaInfo;

    public PartyInfo PartyInfo => CurrentSaveData.Party;

    public int Currency => PartyInfo.Currency;
    public int TotalScore => PartyInfo.TotalScore();

    public int RemainTurns => CurrentStage.Master.Turns - CurrentStage.CurrentTurn + 1;

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
        return PartyInfo.ActorInfos;
    }

    public void LostActors(List<ActorInfo> lostMembers)
    {
        lostMembers.ForEach(a => a.ChangeLost(false));
        return;
        lostMembers.ForEach(a => a.ChangeLost(true));
    }

    public List<ActorInfo> StageMembers()
    {
        return PartyInfo.CurrentActorInfos;
    }

    public List<ActorInfo> BattleMembers()
    {
        var members = StageMembers().FindAll(a => a.BattleIndex >= 0);
        members.Sort((a,b) => a.BattleIndex > b.BattleIndex ? 1 : -1);
        return members;
    }
    

    public List<ActorInfo> PartyMembers()
    {
        return PartyInfo.CurrentActorInfos;
    }

    public List<ActorInfo> ResultMembers()
    {
        var SelectActorIds = PartyInfo.ActorIdList;
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
        return TempInfo.TempStatusActorInfos;
    }

    public string TacticsBgmKey()
    {
        if (CurrentStage != null)
        {
            var bgmData = DataSystem.Data.GetBGM(CurrentStage.Master.BGMId);
            return bgmData.Key;
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
        Effekseer.Internal.EffekseerSoundPlayer.SeVolume = seVolume;
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

    public List<StageEventData> StageEventDates => CurrentStage.Master.StageEvents;

    public List<StageTutorialData> StageTutorialDates => CurrentStage.Master.Tutorials;

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
        yesCommand.Name = DataSystem.GetTextData(yesTextId).Text;
        yesCommand.Id = 0;
        if (noTextId != 0)
        {
            var noCommand = new SystemData.CommandData();
            noCommand.Key = "No";
            noCommand.Name = DataSystem.GetTextData(noTextId).Text;
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

    
    public List<SymbolInfo> TacticsSymbols()
    {
        return CurrentStage.CurrentSymbolInfos;
    }


    public TroopInfo CurrentTroopInfo()
    {
        return CurrentStage.CurrentTroopInfo();
    }


    public void SetIsAlcana(bool isAlcana)
    {
        StageAlcana.SetIsAlcana(isAlcana);
    }

    public void InitializeStageData(int stageId)
    {
        //CurrentStageData.MakeStageData(stageId);
    }

    public List<SkillInfo> CheckAlcanaSkillInfos(TriggerTiming triggerTiming)
    {
        var skillInfos = StageAlcana.CheckAlcanaSkillInfo(triggerTiming);
        return skillInfos.FindAll(a => a.Master.TriggerDates.Find(b => b.Param1 == RemainTurns) != null);
    }

    public bool NeedAlcana()
    {
        return false;
    }
    
    public void SetAlcanaSkillInfo(List<SkillInfo> skillInfos)
    {
        TempInfo.SetAlcanaSkillInfo(skillInfos);
    }

    public List<GetItemInfo> GetAlcanaResults(SkillInfo skillInfo)
    {
        var list = new List<GetItemInfo>();
        if (StageAlcana.IsAlcana)
        {
            foreach (var featureData in skillInfo.Master.FeatureDates)
            {
                var getItemInfo = new GetItemInfo(null);
                switch (featureData.FeatureType)
                {
                    case FeatureType.GainTurn: // param固定
                        CurrentSaveData.CurrentStage.DeSeekStage();
                        getItemInfo.MakeGainTurnResult((RemainTurns).ToString());
                        break;
                    case FeatureType.ActorLvUp: // featureData で param1 = 選択順のActorId、 param2 = 上昇値
                        var actorInfo = StageMembers().Find(a => a.ActorId == PartyInfo.ActorIdList[featureData.Param1]);
                        if (actorInfo != null)
                        {
                            var lv = featureData.Param2;
                            getItemInfo.MakeActorLvUpResult(actorInfo.Master.Name,actorInfo.Level+lv);
                            actorInfo.LevelUp(lv);
                        }
                        break;
                    case FeatureType.AlchemyCostZero: // featureData で param1 = 属性番号
                        var attributeText = DataSystem.GetTextData(330 + featureData.Param1 - 1);
                        getItemInfo.MakeAlchemyCostZeroResult(attributeText.Text);
                        break;
                    case FeatureType.NoBattleLost: // param固定
                        getItemInfo.MakeNoBattleLostResult();
                        break;
                    case FeatureType.ResourceBonus: // param固定
                        getItemInfo.MakeResourceBonusResult();
                        break;
                    case FeatureType.CommandCostZero: // featureData で param1 = tacticsCommand
                        var commandText = DataSystem.GetTextData(featureData.Param1);
                        getItemInfo.MakeCommandCostZeroResult(commandText.Text);
                        break;
                    case FeatureType.AlchemyCostBonus: // param固定
                        getItemInfo.MakeAlchemyCostBonusResult();
                        break;
                    case FeatureType.CommandLvUp: // featureData で param1 = tacticsCommand param2 = 上昇値
                        break;
                    case FeatureType.AddSkillOrCurrency: // skillInfo で param1 = 入手スキルID,featureData で param2 = 上昇Currency値
                        var getSkillId = skillInfo.Param1;
                        var hero = StageMembers().Find(a => a.ActorId == PartyInfo.ActorIdList[0]);
                        if (!hero.IsLearnedSkill(getSkillId))
                        {
                            getItemInfo.MakeSkillLearnResult(hero.Master.Name,DataSystem.FindSkill(skillInfo.Param1));    
                            hero.LearnSkill(getSkillId);
                        } else
                        {
                            PartyInfo.ChangeCurrency(Currency + featureData.Param2);
                            getItemInfo.MakeAddSkillCurrencyResult(skillInfo.Master.Name,featureData.Param2);                                
                        }
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
        return DataSystem.GetReplaceText(textId,actorName);
    }

    public void SetStageActor()
    {
        //　加入しているパーティを生成
        PartyInfo.ClearActorIds();
        foreach (var actorInfo in StageMembers())
        {
            PartyInfo.AddActorId(actorInfo.ActorId);
        }
    }

    public void SetStatusActorInfos()
    {
        TempInfo.SetTempStatusActorInfos(StageMembers());
    }

    public void SetSelectAddActor()
    {
        //　加入しているパーティ
        var stageMembers = StageMembers();
        //　加入していないパーティを生成
        var selectActorIds = Actors().FindAll(a => !StageMembers().Contains(a));
        
        PartyInfo.ClearActorIds();
        foreach (var actorInfo in selectActorIds)
        {
            if (actorInfo.Lost == false)
            {
                if (stageMembers.Find(a => a.Master.ClassId == actorInfo.Master.ClassId) == null)
                {
                    PartyInfo.AddActorId(actorInfo.ActorId);
                }
            }
        }
    }

    public void SetDefineBossIndex(int index)
    {
    }

    public string GetAdvFile(int id)
    {
        return DataSystem.Adventures.Find(a => a.Id == id).AdvName;
    }

    public void StageClear()
    {
        CurrentData.PlayerInfo.StageClear(CurrentStage.Id);
    }
    

    public void ChangeRouteSelectStage(int stageId)
    {
        // stageId + RouteSelect
        int route = GameSystem.CurrentStageData.CurrentStage.RouteSelect;
        CurrentSaveData.ChangeRouteSelectStage(stageId + route);
    }

    public List<SymbolInfo> CurrentTurnSymbolInfos(int turns)
    {
        var symbolInfos = new List<SymbolInfo>();
        var symbols = CurrentStage.Master.StageSymbols.FindAll(a => a.Seek == turns);
        foreach (var symbol in symbols)
        {
            var symbolInfo = new SymbolInfo(symbol);
            // ランダム
            if (symbol.SymbolType > SymbolType.Rebirth){
                var groupId = (int)symbol.SymbolType;
                var groupDates = DataSystem.SymbolGroups.FindAll(a => a.GroupId == groupId);
                var data = PickUpSymbolData(groupDates);
                data.StageId = symbol.StageId;
                data.Seek = symbol.Seek;
                data.SeekIndex = symbol.SeekIndex;
                symbolInfo = new SymbolInfo(data);
            }
            var getItemInfos = new List<GetItemInfo>();
            if (symbolInfo.SymbolType == SymbolType.Battle || symbolInfo.SymbolType == SymbolType.Boss){
                if (symbolInfo.StageSymbolData.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroop(symbolInfo.StageSymbolData.Param1,symbolInfo.StageSymbolData.Param2));
                }
                
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.TroopInfo.Master.PrizeSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    getItemInfos.Add(getItemInfo);
                }
            }
            if (symbolInfo.StageSymbolData.PrizeSetId > 0)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.StageSymbolData.PrizeSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    getItemInfos.Add(getItemInfo);
                }
            }
            symbolInfo.MakeGetItemInfos(getItemInfos);
            symbolInfos.Add(symbolInfo);
        }
        return symbolInfos;
    }

    public StageSymbolData PickUpSymbolData(List<SymbolGroupData> groupDates)
    {
        int targetRand = 0;
        for (int i = 0;i < groupDates.Count;i++)
        {
            targetRand += groupDates[i].Rate;
        }
        targetRand = UnityEngine.Random.Range (0,targetRand);
        int targetIndex = -1;
        for (int i = 0;i < groupDates.Count;i++)
        {
            targetRand -= groupDates[i].Rate;
            if (targetRand <= 0 && targetIndex == -1)
            {
                targetIndex = i;
            }
        }
        var StageSymbolData = new StageSymbolData();
        StageSymbolData.ConvertSymbolGroupData(groupDates[targetIndex]);
        return StageSymbolData;
    }

    public TroopInfo BattleTroop(int troopId,int enemyCount)
    {
        var troopInfo = new TroopInfo(troopId,false);
        troopInfo.MakeEnemyTroopDates(PartyInfo.ClearTroopCount);
        for (int i = 0;i < enemyCount;i++)
        {
            int rand = new System.Random().Next(1, CurrentStage.Master.RandomTroopCount);
            var enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
            var enemy = new BattlerInfo(enemyData,PartyInfo.ClearTroopCount + 1,i,0,false);
            troopInfo.AddEnemy(enemy);
        }
        troopInfo.MakeGetItemInfos();
        return troopInfo;
    }

    public void StartOpeningStage()
    {
        InitSaveStageInfo();
        CurrentSaveData.InitializeStageData(1);
        //CurrentStage.AddSelectActorId(1);
        PartyInfo.ChangeCurrency(DataSystem.System.InitCurrency);
        CurrentStage.SetSymbolInfos(CurrentTurnSymbolInfos(CurrentStage.CurrentTurn));
        MakeSymbolResultInfos();
        SavePlayerStageData(true);
    }

    public void MakeSymbolResultInfos()
    {
        // レコード作成
        foreach (var symbolInfo in CurrentStage.CurrentSymbolInfos)
        {
            var record = new SymbolResultInfo(symbolInfo,GameSystem.CurrentStageData.Party.Currency);
            var actorInfos = new List<ActorInfo>();
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                actorInfos.Add(actorInfo);
            }
            record.SetActorInfos(actorInfos);
            var actorIdList = new List<int>();
            foreach (var actorId in PartyInfo.ActorIdList)
            {
                actorIdList.Add(actorId);
            }
            record.SetActorIdList(actorIdList);
            var alchemyIdList = new List<int>();
            foreach (var alchemyId in PartyInfo.AlchemyIdList)
            {
                alchemyIdList.Add(alchemyId);
            }
            record.SetAlchemyIdList(alchemyIdList);
            CurrentStage.SetSymbolResultInfo(record);
        }
    }
    
    public void StartSelectStage(int stageId)
    {
        CurrentSaveData.MakeStageData(stageId);
        CurrentStage.SetSymbolInfos(CurrentTurnSymbolInfos(CurrentStage.CurrentTurn));
        MakeSymbolResultInfos();
        SavePlayerStageData(true);
    }

    public void StartSymbolRecordStage(int stageId)
    {
        CurrentSaveData.MakeStageData(stageId);
    }

    public void MakeSymbolRecordStage(int seek)
    {
        CurrentSaveData.MakeStageData(CurrentStage.Id);
        CurrentStage.SetRecordStage(true);
        TempInfo.SetRecordActors(PartyInfo.ActorInfos);
        TempInfo.SetRecordActorIdList(PartyInfo.ActorIdList);
        TempInfo.SetRecordAlchemyList(PartyInfo.AlchemyIdList);
        
        PartyInfo.InitActorInfos();
        foreach (var symbolActor in SymbolActorIdList(seek))
        {
            PartyInfo.AddActorId(symbolActor.ActorId);
        }
        foreach (var symbolActor in SymbolActorInfos(seek))
        {
            PartyInfo.UpdateActorInfo(symbolActor);
        }

        PartyInfo.ClearAlchemy();
        foreach (var alchemyId in SymbolAlchemyList(seek))
        {
            PartyInfo.AddAlchemy(alchemyId);
        }

        for (int i = 0;i < seek;i++)
        {
            CurrentStage.SeekStage();
        }
        var list = new List<SymbolInfo>();
        var symbolInfos = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == (seek+1));
        symbolInfos.Sort((a,b) => a.SeekIndex > b.SeekIndex ? 1 : -1);
        var symbolRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
        for (int i = 0;i < symbolInfos.Count;i++)
        {
            var symbolInfo = new SymbolInfo();
            symbolInfo.CopyData(symbolInfos[i].SymbolInfo);
            var saveRecord = symbolRecords.Find(a => a.IsSameSymbol(CurrentStage.Id,seek+1,i));
            symbolInfo.SetSelected(saveRecord != null);
            symbolInfo.SetCleared(symbolInfos[i].Cleared);
            MakePrizeData(saveRecord,symbolInfo.GetItemInfos);
            list.Add(symbolInfo);
        }
        CurrentStage.SetSymbolInfos(list);
        MakeSymbolResultInfos();
    }
    
    public List<ActorInfo> SymbolActorIdList(int seek)
    {
        var symbolRecord = PartyInfo.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek+1);
        return symbolRecord.ActorInfos.FindAll(a => symbolRecord.ActorIdList.Contains(a.ActorId));
    }

    public List<ActorInfo> SymbolActorInfos(int seek)
    {
        var symbolRecord = PartyInfo.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek+1);
        return symbolRecord.ActorInfos;
    }

    public List<int> SymbolAlchemyList(int seek)
    {
        var symbolRecord = PartyInfo.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek+1);
        return symbolRecord.AlchemyIdList;
    }
    

    public void MakePrizeData(SymbolResultInfo saveRecord,List<GetItemInfo> getItemInfos)
    {
        foreach (var getItemInfo in getItemInfos)
        {
            if (saveRecord != null && getItemInfo.GetItemType == GetItemType.SaveHuman)
            {
                getItemInfo.SetParam2(saveRecord.BattleScore);
                getItemInfo.MakeTextData();
            }
        }
    }
    
    
    public void SetParallelMode()
    {
        CurrentStage.SetParallelMode(true);
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
        CurrentSaveData.SetResumeStage(resumeStage);
    }
    
    public void SavePlayerData()
    {
        SaveSystem.SavePlayerInfo(GameSystem.CurrentData);
    }

    public void SavePlayerStageData(bool isResumeStage)
    {
        TempInfo.ClearRankingInfo();
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
        var members = new List<ActorInfo>();
        foreach (var actorInfo in PartyInfo.CurrentActorInfos)
        {
            members.Add(actorInfo);
        }
        return members;
    }

    public string RankingTypeText(RankingType rankingType)
    {
        switch (rankingType)
        {
            case RankingType.Evaluate:
            return DataSystem.GetTextData(16120).Text;
            case RankingType.Turns:
            return DataSystem.GetTextData(16121).Text;
        }
        return "";
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
        if (CurrentStage.Master.RankingStage == RankingType.Evaluate)
        {
            foreach (var actorInfo in EvaluateMembers())
            {
                evaluate += actorInfo.Evaluate();
            }
            if (CurrentStage.EndingType == EndingType.A)
            {
                evaluate += 1000;
            }
            if (CurrentStage.EndingType == EndingType.B)
            {
                evaluate += 500;
            }
        } else
        if (CurrentStage.Master.RankingStage == RankingType.Turns)
        {
            evaluate = CurrentStage.CurrentTurn - 1;
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
        FirebaseController.Instance.CurrentRankingData(CurrentStage.Id,userId);
        await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
        var currentScore = FirebaseController.CurrentScore;
        var evaluate = TotalEvaluate();

        if (evaluate > currentScore)
        {
#if UNITY_ANDROID
            FirebaseController.Instance.WriteRankingData(
                CurrentStage.Id,
                userId,
                evaluate,
                CurrentData.PlayerInfo.PlayerName,
                SelectIdxList(),
                SelectRankList(),
                RankingActorDates()
            );
#elif UNITY_WEBGL
            FirebaseController.Instance.WriteRankingData(
                CurrentStage.Id,
                userId,
                evaluate,
                CurrentData.PlayerInfo.PlayerName,
                SelectIdxList(),
                SelectRankList()
            );
#endif
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);

            FirebaseController.Instance.ReadRankingData(CurrentStage.Id,RankingTypeText(CurrentStage.Master.RankingStage));
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
                rankingText = rank.ToString() + DataSystem.GetTextData(16070).Text;
            } else
            {
                // 圏外
                rankingText = DataSystem.GetTextData(16071).Text;
            }
        } else
        {          
            // 記録更新なし  
            rankingText = DataSystem.GetTextData(16072).Text;
        }
#endif
        endEvent(rankingText);
    }

    public string SavePopupTitle()
    {
        var baseText = DataSystem.GetTextData(11080).Text;
        var subText = DataSystem.GetReplaceText(11081,CurrentStage.Master.SaveLimit.ToString());
        var savedCount = DataSystem.GetReplaceText(11083,(CurrentStage.SavedCount+1).ToString());
        return baseText + savedCount + "\n" + subText;
    }

    public string FailedSavePopupTitle()
    {
        var baseText = DataSystem.GetTextData(11082).Text;
        return baseText;
    }

    public bool NeedAdsSave()
    {
        var needAds = false;
#if UNITY_ANDROID
        needAds = (CurrentStage.SavedCount + 1) >= CurrentStage.Master.SaveLimit;
#endif
        return needAds;
    }

    public void GainSaveCount()
    {
        CurrentStage.GainSaveCount();
    }

    public bool EnableContinue()
    {
        return CurrentStage.Master.ContinueLimit > 0;
    }

    public bool EnableUserContinue()
    {
        var enable = true;
#if UNITY_WEBGL
        enable = CurrentStage.ContinueCount < CurrentStage.Master.ContinueLimit;
#endif
        return enable;
    }

    public string ContinuePopupTitle()
    {
        var baseText = DataSystem.GetTextData(3061).Text;
#if UNITY_ANDROID
        var subText = DataSystem.GetReplaceText(3062,CurrentStage.Master.ContinueLimit.ToString());
#elif UNITY_WEBGL
        var subText = DataSystem.GetReplaceText(3064,CurrentStage.Master.ContinueLimit.ToString());
#endif
        var continueCount = DataSystem.GetReplaceText(3063,(CurrentStage.ContinueCount+1).ToString());
        return baseText + continueCount + "\n" + subText;
    }


    public bool NeedAdsContinue()
    {
        var needAds = false;
#if UNITY_ANDROID
        needAds = (CurrentStage.ContinueCount + 1) >= CurrentStage.Master.ContinueLimit;
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

    public void ClearActorsData()
    {
        PartyInfo.InitActorInfos();
        CurrentSaveData.InitAllActorMembers();
    }

    public void SetActorsData(int index)
    {
        PartyInfo.InitActorInfos();
        PartyInfo.ClearActorIds();
        var slotData = CurrentData.PlayerInfo.SlotSaveList[index];
        var actorInfos = slotData.ActorInfos;
        foreach (var actorInfo in actorInfos)
        {
            PartyInfo.AddActorId(actorInfo.ActorId);
        }
    }

    public int ParallelCost()
    {
        return PartyInfo.ParallelCost();
    }

    public void GainParallelCount()
    {
        PartyInfo.GainParallelCount();
    }

    public bool SelectableSlot(int index)
    {
        return CurrentData.PlayerInfo.SlotSaveList[index].ActorInfos.Count > 0;
    }
}
