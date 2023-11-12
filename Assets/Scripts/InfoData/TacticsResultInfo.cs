using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TacticsResultInfo 
{
    private int _actorId;
    public int ActorId => _actorId;
    private TacticsCommandType _commandType;
    public TacticsCommandType CommandType => _commandType;
    private bool _isBonus;
    public bool IsBonus => _isBonus;
    public TacticsResultInfo(int actorId,TacticsCommandType commandType,bool isBonus)
    {
        _actorId = actorId;
        _commandType = commandType;
        _isBonus = isBonus;
    }
}
