using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Ryneus
{
    public partial class BaseModel
    {
        public SaveInfo CurrentData => GameSystem.CurrentData;
        public SaveStageInfo CurrentSaveData => GameSystem.CurrentStageData;
        public TempInfo TempInfo => GameSystem.TempData;
        public StageInfo CurrentStage => CurrentSaveData.CurrentStage;

        public PartyInfo PartyInfo => CurrentSaveData.Party;

        public int Currency => PartyInfo.GetCurrency(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
        public float TotalScore => PartyInfo.TotalScore(CurrentStage.WorldType);

        public int RemainTurns => CurrentStage.Master.StageSymbols.Max(a => a.Seek) - CurrentStage.Seek + 1;

        public bool BrunchMode => PartyInfo.ReturnSymbol != null;
        public bool NeedEndBrunch => PartyInfo.NeedEndBrunch();
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
        }

        public List<ActorInfo> StageMembers()
        {
            var actorInfos = PartyInfo.CurrentActorInfos(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            actorInfos.Sort((a,b)=> a.LinkedLevel() < b.LinkedLevel() ? 1 : -1);
            return actorInfos;
        }

        public List<ActorInfo> BattleMembers()
        {
            var members = StageMembers().FindAll(a => a.BattleIndex >= 0);
            members.Sort((a,b) => a.BattleIndex > b.BattleIndex ? 1 : -1);
            return members;
        }
        
        public List<ActorInfo> PartyMembers()
        {
            return PartyInfo.CurrentActorInfos(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
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
            int CurrentTurn = CurrentStage.Seek;
            var eventKeys = CurrentStage.ReadEventKeys;
            return StageEventDates.FindAll(a => a.Timing == eventTiming && a.Turns == CurrentTurn && !eventKeys.Contains(a.EventKey));
        }
        
        public bool SetStageTutorials(EventTiming eventTiming)
        {
            int CurrentTurn = CurrentStage.Seek;
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

        public async UniTask<List<AudioClip>> GetBgmData(string bgmKey)
        {
            return await ResourceSystem.LoadBGMAsset(bgmKey);
        }

        public List<SystemData.CommandData> BaseConfirmCommand(int yesTextId,int noTextId = 0)
        {
            var menuCommandDates = new List<SystemData.CommandData>();
            var yesCommand = new SystemData.CommandData
            {
                Key = "Yes",
                Name = DataSystem.GetText(yesTextId),
                Id = 0
            };
            if (noTextId != 0)
            {
                var noCommand = new SystemData.CommandData
                {
                    Key = "No",
                    Name = DataSystem.GetText(noTextId),
                    Id = 1
                };
                menuCommandDates.Add(noCommand);
            }
            menuCommandDates.Add(yesCommand);
            return menuCommandDates;
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
                var skillDates = DataSystem.Skills.Where(a => a.Value.Rank == (RankType)getItemInfo.ResultParam && a.Value.Attribute == (AttributeType)((int)getItemInfo.GetItemType - 10));
                foreach (var skillData in skillDates)
                {
                    var skillInfo = new SkillInfo(skillData.Key);
                    skillInfo.SetEnable(true);
                    skillInfos.Add(skillInfo);
                }
            }
            return skillInfos;
        }

        public List<SkillInfo> BasicSkillGetItemInfos(List<GetItemInfo> getItemInfos)
        {
            var skillInfos = new List<SkillInfo>();
            foreach (var getItemInfo in getItemInfos)
            {
                if (getItemInfo.IsSkill())
                {
                    var skillInfo = new SkillInfo(getItemInfo.Param1);
                    skillInfo.SetEnable(true);
                    skillInfos.Add(skillInfo);
                }
                if (getItemInfo.IsAttributeSkill())
                {
                    var skillDates = DataSystem.Skills.Where(a => a.Value.Rank == (RankType)getItemInfo.ResultParam && a.Value.Attribute == (AttributeType)((int)getItemInfo.GetItemType - 10));
                    foreach (var skillData in skillDates)
                    {
                        var skillInfo = new SkillInfo(skillData.Key);
                        skillInfo.SetEnable(true);
                        skillInfos.Add(skillInfo);
                    }
                }
            }
            return skillInfos;
        }
        
        public List<SymbolResultInfo> TacticsSymbols()
        {
            return PartyInfo.CurrentRecordInfos(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
        }

        public List<SkillInfo> AlcanaSkillInfos()
        {
            var skillInfos = new List<SkillInfo>();
            foreach (var alchemyId in PartyInfo.CurrentAlcanaIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType))
            {
                var skillInfo = new SkillInfo(alchemyId);
                skillInfo.SetEnable(true);
                skillInfos.Add(skillInfo);
            }
            return skillInfos;
        }

        public string SelectAddActorConfirmText(string actorName)
        {
            int textId = 14180;
            return DataSystem.GetReplaceText(textId,actorName);
        }

        /// <summary>
        /// 加入歴あるキャラも含めたステータスメンバー
        /// </summary>
        public List<ActorInfo> PastActorInfos()
        {
            var stageMembers = StageMembers();
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                if (!stageMembers.Contains(actorInfo))
                {
                    var levelUpInfos = actorInfo.LevelUpInfos;
                    if (levelUpInfos.FindAll(a => (int)a.WorldType != (int)CurrentStage.WorldType).Count > 0)
                    {
                        stageMembers.Add(actorInfo);
                    }
                }
            }
            stageMembers.Sort((a,b) => a.Level - b.Level > 0 ? -1 : 1);
            return stageMembers;
        }

        public string GetAdvFile(int id)
        {
            return DataSystem.Adventures.Find(a => a.Id == id).AdvName;
        }

        public void ClearGame()
        {
            CurrentData.PlayerInfo.GainClearCount();
        }

        public List<SymbolResultInfo> OpeningStageResultInfos()
        {
            var recordInfos = new List<SymbolResultInfo>();
            // 初期加入マス
            var symbols = DataSystem.FindStage(0).StageSymbols;
            symbols = symbols.FindAll(a => a.Seek == 0 && a.ClearCount <= CurrentData.PlayerInfo.ClearCount);
            bool addActor = false;
            foreach (var symbol in symbols)
            {
                var symbolInfo = new SymbolInfo(symbol);
                var getItemInfos = new List<GetItemInfo>();
                if (symbol.PrizeSetId > 0)
                {
                    var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbol.PrizeSetId);
                    foreach (var prizeSet in prizeSets)
                    {
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        getItemInfos.Add(getItemInfo);
                    }
                }
                symbolInfo.SetGetItemInfos(getItemInfos);
                var record = new SymbolResultInfo(symbolInfo);
                if (addActor == false)
                {
                    record.SetSelected(true);
                    var addActorGetItemInfo = getItemInfos.Find(a => a.GetItemType == GetItemType.SelectAddActor);
                    if (addActorGetItemInfo != null)
                    {
                        // 初期アクター
                        addActorGetItemInfo.SetResultParam(1);
                        addActorGetItemInfo.SetGetFlag(true);
                        AddPlayerInfoActorSkillId(1);
                        //addActor = true;
                    }
                }
                recordInfos.Add(record);
            }
            // アナザー世界のレコードを作る
            foreach (var symbol in symbols)
            {
                var symbolInfo = new SymbolInfo(symbol);
                var getItemInfos = new List<GetItemInfo>();
                if (symbol.PrizeSetId > 0)
                {
                    var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbol.PrizeSetId);
                    foreach (var prizeSet in prizeSets)
                    {
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        getItemInfos.Add(getItemInfo);
                    }
                }
                symbolInfo.SetGetItemInfos(getItemInfos);
                var record = new SymbolResultInfo(symbolInfo);
                record.SetWorldType(WorldType.Brunch);
                if (addActor == false)
                {
                    record.SetSelected(true);
                    var addActorGetItemInfo = getItemInfos.Find(a => a.GetItemType == GetItemType.SelectAddActor);
                    if (addActorGetItemInfo != null)
                    {
                        // 初期アクター
                        addActorGetItemInfo.SetResultParam(1);
                        addActorGetItemInfo.SetGetFlag(true);
                        AddPlayerInfoActorSkillId(1);
                        //addActor = true;
                    }
                }
                recordInfos.Add(record);
            }
            return recordInfos;
        }

        public List<SymbolResultInfo> StageResultInfos(int stageId)
        {
            var stageData = DataSystem.FindStage(stageId);
            return StageResultInfos(stageData.StageSymbols);
        }

        public void StartOpeningStage()
        {
            InitSaveStageInfo();
            CurrentSaveData.InitializeStageData(1);
            foreach (var record in OpeningStageResultInfos())
            {
                PartyInfo.SetSymbolResultInfo(record);
            }
            PartyInfo.ChangeCurrency(DataSystem.System.InitCurrency);
            foreach (var record in StageResultInfos(CurrentStage.Id))
            {
                PartyInfo.SetSymbolResultInfo(record);
            }
            SavePlayerStageData(true);
        }

        public SymbolResultInfo CurrentSelectRecord()
        {
            var symbolInfos = PartyInfo.CurrentRecordInfos(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            return symbolInfos.Find(a => a.SeekIndex == CurrentStage.CurrentSeekIndex);
        }

        public TroopInfo CurrentTroopInfo()
        {
            return CurrentSelectRecord().SymbolInfo.TroopInfo;
        }

        /// <summary>
        /// ステージ進捗度を設定
        /// </summary>
        /// <param name="returnSymbol"></param>
        public void SetReturnRecordStage(SymbolResultInfo returnSymbol)
        {
            PartyInfo.SetReturnStageIdSeek(CurrentStage.Id,CurrentStage.Seek);
            CurrentStage.SetStageId(returnSymbol.StageId);
            CurrentStage.SetCurrentTurn(returnSymbol.Seek);
            SetStageSeek();
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
            //CurrentSaveData.SetResumeStage(resumeStage);
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
            SavePlayerData();
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

        public async void CurrentRankingData(Action<string> endEvent)
        {
            var userId = CurrentData.PlayerInfo.UserId.ToString();
            var rankingText = "";
    #if (UNITY_WEBGL || UNITY_ANDROID)// && !UNITY_EDITOR
            FirebaseController.Instance.CurrentRankingData(userId);
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
            var currentScore = FirebaseController.CurrentScore;
            var evaluate = TotalScore;

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
                    (int)(evaluate * 100),
                    CurrentData.PlayerInfo.PlayerName,
                    null,
                    null
                );
    #endif
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
                    rankingText = rank.ToString() + DataSystem.GetText(23030);
                } else
                {
                    // 圏外
                    rankingText = DataSystem.GetText(23031);
                }
            } else
            {          
                // 記録更新なし  
                rankingText = DataSystem.GetText(23032);
            }
    #endif
            endEvent(rankingText);
        }

        public string SavePopupTitle()
        {
            return DataSystem.GetText(19500);
        }

        public string FailedSavePopupTitle()
        {
            var baseText = DataSystem.GetText(11082);
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
            return enable;
        }

        public string ContinuePopupTitle()
        {
            var baseText = DataSystem.GetText(3061);
    #if UNITY_ANDROID
            var subText = DataSystem.GetReplaceText(3062,CurrentStage.Master.ContinueLimit.ToString());
    #else
            var subText = DataSystem.GetReplaceText(3064,CurrentStage.Master.ContinueLimit.ToString());
    #endif
            return baseText + "\n" + subText;
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
        }

        public List<int> SaveAdsCommandTextIds()
        {
            return new List<int>(){3053,3051};
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
                var record = CurrentSelectRecord();
                if (record != null && record.SymbolType == SymbolType.Battle || record.SymbolType == SymbolType.Boss)
                {
                    return record.SymbolInfo.BattleEvaluate();
                }
            }
            return 0;
        }

        public string CurrentStageKey()
        {
            var stageKey = new System.Text.StringBuilder();
            if (CurrentStage != null)
            {
                stageKey.Append(string.Format(CurrentStage.Id.ToString("00")));
                stageKey.Append(string.Format(CurrentStage.Seek.ToString("00")));
                stageKey.Append(string.Format(CurrentStage.CurrentSeekIndex.ToString("00")));
            }
            return stageKey.ToString();
        }

        public void SetStageSeek()
        {
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                actorInfo.SetStageSeek(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
        }

        public void ActorLevelUp(ActorInfo actorInfo)
        {
            var cost = ActorLevelUpCost(actorInfo);
            // 新規魔法取得があるか
            var skills = actorInfo.LearningSkills(1);
            var levelUpInfo = actorInfo.LevelUp(cost,CurrentStage.Id,CurrentStage.Seek,-1,CurrentStage.WorldType);
            PartyInfo.ChangeCurrency(Currency - cost);
            PartyInfo.SetLevelUpInfo(levelUpInfo);
            foreach (var skill in skills)
            {
                actorInfo.AddSkillTriggerSkill(skill.Id);
            }
        }

        public int ActorLevelUpCost(ActorInfo actorInfo)
        {
            return TacticsUtility.TrainCost(actorInfo);
        }
        
        public bool EnableActorLevelUp(ActorInfo actorInfo)
        {
            return actorInfo.LevelLinked == false && Currency >= ActorLevelUpCost(actorInfo);
        }

        public bool ActorLevelLinked(ActorInfo actorInfo)
        {
            return actorInfo.LevelLinked;
        }

        public void ActorLearnMagic(ActorInfo actorInfo,int skillId)
        {
            var skillInfo = new SkillInfo(skillId);
            var learningCost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers(),skillInfo.Master.Rank);
            PartyInfo.ChangeCurrency(Currency - learningCost);
            var levelUpInfo = actorInfo.LearnSkill(skillInfo.Id,learningCost,CurrentStage.Id,CurrentStage.Seek,-1,CurrentStage.WorldType);
            PartyInfo.SetLevelUpInfo(levelUpInfo);
            // 作戦項目に追加
            actorInfo.AddSkillTriggerSkill(skillId);
        }

        /// <summary>
        /// ブランチを作る
        /// </summary>
        /// <param name="symbolResultInfo"></param>
        public void MakeBrunch(SymbolResultInfo symbolResultInfo)
        {
            // 今のターン中の成長データを削除
            PartyInfo.ResetCurrentLevelUpInfo(CurrentStage.Id,CurrentStage.Seek,WorldType.Main);
            PartyInfo.SetBrunchStageIdSeek(symbolResultInfo.StageId,symbolResultInfo.Seek,true);
            
            // 以前のデータを作成
            var resultInfos = PartyInfo.SymbolRecordList.FindAll(a => a.WorldType == WorldType.Main);
            // 既存レコード複製
            foreach (var resultInfo in resultInfos)
            {
                var copy = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(resultInfo,WorldType.Brunch));
                // ブランチより前は値をコピー
                if (resultInfo.StageId == symbolResultInfo.StageId && resultInfo.Seek < symbolResultInfo.Seek
                || resultInfo.StageId < symbolResultInfo.StageId)
                {
                    copy.CopyParamData(resultInfo);
                }
            }
            // ブランチの成長データを作成
            var actorInfos = PartyInfo.CurrentActorInfos(symbolResultInfo.StageId,symbolResultInfo.Seek,WorldType.Main);
            foreach (var actorInfo in actorInfos)
            {
                actorInfo.RemoveLevelUpInfos(WorldType.Brunch);
            }
            foreach (var actorInfo in actorInfos)
            {
                var addLevelUpInfos = new List<LevelUpInfo>();
                foreach (var levelUpInfo in actorInfo.LevelUpInfos)
                {
                    if (levelUpInfo.StageId == symbolResultInfo.StageId && levelUpInfo.Seek < symbolResultInfo.Seek || levelUpInfo.StageId < symbolResultInfo.StageId)
                    {
                        addLevelUpInfos.Add(levelUpInfo.CopyBrunchData());
                    }
                }
                foreach (var addLevelUpInfo in addLevelUpInfos)
                {
                    actorInfo.SetLevelUpInfo(addLevelUpInfo);   
                }
            }
                        
            SetReturnRecordStage(symbolResultInfo);
        }

        /// <summary>
        /// マージする
        /// </summary>
        public void MergeBrunch()
        {            
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.WorldType == WorldType.Brunch);
            
            var brunchBaseSymbol = PartyInfo.BrunchBaseSymbol;
            var brunchSymbol = PartyInfo.BrunchSymbol;
            var returnSymbol = PartyInfo.ReturnSymbol;
            // マージする範囲を更新
            selectRecords = selectRecords.FindAll(a => a.IsBeforeStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Brunch) && a.IsAfterStageSeek(brunchBaseSymbol.StageId,brunchBaseSymbol.Seek,WorldType.Brunch));
            // Seek毎にマージする
            var keyList = new List<int>();
            foreach (var resultInfo in selectRecords)
            {
                var key = resultInfo.StageId*10000 + resultInfo.Seek*1000;
                if (!keyList.Contains(key))
                {
                    keyList.Add(key);
                    PartyInfo.ResetActorLevelUpInfos(resultInfo);
                }
                PartyInfo.MergeBrunch(resultInfo);
            }
            // マージしない範囲を更新
            // ブランチのステージを初期化
            PartyInfo.ResetBrunchData();
            CurrentStage.SetStageId(PartyInfo.ReturnSymbol.StageId);
            CurrentStage.SetCurrentTurn(PartyInfo.ReturnSymbol.Seek);
            CurrentStage.SetSeekIndex(0);
            CurrentStage.SetWorldType(WorldType.Main);
            PartyInfo.ClearBrunch();
            SetStageSeek();
        }

        /// <summary>
        /// リバースする
        /// </summary>
        public void ReverseBrunch()
        {            
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.WorldType == WorldType.Brunch);
            
            var brunchSymbol = PartyInfo.BrunchBaseSymbol;
            var returnSymbol = PartyInfo.BrunchSymbol;
            selectRecords = selectRecords.FindAll(a => a.IsBeforeStageSeek(returnSymbol.StageId,returnSymbol.Seek,WorldType.Brunch) && a.IsAfterStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Brunch));
            
            foreach (var resultInfo in selectRecords)
            {
                PartyInfo.ReverseBrunch(resultInfo);
            }
            // ブランチのステージを初期化
            PartyInfo.ResetBrunchData();
            CurrentStage.SetStageId(PartyInfo.ReturnSymbol.StageId);
            CurrentStage.SetCurrentTurn(PartyInfo.ReturnSymbol.Seek);
            CurrentStage.SetSeekIndex(0);
            CurrentStage.SetWorldType(WorldType.Main);
            PartyInfo.ClearBrunch();
            SetStageSeek();
        }

        public void AddPlayerInfoActorSkillId(int actorId)
        {
            foreach (var skillInfo in Actors().Find(a => a.ActorId == actorId).SkillInfos())
            {
                AddPlayerInfoSkillId(skillInfo.Id);
            }
        }

        public void AddPlayerInfoSkillId(int skillId)
        {
            CurrentData.PlayerInfo.AddSkillId(skillId);
        }
    }
}