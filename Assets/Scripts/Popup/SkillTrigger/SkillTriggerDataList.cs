using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class SkillTriggerDataList : BaseList
    {
        public new void Initialize()
        {
            base.Initialize();
            SetSelectedHandler(() => {
                UpdateHelpText();
            });
        }

        public void UpdateHelpText()
        {
            var listData = ListData;
            if (listData != null)
            {
                var triggerData = (SkillTriggerData)listData.Data;
                _helpWindow.SetHelpText(triggerData.Help);
            }
        }
    }
}
