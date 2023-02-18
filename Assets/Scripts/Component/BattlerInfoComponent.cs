using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BattlerInfoComponent : MonoBehaviour
{
    
    [SerializeField] private ActorInfoComponent actorInfoComponent;
    [SerializeField] private EnemyInfoComponent enemyInfoComponent;
    private BattlerInfo _battlerInfo = null;

    public void UpdateInfo(BattlerInfo battlerInfo)
    {
        _battlerInfo = battlerInfo;
        if (battlerInfo.isActor)
        {
            actorInfoComponent.UpdateInfo(battlerInfo.ActorInfo);
        } else
        {
            enemyInfoComponent.UpdateInfo(battlerInfo);
        }
    }

    public void StartDeathAnimation()
    {
    }

    public void ChangeHp(int value)
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.ChangeHp(value,_battlerInfo.ActorInfo.MaxHp);
        } else
        {
            enemyInfoComponent.ChangeHp(value,_battlerInfo.Status.Hp);
        }
    }

    public void ChangeMp(int value)
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.ChangeMp(value,_battlerInfo.ActorInfo.Mp);
        } else
        {
            enemyInfoComponent.ChangeMp(value,_battlerInfo.Status.Mp);
        }
    }

    public void RefreshStatus()
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.UpdateInfo(_battlerInfo.ActorInfo);
            actorInfoComponent.SetAwakeMode(_battlerInfo.IsState(StateType.Demigod));
        } else
        {
            enemyInfoComponent.UpdateInfo(_battlerInfo);
        }
        ChangeHp(_battlerInfo.Hp);
        ChangeMp(_battlerInfo.Mp);
    }

    public void HideUI()
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.HideUI();
        } else
        {
            enemyInfoComponent.HideUI();
        }
    }
}
