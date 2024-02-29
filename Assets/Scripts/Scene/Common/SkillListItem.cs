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
        [SerializeField] private SkillInfoComponent skillInfoComponent2;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (SkillInfo)ListData.Data;
            skillInfoComponent1.UpdateSkillInfo(data);
            skillInfoComponent2.UpdateSkillInfo(data);
        }
    }
}