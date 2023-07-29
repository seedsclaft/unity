[System.Serializable]
public class StateInfo {
    public StatesData.StateData Master {
        get {return DataSystem.States.Find(a => a.Id == _stateId);}
    }
    private int _stateId = 0;
    public int StateId => _stateId;
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
    public StateInfo(int stateId,int turns,int effect,int battlerId,int targetIndex,int skillId){
        _stateId = stateId;
        _turns = turns;
        _baseTurns = turns;
        _effect = effect;
        _battlerId = battlerId;
        _targetIndex = targetIndex;
        _skillId = skillId;
    }

    public bool CheckOverWriteState(StateInfo stateInfo)
    {
        // 重複可能スキルはfalse
        if (CanOverWrite())
        {
            return false;
        }
        if (stateInfo.StateId == (int)StateType.Death)
        {
            return (stateInfo.StateId == _stateId);
        }
        if (stateInfo.Master.OverWrite)
        {
            return (stateInfo.StateId == _stateId) && (stateInfo._skillId == _skillId);
        }
        return (stateInfo.StateId == _stateId);
    }

    public bool UpdateTurn()
    {
        // 挑発は確率でターンを終える
        if ((int)StateType.Substitute == Master.Id)
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

    public bool IsBarrierStateType()
    {
        return _stateId == (int)StateType.Stun || _stateId == (int)StateType.Slow || _stateId == (int)StateType.Curse || _stateId == (int)StateType.SlipDamage;
    }

    public bool CanOverWrite()
    {
        return _stateId == (int)StateType.DamageUp;
    }

    public bool RemovalBuffState()
    {
        if (_stateId >= (int)StateType.Barrier && _stateId <= (int)StateType.TargetRateDown)
        {
            return true;
        } else
        if (_stateId >= (int)StateType.ChainDamageUp && _stateId <= (int)StateType.HealActionSelfHeal)
        {
            return true;
        } else
        if (_stateId >= (int)StateType.AbsoluteHit)
        {
            return true;
        }
        return false;
    }
}
