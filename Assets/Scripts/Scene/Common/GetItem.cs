using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class GetItem : ListItem ,IListViewItem  
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleName;
    [SerializeField] private TextMeshProUGUI resultName;
    private GetItemInfo _data;

    public void UpdateViewItem()
    {
        if (ListData != null) 
        {
            _data = (GetItemInfo)ListData.Data;
        }
        if (_data == null) return;
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(_data.SkillElementId > -1);
            if (_data.SkillElementId > -1) UpdateElementIcon((int)_data.SkillElementId);
        }
        if (titleName != null)
        {
            titleName.gameObject.SetActive(_data.TitleName != "");
            titleName.text = _data.TitleName;
        }
        if (resultName != null)
        {
            resultName.gameObject.SetActive(_data.ResultName != "");
            resultName.text = _data.ResultName;
            resultName.rectTransform.sizeDelta = new Vector2(resultName.preferredWidth,resultName.preferredHeight);
        }
    }

    private void UpdateElementIcon(int index)
    {
        var spriteAtlas = ResourceSystem.LoadSystems();
        if (iconImage != null)
        {
            iconImage.sprite = spriteAtlas.GetSprite("ElementIcon_" + (index-1).ToString());
        }
    }
}
