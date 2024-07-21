using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class GetItem : ListItem ,IListViewItem  
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private TextMeshProUGUI titleName;
        [SerializeField] private TextMeshProUGUI resultName;

        public void UpdateViewItem()
        {
            if (ListData == null) 
            {
                return;
            }
            var _data = (GetItemInfo)ListData.Data;
            if (_data == null) return;
            if (skillInfoComponent != null)
            {
                var skillId = _data.SkillId;
                if (_data.GetItemType == GetItemType.SelectRelic)
                {
                    skillId = 0;
                }
                skillInfoComponent.UpdateData(skillId);
            }
            if (titleName != null)
            {
                titleName.gameObject.SetActive(_data.GetTitleData() != "");
                titleName.text = _data.GetTitleData();
            }
            /*
            if (resultName != null)
            {
                resultName.gameObject.SetActive(_data.ResultName != "");
                resultName.text = _data.ResultName;
                resultName.rectTransform.sizeDelta = new Vector2(resultName.preferredWidth,resultName.preferredHeight);
            }
            */
            if (Disable != null)
            {
                Disable.gameObject.SetActive(ListData.Enable == false);
            }
        }
    }
}