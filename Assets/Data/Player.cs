using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player 
{
    public List< ActorInfo > _actors = new List<ActorInfo>();
    public void InitActors()
    {
        _actors.Clear();
    }
    public void AddActor(ActorInfo actor)
    {
        _actors.Add(actor);
    }
}
