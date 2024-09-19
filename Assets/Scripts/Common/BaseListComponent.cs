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
                var battlerInfo = ListItemData<BattlerInfo>();
                battlerInfoComponent.UpdateInfo(battlerInfo);
                return;
            }
            if (actorInfoComponent != null)
            {
                var actorInfo = ListItemData<ActorInfo>();
                actorInfoComponent.UpdateInfo(actorInfo,null);
                return;
            }
            if (enemyInfoComponent != null)
            {
                var enemyInfo = ListItemData<BattlerInfo>();
                enemyInfoComponent.UpdateInfo(enemyInfo);
                return;
            }
            if (skillInfoComponent != null)
            {
                var skillInfo = ListItemData<SkillInfo>();
                skillInfoComponent.UpdateInfo(skillInfo);
                return;
            }
        }
    }
}
