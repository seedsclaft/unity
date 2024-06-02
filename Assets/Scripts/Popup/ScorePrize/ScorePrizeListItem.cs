using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class ScorePrizeListItem : ListItem ,IListViewItem  
    {   
        [SerializeField] private TextMeshProUGUI prizeName;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private GameObject getFlag;
        [SerializeField] private SideMenuButton getItemButton;
        
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (ScorePrizeInfo)ListData.Data;
            if (data != null)
            {
                score?.SetText(data.Score.ToString());
                prizeName?.SetText(data.ConditionName);
                getFlag.SetActive(data._getFlag);
                
                //getItemButton.SetData("",0);
                getItemButton.UpdateViewItem();
                getItemButton.SetCallHandler((a) => 
                {
                    if (getItemButton.gameObject.activeSelf == false) return;
                });
                getItemButton.Cursor.SetActive(ListData.Enable);
            }
        }
    }
}