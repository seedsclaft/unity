using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class ScorePrizeListItem : ListItem ,IListViewItem  
    {   
        [SerializeField] private TextMeshProUGUI prizeName;
        [SerializeField] private TextMeshProUGUI prizeHelp;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private TextMeshProUGUI getFlag;
        
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (ScorePrizeInfo)ListData.Data;
            if (data != null)
            {
                score?.SetText(DataSystem.GetReplaceDecimalText(data.Score) + "人");
                prizeName?.SetText(data.Title);
                prizeHelp?.SetText(data.Help);
                if (data._getFlag)
                {
                    getFlag?.SetText(DataSystem.GetText(14190));
                } else
                {
                    getFlag?.SetText(DataSystem.GetText(14191));
                }
            }
        }
    }
}