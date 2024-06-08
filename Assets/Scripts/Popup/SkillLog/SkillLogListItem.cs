using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class SkillLogListItem : ListItem ,IListViewItem  
    {   
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (SkillLogListInfo)ListData.Data;
            battlerInfoComponent.UpdateInfo(data.battlerInfo);
            skillInfoComponent.UpdateInfo(data.skillInfo);
        }
    }

    public class SkillLogListInfo
    {
        public BattlerInfo battlerInfo;
        public SkillInfo skillInfo;
    }
}
