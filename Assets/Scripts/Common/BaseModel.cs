using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Ryneus
{
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
            GameSystem.CurrentData = new SaveInfo();
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
            return PartyInfo.CurrentActorInfos(CurrentStage.Id,CurrentStage.CurrentTurn);
        }

        public List<ActorInfo> BattleMembers()
        {
            var members = StageMembers().FindAll(a => a.BattleIndex >= 0);
            members.Sort((a,b) => a.BattleIndex > b.BattleIndex ? 1 : -1);
            return members;
        }
        

        public List<ActorInfo> PartyMembers()
        {
            return PartyInfo.CurrentActorInfos(CurrentStage.Id,CurrentStage.CurrentTurn);
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
            return PartyInfo.CurrentSymbolInfos(CurrentStage.CurrentTurn);
        }

        public List<SkillInfo> AlcanaSkillInfos()
        {
            var skillInfos = new List<SkillInfo>();
            foreach (var alchemyId in PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.CurrentTurn))
            {
                var skillInfo = new SkillInfo(alchemyId);
                skillInfo.SetEnable(true);
                skillInfos.Add(skillInfo);
            }
            return skillInfos;
        }

        public bool NeedAlcana()
        {
            return false;
        }
        
        public void SetAlcanaSkillInfo(List<SkillInfo> skillInfos)
        {
            TempInfo.SetAlcanaSkillInfo(skillInfos);
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

        public void SetStatusActorInfos()
        {
            TempInfo.SetTempStatusActorInfos(StageMembers());
        }

        public string GetAdvFile(int id)
        {
            return DataSystem.Adventures.Find(a => a.Id == id).AdvName;
        }

        public void StageClear()
        {
            CurrentData.PlayerInfo.StageClear(CurrentStage.Id);
        }

        public List<SymbolInfo> OpeningStageSymbolInfos()
        {
            var symbolInfos = new List<SymbolInfo>();
            var symbols = DataSystem.FindStage(0).StageSymbols;
            symbols = symbols.FindAll(a => a.Seek == 0);
            foreach (var symbol in symbols)
            {
                var symbolInfo = new SymbolInfo(symbol);
                var getItemInfos = new List<GetItemInfo>();
                if (symbolInfo.StageSymbolData.PrizeSetId > 0)
                {
                    var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.StageSymbolData.PrizeSetId);
                    foreach (var prizeSet in prizeSets)
                    {
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        getItemInfos.Add(getItemInfo);
                    }
                }
                symbolInfo.SetGetItemInfos(getItemInfos);
                symbolInfo.SetSelected(true);
                symbolInfos.Add(symbolInfo);
            }
            return symbolInfos;
        }

        public List<SymbolInfo> StageSymbolInfos()
        {
            return SymbolUtility.StageSymbolInfos(CurrentStage.Master.StageSymbols);
        }

        public List<SymbolInfo> ClearedSymbolInfos(int stageId)
        {
            var list = new List<SymbolInfo>();
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.SymbolInfo.StageSymbolData.StageId == stageId);
            foreach (var record in records)
            {
                list.Add(record.SymbolInfo);
            }
            return list;
        }

        public void StartOpeningStage()
        {
            InitSaveStageInfo();
            CurrentSaveData.InitializeStageData(1);
            PartyInfo.SetStageSymbolInfos(OpeningStageSymbolInfos());
            foreach (var symbolInfo in PartyInfo.StageSymbolInfos)
            {
                var record = new SymbolResultInfo(symbolInfo,GameSystem.CurrentStageData.Party.Currency);
                var actorInfos = new List<ActorInfo>();
                foreach (var actorInfo in PartyInfo.ActorInfos)
                {
                    actorInfos.Add(actorInfo);
                }
                record.SetActorInfos(actorInfos);
                record.SetSelected(true);
                PartyInfo.SetSymbolResultInfo(record);
            }
            PartyInfo.ChangeCurrency(DataSystem.System.InitCurrency);
            PartyInfo.SetStageSymbolInfos(StageSymbolInfos());
            MakeSymbolResultInfos();
            SavePlayerStageData(true);
        }
        
        public void StartSelectStage(int stageId)
        {
            CurrentSaveData.MakeStageData(stageId);
            //CurrentStage.SetStageSymbolInfos(StageSymbolInfos());
            PartyInfo.SetStageSymbolInfos(StageSymbolInfos());
            MakeSymbolResultInfos();
            SavePlayerStageData(true);
        }

        public void StartClearedStage(int stageId)
        {
            CurrentSaveData.MakeStageData(stageId);
            PartyInfo.SetStageSymbolInfos(ClearedSymbolInfos(stageId));
            SavePlayerStageData(true);
        }

        public SymbolInfo CurrentSelectSymbol()
        {
            return PartyInfo.CurrentSymbolInfos(CurrentStage.CurrentTurn)[CurrentStage.CurrentSeekIndex];
        }

        public SymbolInfo SelectStageInfo(int seek)
        {
            return PartyInfo.CurrentSymbolInfos(seek)[CurrentStage.CurrentSeekIndex];
        }

        public TroopInfo CurrentTroopInfo()
        {
            return CurrentSelectSymbol().TroopInfo;
        }

        public void MakeSymbolResultInfos()
        {
            // ステージ全てのレコードを作成
            var currentStageSymbolInfos = PartyInfo.StageSymbolInfos.FindAll(a => a.StageSymbolData.StageId == CurrentStage.Id);
            foreach (var symbolInfo in currentStageSymbolInfos)
            {
                var record = new SymbolResultInfo(symbolInfo,GameSystem.CurrentStageData.Party.Currency);
                var actorInfos = new List<ActorInfo>();
                foreach (var actorInfo in PartyInfo.ActorInfos)
                {
                    actorInfos.Add(actorInfo);
                }
                record.SetActorInfos(actorInfos);
                PartyInfo.SetSymbolResultInfo(record);
            }
        }

        public void MakeSymbolRecordStage(int seek)
        {
            //CurrentSaveData.MakeStageData(CurrentStage.Id);
            TempInfo.SetRecordActors(PartyInfo.ActorInfos);
            
            PartyInfo.InitActorInfos();
            foreach (var symbolActor in SymbolActorInfos(seek))
            {
                PartyInfo.UpdateActorInfo(symbolActor);
            }
            CurrentStage.SetReturnSeek(CurrentStage.CurrentTurn);
            CurrentStage.SetCurrentTurn(seek);
        }

        public List<ActorInfo> SymbolActorInfos(int seek)
        {
            var symbolRecord = PartyInfo.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek);
            return symbolRecord.ActorInfos;
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
            foreach (var actorInfo in StageMembers())
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
                foreach (var actorInfo in StageMembers())
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
            foreach (var actorInfo in StageMembers())
            {
                selectIdx.Add(actorInfo.ActorId);
            }
            return selectIdx;
        }

        public List<int> SelectRankList()
        {
            var selectIdRank = new List<int>();
            foreach (var actorInfo in StageMembers())
            {
                selectIdRank.Add(actorInfo.Evaluate());
            }
            return selectIdRank;
        }

        public async void CurrentRankingData(Action<string> endEvent)
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
    #else
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
            var slotData = CurrentData.PlayerInfo.SlotSaveList[index];
            var actorInfos = slotData.ActorInfos;
            foreach (var actorInfo in actorInfos)
            {
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
        public int PartyEvaluate()
        {
            var evaluate = 0;
            foreach (var actorInfo in BattleMembers())
            {
                evaluate += actorInfo.Evaluate();
            }
            return evaluate;
        }

        public int TroopEvaluate()
        {
            if (CurrentStage.CurrentSeekIndex >= 0)
            {
                var symbol = CurrentSelectSymbol();
                if (symbol != null && symbol.SymbolType == SymbolType.Battle || symbol.SymbolType == SymbolType.Boss)
                {
                    return symbol.BattleEvaluate();
                }
            }
            return 0;
        }
    }
}