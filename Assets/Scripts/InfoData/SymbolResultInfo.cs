using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SymbolResultInfo
{
    private int _stageId;
    public int StageId => _stageId;
    private int _seek;
    public int Seek => _seek;
    private int _seekIndex;
    public int SeekIndex => _seekIndex;

    public int _currency;
    public int Currency => _currency;
    public bool _selected;
    public bool Selected => _selected;
    public void SetSelected(bool isSelected)
    {
        _selected = isSelected;
    }
    public int _battleScore;
    public int BattleScore => _battleScore;
    public void SetBattleScore(int battleScore)
    {
        _battleScore = battleScore;
    }
    private List<int> _alchemyIdList = new();
    public List<int> AlchemyIdList => _alchemyIdList;
    public void AddAlchemyId(int alchemyId)
    {
        if (!_alchemyIdList.Contains(alchemyId))
        {
            _alchemyIdList.Add(alchemyId);
        }
    }
    public void RemoveAlchemyId(int alchemyId)
    {
        if (_alchemyIdList.Contains(alchemyId))
        {
            _alchemyIdList.Remove(alchemyId);
        }
    }
    public void SetAlchemyIdList(List<int> alchemyIdList)
    {
        _alchemyIdList.Clear();
        foreach (var alchemyId in alchemyIdList)
        {
            _alchemyIdList.Add(alchemyId);
        }
    }

    public List<int> _actorIdList = new ();
    public List<int> ActorIdList => _actorIdList;
    public void SetActorIdList(List<int> actorIdList)
    {
        _actorIdList = actorIdList;
    }
    public void AddActorId(int actorId)
    {
        if (!_actorIdList.Contains(actorId))
        {
            _actorIdList.Add(actorId);
        }
    }
    public void RemoveActorId(int actorId)
    {
        if (_actorIdList.Contains(actorId))
        {
            _actorIdList.Remove(actorId);
        }
    }

    public List<ActorInfo> _actorInfos = new ();
    public List<ActorInfo> ActorInfos => _actorInfos;
    public void SetActorInfos(List<ActorInfo> actorInfos)
    {
        foreach (var actorInfo in actorInfos)
        {
            var recordActorInfo = new ActorInfo(actorInfo.Master);
            recordActorInfo.CopyData(actorInfo);		
            _actorInfos.Add(recordActorInfo);
        }
    }

    public SymbolResultInfo(int stageId,int seek,int seekIndex,int currency)
    {
        _stageId = stageId;
        _seek = seek;
        _seekIndex = seekIndex;
        _currency = currency;
        _selected = false;
    }


    public bool IsSameSymbol(SymbolResultInfo symbolResultInfo)
    {
        return symbolResultInfo._stageId == _stageId && symbolResultInfo._seek == _seek && symbolResultInfo._seekIndex == _seekIndex;
    }

    public bool IsSameSymbol(int stageId,int seek,int seekIndex)
    {
        return _stageId == stageId && _seek == seek && _seekIndex == seekIndex;
    }
}
