using System;
using System.Collections;
using System.Collections.Generic;

public class BattlerInfo 
{
    private StatusInfo _status = null;
    public StatusInfo Status {get {return _status;}}
    private int _index = 0;
    public int Index{get {return _index;}}
    private bool _isActor = false;
    public bool isActor{get {return _isActor;}}
    private int _charaId;
    public int CharaId {get {return _charaId;}}
    private int _level;
    public int Level {get {return _level;}}
    private int _ap;
    public int Ap {get {return _ap;}}
    public BattlerInfo(ActorInfo actorInfo,int index){
        _charaId = actorInfo.ActorId;
        _level = actorInfo.Level;
        _status = actorInfo.Status;
        _index = index;
        _isActor = true;
        ResetAp();
    }
    public BattlerInfo(EnemiesData.EnemyData enemyData,int lv,int index){
        _charaId = enemyData.Id;
        _level = lv;
        _status = enemyData.BaseStatus;
        _index = index;
        _isActor = false;
        ResetAp();
    }


    public bool IsActor()
    {
        return _isActor;
    }

    public void ResetAp()
    {
        _ap = 400 - Status.Spd * 4;
    }
}
