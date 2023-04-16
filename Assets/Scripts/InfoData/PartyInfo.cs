using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PartyInfo 
{
    private List<int> _actorIdList = new List<int>();
    public List<int> ActorIdList {
        get {return _actorIdList;}
    }
    private int _currency = 0;
    public int Currency {get {return _currency;}}
    private List<int> _alchemyIdList = new List<int>();
    public List<int> AlchemyIdList {get {return _alchemyIdList;}}

    private int _stageId = 0;
    public int StageId {get {return _stageId;}}

    private bool _battleResult = false;
    public bool BattleResult {get {return _battleResult;}}

    private Dictionary<TacticsComandType,int> _commandCountInfo = new Dictionary<TacticsComandType, int>();
    private Dictionary<TacticsComandType,int> _commandRankInfo = new Dictionary<TacticsComandType, int>();
    public Dictionary<TacticsComandType,int> CommandRankInfo { get {return _commandRankInfo;}}
    public void AddActor(int actorId)
    {
        if (_actorIdList.IndexOf(actorId) != -1)
        {
            return;
        }
        _actorIdList.Add(actorId);
    }

    public void RemoveActor(int actorId)
    {
        if (_actorIdList.IndexOf(actorId) == -1)
        {
            return;
        }
        _actorIdList.Remove(actorId);
    }

    public void InitActors()
    {
        _actorIdList.Clear();
    }

    public void ChangeCurrency(int currency)
    {
        _currency = currency;
    }

    public void SetStageId(int stageId)
    {
        _stageId = stageId;
    }

    public void SetBattleResult(bool isVictory)
    {
        _battleResult = isVictory;
    }

    public void AddAlchemy(int skillId)
    {
        if (_alchemyIdList.Contains(skillId))
        {
            return;
        }
        _alchemyIdList.Add(skillId);
    }

    public bool AddCommandCountInfo(TacticsComandType commandType)
    {
        if (_commandCountInfo.ContainsKey(commandType) == false)
        {
            _commandCountInfo[commandType] = 0;
        }
        int value = _commandCountInfo[commandType] / 4;
        _commandCountInfo[commandType] += 1;
        if (_commandCountInfo[commandType] / 4 != value)
        {
            return true;
        }
        return false;
    }
    
    public void AddCommandRank(TacticsComandType commandType)
    {
        if (_commandRankInfo.ContainsKey(commandType) == false)
        {
            _commandRankInfo[commandType] = 0;
        }
        _commandRankInfo[commandType] += 1;
    }

    public int GetTrainNuminosValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Train))
        {
            value -= (_commandRankInfo[TacticsComandType.Train]+2) / 2;
        }
        return value;
    }

    public int GetTrainLevelBonusValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Train))
        {
            value += (_commandRankInfo[TacticsComandType.Train]+1) / 2;
        }
        return value;
    }
    
    public int GetAlchemyNuminosValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Alchemy))
        {
            value += _commandRankInfo[TacticsComandType.Alchemy];
        }
        return value;
    }

    public int GetRecoveryBonusValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Recovery))
        {
            value += _commandRankInfo[TacticsComandType.Recovery] * 2;
        }
        return value;
    }

    public int GetResourceBonusValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Resource))
        {
            value += _commandRankInfo[TacticsComandType.Resource];
        }
        return value;
    }

    public int GetBattleBonusValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Battle))
        {
            value += _commandRankInfo[TacticsComandType.Battle];
        }
        return value;
    }
}
