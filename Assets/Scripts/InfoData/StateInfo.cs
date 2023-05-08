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
    private bool _isPassive = false;
    private List<SkillsData.TriggerData> _triggerDatas = null;
    public StateInfo(int stateId,int turns,int effect,int battlerId,int targetIndex,bool isPassive){
        _stateId = stateId;
        _turns = turns;
        _baseTurns = turns;
        _effect = effect;
        _battlerId = battlerId;
        _targetIndex = targetIndex;
        _isPassive = isPassive;
    }

    public void SetPassiveTrigger(List<SkillsData.TriggerData> triggerDatas)
    {
        _triggerDatas = new List<SkillsData.TriggerData>();
        foreach (var triggerData in triggerDatas)
        {
            _triggerDatas.Add(triggerData);
        }
    }

    public bool IsCanPassiveTriggered(BattlerInfo battlerInfo,UnitInfo party,UnitInfo troops)
    {
        if (_isPassive == false) return true;
        if (_triggerDatas == null) return true;
        if (_triggerDatas.Count == 0) return true;
        bool CanUse = true;
        foreach (var triggerData in _triggerDatas)
        {
            if (triggerData.CanUseTrigger(battlerInfo,party,troops) == false)
            {
                CanUse = false;
            }
        }
        return CanUse;
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
}
