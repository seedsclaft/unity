using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattlePartyMemberItem : ListItem ,IListViewItem 
    {        
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private EnemyInfoComponent enemyInfoComponent;
        [SerializeField] private OnOffButton lineIndexButton = null;
        public void SetLineIndexHandler(System.Action<ActorInfo> handler)
        {
            lineIndexButton.OnClickAddListener(() => handler(GetActorInfo()));
        }

        public ActorInfo GetActorInfo()
        {
            ActorInfo actorInfo = null;
            if (ListData != null)
            {
                actorInfo = ListItemData<ActorInfo>();
            }
            return actorInfo;
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            if (battlerInfoComponent != null)
            {
                var battlerInfo = ListItemData<BattlerInfo>();
                battlerInfoComponent.UpdateInfo(battlerInfo);
                lineIndexButton.gameObject.SetActive(true);
                lineIndexButton.SetText(DataSystem.GetText(2010 + (int)battlerInfo.LineIndex));
                return;
            }
            if (actorInfoComponent != null)
            {
                var actorInfo = ListItemData<ActorInfo>();
                actorInfoComponent.UpdateInfo(actorInfo,null);
                lineIndexButton.gameObject.SetActive(true);
                lineIndexButton.SetText(DataSystem.GetText(2010 + (int)actorInfo.LineIndex));
                return;
            }
            if (enemyInfoComponent != null)
            {
                var enemyInfo = ListItemData<BattlerInfo>();
                enemyInfoComponent.UpdateInfo(enemyInfo);
                lineIndexButton.gameObject.SetActive(true);
                return;
            }
        }
    }
}
