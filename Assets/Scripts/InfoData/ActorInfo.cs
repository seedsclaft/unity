using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ActorInfo
    {
        public ActorData Master => DataSystem.FindActor(_actorId);
        private int _actorId = -1;
        public int ActorId => _actorId;
        public int MaxHp => CurrentStatus.Hp;
        public int MaxMp => CurrentStatus.Mp;
        private List<LevelUpInfo> _levelUpInfos = new ();
        public List<LevelUpInfo> LevelUpInfos => _levelUpInfos;
        private int _stageId = 1;
        private int _seek = 1;
        private WorldType _worldType = WorldType.Main;
        public void SetLevelUpInfo(LevelUpInfo levelUpInfo)
        {
            var findIndex = _levelUpInfos.FindIndex(a => a.IsSameLevelUpInfo(levelUpInfo));
            if (findIndex > -1)
            {
                _levelUpInfos.RemoveAt(findIndex);
            }
            _levelUpInfos.Add(levelUpInfo);
        }

        public int ActorLevelReset()
        {
            var currency = 0;
            // リセットされる数
            var levelUpDates = _levelUpInfos.FindAll(a => a.IsTrainData() && a.ActorId == _actorId);
            var resetLv = levelUpDates.Count;
            for (int i = levelUpDates.Count-1;i >= 0;i--)
            {
                currency += levelUpDates[i].Currency;
                _levelUpInfos.Remove(levelUpDates[i]);
            }
            // 習得した魔法をリセット
            var learnedSkill = LearnSkillIds();
            var learnedSkillDates = _levelUpInfos.FindAll(a => a.IsLearnSkillData() && a.ActorId == _actorId);
            for (int i = learnedSkillDates.Count-1;i >= 0;i--)
            {
                currency += learnedSkillDates[i].Currency;
                _levelUpInfos.Remove(learnedSkillDates[i]);
            }
            return currency;
        }

        public void RemoveLevelUpInfos(WorldType worldType)
        {
            for (int i = _levelUpInfos.Count-1;i >= 0;i--)
            {
                var levelUpInfo = _levelUpInfos[i];
                if ( levelUpInfo.WorldType == worldType)
                {
                    _levelUpInfos.Remove(levelUpInfo);
                }
            }
        }

        public void RemoveParamData(int stageId,int seek,WorldType worldType)
        {
            for (int i = _levelUpInfos.Count-1;i >= 0;i--)
            {
                var levelUpInfo = _levelUpInfos[i];
                if (levelUpInfo.StageId == stageId && levelUpInfo.Seek == seek && levelUpInfo.WorldType == worldType)
                {
                    _levelUpInfos.Remove(levelUpInfo);
                }
            }
        }
        
        public void MargeLevelUpInfo(int stageId,int seek,WorldType worldType)
        {
            for (int i = _levelUpInfos.Count-1;i >= 0;i--)
            {
                var levelUpInfo = _levelUpInfos[i];
                if (levelUpInfo.StageId == stageId && levelUpInfo.Seek == seek && levelUpInfo.WorldType == worldType)
                {
                    levelUpInfo.SetWorldType(WorldType.Main);
                }
            }
        }

        private string _addTiming = "";
        public string AddTiming => _addTiming;
        public void SetAddTiming(string addTiming)
        {
            _addTiming = addTiming;
        }

        public int Level => _levelUpInfos.FindAll(a => a.IsLevelUpData() && a.IsEnableStage(_stageId,_seek,_worldType)).Count + 1;
        private int _levelLink = -1;

        public void SetLevelLink(int levelLink)
        {
            _levelLink = levelLink;
        }

        public int LinkedLevel()
        {
            if (_levelLink >= Level)
            {
                return _levelLink;
            }
            return Level;
        }
        private bool _levelLinked = false;
        public bool LevelLinked => _levelLinked;
        public void SetLevelLinked(bool levelLinked) => _levelLinked = levelLinked;
        public void SetStageSeek(int stageId,int seek,WorldType worldType)
        {
            _stageId = stageId;
            _seek = seek;
            _worldType = worldType;
        }
        public StatusInfo CurrentStatus => LevelUpStatus(LinkedLevel());
        private List<SkillTriggerInfo> _skillTriggerInfos = new ();
        public List<SkillTriggerInfo> SkillTriggerInfos => _skillTriggerInfos;

        public void InitSkillTriggerInfos()
        {
            var skillTriggerDates = Master.SkillTriggerDates;
            for (int i = 0;i < skillTriggerDates.Count;i++)
            {
                var skillTriggerData = skillTriggerDates[i];
                var skillTriggerInfo = new SkillTriggerInfo(_actorId,new SkillInfo(skillTriggerData.SkillId));
                skillTriggerInfo.SetPriority(i);
                var skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                var skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>(){skillTriggerData1,skillTriggerData2});
                _skillTriggerInfos.Add(skillTriggerInfo);
            }
        }

        public void AddSkillTriggerSkill(int skillId)
        {
            for (int i = 0;i < _skillTriggerInfos.Count;i++)
            {
                var skillTriggerInfo = _skillTriggerInfos[i];
                if (skillTriggerInfo.SkillId == 0)
                {
                    var skillInfo = new SkillInfo(skillId);
                    // アクティブか覚醒なら自動で加える
                    if (skillInfo.IsBattleActiveSkill())
                    {
                        skillTriggerInfo.SetSkillInfo(new SkillInfo(skillId));
                        break;
                    }
                }
            }
        }
    
        public void SetSkillTriggerSkill(int index,SkillInfo skillInfo)
        {
            if (_skillTriggerInfos.Count > index)
            {
                _skillTriggerInfos[index].SetSkillInfo(skillInfo);
            }
        }
        
        public void SetSkillTriggerTrigger(int index,int triggerIndex,SkillTriggerData triggerType)
        {
            if (_skillTriggerInfos.Count > index)
            {
                var triggerTypes = _skillTriggerInfos[index].SkillTriggerDates;
                SkillTriggerData triggerData1 = null;
                SkillTriggerData triggerData2 = null;
                if (triggerIndex == 1)
                {
                    if (triggerType == null && triggerTypes[1] != null)
                    {
                        triggerData1 = triggerTypes[1];
                        triggerData2 = triggerType;
                    } else
                    {
                        triggerData1 = triggerType;
                        triggerData2 = triggerTypes[1];
                    }
                } else
                if (triggerIndex == 2)
                {
                    triggerData1 = triggerTypes[0];
                    triggerData2 = triggerType;
                }
                var list = new List<SkillTriggerData>
                {
                    triggerData1,
                    triggerData2
                };
                _skillTriggerInfos[index].UpdateTriggerDates(list);
            }
        }

        public void SetTriggerIndexUp(int index)
        {
            if (index > 0)
            {
                var upTriggerData = _skillTriggerInfos[index];
                var downTriggerData = _skillTriggerInfos[index - 1];
                upTriggerData.SetPriority(index-1);
                downTriggerData.SetPriority(index);
            }
            _skillTriggerInfos.Sort((a,b) => a.Priority - b.Priority > 0 ? 1 : -1);
        }

        public void SetTriggerIndexDown(int index)
        {
            if (index+1 >= _skillTriggerInfos.Count)
            {
                return;
            }
            var upTriggerData = _skillTriggerInfos[index+1];
            var downTriggerData = _skillTriggerInfos[index];
            upTriggerData.SetPriority(index);
            downTriggerData.SetPriority(index+1);
            _skillTriggerInfos.Sort((a,b) => a.Priority - b.Priority > 0 ? 1 : -1);
        }

        public List<AttributeRank> GetAttributeRank()
        {
            var list = new List<AttributeRank>();
            var idx = 0;
            foreach (var attribute in Master.Attribute)
            {
                list.Add(attribute);
                idx++;
            }
            return list;
        }

        public void RecommendActiveSkill()
        {
            _skillTriggerInfos.Clear();
            // 初期設定に戻す
            InitSkillTriggerInfos();
            var learnSkills = LearningSkillInfos().FindAll(a => a.Master.SkillType == SkillType.Active && a.Id > 1000 && a.LearningState == LearningState.Learned);
            // 新たに追加したアクティブをアクティブの下に入れる
            foreach (var learnSkill in learnSkills)
            {
                if (_skillTriggerInfos.Find(a => a.SkillId == learnSkill.Id) == null)
                {
                    var skillTriggerInfo = new SkillTriggerInfo(_actorId,new SkillInfo(learnSkill.Id));
                    // 敵データに同じスキルがあればコピーする
                    var enemyDates = DataSystem.Enemies.FindAll(a => a.SkillTriggerDates.Find(b => b.SkillId == learnSkill.Id) != null);
                    var enemyData = enemyDates[enemyDates.Count-1];
                    
                    var skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == 0);
                    var skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == 0);
                    if (enemyData != null)
                    {
                        var skillTriggerData = enemyData.SkillTriggerDates.Find(a => a.SkillId == learnSkill.Id);
                        skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                        skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                    }
                    skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>(){skillTriggerData1,skillTriggerData2});
            
                    var findIndex = _skillTriggerInfos.FindIndex(a => DataSystem.Skills[a.SkillId].SkillType == SkillType.Active);
                    if (findIndex == -1)
                    {
                        findIndex = 1;
                    }
                    findIndex++;
                    _skillTriggerInfos.Insert(findIndex,skillTriggerInfo);
                    //skillTriggerInfo.SetPriority(findIndex);
                }
                for (int i = 0;i < _skillTriggerInfos.Count;i++)
                {
                    _skillTriggerInfos[i].SetPriority(i);
                }
            }
        }
    
        private List<int> LearnSkillIds()
        {
            var list = new List<int>();
            var learnSkills = _levelUpInfos.FindAll(a => a.IsLearnSkillData() && a.IsEnableStage(_stageId,_seek,_worldType));
            foreach (var learnSkill in learnSkills)
            {
                list.Add(learnSkill.SkillId);
            }
            return list;
        }

        private int _lastSelectSkillId = 0;
        public int LastSelectSkillId => _lastSelectSkillId;
        public void SetLastSelectSkillId(int selectSkillId)
        {
            _lastSelectSkillId = selectSkillId;
        }

        private LineType _lineIndex = LineType.Front;
        public LineType LineIndex => _lineIndex;
        public void SetLineIndex(LineType lineIndex)
        {
            _lineIndex = lineIndex;
        }

        private int _currentHp;
        public int CurrentHp => _currentHp;
        private int _currentMp;
        public int CurrentMp => _currentMp;
        // バトル勝利数
        public int DemigodParam => 0 + _levelUpInfos.FindAll(a => a.IsBattleResultData()).Count;

    // Tactics
        private int _tacticsCost = 0;
        public int TacticsCost => _tacticsCost;
        private int _tacticsCostRate = 1;
        public int TacticsCostRate => _tacticsCostRate;

        private int _battleIndex = -1;
        public int BattleIndex => _battleIndex;
        public void SetBattleIndex(int battleIndex) 
        { 
            _battleIndex = battleIndex;
        }
        private bool _lost = false;
        public bool Lost => _lost;
        private StatusInfo _plusStatus = new StatusInfo();

        public ActorInfo(ActorData actorData)
        {
            _actorId = actorData.Id;
            SetInitialParameter(actorData);
            _currentHp = Master.InitStatus.Hp;
            _currentMp = Master.InitStatus.Mp;
            InitSkillInfo();
            InitSkillTriggerInfos();
        }

