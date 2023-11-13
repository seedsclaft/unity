using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using TMPro;
using UnityEngine.U2D;

public class GetItem : ListItem ,IListViewItem  
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleName;
    [SerializeField] private TextMeshProUGUI resultName;
    private GetItemInfo _data; 

    public void SetData(GetItemInfo data,int index){
        _data = data;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<GetItemInfo> handler)
    {
        clickButton.onClick.AddListener(() => handler((GetItemInfo)_data));
    }

    public void UpdateViewItem()
    {
        if (ListData == null) return;
        var data = (GetItemInfo)ListData.Data;
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(data.SkillElementId > -1);
            if (data.SkillElementId > -1) UpdateElementIcon((int)data.SkillElementId);
        }
        if (titleName != null)
        {
            titleName.gameObject.SetActive(data.TitleName != "");
            titleName.text = data.TitleName;
        }
        if (resultName != null)
        {
            resultName.gameObject.SetActive(data.ResultName != "");
            resultName.text = data.ResultName;
            resultName.rectTransform.sizeDelta = new Vector2(resultName.preferredWidth,resultName.preferredHeight);
        }
    }

    private void UpdateElementIcon(int index)
    {
        var spriteAtlas = Resources.Load<SpriteAtlas>("Texture/Systems");
        if (iconImage != null)
        {
            iconImage.sprite = spriteAtlas.GetSprite("ElementIcon_" + (index-1).ToString());
        }
    }
}
