using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyInfo 
{
    public List<int> _actorIdList = new List<int>();
    public Dictionary<int,int> _itemList = new Dictionary<int, int>();
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

}
