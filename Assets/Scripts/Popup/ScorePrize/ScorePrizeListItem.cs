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
                score?.SetText(DataSystem.GetReplaceDecimalText(data.Score) + DataSystem.GetText(113040));
                prizeName?.SetText(data.Title);
                prizeHelp?.SetText(data.Help);
                var textId = data._getFlag ? 113000 : 113010;
                getFlag?.SetText(DataSystem.GetText(textId));
            }
        }
    }
}