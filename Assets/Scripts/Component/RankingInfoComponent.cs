using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class RankingInfoComponent : ListItem ,IListViewItem 
    {   
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TextMeshProUGUI rank;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private List<BattlePartyMemberItem> memberItems;
        [SerializeField] private Button detailButton;

        private bool _isInit = false;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<RankingInfo>();
            playerName.text = data.Name;
            score.SetText(data.Score.ToString());
            rank.SetText(DataSystem.GetReplaceText(23030, data.Rank.ToString()));
            for (int i = 0;i < memberItems.Count;i++)
            {
                if (data.ActorInfos.Count > i)
                {
                    memberItems[i].gameObject.SetActive(true);
                    UpdateMemberItem(memberItems[i],data.ActorInfos[i]);
                } else
                {
                    memberItems[i].gameObject.SetActive(false);
                }
            }
        }
        
        private void UpdateMemberItem(BattlePartyMemberItem memberItem, ActorInfo actorInfo)
        {
            if (memberItem != null) 
            {
                memberItem.SetListData(new ListData(actorInfo),0);
                memberItem.UpdateViewItem();
            }
        }

        public void SetDetailActor(System.Action<List<ActorInfo>> detail)
        {
            if (_isInit == false)
            {
                _isInit = true;
                if (detailButton != null)
                {
                    detailButton.onClick.AddListener(() => 
                    {
                        if (ListData == null) return;
                        var data = ListItemData<RankingInfo>();
                        detail(data.ActorInfos);
                    });
                }
            }
        }
    }
}