using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateInfo {
    public StatesData.StateData Master {
        get {return DataSystem.States.Find(a => a.Id == _stateId);}
    }
    private int _stateId = 0;
    public int StateId{ get {return _stateId;}}
    private int _turns = 0;
    public int Turns{ get {return _turns;} set {_turns = value;}}
    private int _baseTurns = 0;
    public int BaseTurns{ get {return _baseTurns;}}
    private int _effect = 0;
    public int Effect{ get {return _effect;} set {_effect = value;} }
    private int _battlerId = 0;
    public int BattlerId{ get {return _battlerId;}}
    private int _targetIndex = 0;
    public int TargetIndex{ get {return _targetIndex;}}
    public StateInfo(int stateId,int turns,int effect,int battlerId,int targetIndex){
        _stateId = stateId;
        _turns = turns;
        _baseTurns = turns;
        _effect = effect;
        _battlerId = battlerId;
        _targetIndex = targetIndex;
    }

    public bool CheckOverWriteState(StateInfo stateInfo)
    {
        if (stateInfo.StateId == (int)StateType.Death)
        {
            return (stateInfo.StateId == _stateId);
        }
        return (stateInfo.StateId == _stateId) && (stateInfo.BattlerId == _battlerId);
    }

    public bool UpdateTurn()
    {
        _turns--;
        if (_turns < 0)
        {
            return true;
        }
        return false;
    }

    public void ResetTurns()
    {
        _turns = _baseTurns;
    }
}
