﻿using System;
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
    private int _hp;
    public int Hp {get {return _hp;}}
    private int _mp;
    public int Mp {get {return _mp;}}
    private int _ap;
    public int Ap {get {return _ap;}}
    
    private List<SkillInfo> _skills;
    public List<SkillInfo> Skills {get {return _skills;}}
    private ActorInfo _actorInfo;
    public ActorInfo ActorInfo {get {return _actorInfo;} }
    private EnemiesData.EnemyData _enemyData;
    public EnemiesData.EnemyData EnemyData {get {return _enemyData;} }

    private int _lastSkillId = 0;
    public SkillInfo LastSkill {get {return Skills.Find(a => a.Id == _lastSkillId);} }

    private int _lastTargetIndex = 0;
    public void SetLastTargetIndex(int index){
        _lastTargetIndex = index;
    }
    public BattlerInfo(ActorInfo actorInfo,int index){
        _charaId = actorInfo.ActorId;
        _level = actorInfo.Level;
        StatusInfo statusInfo = new StatusInfo();
        statusInfo.SetParameter(actorInfo.Status.Hp,actorInfo.Status.Mp,actorInfo.Status.Atk,actorInfo.Status.Def,actorInfo.Status.Spd);
        _status = statusInfo;
        _index = index;
        _skills = actorInfo.Skills;
        _isActor = true;
        
        _actorInfo = actorInfo;
        _hp = actorInfo.Hp;
        _mp = actorInfo.Mp;
        ResetAp();
    }

    public BattlerInfo(EnemiesData.EnemyData enemyData,int lv,int index){
        _charaId = enemyData.Id;
        _level = lv;
        StatusInfo statusInfo = new StatusInfo();
        statusInfo.SetParameter(enemyData.BaseStatus.Hp,enemyData.BaseStatus.Mp,enemyData.BaseStatus.Atk,enemyData.BaseStatus.Def,enemyData.BaseStatus.Spd);
        _status = statusInfo;
        _index = index;
        _isActor = false;
        _enemyData = enemyData;
        _hp = _status.Hp;
        _mp = _status.Mp;
        _skills = new List<SkillInfo>();
        _skills.Add(new SkillInfo(1));
        ResetAp();
    }


    public bool IsActor()
    {
        return _isActor;
    }

    public void ResetAp()
    {
        int rand = new Random().Next(-10, 10);
        _ap = 400 - (Status.Spd + rand) * 4;
    }

    public void GainAp(int ap)
    {
        _ap += ap;
    }

    public void UpdateAp()
    {
        if (isActor == true) return;
        _ap -= 4;
    }

    public int LastTargetIndex()
    {
        if (isActor){
            return _lastTargetIndex;
        }
        return -1;
    }

    public void ChangeHp(int value)
    {
        _hp += value;
    }

    public void ChangeMp(int value)
    {
        _mp += value;
    }

    public bool IsAlive()
    {
        return _hp > 0;
    }
}
