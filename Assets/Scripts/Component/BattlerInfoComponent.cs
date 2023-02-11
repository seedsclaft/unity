using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlerInfoComponent : MonoBehaviour
{
    
    [SerializeField] private ActorInfoComponent actorInfoComponent;
    public void UpdateInfo(BattlerInfo battlerInfo)
    {
        if (battlerInfo.isActor)
        {
            actorInfoComponent.UpdateInfo(battlerInfo.ActorInfo);
        }
    }
}
