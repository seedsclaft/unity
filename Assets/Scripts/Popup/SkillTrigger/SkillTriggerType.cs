using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class SkillTriggerType : ListItem ,IListViewItem
    {
        [SerializeField] private TextMeshProUGUI triggerText;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<SkillTriggerData>();
            triggerText.SetText(data.Name);
        }
    }
}
