using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseModel
{
    private SavePlayInfo _playInfo;
    public SavePlayInfo PlayInfo{get{return _playInfo;} set{_playInfo = value;}}

    public StatusInfo LevelUpActor(int actorId)
    {
        var actorData = DataSystem.Actors.Find(x => x.Id == actorId);
        var classData = DataSystem.Classes.Find(x => x.Id == actorData.ClassId);

        var actorInfo = _playInfo._playerData._actors.Find(x => x.ActorId == actorId);
        return actorInfo.LevelUp(actorData.GrowthRateStatus,classData.BaseStatus,classData.MaxStatus);
    }
}
