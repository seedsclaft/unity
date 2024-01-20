[System.Serializable]
public class StateInfo {
    public StateData Master {
        get {return DataSystem.States.Find(a => a.StateType == _stateType);}
    }
    private StateType _stateType = 0;
    public StateType StateType => _stateType;
    private int _turns = 0;
    public int Turns{ get {return _turns;} set {_turns = value;}}
    private int _baseTurns = 0;
    public int BaseTurns => _baseTurns;
    private int _effect = 0;
    public int Effect{ get {return _effect;} set {_effect = value;} }
    private int _battlerId = 0;
    public int BattlerId => _battlerId;
    private int _targetIndex = 0;
    public int TargetIndex => _targetIndex;
    private int _skillId = 0;
    public int SkillId => _skillId;
    private RemovalTiming _removeTiming = 0;
    public RemovalTiming RemovalTiming => _removeTiming;
    public void SetRemoveTiming(RemovalTiming removalTiming)
    {
        _removeTiming = removalTiming;
    }

    public bool IsStartPassive()
    {
        var skillData = DataSystem.FindSkill(_skillId);
        if (skillData != null)
        {
            return skillData.FeatureDates.Find(a => a.FeatureType == FeatureType.AddState) != null && skillData.SkillType == SkillType.Passive;
        }
        return false;
    }

    public StateInfo(StateType stateType,int turns,int effect,int battlerId,int targetIndex,int skillId){
        _stateType = stateType;
        _turns = turns;
        _baseTurns = turns;
        _effect = effect;
        _battlerId = battlerId;
        _targetIndex = targetIndex;
        _skillId = skillId;
        _removeTiming = Master.RemovalTiming;
    }

    public bool CheckOverWriteState(StateInfo stateInfo)
    {
        // 重複可能スキルはfalse
        if (stateInfo.Master.OverLap)
        {
            return false;
        }
        if (stateInfo.StateType == StateType.Death)
        {
            return (stateInfo.StateType == _stateType);
        }
        if (stateInfo.Master.OverWrite)
        {
            return (stateInfo.StateType == _stateType) && (stateInfo._skillId == _skillId);
        }
        return (stateInfo.StateType == _stateType);
    }

    public bool UpdateTurn()
    {
        // 挑発は確率でターンを終える
        if (StateType.Substitute == Master.StateType)
        {
            var per = 100 - (_turns * _effect);
            var rand = UnityEngine.Random.Range(0,100);
            if (rand > per)
            {
                return true;
            }
            _turns--;
            return false;
        }
        _turns--;
        if (_turns <= 0)
        {
            return true;
        }
        return false;
    }

    public void ResetTurns()
    {
        _turns = _baseTurns;
    }

    public bool CheckSameStateType(StateInfo otherStateInfo)
    {
        return _stateType == otherStateInfo.StateType && _skillId == otherStateInfo.SkillId && _targetIndex == otherStateInfo.TargetIndex;
    }
}
