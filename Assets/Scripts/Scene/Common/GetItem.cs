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
            var data = ListItemData<GetItemInfo>();
            if (data == null) return;
            if (skillInfoComponent != null)
            {
                var skillId = 0;
                if (data.IsSkill())
                {
                    skillId = data.Param1;
                }
                skillInfoComponent.UpdateData(skillId);
            }
            if (titleName != null)
            {
                var title = data.GetTitleData();
                titleName.gameObject.SetActive(title != "");
                titleName.SetText(title);
            }
            if (Disable != null)
            {
                Disable?.gameObject.SetActive(ListData.Enable == false);
            }
        }
    }
}