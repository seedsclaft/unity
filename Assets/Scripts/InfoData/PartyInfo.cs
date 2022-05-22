using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyInfo 
{
    public List<int> _actorIdList = new List<int>();
    public Dictionary<int,int> _itemList = new Dictionary<int, int>();
    public void AddActor(int actorId)
    {
        _actorIdList.Add(actorId);
    }

}
