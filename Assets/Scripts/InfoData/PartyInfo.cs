using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PartyInfo 
{
    public PartyInfo()
    {
        ClearData();
    }

    public void ClearData()
    {
        _commandRankInfo[TacticsComandType.Train] = 0;
        _commandRankInfo[TacticsComandType.Alchemy] = 0;
        _commandRankInfo[TacticsComandType.Recovery] = 0;
        _commandRankInfo[TacticsComandType.Battle] = 0;
        _commandRankInfo[TacticsComandType.Resource] = 0;
    }

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

    public int GetTrainLevelBonusValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Train))
        {
            int rand = Random.Range(0,10);
            if (_commandRankInfo[TacticsComandType.Train] > rand)
            {
                value += 1;
            }
        }
        return value;
    }
    
    public bool GetAlchemyNuminosValue()
    {
        bool value = false;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Alchemy))
        {
            int rand = Random.Range(0,10);
            if (_commandRankInfo[TacticsComandType.Alchemy] > rand)
            {
                value = true;
            }
        }
        return value;
    }

    public bool GetRecoveryBonusValue()
    {
        bool value = false;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Recovery))
        {
            int rand = Random.Range(0,10);
            if (_commandRankInfo[TacticsComandType.Recovery] > rand)
            {
                value = true;
            }
        }
        return value;
    }

    public bool GetResourceBonusValue()
    {
        bool value = false;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Resource))
        {
            int rand = Random.Range(0,10);
            if (_commandRankInfo[TacticsComandType.Resource] > rand)
            {
                value = true;
            }
        }
        return value;
    }

    public int GetBattleBonusValue(int baseValue)
    {
        int value = baseValue;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Battle))
        {
            value += _commandRankInfo[TacticsComandType.Battle] * 2;
        }
        return value;
    }
}
