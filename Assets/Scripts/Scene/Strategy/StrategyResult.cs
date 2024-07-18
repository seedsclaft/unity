using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class StrategyResult : ListItem ,IListViewItem 
    {   
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private TextMeshProUGUI titleName;     
        public void UpdateViewItem()
        {
            if (ListData == null) 
            {
                return;
            }
            var data = (StrategyResultViewInfo)ListData.Data;
            if (data == null) 
            {
                return;
            }
            skillInfoComponent?.UpdateData(data.SkillId);
            if (titleName != null)
            {
                titleName.gameObject.SetActive(data.Title != "");
                titleName.SetText(data.Title);
                titleName.rectTransform.sizeDelta = new Vector2(titleName.preferredWidth,titleName.preferredHeight);
            }
        }
    }

    public class StrategyResultViewInfo
    {
        private int _skillId;
        public int SkillId => _skillId;
        public void SetSkillId(int skillId) => _skillId = skillId;
        private string _title;
        public string Title => _title;
        public void SetTitle(string title) => _title = title;
    }
}
