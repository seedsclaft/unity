using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PartyInfo 
{
    private List<int> _actorIdList = new List<int>();
    private int _currency = 0;
    public int Currency {get {return _currency;}}
    private List<int> _alchemyIdList = new List<int>();
    public List<int> AlchemyIdList {get {return _alchemyIdList;}}
    private int _stageId = 0;
    public int StageId {get {return _stageId;}}

    public void AddActor(int actorId)
    {
        if (_actorIdList.IndexOf(actorId) == -1)
        {
            return;
        }
        _actorIdList.Add(actorId);
    }

    public void RemoveActor(int actorId)
    {
        if (_actorIdList.IndexOf(actorId) != -1)
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
}
