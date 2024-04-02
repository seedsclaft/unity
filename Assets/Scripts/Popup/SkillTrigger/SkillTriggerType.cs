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
            var data = (SkillTriggerData)ListData.Data;
            triggerText.SetText(data.Name);
        }
    }
}
