using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class ClearPartyListItem : ListItem ,IListViewItem  
    {   
        [SerializeField] private TextMeshProUGUI userName;
        [SerializeField] private TextMeshProUGUI totalEvaluate;
        [SerializeField] private BaseList actorInfos;
        [SerializeField] private SideMenuButton replayButton;

        private bool _eventInit = false;
        
        public void SetBattleReplayHandler(System.Action<SaveBattleInfo> callEvent)
        {
            if (_eventInit) return;
            replayButton.SetCallHandler((a) => 
            {
                if (replayButton.gameObject.activeSelf == false) return;
                callEvent((SaveBattleInfo)ListData.Data);
            });
            _eventInit = true;
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (SaveBattleInfo)ListData.Data;
            if (data != null)
            {
                userName?.SetText("test");
                var totalScore = data.Party.TotalEvaluate();
                totalEvaluate?.SetText(totalScore.ToString());
                
                //getItemButton.SetData("",0);
                replayButton.SetText("再生");
                actorInfos.SetData(ListData.MakeListData(data.Party.BattlerInfos));
                replayButton.Cursor.SetActive(ListData.Enable);
            }
        }
    }
}