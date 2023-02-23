using System;
using System.Collections;
using System.Collections.Generic;

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
    public BattlerInfo(ActorInfo actorInfo,int index){
        _charaId = actorInfo.ActorId;
        _level = actorInfo.Level;
        StatusInfo statusInfo = new StatusInfo();
        statusInfo.SetParameter(actorInfo.Status.Hp,actorInfo.Status.Mp,actorInfo.Status.Atk,actorInfo.Status.Def,actorInfo.Status.Spd);
        _status = statusInfo;
        _index = index;
        _skills = actorInfo.Skills;
        _isActor = true;
        
        _actorInfo = actorInfo;
        _hp = actorInfo.Hp;
        _mp = actorInfo.Mp;
        _lineIndex = 0;
        ResetAp(true);
    }

    public BattlerInfo(EnemiesData.EnemyData enemyData,int lv,int index,int lineIndex){
        _charaId = enemyData.Id;
        _level = lv;
        StatusInfo statusInfo = new StatusInfo();
        statusInfo.SetParameter(enemyData.BaseStatus.Hp,enemyData.BaseStatus.Mp,enemyData.BaseStatus.Atk,enemyData.BaseStatus.Def,enemyData.BaseStatus.Spd);
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
            _skills.Add(new SkillInfo(enemyData.LearningSkills[i].SkillId));
        }
        for (int i = 0;i < enemyData.Kinds.Count;i++)
        {
            _kinds.Add(enemyData.Kinds[i]);
        }

        ResetAp(true);
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
        int rand = new Random().Next(-10, 10);
        if (IsBattleStart == false)
        {
            rand = 0;
        }
        _ap = 400 - (Status.Spd + rand) * 4;
        if (IsState(StateType.SetAfterAp))
        {
            _ap = StateEffect(StateType.SetAfterAp);
            EraseStateInfo(StateType.SetAfterAp);
        }
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
        if (IsState(StateType.Chain))
        {
            _ap += 2;
            return;
        }
        if (IsState(StateType.Slow))
        {
            _ap -= 2;
            return;
        }
        if (IsState(StateType.CounterOura))
        {
            _ap += 2;
            return;
        }
        //if (isActor == false) return;
        _ap -= 4;
    }

    public int LastTargetIndex()
    {
        if (isActor){
            return _lastTargetIndex;
        }
        return -1;
    }

    public void ChangeHp(int value)
    {
        _hp += value;
        if (_hp < 0){
            _hp = 0;
        }
        if (_hp > _status.Hp){
            _hp = _status.Hp;
        }
        if (_hp <= 0)
        {
            StateInfo stateInfo = new StateInfo((int)StateType.Death,0,0,Index,Index);
            AddState(stateInfo);
        }
    }

    public void ChangeMp(int value)
    {
        _mp += value;
        if (_mp < 0){
            _mp = 0;
        }
        if (_mp > _status.Mp){
            _mp = _status.Mp;
        }
    }

    public bool IsAlive()
    {
        return _hp > 0;
    }


    public bool IsState(StateType stateType)
    {
        return _stateInfos.Find(a => a.StateId == (int)stateType) != null;
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
        int RemoveIndex = _stateInfos.FindIndex(a => a.CheckOverWriteState(stateInfo) == true);
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

    public void SetAwaken()
    {
        _isAwaken = true;
    }

    public void GainChainCount(int value)
    {
        _chainSuccessCount += value;
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
                        if ( TriggerdActionResultDeathSkillInfos(triggerDatas[j],actionInfo) )
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
                        if ( TriggerdSelfDeadSkillInfos(triggerDatas[j],actionInfo) )
                        {
                            triggeredSkills.Add(skillInfo);
                        }
                    }
                }
            }
        }
        return triggeredSkills;
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

    private bool TriggerdActionResultDeathSkillInfos(SkillsData.TriggerData triggerData,ActionInfo actionInfo)
    {
        bool IsTriggered = false;
        List<ActionResultInfo> actionResultInfos = actionInfo.actionResults;
        if (actionResultInfos.Find(a => a.IsDead && a.TargetIndex == Index) != null)
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
            if (count > 0 && (count+1) >= battlerInfos.FindAll(a => a.isActor).Count)
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
            if (count > 0 && (count+1) >= battlerInfos.FindAll(a => !a.isActor).Count)
            {
                IsTriggered = true;
            }
        } 
        return IsTriggered;
    }

    private bool TriggerdSelfDeadSkillInfos(SkillsData.TriggerData triggerData,ActionInfo actionInfo){
        bool IsTriggered = false;
        List<ActionResultInfo> actionResultInfos = actionInfo.actionResults;
        if (actionResultInfos.Find(a => a.IsDead && a.TargetIndex == Index) != null)
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
