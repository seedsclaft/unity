using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BattlerInfo 
{
    private StatusInfo _status = null;
    public StatusInfo Status {get {return _status;}}
    private int _index = 0;
    public int Index{get {return _index;}}
    private bool _isActor = false;
    public bool isActor{get {return _isActor;}}
    private int _charaId;
    public int CharaId {get {return _charaId;}}
    private int _level;
    public int Level {get {return _level;}}
    public int MaxHp {get {return _status.GetParameter(StatusParamType.Hp) + StateEffectAll(StateType.MaxHpUp);}}
    public int MaxMp {get {return _status.GetParameter(StatusParamType.Mp) + StateEffectAll(StateType.MaxMpUp);}}
    private int _hp;
    public int Hp {get {return _hp;}}
    private int _mp;
    public int Mp {get {return _mp;}}
    private int _ap;
    public int Ap {get {return _ap;}}
    
    private List<SkillInfo> _skills;
    public List<SkillInfo> Skills {get {return _skills;}}
    private ActorInfo _actorInfo;
    public ActorInfo ActorInfo {get {return _actorInfo;} }
    private EnemiesData.EnemyData _enemyData;
    public EnemiesData.EnemyData EnemyData {get {return _enemyData;} }
    private List<KindType> _kinds = new List<KindType>();
    public List<KindType> Kinds {get {return _kinds;} }
    private int _lastSkillId = 0;
    public SkillInfo LastSkill {get {return Skills.Find(a => a.Id == _lastSkillId);} }
    public void SetLastSkillIndex(int index){
        _lastSkillId = index;
    }
    private List<StateInfo> _stateInfos = new List<StateInfo>();
    public List<StateInfo> StateInfos {get {return _stateInfos;} }

    private bool _isAwaken = false;
    public bool IsAwaken {get {return _isAwaken;} }

    private int _lineIndex = 0;
    public int LineIndex {get {return _lineIndex;} }

    
    private int _chainSuccessCount = 0;
    public int ChainSuccessCount {get {return _chainSuccessCount;} }
    private int _lastTargetIndex = 0;
    public void SetLastTargetIndex(int index){
        _lastTargetIndex = index;
    }

    private int _turnCount = 0;
    public int TurnCount {get {return _turnCount;}}

    public BattlerInfo(ActorInfo actorInfo,int index){
        _charaId = actorInfo.ActorId;
        _level = actorInfo.Level;
        StatusInfo statusInfo = new StatusInfo();
        statusInfo.SetParameter(
            actorInfo.CurrentParameter(StatusParamType.Hp),
            actorInfo.CurrentParameter(StatusParamType.Mp),
            actorInfo.CurrentParameter(StatusParamType.Atk),
            actorInfo.CurrentParameter(StatusParamType.Def),
            actorInfo.CurrentParameter(StatusParamType.Spd)
        );
        _status = statusInfo;
        _index = index;
        _skills = actorInfo.Skills;
        _isActor = true;
        
        _actorInfo = actorInfo;
        _hp = actorInfo.CurrentHp;
        _mp = actorInfo.CurrentMp;
        _lineIndex = 0;

        MakePassiveSkills();
        if (_lastSkillId == 0)
        {
            _lastSkillId = _skills.Find(a => a.Id > 100).Id;
        }
        ResetAp(true);
    }

    public BattlerInfo(EnemiesData.EnemyData enemyData,int lv,int index,int lineIndex){
        _charaId = enemyData.Id;
        _level = lv;
        StatusInfo statusInfo = new StatusInfo();
        statusInfo.SetParameter(
            enemyData.BaseStatus.Hp + (int)Math.Floor(lv * enemyData.BaseStatus.Hp * 0.04f),
            enemyData.BaseStatus.Mp + (int)Math.Floor(lv * enemyData.BaseStatus.Mp * 0.04f),
            enemyData.BaseStatus.Atk + (int)Math.Floor(lv * enemyData.BaseStatus.Atk * 0.04f),
            enemyData.BaseStatus.Def + (int)Math.Floor(lv * enemyData.BaseStatus.Def * 0.04f),
            enemyData.BaseStatus.Spd + (int)Math.Floor(lv * enemyData.BaseStatus.Spd * 0.04f)
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
                SkillInfo skillInfo = new SkillInfo(enemyData.LearningSkills[i].SkillId);
                skillInfo.SetTriggerDatas(enemyData.LearningSkills[i].TriggerDatas);
                skillInfo.SetWeight(enemyData.LearningSkills[i].Weight);
                _skills.Add(skillInfo);
            }
        }
        for (int i = 0;i < enemyData.Kinds.Count;i++)
        {
            _kinds.Add(enemyData.Kinds[i]);
        }

        MakePassiveSkills();
        ResetAp(true);
    }

    private void MakePassiveSkills()
    {
        _stateInfos.Clear();
        List<SkillInfo> passiveSkills = _skills.FindAll(a => a.Master.SkillType == SkillType.Passive);
        foreach (var passiveSkill in passiveSkills)
        {
            foreach (var featureData in passiveSkill.Master.FeatureDatas)
            {
                if (featureData.FeatureType == FeatureType.AddState)
                {
                    StateInfo stateInfo = new StateInfo(featureData.Param1,featureData.Param2,featureData.Param3,Index,Index);
                    _stateInfos.Add(stateInfo);
                }
            }
        }
    }

    public bool IsActor()
    {
        return _isActor;
    }

    public void ResetAp(bool IsBattleStart)
    {
        /* 再行動
        if (IsState(StateType.ChainDamageUp))
        {
            List <StateInfo> stateInfos = GetStateInfoAll(StateType.ChainDamageUp);
            for (var i = stateInfos.Count-1;i >= 0;i--)
            {
                StateInfo stateInfo = stateInfos[i];
                stateInfo.UpdateTurn();
                bool IsRemove = stateInfo.UpdateTurn();
                if (IsRemove)
                {
                    RemoveState(stateInfo);
                }
            }
            _ap = 0;
            return;
        }
        */
        if (IsState(StateType.CounterOura))
        {
            _ap = 1;
            return;
        }
        int rand = new Random().Next(-10, 10);
        if (IsBattleStart == false)
        {
            rand = 0;
        }
        _ap = 500 - (CurrentSpd() + rand) * 4;
    }

    public void GainAp(int ap)
    {
        _ap += ap;
    }

    public void UpdateAp()
    {
        if (IsState(StateType.Death))
        {
            return;
        }
        if (IsState(StateType.CounterOura) || IsState(StateType.Benediction))
        {
            _ap = 1;
            return;
        }
        if (IsState(StateType.Chain))
        {
            _ap += 3;
            return;
        }
        if (IsState(StateType.Slow))
        {
            _ap -= 3;
            return;
        }
        //if (isActor == false) return;
        _ap -= 4;
    }

    public void ChangeAp(int value)
    {
        _ap += value;
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
        if (_hp < 0){
            _hp = 0;
        }
        if (_hp > MaxHp){
            _hp = MaxHp;
        }
        if (_hp <= 0)
        {
            StateInfo stateInfo = new StateInfo((int)StateType.Death,0,0,Index,Index);
            AddState(stateInfo);
        }
    }

    public void GainMp(int value)
    {
        _mp += value;
        if (_mp < 0){
            _mp = 0;
        }
        if (_mp > MaxMp){
            _mp = MaxMp;
        }
    }

    public bool IsAlive()
    {
        return _hp > 0;
    }

    public bool CanMove()
    {
        bool CanMove = true;
        if (IsState(StateType.Death))
        {
            CanMove = false;
        }
        if (IsState(StateType.Stun))
        {
            CanMove = false;
        }
        if (IsState(StateType.Chain))
        {
            CanMove = false;
        }
        return CanMove;
    }


    public bool IsState(StateType stateType)
    {
        return _stateInfos.Find(a => a.StateId == (int)stateType) != null;
    }

    public StateInfo GetStateInfo(StateType stateType)
    {
        return _stateInfos.Find(a => a.StateId == (int)stateType);
    }

    public List<StateInfo> GetStateInfoAll(StateType stateType)
    {
        return _stateInfos.FindAll(a => a.StateId == (int)stateType);
    }

    // ステートを消す
    public void EraseStateInfo(StateType stateType)
    {
        List<StateInfo> getStateInfoAll = GetStateInfoAll(stateType);

        for (int i = getStateInfoAll.Count-1;i >= 0;i--)
        {
            RemoveState(getStateInfoAll[i]);
        }
    }

    public int StateTurn(StateType stateType)
    {
        int turns = 0;
        if (IsState(stateType))
        {
            turns += _stateInfos.Find(a => a.StateId == (int)stateType).Turns;
        }
        return turns;
    }

    public int StateEffect(StateType stateType)
    {
        int effect = 0;
        if (IsState(stateType))
        {
            effect += _stateInfos.Find(a => a.StateId == (int)stateType).Effect;
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

    public bool AddState(StateInfo stateInfo)
    {
        bool IsAdded = false;
        if (IsState(StateType.Barrier))
        {
            if (stateInfo.Master.Id == (int)StateType.Stun || stateInfo.Master.Id == (int)StateType.Slow || stateInfo.Master.Id == (int)StateType.Chain || stateInfo.Master.Id == (int)StateType.Curse)
            {
                return false;
            }
        }
        if (_stateInfos.Find(a => a.CheckOverWriteState(stateInfo) == true) == null)
        {
            _stateInfos.Add(stateInfo);
            IsAdded = true;
        }
        return IsAdded;
    }

    public bool RemoveState(StateInfo stateInfo)
    {
        bool IsRemoved = false;
        int RemoveIndex = _stateInfos.FindIndex(a => a.StateId == stateInfo.StateId);
        if (RemoveIndex > -1)
        {
            _stateInfos.RemoveAt(RemoveIndex);
            IsRemoved = true;
            if (stateInfo.StateId == (int)StateType.Death)
            {
                _hp = 1;
            }
        }
        return IsRemoved;
    }

    public void UpdateState(RemovalTiming removalTiming)
    {
        for (var i = _stateInfos.Count-1;i >= 0;i--)
        {
            StateInfo stateInfo = _stateInfos[i];
            if (stateInfo.Master.RemovalTiming == removalTiming)
            {
                bool IsRemove = stateInfo.UpdateTurn();
                if (IsRemove)
                {
                    RemoveState(stateInfo);
                }
            }
        }
    }

    public void UpdateStateCount(RemovalTiming removalTiming,int stateId)
    {
        for (var i = _stateInfos.Count-1;i >= 0;i--)
        {
            StateInfo stateInfo = _stateInfos[i];
            if (stateInfo.Master.RemovalTiming == removalTiming && stateInfo.StateId == stateId)
            {
                bool IsRemove = stateInfo.UpdateTurn();
                if (IsRemove)
                {
                    RemoveState(stateInfo);
                }
            }
        }
    }

    public List<StateInfo> UpdateChainState()
    {
        List<StateInfo> stateInfos = new List<StateInfo>();
        for (var i = _stateInfos.Count-1;i >= 0;i--)
        {
            StateInfo stateInfo = _stateInfos[i];
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


    public int CurrentAtk()
    {
        int atk = Status.Atk;
        if (IsState(StateType.Demigod))
        {
            atk += StateEffect(StateType.Demigod);
        }
        return atk;
    }
    
    public int CurrentDef()
    {
        int def = Status.Def;
        if (IsState(StateType.Demigod))
        {
            def += StateEffect(StateType.Demigod);
        }
        return def;
    }
    
    public int CurrentSpd()
    {
        int spd = Status.Spd;
        if (IsState(StateType.Demigod))
        {
            spd += StateEffect(StateType.Demigod);
        }
        if (IsState(StateType.Accel))
        {
            spd += StateEffect(StateType.Accel) * StateTurn(StateType.Accel);
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
        if (rate < 0)
        {
            rate = 0;
        }
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

    // Triggerを満たすSkillInfoを取得
    public List<SkillInfo> TriggerdSkillInfos(TriggerTiming triggerTiming,ActionInfo actionInfo,List<BattlerInfo> battlers)
    {
        List <SkillInfo> triggeredSkills = new List<SkillInfo>();
        for (var i = 0;i < Skills.Count;i++)
        {
            SkillInfo skillInfo = Skills[i];
            if (skillInfo.Master.SkillType == SkillType.Demigod && _isAwaken){
                continue;
            }
            var triggerDatas = skillInfo.Master.TriggerDatas.FindAll(a => a.TriggerTiming == triggerTiming);
            if (triggerDatas.Count > 0)
            {
                for (var j = 0;j < triggerDatas.Count;j++)
                {
                    if (triggerDatas[j].TriggerType == TriggerType.HpRateUnder)
                    {
                        if ( TriggerdHpRateUnderSkillInfos(triggerDatas[j]) )
                        {
                            triggeredSkills.Add(skillInfo);
                        }
                    }
                    if (triggerDatas[j].TriggerType == TriggerType.AfterMp)
                    {
                        if ( TriggerdAfterMpSkillInfos(triggerDatas[j],actionInfo) )
                        {
                            triggeredSkills.Add(skillInfo);
                        }
                    }
                    if (triggerDatas[j].TriggerType == TriggerType.ChainCount)
                    {
                        if ( TriggerdChainCountSkillInfos(triggerDatas[j],actionInfo) )
                        {
                            triggeredSkills.Add(skillInfo);
                        }
                    }
                    if (triggerDatas[j].TriggerType == TriggerType.ActionResultDeath)
                    {
                        if ( TriggerdActionResultDeathSkillInfos(triggerDatas[j],actionInfo,battlers) )
                        {
                            skillInfo.SetInterrupt(true);
                            triggeredSkills.Add(skillInfo);
                        }
                    }
                    if (triggerDatas[j].TriggerType == TriggerType.DeadWithoutSelf)
                    {
                        if ( TriggerdDeadWithoutSelfSkillInfos(triggerDatas[j],battlers) )
                        {
                            triggeredSkills.Add(skillInfo);
                        }
                    }
                    if (triggerDatas[j].TriggerType == TriggerType.SelfDead)
                    {
                        if ( TriggerdSelfDeadSkillInfos(triggerDatas[j],actionInfo.actionResults) )
                        {
                            triggeredSkills.Add(skillInfo);
                        }
                    }
                }
            }
        }
        return triggeredSkills;
    }

    private bool TriggerdHpRateUnderSkillInfos(SkillsData.TriggerData triggerData)
    {
        bool IsTriggered = false;
        if (triggerData.Param1 * 0.01f >= ((float)Hp / (float)MaxHp))
        {
            IsTriggered = true;
        }
        return IsTriggered;
    }

    private bool TriggerdAfterMpSkillInfos(SkillsData.TriggerData triggerData,ActionInfo actionInfo)
    {
        bool IsTriggered = false;
        if (triggerData.Param1 == Mp)
        {
            if (actionInfo.SubjectIndex == Index && actionInfo.MpCost > 0)
            {
                IsTriggered = true;
            } else
            {
                var results = actionInfo.actionResults.FindAll(a => a.TargetIndex == Index);
                var find = results.Find(a => a.MpDamage > 0);
                if (find != null)
                {              
                    IsTriggered = true;
                }
            }
        }
        return IsTriggered;
    }

    private bool TriggerdChainCountSkillInfos(SkillsData.TriggerData triggerData,ActionInfo actionInfo)
    {
        bool IsTriggered = false;
        if (_chainSuccessCount >= triggerData.Param1)
        {
            IsTriggered = true;
        }
        return IsTriggered;
    }

    private bool TriggerdActionResultDeathSkillInfos(SkillsData.TriggerData triggerData,ActionInfo actionInfo,List<BattlerInfo> battlerInfos)
    {
        bool IsTriggered = false;
        List<ActionResultInfo> actionResultInfos = actionInfo.actionResults;
        if (actionResultInfos.Find(a => a.IsDead && battlerInfos.Find(b => b.Index == a.TargetIndex && b.isActor) != null) != null)
        {
            IsTriggered = true;
        }
        return IsTriggered;
    }

    private bool TriggerdDeadWithoutSelfSkillInfos(SkillsData.TriggerData triggerData,List<BattlerInfo> battlerInfos)
    {
        bool IsTriggered = false;
        if (isActor)
        {
            int count = 0;
            for (var i = 0;i < battlerInfos.Count;i++)
            {
                if (battlerInfos[i].isActor && battlerInfos[i].IsState(StateType.Death))
                {
                    count++;
                }
            }
            if (IsAlive() && count > 0 && (count+1) >= battlerInfos.FindAll(a => a.isActor).Count)
            {
                IsTriggered = true;
            }
        } else
        {
            int count = 0;
            for (var i = 0;i < battlerInfos.Count;i++)
            {
                if (!battlerInfos[i].isActor && battlerInfos[i].IsState(StateType.Death))
                {
                    count++;
                }
            }
            if (IsAlive() && count > 0 && (count+1) >= battlerInfos.FindAll(a => !a.isActor).Count)
            {
                IsTriggered = true;
            }
        } 
        return IsTriggered;
    }

    private bool TriggerdSelfDeadSkillInfos(SkillsData.TriggerData triggerData,List<ActionResultInfo> actionResultInfos){
        bool IsTriggered = false;
        if (actionResultInfos.Find(a => a.DeadIndexList.Contains(Index) == true) != null)
        {
            IsTriggered = true;
        }
        if (IsTriggered)
        {
            List<StateInfo> stateInfos = GetStateInfoAll(StateType.Death);
            for (var i = 0;i < stateInfos.Count;i++){
                RemoveState(stateInfos[i]);
            }
        }
        return IsTriggered;
    }
    
}
