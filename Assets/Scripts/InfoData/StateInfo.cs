using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateInfo {
    public StatesData.StateData Master {
        get {return DataSystem.States.Find(a => a.Id == _stateId);}
    }
    private int _stateId = 0;
    public int StateId{ get {return _stateId;}}
    private int _turns = 0;
    public int Tunrs{ get {return _turns;}}
    private int _effect = 0;
    public int Effect{ get {return _effect;}}
    private int _battlerId = 0;
    public int BattlerId{ get {return _battlerId;}}
    public StateInfo(int stateId,int turns,int effect,int battlerId){
        _stateId = stateId;
        _turns = turns;
        _effect = effect;
        _battlerId = battlerId;
    }

    public bool CheckOverWriteState(StateInfo stateInfo)
    {
        return (stateInfo.StateId != _stateId) && (stateInfo.BattlerId != _battlerId);
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
}
