using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BaseListComponent : ListItem ,IListViewItem 
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private EnemyInfoComponent enemyInfoComponent;
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            if (battlerInfoComponent != null)
            {
                var battlerInfo = (BattlerInfo)ListData.Data;
                battlerInfoComponent.UpdateInfo(battlerInfo);
                return;
            }
            if (actorInfoComponent != null)
            {
                var actorInfo = (ActorInfo)ListData.Data;
                actorInfoComponent.UpdateInfo(actorInfo,null);
                return;
            }
            if (enemyInfoComponent != null)
            {
                var enemyInfo = (BattlerInfo)ListData.Data;
                enemyInfoComponent.UpdateInfo(enemyInfo);
                return;
            }
            if (skillInfoComponent != null)
            {
                var skillInfo = (SkillInfo)ListData.Data;
                skillInfoComponent.UpdateInfo(skillInfo);
                return;
            }
        }
    }
}
