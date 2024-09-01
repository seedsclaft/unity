using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class GetItem : ListItem ,IListViewItem  
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private TextMeshProUGUI titleName;

        public void UpdateViewItem()
        {
            if (ListData == null) 
            {
                return;
            }
            var data = (GetItemInfo)ListData.Data;
            if (data == null) return;
            if (skillInfoComponent != null)
            {
                var skillId = data.SkillId;
                if (data.GetItemType == GetItemType.SelectRelic)
                {
                    skillId = 0;
                }
                skillInfoComponent.UpdateData(skillId);
            }
            if (titleName != null)
            {
                titleName.gameObject.SetActive(data.GetTitleData() != "");
                titleName.text = data.GetTitleData();
            }
            if (Disable != null)
            {
                Disable?.gameObject.SetActive(ListData.Enable == false || data.GetFlag);
            }
        }
    }
}