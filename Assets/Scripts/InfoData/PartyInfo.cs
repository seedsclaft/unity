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

    private List<int> _actorIdList = new();
    public List<int> ActorIdList => _actorIdList;
    private int _currency = 0;
    public int Currency => _currency;
    private List<int> _alchemyIdList = new();
    public List<int> AlchemyIdList => _alchemyIdList;

    private int _stageId = 0;
    public int StageId => _stageId;
    private bool _battleResult = false;
    public bool BattleResult => _battleResult;

    private Dictionary<TacticsComandType,int> _commandCountInfo = new();
    private Dictionary<TacticsComandType,int> _commandRankInfo = new();
    public Dictionary<TacticsComandType,int> CommandRankInfo => _commandRankInfo;
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
        int needLvUpCount = DataSystem.System.TrainCount;
        if (commandType == TacticsComandType.Alchemy)
        {
            needLvUpCount = DataSystem.System.AlchemyCount;
        }
        if (commandType == TacticsComandType.Resource)
        {
            needLvUpCount = DataSystem.System.ResourceCount;
        }
        if (commandType == TacticsComandType.Recovery)
        {
            needLvUpCount = DataSystem.System.RecoveryCount;
        }
        if (commandType == TacticsComandType.Battle)
        {
            needLvUpCount = DataSystem.System.BattleCount;
        }
        int value = _commandCountInfo[commandType] / needLvUpCount;
        _commandCountInfo[commandType] += 1;
        if (_commandCountInfo[commandType] / needLvUpCount != value)
        {
            if ((_commandCountInfo[commandType] / needLvUpCount) > 10)
            {
                return false;
            }
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
        _commandRankInfo[commandType] = Mathf.Min(_commandRankInfo[commandType],10);
    }

    public int GetTrainLevelBonusValue()
    {
        int value = 0;
        if (_commandRankInfo.ContainsKey(TacticsComandType.Train))
        {
            int rand = Random.Range(0,100);
            if (_commandRankInfo[TacticsComandType.Train]*10 > rand)
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
            int rand = Random.Range(0,100);
            if (_commandRankInfo[TacticsComandType.Alchemy]*10 > rand)
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
            int rand = Random.Range(0,100);
            if (_commandRankInfo[TacticsComandType.Recovery]*10 > rand)
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
            int rand = Random.Range(0,100);
            if (_commandRankInfo[TacticsComandType.Resource]*10 > rand)
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
            value += _commandRankInfo[TacticsComandType.Battle] * 4;
        }
        return value;
    }
}
