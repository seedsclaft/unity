using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillTriggerListItem : ListItem ,IListViewItem
    {
        
        [SerializeField] private SideMenuButton skillButton;
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private SideMenuButton trigger1Button;
        [SerializeField] private TextMeshProUGUI trigger1Text;
        [SerializeField] private SideMenuButton trigger2Button;
        [SerializeField] private TextMeshProUGUI trigger2Text;

        private bool _buttonInit = false;

        public void SetInputListHandler(Action skillEvent,Action trigger1Event,Action trigger2Event)
        {
            if (_buttonInit) return;
            skillButton.SetCallHandler(() => skillEvent());
            trigger1Button.SetCallHandler(() => trigger1Event());
            trigger2Button.SetCallHandler(() => trigger2Event());
            _buttonInit = true;
            UpdateItemIndex(-1);
        }

        public void SetSelectItemHandler(Action<int> skillEvent,Action<int> trigger1Event,Action<int> trigger2Event)
        {
            skillButton.SetSelectHandler((a) => skillEvent(Index));
            trigger1Button.SetSelectHandler((a) => trigger1Event(Index));
            trigger2Button.SetSelectHandler((a) => trigger2Event(Index));
        }

        public void UpdateItemIndex(int index)
        {
            if (index == -1)
            {
                skillButton.SetUnSelect();
                trigger1Button.SetUnSelect();
                trigger2Button.SetUnSelect();
            }
            if (index == 0)
            {
                skillButton.SetSelect();
                trigger1Button.SetUnSelect();
                trigger2Button.SetUnSelect();
            }
            if (index == 1)
            {
                skillButton.SetUnSelect();
                trigger1Button.SetSelect();
                trigger2Button.SetUnSelect();
            }
            if (index == 2)
            {
                skillButton.SetUnSelect();
                trigger1Button.SetUnSelect();
                trigger2Button.SetSelect();
            }
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (SkillTriggerInfo)ListData.Data;
            skillInfoComponent.UpdateSkillData(data.SkillId);
            if (data.SkillTriggerDates.Count > 0 && data.SkillTriggerDates[0] != null)
            {
                trigger1Text.SetText(data.SkillTriggerDates[0].Name);
            } else
            {
                trigger1Text.SetText("-");
            }
            
            if (data.SkillTriggerDates.Count > 1 && data.SkillTriggerDates[1] != null)
            {
                trigger2Text.SetText(data.SkillTriggerDates[1].Name);
            } else
            {
                trigger2Text.SetText("-");
            }
        }
    }
}
