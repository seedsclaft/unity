using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class MainMenuStage : ListItem ,IListViewItem  
    {   
        [SerializeField] private StageInfoComponent component;
        [SerializeField] private Button rankingButton;
        private bool IsInit = false;
        
        public void SetRankingDetailHandler(System.Action<int> callEvent)
        {
            if (IsInit) return;
            rankingButton.onClick.AddListener(() => 
            {
                if (ListData == null) return;
                var data = (StageInfo)ListData.Data;
                callEvent(data.Id);
            });
            IsInit = true;
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<StageInfo>();
            component.UpdateInfo(data);
            if (rankingButton != null)
            {
                rankingButton.gameObject.SetActive(data.Master.RankingStage > 0);
            }
            var rect = gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x,data.Master.StageRect);
        }
    }
}