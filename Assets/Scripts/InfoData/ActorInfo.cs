using System;
using System.Collections.Generic;

[Serializable]
public class ActorInfo
{
    public ActorData Master {get {return DataSystem.Actors.Find(a => a.Id == ActorId);}}
    private int _actorId;
    public int ActorId => _actorId;
    public int MaxHp => CurrentStatus.Hp;
    public int MaxMp => CurrentStatus.Mp;
    private int _level;
    public int Level => _level;
    
    private StatusInfo _baseStatus;
    private StatusInfo _currentStatus;
    public StatusInfo CurrentStatus => _currentStatus;
    private StatusInfo _upperRate;
    private List<AttributeRank> _attribute;
    public List<AttributeRank> Attribute => _attribute;
    private List<SkillInfo> _skills = new ();
    public List<SkillInfo> Skills => _skills;

    private int _lastSelectSkillId = 0;
    public int LastSelectSkillId => _lastSelectSkillId;
    public void SetLastSelectSkillId(int selectSkillId)
    {
        _lastSelectSkillId = selectSkillId;
    }

    private int _currentHp;
    public int CurrentHp => _currentHp;
    private int _currentMp;
    public int CurrentMp => _currentMp;
    private int _ap;
    public int Ap => _ap;
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
    private int _nextLearnSkillId = 0;
    public int NextLearnSkillId => _nextLearnSkillId;
    private int _nextLearnCost = 0;
    public int NextLearnCost => _nextLearnCost;
    private int _nextBattleEnemyIndex = -1;
    public int NextBattleEnemyIndex => _nextBattleEnemyIndex;
    private int _nextBattleEnemyId = -1;
    public int NextBattleEnemyId => _nextBattleEnemyId;

    private bool _inBattle = false;
    public bool InBattle => _inBattle;
    public void SetInBattle(bool inBattle) { _inBattle = inBattle;}
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
        foreach (var skillInfo in baseActorInfo.Skills)
        {
            skillInfo.SetLearningState(LearningState.Learned);
            _skills.Add(skillInfo);
        }
        _lastSelectSkillId = baseActorInfo.LastSelectSkillId;
        _currentHp = baseActorInfo.CurrentHp;
        _currentMp = baseActorInfo.CurrentMp;
        _ap = baseActorInfo.Ap;
        _demigodParam = baseActorInfo.DemigodParam;
        _tacticsCommandType = baseActorInfo.TacticsCommandType;
        _tacticsEnable = baseActorInfo._tacticsEnable;
        _tacticsCost = baseActorInfo.TacticsCost;
        _tacticsCostRate = baseActorInfo.TacticsCostRate;
        _nextLearnSkillId = baseActorInfo.NextLearnSkillId;
        _nextLearnCost = baseActorInfo.NextLearnCost;
        _nextBattleEnemyIndex = baseActorInfo.NextBattleEnemyIndex;
        _nextBattleEnemyId = baseActorInfo.NextBattleEnemyId;
        _inBattle = baseActorInfo.InBattle;
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
            skillInfo.SetLearningState(LearningState.Learned);
            _skills.Add(skillInfo);
        }
        _lastSelectSkillId = 0;
        var selectSkill = _skills.Find(a => a.Id >= 100);
        if (selectSkill != null){
            _lastSelectSkillId = selectSkill.Id;
        }

        foreach (var learningSkills in Master.LearningSkills)
        {
            for (int i = 0;i < learningSkills.DeckNum;i++)
            {
                var skillInfo = new SkillInfo(learningSkills.SkillId);
                skillInfo.SetLearningState(LearningState.Learned);
            }
        }
    }

    public StatusInfo LevelUp(int bonus)
    {
        var lvUpStatus = new StatusInfo();
        lvUpStatus.SetParameter(0,0,0,0,0);
        CalcLevelUpStatusInfo(lvUpStatus);
        _level++;
        _sp += 10;
        for (int i = 0;i < bonus;i++)
        {
            CalcLevelUpStatusInfo(lvUpStatus);
            _level++;
            _sp += 10;
        }
        return lvUpStatus;
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
        _nextLearnSkillId = 0;
        _nextLearnCost = 0;
        _nextBattleEnemyIndex = -1;
        _nextBattleEnemyId = 0;
    }
    
    public void SetNextLearnSkillId(int skillId)
    {
        _nextLearnSkillId = skillId;
    }

    public void SetNextLearnCost(int learningCost)
    {
        _nextLearnCost = learningCost;
    }

    public void SetNextBattleEnemyIndex(int enemyIndex,int enemyId)
    {
        _nextBattleEnemyIndex = enemyIndex;
        _nextBattleEnemyId = enemyId;
    }

    public void ChangeSp(int value)
    {
        _sp = value;
    }

    public int UsePointCost(StatusParamType statusParamType)
    {
        int UseCost = _upperRate.GetParameter(statusParamType) + ((_level-1) * 10);
        /*
        UseCost += (_plusStatus.GetParameter(statusParamType)+TempStatus.GetParameter(statusParamType)) / 5;
        var _currentAlcana = GameSystem.CurrentData.CurrentAlcana;
        if (_currentAlcana != null)
        {
            if (_currentAlcana.IsStatusCostDown(statusParamType))
            {
                UseCost += 20;
            }
        }
        */
        return UseCost;
    }

    public void DecideStrength(int useNuminous)
    {
        _plusStatus.AddParameter(StatusParamType.Hp,_tempStatus.GetParameter(StatusParamType.Hp));
        _plusStatus.AddParameter(StatusParamType.Mp,_tempStatus.GetParameter(StatusParamType.Mp));
        _plusStatus.AddParameter(StatusParamType.Atk,_tempStatus.GetParameter(StatusParamType.Atk));
        _plusStatus.AddParameter(StatusParamType.Def,_tempStatus.GetParameter(StatusParamType.Def));
        _plusStatus.AddParameter(StatusParamType.Spd,_tempStatus.GetParameter(StatusParamType.Spd));
        _currentStatus.SetParameter(
            CurrentParameter(StatusParamType.Hp),
            CurrentParameter(StatusParamType.Mp),
            CurrentParameter(StatusParamType.Atk),
            CurrentParameter(StatusParamType.Def),
            CurrentParameter(StatusParamType.Spd)
        );
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

    public List<AttributeRank> AttributeParams(List<ActorInfo> actorInfos)
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
        foreach (var attribute in Attribute)
        {
            var attributeValue = attribute;
            foreach (var alchemyFeature in alchemyFeatures)
            {
                if (alchemyFeature.Param2 == idx)
                {
                    attributeValue += alchemyFeature.Param3;
                }
            }
            attributeValues.Add(attributeValue);
            idx++;
        }
        return attributeValues;
    }

    public List<string> AttributeValues(List<ActorInfo> actorInfos)
    {
        var attributeParams = AttributeParams(actorInfos);
        var attributeValues = new List<string>();
        foreach (var attribute in Attribute)
        {
            int textId = 320 + (int)attribute;
            attributeValues.Add(DataSystem.System.GetTextData(textId).Text);
        }
        return attributeValues;
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
        if (_rebornSkillInfos == null)
        {
            _rebornSkillInfos = new List<SkillInfo>();
        }
        _rebornSkillInfos.Add(rebornSkillInfo);
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