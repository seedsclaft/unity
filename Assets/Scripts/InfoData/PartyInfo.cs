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

    private Dictionary<int,int> _alchemyHintList = new Dictionary<int, int>();
    public Dictionary<int, int> AlchemyHintList {get {return _alchemyHintList;}}
    private int _stageId = 0;
    public int StageId {get {return _stageId;}}

    private bool _battleResult = false;
    public bool BattleResult {get {return _battleResult;}}

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
        if (_alchemyHintList.ContainsKey(skillId) == false)
        {
            _alchemyHintList[skillId] = 0;
        }
        _alchemyIdList.Add(skillId);
    }

    public int SkillHintLevel(int skillId)
    {
        if (_alchemyHintList.ContainsKey(skillId) == false) return 0;
        return _alchemyHintList[skillId];
    }

    public void PlusSkillHintLv(int skillId)
    {
        _alchemyHintList[skillId] += 1;
    }
}
