using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlerInfoComponent : MonoBehaviour
{
    
    [SerializeField] private ActorInfoComponent actorInfoComponent;
    [SerializeField] private EnemyInfoComponent enemyInfoComponent;
    public void UpdateInfo(BattlerInfo battlerInfo)
    {
        if (battlerInfo.isActor)
        {
            actorInfoComponent.UpdateInfo(battlerInfo.ActorInfo);
        } else
        {
            enemyInfoComponent.UpdateInfo(battlerInfo);
        }
    }
    
}
