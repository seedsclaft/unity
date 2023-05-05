using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using TMPro;

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
        if (_data == null) return;
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(_data.IsSkill());
            if (_data.IsSkill()) UpdateElementIcon((int)_data.SkillElementId);
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
        //var handle = await ResourceSystem.LoadAsset<IList<Sprite>>("Assets/Images/System/ElementIcon.png");
        var handle = Resources.LoadAll<Sprite>("System/ElementIcon");
        if (iconImage != null)
        {
            iconImage.sprite = handle[index-1];
        }
    }
}
