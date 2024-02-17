using System;
using System.Collections.Generic;

[Serializable]
public class ActorInfo
{
    public ActorData Master {get {return DataSystem.FindActor(ActorId);}}
    private int _actorId;
    public int ActorId => _actorId;
    public int MaxHp => CurrentStatus.Hp;
    public int MaxMp => CurrentStatus.Mp;
    private int _level;
    public int Level => _level;
    private int _levelUpCost = 0;
    public int LevelUpCost => _levelUpCost;
    
    private StatusInfo _baseStatus;
    private StatusInfo _currentStatus;
    public StatusInfo CurrentStatus => _currentStatus;
    private StatusInfo _upperRate;
    private List<AttributeRank> _attribute;
    public List<AttributeRank> Attribute => _attribute;
    public List<AttributeRank> GetAttributeRank()
    {
        var list = new List<AttributeRank>();
        var idx = 0;
        foreach (var attribute in _attribute)
        {
            list.Add(attribute);
            idx++;
        }
        return list;
    }
    private List<SkillInfo> _skills = new ();
    public List<SkillInfo> Skills => _skills;

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
    private int _demigodParam;
    public int DemigodParam => _demigodParam;

    private TacticsCommandType _tacticsCommandType = TacticsCommandType.None;
    public TacticsCommandType TacticsCommandType => _tacticsCommandType;

// Tactics
    private Dictionary<TacticsCommandType,bool> _tacticsEnable = new ();
    private int _tacticsCost = 0;
    public int TacticsCost => _tacticsCost;
    private int _tacticsCostRate = 1;
    public int TacticsCostRate => _tacticsCostRate;

    private int _battleIndex = -1;
    public int BattleIndex => _battleIndex;
    public void SetBattleIndex(int battleIndex) { _battleIndex = battleIndex;}
    private bool _lost = false;
    public bool Lost => _lost;
    private int _sp = 0;
    public int Sp => _sp;
    private int _numinous = 0;
    public int Numinous => _numinous;
    private StatusInfo _plusStatus;
    private StatusInfo _tempStatus;
    public StatusInfo TempStatus => _tempStatus;

    private List<SkillInfo> _rebornSkillInfos = new ();
    public List<SkillInfo> RebornSkillInfos => _rebornSkillInfos;
    public ActorInfo(ActorData actorData)
    {
        _actorId = actorData.Id;
        _attribute = actorData.Attribute;
        _level = actorData.InitLv;
        _sp = 0;
        _upperRate = actorData.NeedStatus;
        SetInitialParameter(actorData);
        _currentHp = _baseStatus.Hp;
        _currentMp = _baseStatus.Mp;
        _demigodParam = 10;
        InitSkillInfo();
    }

#if UNITY_ANDROID
    public ActorInfo(RankingActorData rankingActorData)
    {
        _actorId = rankingActorData.ActorId;
        _attribute = Master.Attribute;
        _level = rankingActorData.Level;
        _sp = 0;
        _upperRate = Master.NeedStatus;
        SetInitialParameter(Master);
        _currentHp = _baseStatus.Hp;
        _currentMp = _baseStatus.Mp;
        _demigodParam = rankingActorData.DemigodParam;
        InitSkillInfo();
        
        _plusStatus.SetParameter(
            rankingActorData.Hp - _baseStatus.Hp,
            rankingActorData.Mp - _baseStatus.Mp,
            rankingActorData.Atk - _baseStatus.Atk,
            rankingActorData.Def - _baseStatus.Def,
            rankingActorData.Spd - _baseStatus.Spd
        );
    }
#endif

