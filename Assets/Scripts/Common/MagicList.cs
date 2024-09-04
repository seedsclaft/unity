using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class MagicList : BaseList
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        public new void Initialize()
        {
            base.Initialize();
            SetSelectedHandler(() => UpdateSkillHelp());
        }

        public void UpdateSkillHelp()
        {
            if (skillInfoComponent == null)
            {
                return;
            }
            var listData = ListData;
            if (listData != null)
            {
                var skillInfo = (SkillInfo)listData.Data;
                skillInfoComponent.UpdateInfo(skillInfo);
            } else
            {
                skillInfoComponent.Clear();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            skillInfoComponent.gameObject.SetActive(true);
            UpdateSkillHelp();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            skillInfoComponent.gameObject.SetActive(false);
        }

        public override void UpdateHelpWindow()
        {
            UpdateSkillHelp();
        }
    }
}
