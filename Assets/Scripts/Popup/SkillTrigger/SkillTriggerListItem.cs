using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillTriggerListItem : ListItem ,IListViewItem
    {
        [SerializeField] private OnOffButton skillButton;
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private OnOffButton trigger1Button;
        [SerializeField] private TextMeshProUGUI trigger1Text;
        [SerializeField] private OnOffButton trigger2Button;
        [SerializeField] private TextMeshProUGUI trigger2Text;
        [SerializeField] private OnOffButton upButton;
        [SerializeField] private OnOffButton downButton;

        private bool _buttonInit = false;

        public void SetInputListHandler(Action skillEvent,Action trigger1Event,Action trigger2Event,Action upButtonEvent,Action downButtonEvent)
        {
            if (_buttonInit) return;
            skillButton.OnClickAddListener(() => skillEvent());
            trigger1Button.OnClickAddListener(() => trigger1Event());
            trigger2Button.OnClickAddListener(() => trigger2Event());
            upButton.OnClickAddListener(() => upButtonEvent());
            downButton.OnClickAddListener(() => downButtonEvent());
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
            } else
            if (index == 0)
            {
                skillButton.SetSelect();
                trigger1Button.SetUnSelect();
                trigger2Button.SetUnSelect();
            } else
            if (index == 1)
            {
                skillButton.SetUnSelect();
                trigger1Button.SetSelect();
                trigger2Button.SetUnSelect();
            } else
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
            var data = ListItemData<SkillTriggerInfo>();
            skillInfoComponent.UpdateData(data.SkillId);
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
            Disable?.SetActive(!ListData.Enable);
        }
    }
}