    public void CopyData(ActorInfo baseActorInfo)
    {
        _level = baseActorInfo.Level;
        _plusStatus = new StatusInfo();
        _plusStatus.SetParameter(
            baseActorInfo._plusStatus.GetParameter(StatusParamType.Hp),
            baseActorInfo._plusStatus.GetParameter(StatusParamType.Mp),
            baseActorInfo._plusStatus.GetParameter(StatusParamType.Atk),
            baseActorInfo._plusStatus.GetParameter(StatusParamType.Def),
            baseActorInfo._plusStatus.GetParameter(StatusParamType.Spd)
        );
        _currentStatus = new StatusInfo();
        _currentStatus.SetParameter(
            baseActorInfo.CurrentStatus.GetParameter(StatusParamType.Hp),
            baseActorInfo.CurrentStatus.GetParameter(StatusParamType.Mp),
            baseActorInfo.CurrentStatus.GetParameter(StatusParamType.Atk),
            baseActorInfo.CurrentStatus.GetParameter(StatusParamType.Def),
            baseActorInfo.CurrentStatus.GetParameter(StatusParamType.Spd)
        );
        _tempStatus = new StatusInfo();
        _tempStatus.SetParameter(
            baseActorInfo.TempStatus.GetParameter(StatusParamType.Hp),
            baseActorInfo.TempStatus.GetParameter(StatusParamType.Mp),
            baseActorInfo.TempStatus.GetParameter(StatusParamType.Atk),
            baseActorInfo.TempStatus.GetParameter(StatusParamType.Def),
            baseActorInfo.TempStatus.GetParameter(StatusParamType.Spd)
        );
        _skills.Clear();
        foreach (var baseSkill in baseActorInfo.Skills)
        {
            var skillInfo = new SkillInfo(baseSkill.Id);
            skillInfo.CopyData(baseSkill);
            _skills.Add(skillInfo);
        }
        _lastSelectSkillId = baseActorInfo.LastSelectSkillId;
        _currentHp = baseActorInfo.CurrentHp;
        _currentMp = baseActorInfo.CurrentMp;
        _demigodParam = baseActorInfo.DemigodParam;
        _tacticsCommandType = baseActorInfo.TacticsCommandType;
        _tacticsEnable = baseActorInfo._tacticsEnable;
        _tacticsCost = baseActorInfo.TacticsCost;
        _tacticsCostRate = baseActorInfo.TacticsCostRate;
        _battleIndex = baseActorInfo.BattleIndex;
        _lost = baseActorInfo.Lost;
        _sp = baseActorInfo.Sp;
        _numinous = baseActorInfo.Numinous;
    }

    private void SetInitialParameter(ActorData actorData)
    {
        _baseStatus = actorData.InitStatus;
        _plusStatus = new StatusInfo();
        _plusStatus.SetParameter(actorData.PlusStatus.Hp,actorData.PlusStatus.Mp,actorData.PlusStatus.Atk,actorData.PlusStatus.Def,actorData.PlusStatus.Spd);
        _tempStatus = new StatusInfo();
        _currentStatus = new StatusInfo();
        _currentStatus.SetParameter(_baseStatus.Hp,_baseStatus.Mp,_baseStatus.Atk,_baseStatus.Def,_baseStatus.Spd);
    }

    private void InitSkillInfo()
    {
        _skills.Clear();
        for (int i = 0;i < Master.LearningSkills.Count;i++)
        {
            var _learningData = Master.LearningSkills[i];
            if (_skills.Find(a =>a.Id == _learningData.SkillId) != null) continue;
            var skillInfo = new SkillInfo(_learningData.SkillId);
            if (_level >= _learningData.Level)
            {
                skillInfo.SetLearningState(LearningState.Learned);
            } else
            {
                skillInfo.SetLearningLv(_learningData.Level);
                skillInfo.SetLearningState(LearningState.NotLearn);
            }
            _skills.Add(skillInfo);
        }
        _lastSelectSkillId = 0;
        var selectSkill = _skills.Find(a => a.Id >= 100);
        if (selectSkill != null){
            _lastSelectSkillId = selectSkill.Id;
        }

    }

    public void ResetData()
    {
        ChangeLost(false);
        ClearTacticsCommand();
        ChangeHp(9999);
        ChangeMp(9999);
    }

    public StatusInfo LevelUp(int bonus)
    {
        var lvUpStatus = new StatusInfo();
        lvUpStatus.SetParameter(0,0,0,0,0);
        CalcLevelUpStatusInfo(lvUpStatus);
        _level++;
        //_sp += 10;
        for (int i = 0;i < bonus;i++)
        {
            CalcLevelUpStatusInfo(lvUpStatus);
            _level++;
            //_sp += 10;
        }
        foreach (var skillInfo in LearningSkills())
        {
            skillInfo.SetLearningState(LearningState.Learned);
        }
        return lvUpStatus;
    }

