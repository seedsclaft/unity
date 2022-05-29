using System;
using System.Collections;
using System.Collections.Generic;

public class BattlerInfo 
{
    private StatusInfo _status = null;
    public StatusInfo Status {get {return _status;}}
    private bool _isActor = false;
    private int _charaId;
    public int CharaId {get {return _charaId;}}
    private int _level;
    public int Level {get {return _level;}}
    public BattlerInfo(ActorInfo actorInfo){
        _charaId = actorInfo.ActorId;
        _level = actorInfo.Level;
        _status = actorInfo.Status;
        _isActor = true;
    }
    public BattlerInfo(EnemiesData.EnemyData enemyData,int lv){
        _charaId = enemyData.Id;
        _level = lv;
        _status = enemyData.BaseStatus;
        _isActor = false;
    }


    public bool IsActor(){
        return _isActor;
    } 
}
