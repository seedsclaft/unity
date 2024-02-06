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
    private int _battleResultScore = 0;
    public int BattleResultScore => _battleResultScore;

    private List<SymbolResultInfo> _symbolRecordList = new ();
    public List<SymbolResultInfo> SymbolRecordList => _symbolRecordList;
    public void SetSymbolResultInfo(SymbolResultInfo symbolResultInfo)
    {
        var findIndex = _symbolRecordList.FindIndex(a => a.IsSameSymbol(symbolResultInfo));
        if (findIndex < 0)
        {
        } else{
            _symbolRecordList.RemoveAt(findIndex);
        }
        _symbolRecordList.Add(symbolResultInfo);
        _symbolRecordList.Sort((a,b) => a.Seek - b.Seek > 0 ? 1 : -1);
    }

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

    public void InitActorIds()
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

    public void SetBattleScore(int score)
    {
        _battleResultScore = score;
    }

    public void AddAlchemy(int skillId)
    {
        _alchemyIdList.Add(skillId);
    }

    public void RemoveAlchemy(int skillId)
    {
        var findIndex = _alchemyIdList.FindIndex(a => a == skillId);
        if (findIndex > -1)
        {
            _alchemyIdList.RemoveAt(findIndex);
        }
    }


    public void ClearAlchemy()
    {
        _alchemyIdList.Clear();
    }
}
