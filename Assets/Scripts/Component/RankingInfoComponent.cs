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
        [SerializeField] private TextMeshProUGUI rankingTypeText = null; 

        private bool _isInit = false;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<RankingInfo>();
            playerName.text = data.Name;
            score.SetText(data.Score.ToString());
            rank.SetText(data.Rank.ToString() + DataSystem.GetText(23030));
            //rankingTypeText.text = data.RankingTypeText;
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
            if (_isInit == false)
            {
                if (detailButton != null)
                {
                    detailButton.onClick.AddListener(() => data.DetailEvent(Index));
                }
            }
            _isInit = true;
        }
        
        private void UpdateMemberItem(BattlePartyMemberItem memberItem, ActorInfo actorInfo)
        {
            if (memberItem != null) 
            {
                memberItem.SetListData(new ListData(actorInfo),0);
            }
        }
    }
}