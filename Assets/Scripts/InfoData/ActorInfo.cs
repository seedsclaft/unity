using System;
using System.Collections.Generic;

[Serializable]
public class ActorInfo
{
    public ActorsData.ActorData Master {get {return DataSystem.Actors.Find(a => a.Id == ActorId);}}
    private int _actorId;
    public int ActorId {get {return _actorId;}}
    public int MaxHp { get
        {
            return CurrentStatus.Hp;
        }
    }
    public int MaxMp { get
        {
            return CurrentStatus.Mp;
        }
    }

    private int _level;
    public int Level {get {return _level;}}
    
    private StatusInfo _baseStatus;
    private StatusInfo _currentStatus;
    public StatusInfo CurrentStatus {get {return _currentStatus;}}
    private StatusInfo _usePoint;
    public StatusInfo UsePoint {get {return _usePoint;}}
    private List<int> _attribute;
    public List<int> Attribute {get {return _attribute;}}
    private List<SkillInfo> _skills;
    public List<SkillInfo> Skills {get {return _skills;}}


    private int _currentHp;
    public int CurrentHp {get {return _currentHp;}}
    private int _currentMp;
    public int CurrentMp {get {return _currentMp;}}
    private int _ap;
    public int Ap {get {return _ap;}}

    private TacticsComandType _tacticsComandType = TacticsComandType.None;
    public TacticsComandType TacticsComandType {get {return _tacticsComandType;}}

// Tactics
    private Dictionary<TacticsComandType,bool> _tacticsEnable = new Dictionary<TacticsComandType, bool>();
    private int _tacticsCost = 0;
    public int TacticsCost {get {return _tacticsCost;}}
    private int _tacticsCostRate = 1;
    public int TacticsCostRate {get {return _tacticsCostRate;}}
    private int _nextLearnSkillId = 0;
    public int NextLearnSkillId {get {return _nextLearnSkillId;}}
    private int _nextBattleEnemyIndex = -1;
    public int NextBattleEnemyIndex {get {return _nextBattleEnemyIndex;}}
    private int _nextBattleEnemyId = -1;
    public int NextBattleEnemyId {get {return _nextBattleEnemyId;}}

    private bool _inBattle = false;
    public bool InBattle {get {return _inBattle;} set {_inBattle = value;}}
    private bool _lost = false;
    public bool Lost {get {return _lost;}}
// Status
    private int _sp = 0;
    public int Sp {get {return _sp;}}
    private int _numinous = 0;
    public int Numinous {get {return _numinous;}}
    private StatusInfo _plusStatus;
    private StatusInfo _tempStatus;
    public StatusInfo TempStatus {get {return _tempStatus;}}

    public ActorInfo(ActorsData.ActorData actorData)
    {
        _actorId = actorData.Id;
        _attribute = actorData.Attribute;
        _level = actorData.InitLv;
        _sp = 0;
        _usePoint = actorData.NeedStatus; 
        SetInitialParameter(actorData);
        _currentHp = _baseStatus.Hp;
        _currentMp = _baseStatus.Mp;   
    }

    private void SetInitialParameter(ActorsData.ActorData actorData)
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
            _skills.Add(skillInfo);
        }
    }

    public void LevelUp()
    {
        _level++;
        _sp += 10;
    }

    public void LearnSkill(int skillId)
    {
        SkillInfo skillInfo = new SkillInfo(skillId);
        _skills.Add(skillInfo);
    }

    public void ForgetSkill(int skillId)
    {
        SkillInfo skillInfo = _skills.Find(a => a.Id == skillId);
        skillInfo.SetForget(true);
    }

    public void RefreshTacticsEnable(TacticsComandType tacticsComandType,bool enable)
    {
        _tacticsEnable[tacticsComandType] = enable;
    }

    public bool EnableTactics(TacticsComandType tacticsComandType)
    {
        return _tacticsEnable[tacticsComandType];
    }

    public void ChangeTacticsCostRate(int tacticsCostRate)
    {
        _tacticsCostRate = tacticsCostRate;
    }

    public void SetTacticsCommand(TacticsComandType tacticsComandType,int tacticsCost)
    {
        _tacticsComandType = tacticsComandType;
        _tacticsCost = tacticsCost;
    }

    public void ClearTacticsCommand()
    {
        _tacticsComandType = TacticsComandType.None;
        _tacticsCost = 0;
        _nextLearnSkillId = 0;
        _nextBattleEnemyIndex = -1;
        _nextBattleEnemyId = 0;
    }

    public void SetNextLearnSkillId(int skillId)
    {
        _nextLearnSkillId = skillId;
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
        int useCost = UsePoint.GetParameter(statusParamType);
        useCost += (_plusStatus.GetParameter(statusParamType)+TempStatus.GetParameter(statusParamType)) / 5;
        return useCost;
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
        TempStatus.Clear();
        _numinous += useNuminous;
    }

    public void StrengthReset()
    {
        ActorsData.ActorData actorData = DataSystem.Actors.Find(a => a.Id == _actorId);
        SetInitialParameter(actorData);
        ChangeHp(_currentHp);
        ChangeMp(_currentMp);
        ChangeSp((_level-1) * 10);
        _numinous = 0;
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
}
