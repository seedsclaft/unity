using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class SkillCategory : ListItem ,IListViewItem 
    {
        [SerializeField] private TextMeshProUGUI commandName;

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<SkillType>();
            commandName.text = data.ToString();
        }
    }
}
