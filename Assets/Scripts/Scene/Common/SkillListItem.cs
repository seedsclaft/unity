using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillListItem : ListItem ,IListViewItem  
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent1;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<SkillInfo>();
            skillInfoComponent1.UpdateInfo(data);
        }
    }
}