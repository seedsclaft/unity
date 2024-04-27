using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public void SetLevelUpInfo(List<LevelUpInfo> levelUpInfos)
        {
            _levelUpInfos = levelUpInfos;
        }
        
        public int Level => _levelUpInfos.FindAll(a => a.Enable && a.SkillId == -1).Count + 1;
        
        private List<SkillTriggerInfo> _skillTriggerInfo = new ();
        public List<SkillTriggerInfo> SkillTriggerInfo => _skillTriggerInfo;
        public void SetSkillTriggerInfo(List<SkillTriggerInfo> skillTriggerInfo)
        {
            _skillTriggerInfo = skillTriggerInfo;
        }

        public StatusInfo CurrentStatus => LevelUpStatus(Level);
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
        private List<int> LearnSkillIds()
        {
            var list = new List<int>();
            var learnSkills = _levelUpInfos.FindAll(a => a.Enable && a.SkillId > -1);
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
        public int DemigodParam => 0 + _levelUpInfos.FindAll(a => a.Enable && a.StageId > -1 && a.Currency == 0 && a.SkillId == -1).Count;

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
        private StatusInfo _plusStatus;

        private List<SkillInfo> _rebornSkillInfos = new ();
        public List<SkillInfo> RebornSkillInfos => _rebornSkillInfos;
        public ActorInfo(ActorData actorData)
        {
            _actorId = actorData.Id;
            SetInitialParameter(actorData);
            _currentHp = Master.InitStatus.Hp;
            _currentMp = Master.InitStatus.Mp;
            InitSkillInfo();
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
            _plusStatus = new StatusInfo();
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
        }

        private void SetInitialParameter(ActorData actorData)
        {
            _plusStatus = new StatusInfo();
            _plusStatus.SetParameter(actorData.PlusStatus.Hp,actorData.PlusStatus.Mp,actorData.PlusStatus.Atk,actorData.PlusStatus.Def,actorData.PlusStatus.Spd);
        }

        private void InitSkillInfo()
        {
            _lastSelectSkillId = 0;
            var selectSkill = LearningSkillInfos().Find(a => a.Id >= 100);
            if (selectSkill != null){
                _lastSelectSkillId = selectSkill.Id;
            }

        }

        public List<SkillInfo> LearningSkillInfos()
        {
            var list = new List<SkillInfo>();
            for (int i = 0;i < Master.LearningSkills.Count;i++)
            {
                var _learningData = Master.LearningSkills[i];
                if (list.Find(a =>a.Id == _learningData.SkillId) != null) continue;
                var skillInfo = new SkillInfo(_learningData.SkillId);
                if (Level >= _learningData.Level)
                {
                    skillInfo.SetLearningState(LearningState.Learned);
                } else
                {
                    skillInfo.SetLearningLv(_learningData.Level);
                    skillInfo.SetLearningState(LearningState.NotLearn);
                }
                list.Add(skillInfo);
            }
            foreach (var _learnSkillId in LearnSkillIds())
            {
                var skillInfo = new SkillInfo(_learnSkillId);
                skillInfo.SetLearningState(LearningState.Learned);
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

        public LevelUpInfo LevelUp(int useCost = 0,int stageId = -1,int seek = -1,int seekIndex = -1)
        {
            var levelUpInfo = new LevelUpInfo(
                _actorId,useCost,stageId,seek,seekIndex
            );
            levelUpInfo.SetLevel(Level);
            _levelUpInfos.Add(levelUpInfo);
            foreach (var skillInfo in LearningSkills())
            {
                skillInfo.SetLearningState(LearningState.Learned);
            }
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
            return LearningSkillInfos().FindAll(a => a.LearningState == LearningState.NotLearn && a.LearningLv <= (Level+plusLv));
        }

        public bool IsLearnedSkill(int skillId)
        {
            return LearnSkillIds().Contains(skillId);
        }

        public LevelUpInfo LearnSkill(int skillId,int cost,int stageId = -1,int seek = -1,int seekIndex = -1)
        {
            var skillLevelUpInfo = new LevelUpInfo(_actorId,cost,stageId,seek,seekIndex);
            skillLevelUpInfo.SetSkillId(skillId);
            return skillLevelUpInfo;
        }

        public void ChangeTacticsCostRate(int tacticsCostRate)
        {
            _tacticsCostRate = tacticsCostRate;
        }

        public int CurrentParameter(StatusParamType statusParamType)
        {
            return LevelUpStatus(Level).GetParameter(statusParamType);
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

        public List<string> AttributeValues(List<ActorInfo> actorInfos)
        {
            var attributeParams = AttributeRanks(actorInfos);
            var attributeValues = new List<string>();
            foreach (var attribute in GetAttributeRank())
            {
                int textId = 320 + (int)attribute;
                attributeValues.Add(DataSystem.GetText(textId));
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
                    magicValue += (rate * 100);
                    if (skillInfo.Master.SkillType == SkillType.Messiah || skillInfo.Master.SkillType == SkillType.Awaken)
                    {
                        magicValue += 200;
                    }
                }
            }
            int total = statusValue + (int)magicValue + DemigodParam * 10;
            return total;
        }

        public void AddRebornSkill(SkillInfo rebornSkillInfo)
        {
            rebornSkillInfo.SetEnable(true);
            _rebornSkillInfos.Add(rebornSkillInfo);
        }

        public List<ListData> SkillActionList()
        {
            var skillInfos = LearningSkillInfos().FindAll(a => a.Id > 100);

            skillInfos.ForEach(a => a.SetEnable(a.LearningState == LearningState.Learned));
            var sortList1 = new List<SkillInfo>();
            var sortList2 = new List<SkillInfo>();
            var sortList3 = new List<SkillInfo>();
            skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
            foreach (var skillInfo in skillInfos)
            {
                if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Active || skillInfo.Master.SkillType == SkillType.Messiah || skillInfo.Master.SkillType == SkillType.Awaken)
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
            //skillInfos.Sort((a,b) => {return a.LearningState > b.LearningState ? 1 : -1;});
            return ListData.MakeListData(skillInfos);
        }

    }


    public enum AttributeRank {
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