    public List<SkillInfo> LearningSkills(int plusLv = 0)
    {
        return _skills.FindAll(a => a.LearningState == LearningState.NotLearn && a.LearningLv <= (_level+plusLv));
    }

    private void CalcLevelUpStatusInfo(StatusInfo statusInfo)
    {
        statusInfo.AddParameter(StatusParamType.Hp,IsLevelUpStatus(StatusParamType.Hp)); 
        statusInfo.AddParameter(StatusParamType.Mp,IsLevelUpStatus(StatusParamType.Mp));  
        statusInfo.AddParameter(StatusParamType.Atk,IsLevelUpStatus(StatusParamType.Atk));  
        statusInfo.AddParameter(StatusParamType.Def,IsLevelUpStatus(StatusParamType.Def));  
        statusInfo.AddParameter(StatusParamType.Spd,IsLevelUpStatus(StatusParamType.Spd));     
    }

    private int IsLevelUpStatus(StatusParamType statusParamType)
    {
        return GrowthRate(statusParamType);
        /*
        int IsLevelUpStatus = 0;
        int upperRate = _upperRate.GetParameter(statusParamType) + ((_level-1) * 10);
        while (upperRate >= 0)
        {
            int rate = UnityEngine.Random.Range(0,100);
            if (rate < upperRate)
            {
                IsLevelUpStatus += 1;
            }
            upperRate -= 100;
            if (upperRate < 0)
            {
                break;
            }
        }
        return IsLevelUpStatus;
        */
    }

    public void GainLevelUpCost(int cost)
    {
        _levelUpCost += cost;
    }

    public bool IsLearnedSkill(int skillId)
    {
        return _skills.Find(a => a.Id == skillId) != null;
    }

    public void LearnSkill(int skillId)
    {
        var skillInfo = new SkillInfo(skillId);
        skillInfo.SetLearningState(LearningState.Learned);
        _skills.Add(skillInfo);
    }

    public void ForgetSkill(SkillInfo skillInfo)
    {
        int skillIdx = _skills.FindIndex(a => a == skillInfo);
        if (skillIdx > 0)
        {
            _skills.RemoveAt(skillIdx);
        }
    }

    public void RefreshTacticsEnable(TacticsCommandType tacticsCommandType,bool enable)
    {
        _tacticsEnable[tacticsCommandType] = enable;
    }

    public bool EnableTactics(TacticsCommandType tacticsCommandType)
    {
        return _tacticsEnable[tacticsCommandType];
    }

    public void ChangeTacticsCostRate(int tacticsCostRate)
    {
        _tacticsCostRate = tacticsCostRate;
    }

    public void SetTacticsCommand(TacticsCommandType tacticsCommandType,int tacticsCost)
    {
        _tacticsCommandType = tacticsCommandType;
        _tacticsCost = tacticsCost;
    }

    public void ClearTacticsCommand()
    {
        _tacticsCommandType = TacticsCommandType.None;
        _tacticsCost = 0;
    }
    

    public void ChangeSp(int value)
    {
        _sp = value;
    }

    public int GrowthRate(StatusParamType statusParamType)
    {
        return _upperRate.GetParameter(statusParamType) + ((_level-1) * 1);
    }

    public void DecideStrength(int useNuminous)
    {
        // ここは百分率代入
        _plusStatus.AddParameter(StatusParamType.Hp,_tempStatus.GetParameter(StatusParamType.Hp) * 0.01f);
        _plusStatus.AddParameter(StatusParamType.Mp,_tempStatus.GetParameter(StatusParamType.Mp) * 0.01f);
        _plusStatus.AddParameter(StatusParamType.Atk,_tempStatus.GetParameter(StatusParamType.Atk) * 0.01f);
        _plusStatus.AddParameter(StatusParamType.Def,_tempStatus.GetParameter(StatusParamType.Def) * 0.01f);
        _plusStatus.AddParameter(StatusParamType.Spd,_tempStatus.GetParameter(StatusParamType.Spd) * 0.01f);
        _currentStatus.SetParameter(
            CurrentParameter(StatusParamType.Hp),
            CurrentParameter(StatusParamType.Mp),
            CurrentParameter(StatusParamType.Atk),
            CurrentParameter(StatusParamType.Def),
            CurrentParameter(StatusParamType.Spd)
        );
        var plusHp = (int)Math.Round(_tempStatus.GetParameter(StatusParamType.Hp) * 0.01f);
        if (plusHp > 0)
        {
            ChangeHp(_currentHp + plusHp);
        }
        ClearStrength();
        _numinous += useNuminous;
    }

