using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillAction : ListItem ,IListViewItem  
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private GameObject BgObj;
        [SerializeField] private GameObject AwakenObj;
        [SerializeField] private GameObject MessiahObj;
        [SerializeField] private GameObject DisableSkill;
        private SkillInfo _skillInfo; 
        public void SetData(SkillInfo data,int index)
        {
            _skillInfo = data;
            SetIndex(index);
        }

        public void SetCallHandler(System.Action<int> handler)
        {
            clickButton.onClick.AddListener(() =>
                {
                    if (Disable.gameObject.activeSelf) return;
                    handler(_skillInfo.Id);
                }
            );
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (SkillInfo)ListData.Data;
            skillInfoComponent.UpdateInfo(data);
            AwakenObj?.SetActive(data.Master.SkillType == SkillType.Awaken);
            MessiahObj?.SetActive(data.Master.SkillType == SkillType.Messiah);
            BgObj?.SetActive(data.Master.SkillType != SkillType.Messiah && data.Master.SkillType != SkillType.Awaken);
            DisableSkill?.SetActive(data.Enable == false);
        }
    }
}