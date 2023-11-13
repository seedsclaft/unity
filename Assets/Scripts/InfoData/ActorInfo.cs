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
    private List<int> _attribute;
    public List<int> Attribute => _attribute;
    private List<SkillInfo> _skills;
    public List<SkillInfo> Skills => _skills;
    private List<SkillInfo> _decksData = new ();
    public List<SkillInfo> DecksData => _decksData;

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
    public bool InBattle {get {return _inBattle;} }
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

    public void InitSkillInfo(List<LearningData> learningData)
    {
        _skills = new List<SkillInfo>();
        for (int i = 0;i < learningData.Count;i++)
        {
            LearningData _learningData = learningData[i];
            if (_skills.Find(a =>a.Id == _learningData.SkillId) != null) continue;
            SkillInfo skillInfo = new SkillInfo(_learningData.SkillId);
            skillInfo.SetLearningState(LearningState.Learned);
            _skills.Add(skillInfo);
        }
        _lastSelectSkillId = 0;
        var selectSkill = _skills.Find(a => a.Id >= 100);
        if (selectSkill != null){
            _lastSelectSkillId = selectSkill.Id;
        }

        _decksData.Clear();
        foreach (var learningSkills in Master.LearningSkills)
        {
            for (int i = 0;i < learningSkills.DeckNum;i++)
            {
                SkillInfo skillInfo = new SkillInfo(learningSkills.SkillId);
                skillInfo.SetLearningState(LearningState.Learned);
                _decksData.Add(skillInfo);
            }
        }
    }

    public StatusInfo LevelUp(int bonus)
    {
        var lvUpstatus = new StatusInfo();
        lvUpstatus.SetParameter(0,0,0,0,0);
        CalcLevelUpStatusInfo(lvUpstatus);
        _level++;
        _sp += 10;
        for (int i = 0;i < bonus;i++)
        {
            CalcLevelUpStatusInfo(lvUpstatus);
            _level++;
            _sp += 10;
        }
        return lvUpstatus;
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
        SkillInfo skillInfo = new SkillInfo(skillId);
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

    public void SetNextLearnCost(int leariningCost)
    {
        _nextLearnCost = leariningCost;
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
        int addHp = TempStatus.GetParameter(StatusParamType.Hp);
        _plusStatus.AddParameter(StatusParamType.Hp,addHp);
        int addMp = TempStatus.GetParameter(StatusParamType.Mp);
        _plusStatus.AddParameter(StatusParamType.Mp,addMp);
        int addAtk = TempStatus.GetParameter(StatusParamType.Atk);
        _plusStatus.AddParameter(StatusParamType.Atk,addAtk);
        int addDef = TempStatus.GetParameter(StatusParamType.Def);
        _plusStatus.AddParameter(StatusParamType.Def,addDef);
        int addSpd = TempStatus.GetParameter(StatusParamType.Spd);
        _plusStatus.AddParameter(StatusParamType.Spd,addSpd);
        CurrentStatus.SetParameter(
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
        ActorData actorData = Master;
        SetInitialParameter(actorData);
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
        _currentHp = hp;
        if (_currentHp > CurrentParameter(StatusParamType.Hp)){
            _currentHp = CurrentParameter(StatusParamType.Hp);
        }
    }

    public void ChangeMp(int mp)
    {
        _currentMp = mp;
        if (_currentMp > CurrentParameter(StatusParamType.Mp)){
            _currentMp = CurrentParameter(StatusParamType.Mp);
        }
    }
    
    public void ChangeLost(bool isLost)
    {
        if (isLost == false && _lost == true)
        {
            ChangeHp(1);
        }
        _lost = isLost;
    }

    public List<int> AttirbuteParams(List<ActorInfo> actorInfos)
    {
        List<SkillData.FeatureData> alchemyFeatures = new List<SkillData.FeatureData>();
        foreach (var actorInfo in actorInfos)
        {
            List<SkillInfo> skillInfos = actorInfo.Skills.FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.MagicAlchemy)!= null);
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
        List<int> attributeValues = new List<int>();
        int idx = 1;
        foreach (var attribute in Attribute)
        {
            int attributeValue = attribute;
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

    public List<string> AttirbuteValues(List<ActorInfo> actorInfos)
    {
        List<int> attributeParams = AttirbuteParams(actorInfos);
        List<string> attributeValues = new List<string>();
        foreach (var attribute in Attribute)
        {
            int textId = 320;
            if (attribute > 100){
                textId += 1;
            } else
            if (attribute > 80){
                textId += 2;
            } else
            if (attribute > 60){
                textId += 3;
            } else
            if (attribute > 40){
                textId += 4;
            } else
            if (attribute > 20){
                textId += 5;
            } else
            if (attribute > 10){
                textId += 6;
            } else
            if (attribute > 0){
                textId += 7;
            } else{
                textId += 8;
            }
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
        int evaluate = 0;
        int statusParam = CurrentParameter(StatusParamType.Hp) * 2
        + CurrentParameter(StatusParamType.Mp) * 2
        + CurrentParameter(StatusParamType.Atk) * 3
        + CurrentParameter(StatusParamType.Def) * 3
        + CurrentParameter(StatusParamType.Spd) * 3;
        int magicParam = 0;
        foreach (var skillInfo in Skills)
        {
            magicParam += 1 * 25;
        }
        int attibuteParam = 0;
        foreach (var attribute in Attribute)
        {
            attibuteParam += attribute / 5;
        }
        int total = evaluate + statusParam + magicParam + attibuteParam + DemigodParam * 5;
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