    public void ClearStrength()
    {
        TempStatus.Clear();
    }

    public void StrengthReset()
    {
        SetInitialParameter(Master);
        ChangeHp(_currentHp);
        ChangeMp(_currentMp);
        ChangeSp((_level-1) * 10);
        _numinous = 0;
        ClearStrength();
    }

    public int CurrentParameter(StatusParamType statusParamType)
    {
        return _baseStatus.GetParameter(statusParamType) + _plusStatus.GetParameter(statusParamType);
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
            var skillInfos = actorInfo.Skills.FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.MagicAlchemy)!= null);
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
            attributeValues.Add(DataSystem.GetTextData(textId).Text);
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
        int targetRand = 0;
        foreach (var alchemyAttributeRate in alchemyAttributeRates)
        {
            targetRand += alchemyAttributeRate;
        }
        targetRand = UnityEngine.Random.Range (0,targetRand);
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

    public void GainDemigod(int param)
    {
        _demigodParam += param;
    }

    public int Evaluate()
    {
        int statusValue = CurrentParameter(StatusParamType.Hp) * 2
        + CurrentParameter(StatusParamType.Mp) * 2
        + CurrentParameter(StatusParamType.Atk) * 3
        + CurrentParameter(StatusParamType.Def) * 3
        + CurrentParameter(StatusParamType.Spd) * 3;
        float magicValue = 0;
        foreach (var skillInfo in _skills)
        {
            var rate = 1.0f;
            if (skillInfo.Attribute != AttributeType.None)
            {
                switch (_attribute[(int)skillInfo.Attribute-1])
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
            magicValue += (rate * 25);
        }
        int total = statusValue + (int)magicValue + _demigodParam * 5;
        if (Lost == true)
        {
            total = total / 2;
        }
        return total;
    }

    public void AddRebornSkill(SkillInfo rebornSkillInfo)
    {
        rebornSkillInfo.SetEnable(true);
        _rebornSkillInfos.Add(rebornSkillInfo);
    }

    public List<ListData> SkillActionList()
    {
        var skillInfos = Skills.FindAll(a => a.Id > 100);

        skillInfos.ForEach(a => a.SetEnable(a.LearningState == LearningState.Learned));
        var sortList1 = new List<SkillInfo>();
        var sortList2 = new List<SkillInfo>();
        var sortList3 = new List<SkillInfo>();
        skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
        foreach (var skillInfo in skillInfos)
        {
            if (skillInfo.LearningState == LearningState.Learned && skillInfo.Master.SkillType == SkillType.Magic || skillInfo.Master.SkillType == SkillType.Demigod || skillInfo.Master.SkillType == SkillType.Awaken)
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

    public void UpdateLearningDates(List<LearningData> learningDates)
    {
        foreach (var skillInfo in _skills)
        {
            var learningDate = learningDates.Find(a => a.SkillId == skillInfo.Id);
            if (learningDate != null)
            {
                skillInfo.SetLearningLv(learningDate.Level);
                if (_level >= learningDate.Level)
                {
                    skillInfo.SetLearningState(LearningState.Learned);
                } else
                {
                    skillInfo.SetLearningState(LearningState.NotLearn);
                }
            }
        }
    }

    public int LevelUpByCost()
    {
        var levelUpNum = 0;
        var useCost = 0;
        for (int i = 0;i <= LevelUpCost;i++)
        {
            if (i >= (TacticsUtility.TrainCost(_level+levelUpNum,this) + useCost))
            {
                useCost += i;
                levelUpNum++;
            }
        }

        _levelUpCost -= useCost;

        return levelUpNum;
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