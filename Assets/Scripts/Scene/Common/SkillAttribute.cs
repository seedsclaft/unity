using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class SkillAttribute : ListItem ,IListViewItem  
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI valueText;
    private AttributeType _data; 
    private string _valueText; 
    public void SetData(AttributeType data,string valueText,int index){
        _data = data;
        _valueText = valueText;
        SetIndex(index);
    }

    public void SetCallHandler(System.Action<AttributeType> handler)
    {
        clickButton.onClick.AddListener(() => handler((AttributeType)Index));
    }

    public void UpdateViewItem()
    {
        if (_data == AttributeType.None) return;
        if (icon != null)
        {
            UpdateSkillIcon(Index-1);
        }
        
        if (valueText != null && _valueText != null)
        {
            valueText.text = _valueText;
        }
    }
    
    private void UpdateSkillIcon(int index)
    {
        Addressables.LoadAssetAsync<IList<Sprite>>(
            "Assets/Images/System/ElementIcon.png"
        ).Completed += op => {
            icon.sprite = op.Result[index];
        };
    }

    public new void SetSelect()
    {
        icon.color = new Color(1,1,1,1);
    }

    public new void SetUnSelect()
    {
        base.SetUnSelect();
        icon.color = new Color(1,1,1,0.5f);
    }
}
