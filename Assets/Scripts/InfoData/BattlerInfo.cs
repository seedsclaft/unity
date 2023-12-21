﻿using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BattlerInfo 
{
    private StatusInfo _status = null;
    public StatusInfo Status => _status;
    public StatusInfo CurrentStatus(bool isNoEffect){
        var currentStatus = new StatusInfo();
        currentStatus.SetParameter(MaxHp,MaxMp,CurrentAtk(isNoEffect),CurrentDef(isNoEffect),CurrentSpd(isNoEffect));
        return currentStatus;
    }
    private int _index = 0;
    public int Index => _index;
    private bool _isActor = false;
    public bool isActor => _isActor;
    private int _charaId;
    public int CharaId => _charaId;
    private int _level;
    public int Level => _level;
    public int MaxHp {get {return _status.GetParameter(StatusParamType.Hp) + StateEffectAll(StateType.MaxHpUp);}}
    public int MaxMp {get {return _status.GetParameter(StatusParamType.Mp) + StateEffectAll(StateType.MaxMpUp);}}
    private int _hp;
    public int Hp => _hp;
    private int _mp;
    public int Mp => _mp;
    private int _ap;
    public int Ap => _ap;
    
    private List<SkillInfo> _skills;
    public List<SkillInfo> Skills => _skills;
    private ActorInfo _actorInfo;
    public ActorInfo ActorInfo => _actorInfo;
    private EnemyData _enemyData;
    public EnemyData EnemyData => _enemyData;
    private List<KindType> _kinds = new ();
    public List<KindType> Kinds => _kinds;
    private int _lastSelectSkillId = 0;
    public int LastSelectSkillId => _lastSelectSkillId;
    public void SetLastSelectSkillId(int selectSkillId){
        _lastSelectSkillId = selectSkillId;
    }
    private List<StateInfo> _stateInfos = new ();
    public List<StateInfo> StateInfos => _stateInfos;

    private bool _isAwaken = false;
    public bool IsAwaken => _isAwaken;

    private LineType _lineIndex = 0;
    public LineType LineIndex => _lineIndex;

    private bool _bossFlag = false;
    public bool BossFlag => _bossFlag;
    
    private int _chainSuccessCount = 0;
    public int ChainSuccessCount =>  _chainSuccessCount;
    private int _payBattleMp = 0;
    public int PayBattleMp => _payBattleMp;
    private int _attackedCount = 0;
    public int AttackedCount => _attackedCount;
    
    private int _lastTargetIndex = 0;
    public void SetLastTargetIndex(int index){
        _lastTargetIndex = index;
    }

    private int _turnCount = 0;
    public int TurnCount => _turnCount;
    private int _demigodParam = 0;
    public int DemigodParam => _demigodParam;

    private bool _preserveAlive = false;
    public bool PreserveAlive => _preserveAlive;

    public BattlerInfo(ActorInfo actorInfo,int index){
        _charaId = actorInfo.ActorId;
        _level = actorInfo.Level;
        var statusInfo = new StatusInfo();
        statusInfo.SetParameter(
            actorInfo.CurrentParameter(StatusParamType.Hp),
            actorInfo.CurrentParameter(StatusParamType.Mp),
            actorInfo.CurrentParameter(StatusParamType.Atk),
            actorInfo.CurrentParameter(StatusParamType.Def),
            actorInfo.CurrentParameter(StatusParamType.Spd)
        );
        _status = statusInfo;
        _index = index;
        _skills = actorInfo.Skills.FindAll(a => a.LearningState == LearningState.Learned);
        foreach (var skill in _skills)
        {
            skill.SetUseCount(0);
        }
        _demigodParam = actorInfo.DemigodParam;
        _isActor = true;
        
        _actorInfo = actorInfo;
        _hp = actorInfo.CurrentHp;
        _mp = actorInfo.CurrentMp;
        _lineIndex = LineType.Front;

        if (_lastSelectSkillId == 0)
        {
            _lastSelectSkillId = _skills.Find(a => a.Id > 100).Id;
        }
        foreach (var kind in actorInfo.Master.Kinds)
        {
            _kinds.Add(kind);
        }
        AddKindPassive();
        ResetAp(true);
    }

    public BattlerInfo(EnemyData enemyData,int lv,int index,LineType lineIndex,bool isBoss){
        _charaId = enemyData.Id;
        _level = lv;
        _bossFlag = isBoss;
        var statusInfo = new StatusInfo();
        int plusHpParam = isBoss == true ? 50 : 0;
        statusInfo.SetParameter(
            enemyData.BaseStatus.Hp + (int)Math.Floor(plusHpParam + lv + lv * enemyData.BaseStatus.Hp * 0.1f),
            Math.Min(50, enemyData.BaseStatus.Mp + lv),
            enemyData.BaseStatus.Atk + (int)Math.Floor(lv + lv * enemyData.BaseStatus.Atk * 0.05f),
            enemyData.BaseStatus.Def + (int)Math.Floor(lv + lv * enemyData.BaseStatus.Def * 0.05f),
            Math.Min(100, enemyData.BaseStatus.Spd + (int)Math.Floor(lv * enemyData.BaseStatus.Spd * 0.05f))
        );
        _status = statusInfo;
        _index = index + 100;
        _isActor = false;
        _enemyData = enemyData;
        _hp = _status.Hp;
        _mp = _status.Mp;
        _lineIndex = lineIndex;
        _skills = new List<SkillInfo>();
        for (int i = 0;i < enemyData.LearningSkills.Count;i++)
        {
            if (_level >= enemyData.LearningSkills[i].Level)
            {
                var skillInfo = new SkillInfo(enemyData.LearningSkills[i].SkillId);
                skillInfo.SetTriggerDates(enemyData.LearningSkills[i].TriggerDates);
                skillInfo.SetWeight(enemyData.LearningSkills[i].Weight);
                _skills.Add(skillInfo);
            }
        }
        foreach (var kind in enemyData.Kinds)
        {
            _kinds.Add(kind);
        }
        AddKindPassive();
        ResetAp(true);
    }

    public void ResetData()
    {
        _stateInfos.Clear();
        GainHp(_status.Hp);
        GainMp(_status.Mp);
        _isAwaken = false;
        _preserveAlive = false;
        _chainSuccessCount = 0;
        _payBattleMp = 0;
        _attackedCount = 0;
        ResetAp(true);
    }

    private void AddKindPassive()
    {
        foreach (var kind in _kinds)
        {
            if (kind > 0)
            {
                var skillInfo = new SkillInfo((int)kind * 10 + 10000);
                if (_skills.Find(a => a.Id == skillInfo.Id) == null)
                {
                    _skills.Add(skillInfo);
                }
            }
        }
    }

    public void ResetAp(bool IsBattleStart)
    {
        if (IsState(StateType.CounterAura))
        {
            _ap = 1;
            return;
        }
        int rand = 0;
        if (IsBattleStart == true)
        {
            rand = new Random().Next(-10, 10);
        }
        _ap = 1000 - (CurrentSpd(false) + rand) * 8;
        _ap = Math.Max(_ap,120);
    }

    public void UpdateAp()
    {
        if (IsState(StateType.Death))
        {
            return;
        }
        if (IsState(StateType.Stun))
        {
            return;
        }
        if (IsState(StateType.Chain))
        {
            _ap += 6;
            return;
        }
        if (IsState(StateType.CounterAura) || IsState(StateType.Benediction))
        {
            _ap = 1;
            return;
        }
        if (IsState(StateType.RevengeAct))
        {
            _ap += 2;
            return;
        }
        if (IsState(StateType.Slow))
        {
            _ap -= 4;
            return;
        }
        if (IsState(StateType.Heist))
        {
            _ap -= 12;
            return;
        }
        _ap -= 8;
    }

    public void ChangeAp(int value)
    {
        _ap += value;
        if (_ap < 0){
            _ap = 0;
        }
    }

    public void SetAp(int value)
    {
        _ap = value;
        if (_ap < 0){
            _ap = 0;
        }
    }

    public int LastTargetIndex()
    {
        if (isActor){
            return _lastTargetIndex;
        }
        return -1;
    }
    

    public void GainHp(int value)
    {
        _hp += value;
        _hp = Math.Max(0,_hp);
        _hp = Math.Min(_hp,MaxHp);
        if (_hp <= 0)
        {
            _stateInfos.Clear();
            StateInfo stateInfo = new StateInfo(StateType.Death,0,0,Index,Index,-1);
            AddState(stateInfo,true);
        }
    }

    public void GainMp(int value)
    {
        _mp += value;
        _mp = Math.Max(0,_mp);
        _mp = Math.Min(_mp,MaxMp);
    }

    public bool IsAlive()
    {
        return _hp > 0;
    }

    public bool CanMove()
    {
        if (IsState(StateType.Death))
        {
            return false;
        }
        if (IsState(StateType.Stun))
        {
            return false;
        }
        if (IsState(StateType.Chain))
        {
            return false;
        }
        return true;
    }


    public bool IsState(StateType stateType)
    {
        return _stateInfos.Find(a => a.StateType == stateType) != null;
    }

    public StateInfo GetStateInfo(StateType stateType)
    {
        return _stateInfos.Find(a => a.StateType == stateType);
    }

    public List<StateInfo> GetStateInfoAll(StateType stateType)
    {
        return _stateInfos.FindAll(a => a.StateType == stateType);
    }

    // ステートを消す
    public void EraseStateInfo(StateType stateType)
    {
        var getStateInfoAll = GetStateInfoAll(stateType);

        for (int i = getStateInfoAll.Count-1;i >= 0;i--)
        {
            RemoveState(getStateInfoAll[i],true);
        }
    }

    public int StateTurn(StateType stateType)
    {
        int turns = 0;
        if (IsState(stateType))
        {
            turns += _stateInfos.Find(a => a.StateType == stateType).Turns;
        }
        return turns;
    }

    public int StateEffect(StateType stateType)
    {
        int effect = 0;
        if (IsState(stateType))
        {
            effect += _stateInfos.Find(a => a.StateType == stateType).Effect;
        }
        return effect;
    }

    public int StateEffectAll(StateType stateType)
    {
        int effect = 0;
        if (IsState(stateType))
        {
            List<StateInfo> stateInfos = GetStateInfoAll(stateType);
            
            for (var i = 0;i < stateInfos.Count;i++)
            {
                effect += stateInfos[i].Effect;
            }
        }
        return effect;
    }

    public bool AddState(StateInfo stateInfo,bool doAdd)
    {
        bool IsAdded = false;
        if (IsState(StateType.Barrier))
        {
            if (stateInfo.Master.Abnormal)
            {
                return false;
            }
        }
        if (IsState(StateType.Undead))
        {
            if ((StateType)stateInfo.Master.StateType == StateType.Regenerate)
            {
                return false;
            }
        }
        if (_stateInfos.Find(a => a.CheckOverWriteState(stateInfo) == true) == null)
        {
            if (doAdd)
            {
                _stateInfos.Add(stateInfo);
                if (stateInfo.Master.StateType == StateType.MaxHpUp)
                {
                    GainHp(stateInfo.Effect);
                }
                if (stateInfo.Master.StateType == StateType.MaxMpUp)
                {
                    GainMp(stateInfo.Effect);
                }
            }
            IsAdded = true;
        }
        return IsAdded;
    }

    public bool RemoveState(StateInfo stateInfo,bool doRemove)
    {
        bool IsRemoved = false;
        int RemoveIndex = _stateInfos.FindIndex(a => a.StateType == stateInfo.StateType && (a.SkillId == stateInfo.SkillId || stateInfo.SkillId == -1));
        if (RemoveIndex > -1)
        {
            if (doRemove)
            {
                if (stateInfo.SkillId == -1)
                {
                    // 効果による解除は全て複数効果あっても全部解除する
                    for (int i = _stateInfos.Count-1;0 <= i;i--)
                    {
                        if (_stateInfos[i].StateType == (StateType)stateInfo.Master.StateType)
                        {
                            _stateInfos.Remove(_stateInfos[i]);
                        }
                    }
                } else
                {
                    _stateInfos.RemoveAt(RemoveIndex);
                }
                if (stateInfo.StateType == StateType.Death)
                {
                    _preserveAlive = true;
                    if (_hp == 0)_hp = 1;
                }
            }
            IsRemoved = true;
        }
        return IsRemoved;
    }

    public List<StateInfo> UpdateState(RemovalTiming removalTiming)
    {
        var stateInfos = new List<StateInfo>();
        for (var i = _stateInfos.Count-1;i >= 0;i--)
        {
            var stateInfo = _stateInfos[i];
            if (stateInfo.Master.RemovalTiming == removalTiming)
            {
                bool IsRemove = stateInfo.UpdateTurn();
                if (IsRemove)
                {
                    RemoveState(stateInfo,true);
                    stateInfos.Add(stateInfo);
                }
            }
        }
        return stateInfos;
    }

    public void UpdateStateTurn(RemovalTiming removalTiming,int stateId)
    {
        for (var i = _stateInfos.Count-1;i >= 0;i--)
        {
            var stateInfo = _stateInfos[i];
            if (stateInfo.Master.RemovalTiming == removalTiming && stateInfo.StateType == (StateType)stateId)
            {
                bool IsRemove = stateInfo.UpdateTurn();
                if (IsRemove)
                {
                    RemoveState(stateInfo,true);
                }
            }
        }
    }

    public void UpdateStateCount(RemovalTiming removalTiming,StateInfo stateInfo)
    {
        if (stateInfo.Master.RemovalTiming == removalTiming)
        {
            bool IsRemove = stateInfo.UpdateTurn();
            if (IsRemove)
            {
                RemoveState(stateInfo,true);
            }
        }
    }

    public List<StateInfo> UpdateChainState()
    {
        var stateInfos = new List<StateInfo>();
        for (var i = _stateInfos.Count-1;i >= 0;i--)
        {
            var stateInfo = _stateInfos[i];
            if (stateInfo.Master.RemovalTiming == RemovalTiming.UpdateChain)
            {
                bool IsChainDamage = stateInfo.UpdateTurn();
                if (IsChainDamage)
                {
                    stateInfo.ResetTurns();
                    stateInfos.Add(stateInfo);
                }
            }
        }
        return stateInfos;
    }


    public int CurrentAtk(bool isNoEffect)
    {
        int atk = Status.Atk;
        if (isNoEffect == false)
        {
            if (IsState(StateType.Demigod))
            {
                atk += _demigodParam;
            }
            if (IsState(StateType.StatusUp))
            {
                atk += StateEffectAll(StateType.StatusUp);
            }
            if (IsState(StateType.AtkUp))
            {
                atk += StateEffectAll(StateType.AtkUp);
            }
            if (IsState(StateType.AtkDown))
            {
                atk -= StateEffectAll(StateType.AtkDown);
            }
        }
        return atk;
    }
    
    public int CurrentDef(bool isNoEffect)
    {
        int def = Status.Def;
        if (isNoEffect == false)
        {
            if (IsState(StateType.Demigod))
            {
                def += _demigodParam;
            }
            if (IsState(StateType.StatusUp))
            {
                def += StateEffectAll(StateType.StatusUp);
            }
            if (IsState(StateType.DefUp))
            {
                def += StateEffectAll(StateType.DefUp);
            }
            if (IsState(StateType.DefDown))
            {
                def -= StateEffectAll(StateType.DefDown);
            }
            if (IsState(StateType.DefPerDown))
            {
                def = (int)((float)def * ((100 - StateEffectAll(StateType.DefPerDown)) * 0.01f));
            }
        }
        return def;
    }
    
    public int CurrentSpd(bool isNoEffect)
    {
        int spd = Status.Spd;
        if (isNoEffect == false)
        {
            if (IsState(StateType.Demigod))
            {
                spd += _demigodParam;
            }
            if (IsState(StateType.SpdUp))
            {
                spd += StateEffectAll(StateType.SpdUp);
            }
            if (IsState(StateType.Accel))
            {
                spd += StateEffect(StateType.Accel) * StateTurn(StateType.Accel);
            }
            if (IsState(StateType.StatusUp))
            {
                spd += StateEffectAll(StateType.StatusUp);
            }
        }
        return spd;
    }

    public int TargetRate()
    {
        int rate = 100;
        if (IsState(StateType.TargetRateDown))
        {
            rate -= StateEffectAll(StateType.TargetRateDown);
        }
        if (IsState(StateType.TargetRateUp))
        {
            rate += StateEffectAll(StateType.TargetRateUp);
        }
        rate = Math.Max(0,rate);
        return rate;
    }

    public void SetAwaken()
    {
        _isAwaken = true;
    }

    public void GainChainCount(int value)
    {
        _chainSuccessCount += value;
    }

    public void GainPayBattleMp(int value)
    {
        _payBattleMp += value;
    }

    public void GainAttackedCount(int value)
    {
        _attackedCount += value;
    }

    public void TurnEnd()
    {
        _turnCount += 1;
        // アクセル
        if (IsState(StateType.Accel))
        {
            StateInfo stateInfo = GetStateInfo(StateType.Accel);
            stateInfo.Turns += 1;
        }
    }

    public List<SkillInfo> ActiveSkills()
    {
        return Skills.FindAll(a => a.Master.SkillType != SkillType.Passive);
    }

    public List<SkillInfo> PassiveSkills()
    {
        return Skills.FindAll(a => a.Master.SkillType == SkillType.Passive);
    }

    public List<StateInfo> IconStateInfos()
    {
        var iconStates = new List<StateInfo>();
        foreach (var stateInfo in _stateInfos)
        {
            if (stateInfo.Master.IconPath != "" && stateInfo.Master.IconPath != "\"\"")
            {
                iconStates.Add(stateInfo);
            }
        }
        return iconStates;
    }

    public void SetPreserveAlive(bool preserveAlive)
    {
        _preserveAlive = preserveAlive;
    }

    // バフ解除効果で解除するstateを取得
    public List<StateInfo> GetRemovalBuffStates()
    {
        return _stateInfos.FindAll(a => a.Master.Removal);
    }
}
