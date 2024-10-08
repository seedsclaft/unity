
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class DictionarySkill : ListItem ,IListViewItem  
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private GameObject BgObj;
        [SerializeField] private GameObject AwakenObj;
        [SerializeField] private GameObject MessiahObj;


        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<SkillInfo>();
            skillInfoComponent.UpdateInfo(data);
            AwakenObj?.SetActive(data.Master.SkillType == SkillType.Awaken);
            MessiahObj?.SetActive(data.Master.SkillType == SkillType.Unique);
            BgObj?.SetActive(data.Master.SkillType != SkillType.Unique && data.Master.SkillType != SkillType.Awaken);
            if (ListData.Enable == false)
            {
                var question = data.Master.Name.Length - 1;
                skillInfoComponent.SetName(DataSystem.GetText(121010 + question));
            } else
            {
                skillInfoComponent.SetName(data.Master.Name);
            }
        }
    }
}