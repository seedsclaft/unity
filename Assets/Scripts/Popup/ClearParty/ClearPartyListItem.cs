using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class ClearPartyListItem : ListItem ,IListViewItem  
    {   
        [SerializeField] private TextMeshProUGUI userName;
        [SerializeField] private TextMeshProUGUI totalEvaluate;
        [SerializeField] private BaseList actorInfos;
        [SerializeField] private OnOffButton replayButton;

        private bool _eventInit = false;
        
        public void SetBattleReplayHandler(System.Action<SaveBattleInfo> callEvent)
        {
            if (_eventInit) return;
            replayButton.SetCallHandler(() => 
            {
                if (replayButton.gameObject.activeSelf == false) return;
                callEvent((SaveBattleInfo)ListData.Data);
            });
            _eventInit = true;
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<SaveBattleInfo>();
            if (data != null)
            {
                userName?.SetText(data.UserName);
                var totalScore = data.Party.TotalEvaluate();
                totalEvaluate?.SetText(totalScore.ToString());
                
                replayButton.SetText("再生");
                actorInfos.Initialize();
                actorInfos.SetData(ListData.MakeListData(data.Party.BattlerInfos));
            }
        }
    }
}