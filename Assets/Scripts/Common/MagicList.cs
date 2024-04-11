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
            var listData = ListData;
            if (listData != null)
            {
                var skillData = (SkillInfo)listData.Data;
                skillInfoComponent?.UpdateSkillData(skillData.Id);
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
    }
}
