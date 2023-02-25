using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyInfo 
{
    private List<int> _actorIdList = new List<int>();
    private int _currency = 0;
    public int Currency {get {return _currency;}}
    private List<int> _alchemyIdList = new List<int>();
    public List<int> AlchemyIdList {get {return _alchemyIdList;}}
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

    public void ChangeCurrency(int currency)
    {
        _currency = currency;
    }
}