#if UNITY_ANDROID
        public ActorInfo(RankingActorData rankingActorData)
        {
            _actorId = rankingActorData.ActorId;
            _attribute = Master.Attribute;
            _sp = 0;
            _upperRate = Master.NeedStatus;
            SetInitialParameter(Master);
            _currentHp = Master.InitStatus.Hp;
            _currentMp = Master.InitStatus.Mp;
            _demigodParam = rankingActorData.DemigodParam;
            InitSkillInfo();
            
            _plusStatus.SetParameter(
                rankingActorData.Hp - Master.InitStatus.Hp,
                rankingActorData.Mp - Master.InitStatus.Mp,
                rankingActorData.Atk - Master.InitStatus.Atk,
                rankingActorData.Def - Master.InitStatus.Def,
                rankingActorData.Spd - Master.InitStatus.Spd
            );
        }
#endif

        public void CopyData(ActorInfo baseActorInfo)
        {
            _plusStatus.SetParameter(
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Hp),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Mp),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Atk),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Def),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Spd)
            );
            _lastSelectSkillId = baseActorInfo.LastSelectSkillId;
            _currentHp = baseActorInfo.CurrentHp;
            _currentMp = baseActorInfo.CurrentMp;
            _tacticsCost = baseActorInfo.TacticsCost;
            _tacticsCostRate = baseActorInfo.TacticsCostRate;
            _battleIndex = baseActorInfo.BattleIndex;
            _lost = baseActorInfo.Lost;
            _levelUpInfos = baseActorInfo._levelUpInfos;
            _lineIndex = baseActorInfo._lineIndex;
            _skillTriggerInfos = baseActorInfo._skillTriggerInfos;
            _stageId = baseActorInfo._stageId;
            _seek = baseActorInfo._seek;
        }

        private void SetInitialParameter(ActorData actorData)
        {
            _plusStatus.SetParameter(actorData.PlusStatus.Hp,actorData.PlusStatus.Mp,actorData.PlusStatus.Atk,actorData.PlusStatus.Def,actorData.PlusStatus.Spd);
        }

        private void InitSkillInfo()
        {
            _lastSelectSkillId = 0;
            var selectSkill = LearningSkillInfos().Find(a => a.Id >= 100);
            if (selectSkill != null)
            {
                _lastSelectSkillId = selectSkill.Id;
            }
        }

        public List<SkillInfo> LearningSkillInfos(List<int> alchemyIds = null)
        {
            var list = new List<SkillInfo>();
            foreach (var _learningData in Master.LearningSkills)
            {
                if (list.Find(a => a.Id == _learningData.SkillId) != null) continue;
                if (LearnSkillIds().Contains(_learningData.SkillId)) continue;
                var skillInfo = new SkillInfo(_learningData.SkillId);
                if (LinkedLevel() >= _learningData.Level)
                {
                    skillInfo.SetLearningState(LearningState.Learned);
                } else
                {
                    skillInfo.SetLearningLv(_learningData.Level);
                    skillInfo.SetLearningState(LearningState.NotLearn);
                }
                list.Add(skillInfo);
            }
            foreach (var learnSkillId in LearnSkillIds())
            {
                var skillInfo = new SkillInfo(learnSkillId);
                skillInfo.SetLearningState(LearningState.Learned);
                if (alchemyIds != null)
                {
                    if (!alchemyIds.Contains(learnSkillId))
                    {
                        skillInfo.SetLearningState(LearningState.NotLearnedByAlchemy);
                    }
                }
                list.Add(skillInfo);
            }
            return list;
        }

        public void ResetData()
        {
            ChangeLost(false);
            ChangeHp(9999);
            ChangeMp(9999);
        }

        public int TrainCost()
        {
            return _levelUpInfos.FindAll(a => a.IsEnableStage(_stageId,_seek,_worldType) && a.IsTrainData()).Count + 1;
        }

        public LevelUpInfo LevelUp(int useCost,int stageId,int seek,int seekIndex ,WorldType worldType)
        {
            var levelUpInfo = new LevelUpInfo
            (
                _actorId,useCost,stageId,seek,seekIndex,worldType
            );
            levelUpInfo.SetLevel(Level);
            _levelUpInfos.Add(levelUpInfo);
            ChangeHp(CurrentParameter(StatusParamType.Hp));
            ChangeMp(CurrentParameter(StatusParamType.Mp));
            return levelUpInfo;
        }

        public bool EnableLvReset()
        {
            return LearnSkillIds().Count > 0 || _levelUpInfos.Find(a => a.Enable && a.Currency > 0) != null;
        }

        public StatusInfo LevelUpStatus(int level)
        {
            var lvUpStatus = new StatusInfo();
            LevelUpStatusInfo(lvUpStatus,level);
            return lvUpStatus;
        }

        private void LevelUpStatusInfo(StatusInfo statusInfo,int level)
        {
            statusInfo.AddParameter(StatusParamType.Hp,Master.InitStatus.Hp);
            statusInfo.AddParameter(StatusParamType.Mp,Master.InitStatus.Mp);
            statusInfo.AddParameter(StatusParamType.Atk,Master.InitStatus.Atk);
            statusInfo.AddParameter(StatusParamType.Def,Master.InitStatus.Def);
            statusInfo.AddParameter(StatusParamType.Spd,Master.InitStatus.Spd);
        
            statusInfo.AddParameter(StatusParamType.Hp,LevelGrowthRate(StatusParamType.Hp,level)); 
            statusInfo.AddParameter(StatusParamType.Mp,LevelGrowthRate(StatusParamType.Mp,level));  
            statusInfo.AddParameter(StatusParamType.Atk,LevelGrowthRate(StatusParamType.Atk,level));  
            statusInfo.AddParameter(StatusParamType.Def,LevelGrowthRate(StatusParamType.Def,level));  
            statusInfo.AddParameter(StatusParamType.Spd,LevelGrowthRate(StatusParamType.Spd,level));     
        }

        public int LevelGrowthRate(StatusParamType statusParamType,int level)
        {
            return (int)Mathf.Round(Master.NeedStatus.GetParameter(statusParamType) * 0.01f * (level-1));
        }

        public List<SkillInfo> LearningSkills(int plusLv = 0)
        {
            return LearningSkillInfos().FindAll(a => a.LearningState == LearningState.NotLearn && a.LearningLv <= (LinkedLevel()+plusLv));
        }

        public bool IsLearnedSkill(int skillId)
        {
            var learnedSkill = LearningSkillInfos().FindAll(a => a.LearningState == LearningState.Learned);
            return LearnSkillIds().Contains(skillId) || learnedSkill.Find(a => a.Id == skillId) != null;
        }

        public LevelUpInfo LearnSkill(int skillId,int cost,int stageId,int seek,int seekIndex = -1,WorldType worldType = WorldType.Main)
        {
            var skillLevelUpInfo = new LevelUpInfo(_actorId,cost,stageId,seek,seekIndex,worldType);
            skillLevelUpInfo.SetSkillId(skillId);
            return skillLevelUpInfo;
        }

        public void ChangeTacticsCostRate(int tacticsCostRate)
        {
            _tacticsCostRate = tacticsCostRate;
        }

        public int CurrentParameter(StatusParamType statusParamType)
        {
            return LevelUpStatus(LinkedLevel()).GetParameter(statusParamType);
        }

        public void ChangeHp(int hp)
        {
            _currentHp = Math.Min(hp,CurrentParameter(StatusParamType.Hp));
        }

        public void ChangeMp(int mp)
        {
            _currentMp = Math.Min(mp,CurrentParameter(StatusParamType.Mp));
        }
        
        public void ChangeLost(bool isLost)
        {
            if (isLost == false && _lost == true)
            {
                ChangeHp(1);
            }
            _lost = isLost;
        }

        public List<AttributeRank> AttributeRanks(List<ActorInfo> actorInfos)
        {
            var alchemyFeatures = new List<SkillData.FeatureData>();
            if (actorInfos != null)
            {
                foreach (var actorInfo in actorInfos)
                {
                    var skillInfos = actorInfo.LearningSkillInfos().FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.MagicAlchemy)!= null);
                    foreach (var skillInfo in skillInfos)
                    {
                        foreach (var featureData in skillInfo.Master.FeatureDates)
                        {
                            if (featureData.FeatureType == FeatureType.MagicAlchemy)
                            {
                                alchemyFeatures.Add(featureData);
                            }
                        }
                    }
                }
            }
            var attributeValues = new List<AttributeRank>();
            int idx = 1;
            foreach (var attribute in GetAttributeRank())
            {
                var attributeValue = attribute;
                foreach (var alchemyFeature in alchemyFeatures)
                {
                    if (alchemyFeature.Param2 == idx)
                    {
                        attributeValue -= alchemyFeature.Param3;
                    }
                }
                if (attributeValue < 0)
                {
                    attributeValue = AttributeRank.S;
                }
                attributeValues.Add(attributeValue);
                idx++;
            }
            return attributeValues;
        }

        private List<int> AlchemyAttributeRates(List<ActorInfo> actorInfos)
        {
            var attributeRanks = AttributeRanks(actorInfos);
            var rateList = new List<int>();
            foreach (var attributeRank in attributeRanks)
            {
                var rate = (int)AttributeRank.G - (int)attributeRank;
                rateList.Add(rate * 50);
            }
            return rateList;
        }

        public AttributeType AlchemyAttribute(List<ActorInfo> actorInfos)
        {
            var alchemyAttributeRates = AlchemyAttributeRates(actorInfos);
            int targetRand = UnityEngine.Random.Range(0,alchemyAttributeRates.Sum(a => a));
            int targetIndex = -1;
            for (int i = 0;i < alchemyAttributeRates.Count;i++)
            {
                targetRand -= alchemyAttributeRates[i];
                if (targetRand <= 0 && targetIndex == -1)
                {
                    targetIndex = i;
                }
            }
            return (AttributeType)(targetIndex+1);
        }

        public int Evaluate()
        {
            int statusValue = CurrentParameter(StatusParamType.Hp) * 7
            + CurrentParameter(StatusParamType.Mp) * 4
            + CurrentParameter(StatusParamType.Atk) * 8
            + CurrentParameter(StatusParamType.Def) * 8
            + CurrentParameter(StatusParamType.Spd) * 8;
            float magicValue = 0;
            foreach (var skillInfo in LearningSkillInfos())
            {
                if (skillInfo.LearningState == LearningState.Learned)
                {
                    var rate = 1.0f;
                    if (skillInfo.Attribute != AttributeType.None)
                    {
                        switch (Master.Attribute[(int)skillInfo.Attribute-1])
                        {
                            case AttributeRank.S:
                            case AttributeRank.A:
                                rate = 1.1f;
                                break;
                            case AttributeRank.B:
                            case AttributeRank.C:
                                rate = 0.9f;
                                break;
                            case AttributeRank.D:
                            case AttributeRank.E:
                            case AttributeRank.F:
                                rate = 0.8f;
                                break;
                            case AttributeRank.G:
                                rate = 0.7f;
                                break;
                        }
                    }
                    magicValue += rate * 100;
                    if (skillInfo.IsBattleSpecialSkill())
                    {
                        magicValue += 200;
                    }
                }
            }
            int total = statusValue + (int)magicValue + DemigodParam * 10;
            return total;
        }
    
        public List<SkillInfo> SkillActionList(List<int> alchemyIds)
        {
            var skillInfos = LearningSkillInfos(alchemyIds).FindAll(a => a.Id > 100);

            skillInfos.ForEach(a => a.SetEnable(a.LearningState == LearningState.Learned));
            var sortList1 = new List<SkillInfo>();
            var sortList2 = new List<SkillInfo>();
            var sortList3 = new List<SkillInfo>();
            skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
            foreach (var skillInfo in skillInfos)
            {
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Active || skillInfo.IsBattleSpecialSkill())
                {
                    sortList1.Add(skillInfo);
                } else
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Passive)
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
            sortList3.Sort((a,b) => {return a.LearningLv > b.LearningLv ? 1 : -1;});
            skillInfos.AddRange(sortList3);
            return skillInfos;
        }

        public List<SkillInfo> SkillInfos()
        {
            var skillInfos = LearningSkillInfos().FindAll(a => a.Id > 100);

            skillInfos.ForEach(a => a.SetEnable(a.LearningState == LearningState.Learned));
            var sortList1 = new List<SkillInfo>();
            var sortList2 = new List<SkillInfo>();
            var sortList3 = new List<SkillInfo>();
            skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
            foreach (var skillInfo in skillInfos)
            {
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Active || skillInfo.IsBattleSpecialSkill())
                {
                    sortList1.Add(skillInfo);
                } else
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Passive)
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
            sortList3.Sort((a,b) => {return a.LearningLv > b.LearningLv ? 1 : -1;});
            skillInfos.AddRange(sortList3);
            return skillInfos;
        }
    }


    public enum AttributeRank 
    {
        S = 0,
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6,
        G = 7
    }
}