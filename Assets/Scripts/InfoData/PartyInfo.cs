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
        _commandCountInfo = new();
        _commandRankInfo[TacticsCommandType.Train] = 0;
        _commandRankInfo[TacticsCommandType.Alchemy] = 0;
        _commandRankInfo[TacticsCommandType.Recovery] = 0;
    
        _alchemyIdList = new();
        _battleResultVictory = false;
    }

    private List<int> _actorIdList = new();
    public List<int> ActorIdList => _actorIdList;
    private int _currency = 0;
    public int Currency => _currency;
    private List<int> _alchemyIdList = new();
    public List<int> AlchemyIdList => _alchemyIdList;

    private bool _battleResultVictory = false;
    public bool BattleResultVictory => _battleResultVictory;

    private Dictionary<TacticsCommandType,int> _commandCountInfo = new();
    private Dictionary<TacticsCommandType,int> _commandRankInfo = new();
    public Dictionary<TacticsCommandType,int> CommandRankInfo => _commandRankInfo;
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

    public void SetBattleResultVictory(bool isVictory)
    {
        _battleResultVictory = isVictory;
    }

    public void AddAlchemy(int skillId)
    {
        if (_alchemyIdList.Contains(skillId))
        {
            return;
        }
        _alchemyIdList.Add(skillId);
    }

    public bool AddCommandCountInfo(TacticsCommandType commandType)
    {
        if (_commandCountInfo.ContainsKey(commandType) == false)
        {
            _commandCountInfo[commandType] = 0;
        }
        int needLvUpCount = DataSystem.System.TrainCount;
        if (commandType == TacticsCommandType.Alchemy)
        {
            needLvUpCount = DataSystem.System.AlchemyCount;
        }
        if (commandType == TacticsCommandType.Recovery)
        {
            needLvUpCount = DataSystem.System.RecoveryCount;
        }
        int value = _commandCountInfo[commandType] / needLvUpCount;
        _commandCountInfo[commandType] += 1;
        if (_commandCountInfo[commandType] / needLvUpCount != value)
        {
            if ((_commandCountInfo[commandType] / needLvUpCount) > 10)
            {
                return false;
            }
            if (_commandRankInfo[commandType] >= 10)
            {
                return false;
            }
            return true;
        }
        return false;
    }
    
    public void AddCommandRank(TacticsCommandType commandType)
    {
        if (_commandRankInfo.ContainsKey(commandType) == false)
        {
            _commandRankInfo[commandType] = 0;
        }
        _commandRankInfo[commandType] += 1;
        _commandRankInfo[commandType] = Mathf.Min(_commandRankInfo[commandType],10);
    }

    public int GetTrainLevelBonusValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsCommandType.Train))
        {
            int rand = Random.Range(0,100);
            if (_commandRankInfo[TacticsCommandType.Train]*10 > rand)
            {
                value += 1;
            }
        }
        return value;
    }
    
    public bool GetAlchemyBonusValue()
    {
        bool value = false;
        if (_commandRankInfo.ContainsKey(TacticsCommandType.Alchemy))
        {
            int rand = Random.Range(0,100);
            if (_commandRankInfo[TacticsCommandType.Alchemy]*10 > rand)
            {
                value = true;
            }
        }
        return value;
    }

    public bool GetRecoveryBonusValue()
    {
        bool value = false;
        if (_commandRankInfo.ContainsKey(TacticsCommandType.Recovery))
        {
            int rand = Random.Range(0,100);
            if (_commandRankInfo[TacticsCommandType.Recovery]*10 > rand)
            {
                value = true;
            }
        }
        return value;
    }

    public bool GetResourceBonusValue()
    {
        bool value = false;
        return value;
    }

    public int GetBattleBonusValue(int baseValue)
    {
        int value = baseValue;
        return value;
    }
}
