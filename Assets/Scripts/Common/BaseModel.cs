using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModel
{
    public SavePlayInfo CurrentData{get {return GameSystem.CurrentData;}}
    public TempInfo CurrentTempData{get {return GameSystem.CurrentTempData;}}

    public StatusInfo LevelUpActor(int actorId)
    {
        var actorData = DataSystem.Actors.Find(x => x.Id == actorId);

        //var actorInfo = _playInfo._playerData._actors.Find(x => x.ActorId == actorId);
        return actorData.LevelUpStatus(1);
    }
}
